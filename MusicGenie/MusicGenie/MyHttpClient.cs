/*
 * Copyright (c) 2013 avatar29A
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
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
