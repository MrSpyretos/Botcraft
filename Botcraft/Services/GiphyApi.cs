using Botcraft.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Botcraft.Services
{
    public class GiphyApi
    {
        private readonly IConfiguration _config;

        public GiphyApi(IConfiguration config)
        {
            _config = config;
        }

        public string ApiRequest(string url, string appendUrl = null)
        {
            string response = string.Empty; ;
            string apiKey = $"{_config["GiphyApi"]}";
            if (string.IsNullOrEmpty(appendUrl))
            {
                url = $"http://api.giphy.com/v1{url}?api_key={apiKey}";
            }
            else
            {
                url = $"http://api.giphy.com/v1{url}?api_key={apiKey}{appendUrl}";
            }
            Console.WriteLine($"Giphy request -> {url}");
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //test = httpClient.PostAsJsonAsync<FaceRequest>(fullUrl, request).Result;                             
                response = httpClient.GetStringAsync(url).Result;
            }

            return response;
        }

        public GiphyResponses GetRandomImage(string tags)
        {
            GiphyResponses r = new GiphyResponses();
            string url = string.Empty;
            string appendUrl = string.Empty;

            if (string.IsNullOrEmpty(tags))
            {
                url = $"/gifs/random";
            }
            else
            {
                tags = tags.Replace(" ", "+");
                url = $"/gifs/random";
                appendUrl = $"&tag={tags}";
            }
            r = JsonConvert.DeserializeObject<GiphyResponses>(ApiRequest(url, appendUrl));
            return r;
        }
    }
}
