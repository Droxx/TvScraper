using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TvScraper.Scraper
{
    internal class TvMazeClient
    {
        private const string BASE_URL = "https://api.tvmaze.com/";

        private readonly RestClient client;

        public TvMazeClient() 
        {
            Uri baseUrl = new Uri(BASE_URL);
            client = new RestClient(baseUrl);
        }

        public T Get<T>(string endpoint, object args = null)
        {
            RestRequest request = new RestRequest(endpoint, Method.Get) {  };
            RestResponse<T> response = client.Execute<T>(request);

            if (response != null && response.IsSuccessful)
            {
                return response.Data;
            }
            else
            {
                Console.WriteLine(response?.ErrorMessage);
                return default(T);
            }
        }
    }
}
