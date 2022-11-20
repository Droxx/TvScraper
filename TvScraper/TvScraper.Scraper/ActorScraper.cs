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
    public class ActorScraper
    {
        public async Task Execute(CancellationToken token)
        {
            // TODO: Make sure this client is shared globally
            var client = new TvMazeClient();
            var showsToScrape = GetNextXScrapingBatch(250);
            IEnumerable<CastMember> result = null;
            do
            {
                foreach (var showId in showsToScrape)
                {
                    try
                    {
                        result = await client.Get<IEnumerable<CastMember>>($"shows/{showId}/cast", token);
                    }
                    catch (HttpRequestException ex)
                    {
                        // TODO: Catch errors
                    }
                    if(result == null)
                    {
                        // TODO: Log error
                        continue;
                    }
                    StoreActors(result.Select(c => c.Person));
                    StoreLinks(showId, result);
                }
            } while(showsToScrape.Count > 0);
           
        }

        private List<int> GetNextXScrapingBatch(int batchSize) 
        {
            using(var database = new DataContext())
            {
                var next10UnscrapedShows = database
                    .Shows
                    .OrderBy(s => s.TvMazeId)
                    .Where(s => !s.ActorsScraped)
                    .Take(batchSize)
                    .Select(s => s.TvMazeId);

                return next10UnscrapedShows.ToList();
            }
        }

        private void StoreActors(IEnumerable<Person> actors)
        {
            using(var database = new DataContext())
            {
                var actorIds = actors.Select(a => a.Id);
                var duplicateActors = database
                    .Actors
                    .Where(a => actorIds.Contains(a.TvMazeId))
                    .Select(a => a.TvMazeId)
                    .ToList();

                var toStore = new List<Database.Model.Actor>();

                foreach(var validActor in actors.Where(a => !duplicateActors.Contains(a.Id)))
                {
                    toStore.Add(new Database.Model.Actor
                    {
                        Name = validActor.Name,
                        DateOfBirth = validActor.Birthday,
                        TvMazeId = validActor.Id
                    });
                }

                database.Actors.AddRange(toStore);
                database.SaveChanges();
            }
        }

        private void StoreLinks(int showId, IEnumerable<CastMember> cast)
        {
            using (var database = new DataContext())
            {
                var localDbShow = database.Shows.FirstOrDefault(s => s.TvMazeId == showId);

                foreach (var member in cast)
                {
                    var localDbCast = database.Actors.FirstOrDefault(a => a.TvMazeId == member.Person.Id);

                    if (localDbShow == null || localDbCast == null)
                    {
                        throw new KeyNotFoundException();
                    }

                    database.CastMembers.Add(new Database.Model.CastMember
                    {
                        ActorId = localDbCast.Id,
                        ShowId = localDbShow.Id
                    });
                }
                localDbShow.ActorsScraped = true;
                database.SaveChanges();
            }
        }

    }
}
