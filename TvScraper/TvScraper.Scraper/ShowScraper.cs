using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TvScraper.Database;
using TvScraper.Scraper.TvMazeModel;

namespace TvScraper.Scraper
{
    public interface IShowScraper
    {
        Task Execute(CancellationToken token);
    }

    public class ShowScraper : IDisposable, IShowScraper
    {
        private readonly DataContext database;
        private readonly ITvMazeClient client;
        private readonly ILogger<ShowScraper> logger;

        public ShowScraper(ITvMazeClient client, ILogger<ShowScraper> logger)
        {
            database = new DataContext();
            this.client = client;
            this.logger = logger;
        }

        public void Dispose()
        {
            database.Dispose();
        }

        public async Task Execute(CancellationToken token)
        {
            var page = await GetStartingPageNumber();
            IEnumerable<Show> result = null;
            do
            {
                var parameters = new List<GetParameter> { new GetParameter { Name = "page", Value = page } };
                try
                {
                    result = await client.Get<IEnumerable<Show>>("shows", token, parameters);
                }
                catch(HttpRequestException ex)
                {
                    if(ex.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        continue;
                    }
                    if(ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        break;
                    }

                    logger.LogError("HTTPRequest exception encountered when scraping shows", ex);
                }
                await StoreBatch(result);
                await database.SaveChangesAsync();
                page++;
            } while (result.Count() > 0);
            logger.LogInformation("Completed scraping shows, exiting scraper");
        }

        private async Task StoreBatch(IEnumerable<Show> shows)
        {
            logger.LogDebug($"Storing batch of {shows.Count()} shows");
            var collectionIds = shows.Select(s => s.Id);

            var duplicateShows = database
                .Shows
                .Where(s => collectionIds.Contains(s.TvMazeId))
                .Select(s => s.TvMazeId);

            var showsToBeInserted = new List<Database.Model.Show>();

            logger.LogDebug($"Duplicates removed and {shows.Count()} shows to be written to database");

            foreach (var validShow in shows.Where(s => !duplicateShows.Contains(s.Id)))
            {
                showsToBeInserted.Add(new Database.Model.Show
                {
                    Name = validShow.Name,
                    TvMazeId = validShow.Id,
                    TvMazeUrl = validShow.Url,
                    LastScrapeDate = DateTime.UtcNow
                });
            }

            await database.Shows.AddRangeAsync(showsToBeInserted);
        }

        private async Task<int> GetStartingPageNumber()
        {

            var mostRecentShow = await database
                .Shows
                .OrderByDescending(s => s.TvMazeId)
                .FirstOrDefaultAsync();

            var mostRecentId = mostRecentShow?.TvMazeId ?? 0;
            var pageNumber = (int)Math.Floor(mostRecentId / 250.0);
            logger.LogInformation($"Starting at page number {pageNumber}, last imported show TvMazeID {mostRecentId}");

            return pageNumber;

        }
    }
}
