using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TvScraper.API.ApiModel;
using TvScraper.Scraper;

namespace TvScraper.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ShowInfoController : ControllerBase
    {
        private readonly ILogger<ShowInfoController> _logger;

        private readonly ShowInfoBuilder builder;


        public ShowInfoController(ILogger<ShowInfoController> logger)
        {
            _logger = logger;
            this.builder = new ShowInfoBuilder();
        }

        [HttpGet(Name = "GetShowInfo")]
        public async Task<IEnumerable<ShowInfo>> Get(int page, CancellationToken token)
        {
            var info = builder.Shows(page);
            return info;
        }
    }
}
