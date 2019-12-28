using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Slack;
using BabouExtensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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

        public ClaimsIdentity userIdentity { get; set; }
        public ClaimsPrincipal UserPrincipal { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
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

            services.AddAuthentication(options =>
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "StanLeeSlackBotAuthCookie";
                    //options.Cookie.Domain = "babou.io";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.MaxAge = TimeSpan.FromDays(7);
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);

                    options.LoginPath = $"/login";
                    options.LogoutPath = $"/logout";
                    options.AccessDeniedPath = $"/AccessDenied";
                    options.SlidingExpiration = true;
                    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                })
                //.AddSlack(options =>
                //{
                //    options.ClientId = Configuration["Slack:ClientId"];
                //    options.ClientSecret = Configuration["Slack:ClientSecret"];
                //});
                .AddOAuth("Slack", options =>
                {
                    options.ClientId = Configuration["Slack:ClientId"];
                    options.ClientSecret = Configuration["Slack:ClientSecret"];
                    options.CallbackPath = new PathString("/signin-slack");
                    options.AuthorizationEndpoint = $"https://slack.com/oauth/authorize";
                    options.TokenEndpoint = "https://slack.com/api/oauth.access";
                    options.UserInformationEndpoint = "https://slack.com/api/users.identity?token=";
                    options.Scope.Add("identity.basic");
                    options.Events = new OAuthEvents()
                    {
                        OnCreatingTicket = async context =>
                        {
                            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint + context.AccessToken);
                            var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                            response.EnsureSuccessStatusCode();
                            var userObject = JObject.Parse(await response.Content.ReadAsStringAsync());
                            var user = userObject.SelectToken("user");
                            var userId = user.Value<string>("id");

                            //userIdentity = new ClaimsIdentity("Slack");
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
            services.AddRazorPages();

            services.Configure<AppSettings>(Configuration);

            services.AddSingleton<ISlackService, SlackService>();
            services.AddHostedService<SlackBackgroundService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
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

            app.Map("/login", builder =>
            {
                builder.Run(async context =>
                {
                    await context.ChallengeAsync("Slack", properties: new AuthenticationProperties { RedirectUri = "/" });
                });
            });

            app.Map("/logout", builder =>
            {
                builder.Run(async context =>
                {
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    context.Response.Redirect("/");
                });
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
