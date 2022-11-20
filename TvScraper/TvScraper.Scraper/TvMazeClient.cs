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
    internal class TvMazeClient
    {
        private const string BASE_URL = "https://api.tvmaze.com/";


        private readonly RestClient client;
        private readonly RateLimiter limiter;

        public TvMazeClient() 
        {
            Uri baseUrl = new Uri(BASE_URL);
            client = new RestClient(baseUrl);
            limiter = new RateLimiter(20, TimeSpan.FromSeconds(11));
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
            RestResponse<T> response = client.Execute<T>(request);

            if (response != null && response.IsSuccessful)
            {
                return response.Data;
            }
            else
            {
                // TODO: Better error handling
                if(response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    throw new HttpRequestException("(429) Too Many Requests", null, response.StatusCode);
                }
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new HttpRequestException("(404) Not Found", null, response.StatusCode);
                }
                Console.WriteLine(response?.ErrorMessage);
                return default(T);
            }
        }

    }
}
