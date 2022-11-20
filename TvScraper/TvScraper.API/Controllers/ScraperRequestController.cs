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
            var showGetter = new ShowScraper();
            await showGetter.Execute(token);
        }
    }
}
