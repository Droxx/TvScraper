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
    public class ShowScrapeService : BackgroundService
    {
        private readonly ILogger<ShowScrapeService> logger;

        private readonly IServiceScopeFactory scopeFactory;

        public bool IsEnabled { get; set; } = true;
        private int executionCount = 0;

        public ShowScrapeService(
            ILogger<ShowScrapeService> logger,
            IServiceScopeFactory scopeFactory)
        {
            this.logger = logger;
            this.scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            logger.LogInformation("Starting show scraping service");
            using (var database = new DataContext())
            {
                database.Database.EnsureCreated();
            }
            
            // Every 2 hours scrape for new shows
            using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromHours(2));
            do
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                try
                {
                    logger.LogInformation("Beginning scrape for unknown TV shows");
                    await using AsyncServiceScope asyncScope = scopeFactory.CreateAsyncScope();

                    var showScraper = asyncScope.ServiceProvider.GetRequiredService<IShowScraper>();
                    await showScraper.Execute(token);
                    logger.LogInformation("Scrape for TV shows completed, waiting for next sync time");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error thrown while scraping shows!");
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
