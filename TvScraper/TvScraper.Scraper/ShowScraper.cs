using Microsoft.EntityFrameworkCore;
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

        public ShowScraper(ITvMazeClient client)
        {
            database = new DataContext();
            this.client = client;   
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
                }
                await StoreBatch(result);
                await database.SaveChangesAsync();
                page++;
            } while (result.Count() > 0);
        }

        private async Task StoreBatch(IEnumerable<Show> shows)
        {
            var collectionIds = shows.Select(s => s.Id);

            var duplicateShows = database
                .Shows
                .Where(s => collectionIds.Contains(s.TvMazeId))
                .Select(s => s.TvMazeId);

            var showsToBeInserted = new List<Database.Model.Show>();

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

            return (int)Math.Floor(mostRecentId / 250.0);

        }
    }
}
