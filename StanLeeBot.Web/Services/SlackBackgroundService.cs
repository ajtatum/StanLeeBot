using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StanLeeBot.Web.Services.Interfaces;

namespace StanLeeBot.Web.Services
{
    public class SlackBackgroundService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public SlackBackgroundService(IServiceProvider services)
        {
            _serviceProvider = services;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var slackService = scope.ServiceProvider.GetRequiredService<ISlackService>();

                //slackService.SendBotMessage();
                return Task.CompletedTask;

            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
