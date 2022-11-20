using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TvScraper.Scraper.Services;

namespace TvScraper.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ScraperStatusController : ControllerBase
    {
        private readonly ILogger<ScraperStatusController> _logger;
        private readonly ShowScrapeService scrapeService;

        public ScraperStatusController(ILogger<ScraperStatusController> logger,
            ShowScrapeService scrapeService)
        {
            _logger = logger;
            this.scrapeService = scrapeService;
        }

        [HttpGet(Name = "GetScraperStatus")]
        public async Task<ScraperStatus> Get(CancellationToken token)
        {
            return new ScraperStatus(scrapeService.IsEnabled);
        }
    }
}
