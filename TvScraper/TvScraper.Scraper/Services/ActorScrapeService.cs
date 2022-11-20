using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TvScraper.Database;

namespace TvScraper.Scraper.Services
{
    public class ActorScrapeService : BackgroundService
    {
        private readonly ILogger<ActorScrapeService> logger;

        private readonly IServiceScopeFactory scopeFactory;

        public bool IsEnabled { get; set; } = true;
        private int executionCount = 0;

        public ActorScrapeService(
            ILogger<ActorScrapeService> logger,
            IServiceScopeFactory scopeFactory)
        {
            this.logger = logger;
            this.scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            logger.LogInformation("Starting actor scraping service");
            using (var database = new DataContext())
            {
                database.Database.EnsureCreated();
            }

            // Every two minutes start up an actor scraper to grab and scrape any actors for shows
            // that have been updated
            using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMinutes(2));
            do
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                try
                {
                    logger.LogInformation("Beginning scrape for actors of recently imported shows");
                    await using AsyncServiceScope asyncScope = scopeFactory.CreateAsyncScope();

                    var actorScraper = asyncScope.ServiceProvider.GetRequiredService<IActorScraper>();
                    await actorScraper.Execute(token);
                    logger.LogInformation("Scrape for actors completed, waiting for next sync time");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error thrown while scraping Actors!");
                    break;
                }

                executionCount++;
            } while (
                    !token.IsCancellationRequested &&
                    await timer.WaitForNextTickAsync(token));
            IsEnabled = false;
        }
    }
}
