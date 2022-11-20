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
    public interface IActorScraper
    {
        Task Execute(CancellationToken token);
    }

    public class ActorScraper : IDisposable, IActorScraper
    {
        private readonly DataContext database;
        private ILogger<ActorScraper> logger;
        private ITvMazeClient client;

        public ActorScraper(ITvMazeClient client, ILogger<ActorScraper> logger)
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
                        logger.LogDebug($"Fetching actor information for show {showId}");
                        result = await client.Get<IEnumerable<CastMember>>($"shows/{showId}/cast", token);
                    }
                    catch (HttpRequestException ex)
                    {
                        if (ex.StatusCode == HttpStatusCode.TooManyRequests)
                        {
                            // Wait for 5 seconds, we may have hit a temporary strict rate limit
                            await Task.Delay(5000);
                            continue;
                        }
                        if (ex.StatusCode == HttpStatusCode.NotFound)
                        {
                            break;
                        }

                        logger.LogError("HTTPRequest exception encountered when scraping actors", ex);
                    }
                    if(result == null)
                    {
                        logger.LogError($"A null result was returned when scraping actors for show {showId}");
                        continue;
                    }
                    StoreActors(result.Select(c => c.Person));
                    await database.SaveChangesAsync();
                    StoreLinks(showId, result);
                    await database.SaveChangesAsync();
                }
                showsToScrape = await GetNextXScrapingBatch(250);
            } while (showsToScrape.Count > 0);
            logger.LogInformation("Completed scraping for actors, waiting until next batch available");
        }

        /// <summary>
        /// Grabs the MazeIDs of shows which have not had their actors scraped
        /// </summary>
        /// <param name="batchSize">Specify the maximum number of IDs to fetch</param>
        /// <returns>A list of TvMaze IDs for shows awaiting actors</returns>
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

        /// <summary>
        /// Stores the actors in the database after removing duplicates
        /// </summary>
        private void StoreActors(IEnumerable<Person> actors)
        {
            logger.LogDebug($"Storing {actors.Count()} actors");

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
            logger.LogDebug($"Duplicates removed, writing {toStore.Count()} actors to database");

            database.Actors.AddRange(toStore);
        }

        /// <summary>
        /// Stores records in the joining table between cast and actors
        /// </summary>
        private void StoreLinks(int showId, IEnumerable<CastMember> cast)
        {
            logger.LogDebug($"Storing {cast.Count()} links for show {showId}");
            var localDbShow = database.Shows.FirstOrDefault(s => s.TvMazeId == showId);

            if (localDbShow != null)
            {
                foreach (var member in cast)
                {
                    var localDbCast = database.Actors.FirstOrDefault(a => a.TvMazeId == member.Person.Id);

                    if (localDbCast == null)
                    {
                        throw new KeyNotFoundException();
                    }

                    database.CastMembers.Add(new Database.Model.CastMember
                    {
                        ActorId = localDbCast.Id,
                        ShowId = localDbShow.Id
                    });
                }
                localDbShow.LastScrapeDate = DateTime.UtcNow;
                localDbShow.ActorsScraped = true;
            }
        }

    }
}
