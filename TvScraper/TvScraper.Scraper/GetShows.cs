using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TvScraper.Scraper.TvMazeModel;

namespace TvScraper.Scraper
{
    public class GetShows
    {
        public ShowCollection Execute()
        {
            var client = new TvMazeClient();
            var result = client.Get<IEnumerable<Show>>("shows");
            return new ShowCollection() { Shows = result };
        }
    }
}
