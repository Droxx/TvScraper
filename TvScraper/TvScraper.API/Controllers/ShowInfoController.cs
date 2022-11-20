using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TvScraper.Scraper;

namespace TvScraper.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ShowInfoController : ControllerBase
    {
        private readonly ILogger<ShowInfoController> _logger;


        public ShowInfoController(ILogger<ShowInfoController> logger)
        {
            _logger = logger;
        }

    }
}
