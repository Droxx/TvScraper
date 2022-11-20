using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TvScraper.Scraper
{
    public interface ITvMazeClient
    {
        Task<T> Get<T>(string endpoint, CancellationToken token, IEnumerable<GetParameter> args = null);
    }

    public class TvMazeClient : ITvMazeClient
    {
        private const string BASE_URL = "https://api.tvmaze.com/";
        private readonly ILogger<TvMazeClient> logger;  

        private readonly RestClient client;
        private readonly RateLimiter limiter;

        public TvMazeClient(ILogger<TvMazeClient> logger) 
        {
            Uri baseUrl = new Uri(BASE_URL);
            client = new RestClient(baseUrl);
            limiter = new RateLimiter(20, TimeSpan.FromSeconds(11));
            this.logger = logger;
        }

        public async Task<T> Get<T>(string endpoint, CancellationToken token, IEnumerable<GetParameter> args = null)
        {
            await limiter.Wait();
            RestRequest request = new RestRequest(endpoint, Method.Get) {  };
            if (args != null)
            {
                foreach (var arg in args)
                {
                    request.AddParameter(arg.Name, arg.Value, ParameterType.QueryString);
                }
            }
            logger.LogDebug($"Calling Maze endpoint '{endpoint}'");
            RestResponse<T> response = client.Execute<T>(request);

            if (response != null && response.IsSuccessful)
            {
                logger.LogTrace($"Data recieved OK");
                return response.Data;
            }
            else if(response != null)
            {
                logger.LogWarning("Bad response recieved from Maze API");
                if(response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    throw new HttpRequestException("(429) Too Many Requests", null, response.StatusCode);
                }
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new HttpRequestException("(404) Not Found", null, response.StatusCode);
                }
                Console.WriteLine(response?.ErrorMessage);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new HttpRequestException($"Unexpected error code returned {response?.ErrorMessage}", new HttpRequestException("Unexpected error code returned", null, response.StatusCode));
                }
            }
            else
            {
                logger.LogError($"Null response recieved from RestSharp, Endpoint {endpoint}");
            }
            return default(T);
        }

    }
}
