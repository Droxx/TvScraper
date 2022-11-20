using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TvScraper.Scraper;

namespace TvScraper.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ScraperRequestController : ControllerBase
    {
        private readonly ILogger<ScraperRequestController> _logger;

        public ScraperRequestController(ILogger<ScraperRequestController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetScraperRequest")]
        public async Task Get(CancellationToken token)
        {
            // TODO: Make this a service that runs on boot and every X hours
            var showGetter = new ShowScraper();
            var actorGetter = new ActorScraper();
            //await actorGetter.Execute(token);
            await showGetter.Execute(token);
        }
    }
}
