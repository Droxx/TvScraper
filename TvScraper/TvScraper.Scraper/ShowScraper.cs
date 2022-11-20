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
    public class ShowScraper
    {
        public async Task Execute(CancellationToken token)
        {
            var client = new TvMazeClient();
            var page = GetStartingPageNumber();
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
                StoreBatch(result);
                page++;
            } while (result.Count() > 0);
        }

        private void StoreBatch(IEnumerable<Show> shows)
        {
            using(var database = new DataContext())
            {
                var collectionIds = shows.Select(s => s.Id);

                var duplicateShows = database
                    .Shows
                    .Where(s => collectionIds.Contains(s.TvMazeId))
                    .Select(s => s.TvMazeId);

                var showsToBeInserted = new List<Database.Model.Show>();

                foreach(var validShow in shows.Where(s => !duplicateShows.Contains(s.Id)))
                {
                    showsToBeInserted.Add(new Database.Model.Show
                    {
                        Name = validShow.Name,
                        TvMazeId = validShow.Id,
                        TvMazeUrl = validShow.Url,
                        LastScrapeDate = DateTime.UtcNow
                    });
                }

                database.Shows.AddRange(showsToBeInserted);
                database.SaveChanges();
            }
        }

        private int GetStartingPageNumber()
        {
            using(var database = new DataContext())
            {
                var mostRecentShow = database
                    .Shows
                    .OrderByDescending(s => s.TvMazeId)
                    .FirstOrDefault();

                var mostRecentId = mostRecentShow?.TvMazeId ?? 0;

                return (int)Math.Floor(mostRecentId / 250.0);
            }
        }
    }
}
