/*
 * Copyright (c) 2013 avatar29A
 * Edited by gabrwagn
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
using System.Web.Http;
using MusicGenie;

namespace Hqub.MusicBrainz.API
{
    public class MyHttpClient
    {
        public const string UserAgent = "MusicGenie/1.0 (gabrwagn@gmail.com)";

        private HttpClient _httpClient;
        private ICacheService _cache;

        private readonly int _maxRetries;
        private readonly int _delayMilliseconds;
        private readonly int _maxDelayMilliseconds;


        public MyHttpClient(ICacheService cache)
        {
            _cache = cache;
            _httpClient = new HttpClient();

            _maxRetries = 10;
            _delayMilliseconds = 300;
            _maxDelayMilliseconds = 5000;
           
        }

        public async Task<string> SendRequestAsync(string url)
        {
            return await _cache.GetOrSet(url, () => SendRequestAsyncUncached(url));
        }

        public async Task<string> SendRequestAsyncUncached(string url)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            request.Headers.Add("User-Agent", UserAgent);

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                HttpResponseException e = new HttpResponseException(response);
                throw new HttpResponseException(response);
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> RetryWithExponentialBackoff(string url)
        {
            ExponentialBackoff backoff = new ExponentialBackoff(_maxRetries, _delayMilliseconds, _maxDelayMilliseconds);

            retry:
            try
            {
                string response = await SendRequestAsync(url);
               

                return response;
            }
            catch (TimeoutException e)
            {
                return "";
            }
            catch (HttpResponseException e)
            {
                if (e.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return "";
                }

                Debug.WriteLine("HttpResponseException –Message: " + e.Response.StatusCode);
                await backoff.Delay();
                goto retry;
            }
           

        }
    }

    public struct ExponentialBackoff
    {
        private readonly int m_maxRetries, m_delayMilliseconds, m_maxDelayMilliseconds;
        private int m_retries, m_pow;

        public ExponentialBackoff(int maxRetries, int delayMilliseconds, int maxDelayMilliseconds)
        {
            m_maxRetries = maxRetries;
            m_delayMilliseconds = delayMilliseconds;
            m_maxDelayMilliseconds = maxDelayMilliseconds;
            m_retries = 0;
            m_pow = 1;
        }

        public Task Delay()
        {
            if (m_retries == m_maxRetries)
            {
                throw new TimeoutException("Max retry attempts exceeded.");
            }
            ++m_retries;
            if (m_retries < 31)
            {
                m_pow = m_pow << 1; // m_pow = Pow(2, m_retries - 1)
            }
            int delay = Math.Min(m_delayMilliseconds * (m_pow - 1) / 2, m_maxDelayMilliseconds);
            return Task.Delay(delay);
        }
    }
}
