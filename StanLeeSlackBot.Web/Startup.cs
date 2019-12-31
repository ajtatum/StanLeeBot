using System;
using System.Net.Http;
using System.Security.Claims;
using AspNet.Security.OAuth.Slack;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Newtonsoft.Json.Linq;
using StanLeeSlackBot.Web.Models;
using StanLeeSlackBot.Web.Services;
using StanLeeSlackBot.Web.Services.Interfaces;

namespace StanLeeSlackBot.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var azureStorageAccountConnection = Configuration["Azure:Storage:ConnectionString"];
            var cloudStorageAccount = CloudStorageAccount.Parse(azureStorageAccountConnection);
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            var container = cloudBlobClient.GetContainerReference(Configuration["Azure:Storage:DataProtectionContainer"]);

            services.AddDataProtection()
                .SetApplicationName("StanLeeSlackBot")
                .PersistKeysToAzureBlobStorage(container, $"{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}/dataprotectionkeys.xml")
                .ProtectKeysWithAzureKeyVault(Configuration["Azure:KeyVault:EncryptionKey"], Configuration["Azure:KeyVault:ClientId"], Configuration["Azure:KeyVault:ClientSecret"]);

            var slackState = Guid.NewGuid().ToString("N");

            services.AddAuthentication(options =>
                    {
                        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    })
                    .AddCookie(options =>
                    {
                        options.LoginPath = "/login";
                        options.LogoutPath = "/logout";
                    })
                     .AddSlack(options =>
                    {
                        options.ClientId = Configuration["Slack:ClientId"];
                        options.ClientSecret = Configuration["Slack:ClientSecret"];
                        options.CallbackPath = $"{SlackAuthenticationDefaults.CallbackPath}?state={slackState}";
                        options.ReturnUrlParameter = new PathString("/");
                        options.Events = new OAuthEvents()
                        {
                            OnCreatingTicket = async context =>
                            {
                                var request = new HttpRequestMessage(HttpMethod.Get, $"{context.Options.UserInformationEndpoint}?token={context.AccessToken}");
                                var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                                response.EnsureSuccessStatusCode();
                                var userObject = JObject.Parse(await response.Content.ReadAsStringAsync());
                                var user = userObject.SelectToken("user");
                                var userId = user.Value<string>("id");


                                if (!string.IsNullOrEmpty(userId))
                                {
                                    context.Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId, ClaimValueTypes.String, context.Options.ClaimsIssuer));
                                }

                                var fullName = user.Value<string>("name");
                                if (!string.IsNullOrEmpty(fullName))
                                {
                                    context.Identity.AddClaim(new Claim(ClaimTypes.Name, fullName, ClaimValueTypes.String, context.Options.ClaimsIssuer));
                                }
                            }
                        };
                    });

            services.AddMvc();
            services.AddRazorPages()
                .AddRazorRuntimeCompilation();

            services.Configure<AppSettings>(Configuration);

            services.AddTransient<IEmailService, EmailService>();
            services.AddSingleton<ISlackService, SlackService>();
            services.AddHostedService<SlackBackgroundService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
