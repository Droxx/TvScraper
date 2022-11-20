using Microsoft.EntityFrameworkCore;
using TvScraper.API.ApiModel;
using TvScraper.Database;

namespace TvScraper.API
{
    public class ShowInfoBuilder
    {
        private const int SHOWS_PER_PAGE = 50;
        public List<ShowInfo> Shows(int page = 0)
        {
            using(var database = new DataContext())
            {
                var shows = database
                    .Shows
                    .Skip(SHOWS_PER_PAGE * page)
                    .Take(SHOWS_PER_PAGE);

                return shows.Select(s => new ShowInfo
                {
                    Name = s.Name,
                    Id = s.Id,
                    Cast = s.Cast
                    .OrderByDescending(c => c.Actor.DateOfBirth)
                    .Select(c => new CastInfo
                    {
                        Id = c.Actor.Id,
                        Birthday = c.Actor.DateOfBirth,
                        Name = c.Actor.Name
                    }),
                }).ToList();            
            }
        }
    }
}
