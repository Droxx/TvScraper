using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TvScraper.Database;

namespace TvScraper.Scraper
{
    public class ScrapeService : BackgroundService
    {
        private readonly ILogger<ScrapeService> logger;
        private readonly TvMazeClient mazeClient;

        public bool IsEnabled { get; set; } = true;
        private int executionCount = 0;

        public ScrapeService(
            ILogger<ScrapeService> logger,
            ITvMazeClient mazeClient)
        {
            logger = logger;
            mazeClient = mazeClient;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            using(var database = new DataContext())
            {
                database.Database.EnsureCreated();
            }

            using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromHours(12));
            do
            {
                try
                {
                    using (var showScraper = new ShowScraper())
                    {
                        await showScraper.Execute(token);
                    }

                    using (var actorScraper = new ActorScraper())
                    {
                        await actorScraper.Execute(token);
                    }
                }catch(Exception ex)
                {
                    logger.LogError(ex, "Error thrown while scraping!");
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
