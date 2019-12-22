using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
