﻿// AUTHOR: avatar29A
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Hqub.MusicBrainz.API
{
    public class MyHttpClient
    {
        public const string UserAgent = "SangeetManager/1.0";

        private HttpClient _httpClient;
        private HttpRequestMessage _requestMessage;

        public MyHttpClient(string url)
            : this(new Uri(url)) { }

        public MyHttpClient(Uri uri)
        {
            _httpClient = new HttpClient();
            _requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            _requestMessage.Headers.Add("User-Agent", UserAgent);
        }

        public async Task<HttpResponseMessage> SendRequestAsync()
        {
            HttpResponseMessage response = null;

            try
            {
                response = await _httpClient.SendAsync(_requestMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception:" + ex.Message);
                throw new HttpClientException(ex.HResult);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpClientException(response.StatusCode);
            }

            return response;
        }
    }
}
