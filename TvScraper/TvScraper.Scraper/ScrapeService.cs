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
        private readonly ActorScraper actorScraper;
        private readonly ShowScraper showScraper;

        public bool IsEnabled { get; set; } = true;
        private int executionCount = 0;

        public ScrapeService(
            ILogger<ScrapeService> logger,
            ITvMazeClient mazeClient)
        {
            logger = logger;
            mazeClient = mazeClient;
            actorScraper = new ActorScraper();
            showScraper = new ShowScraper();
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
                await showScraper.Execute(token);
                await actorScraper.Execute(token);
                executionCount++;
            } while (
                    !token.IsCancellationRequested &&
                    await timer.WaitForNextTickAsync(token));
            IsEnabled = false;
        }
    }
}
