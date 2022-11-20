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
    public interface IActorScraper
    {
        Task Execute(CancellationToken token);
    }

    public class ActorScraper : IDisposable, IActorScraper
    {
        private readonly DataContext database;
        private ITvMazeClient client;

        public ActorScraper(ITvMazeClient client)
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
            var showsToScrape = await GetNextXScrapingBatch(250);
            IEnumerable<CastMember> result = null;
            do
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

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
                    await database.SaveChangesAsync();
                    StoreLinks(showId, result);
                    await database.SaveChangesAsync();
                }
                showsToScrape = await GetNextXScrapingBatch(250);
            } while (showsToScrape.Count > 0);
           
        }

        private async Task<List<int>> GetNextXScrapingBatch(int batchSize)
        {
            var next10UnscrapedShows = database
                .Shows
                .OrderBy(s => s.TvMazeId)
                .Where(s => !s.ActorsScraped)
                .Take(batchSize)
                .Select(s => s.TvMazeId);

            return await next10UnscrapedShows.ToListAsync();
        }

        private void StoreActors(IEnumerable<Person> actors)
        {

            var actorIds = actors.Select(a => a.Id);
            var duplicateActors = database
                .Actors
                .Where(a => actorIds.Contains(a.TvMazeId))
                .Select(a => a.TvMazeId)
                .ToList();

            var toStore = new List<Database.Model.Actor>();

            foreach (var validActor in actors.Where(a => !duplicateActors.Contains(a.Id)))
            {
                toStore.Add(new Database.Model.Actor
                {
                    Name = validActor.Name,
                    DateOfBirth = validActor.Birthday,
                    TvMazeId = validActor.Id
                });
            }

            database.Actors.AddRange(toStore);
        }

        private void StoreLinks(int showId, IEnumerable<CastMember> cast)
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
        }

    }
}
