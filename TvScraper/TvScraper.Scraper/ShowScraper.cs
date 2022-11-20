using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TvScraper.Scraper.TvMazeModel;

namespace TvScraper.Scraper
{
    public class ShowScraper
    {
        public async Task<ShowCollection> Execute(CancellationToken token)
        {
            var client = new TvMazeClient();
            var shows = new List<Show>();
            var page = 0;
            IEnumerable<Show> result = null;
            do
            {
                var parameters = new List<GetParameter> { new GetParameter { Name = "page", Value = page } };
                try
                {
                    result = await client.Get<IEnumerable<Show>>("shows", token, parameters);
                }
                catch (KeyNotFoundException)
                {
                    break;
                }
                catch(HttpRequestException ex)
                {

                }
                shows.AddRange(result);
                page++;
            } while (result.Count() > 0);
            return new ShowCollection() { Shows = shows };
        }
    }
}
