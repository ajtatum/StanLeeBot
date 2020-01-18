using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using StanLeeBot.Web.Builders.Search;
using StanLeeBot.Web.Builders.Search.Interfaces;
using StanLeeBot.Web.Models;
using StanLeeBot.Web.Services;
using StanLeeBot.Web.Services.Interfaces;

namespace StanLeeBot.Web
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
                .SetApplicationName("StanLeeBot")
                .PersistKeysToAzureBlobStorage(container, $"{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}/dataprotectionkeys.xml")
                .ProtectKeysWithAzureKeyVault(Configuration["Azure:KeyVault:EncryptionKey"], Configuration["Azure:KeyVault:ClientId"], Configuration["Azure:KeyVault:ClientSecret"]);

            services.AddMvc()
                .AddNewtonsoftJson(x =>
                {
                    x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    //x.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });
            services.AddRazorPages()
                .AddNewtonsoftJson(x =>
                {
                    x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    //x.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                })
                .AddRazorRuntimeCompilation();

            services.Configure<AppSettings>(Configuration);

            services.AddScoped<IGoogleSearchService, GoogleSearchService>();
            services.AddScoped<ISearchBuilder<MarvelSearchBuilder>, MarvelSearchBuilder>();
            services.AddScoped<ISearchBuilder<DCComicsSearchBuilder>, DCComicsSearchBuilder>();
            
            services.AddScoped<ISlackService, SlackService>();
            services.AddScoped<ITelegramMessagingService, TelegramMessagingService>();
            services.AddSingleton<ITelegramBotService, TelegramBotService>();

            services.AddTransient<IEmailService, EmailService>();
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
