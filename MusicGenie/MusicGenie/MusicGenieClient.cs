using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace MusicGenie
{ 

    /// <summary>
    /// Class used for sending http requests.
    /// Implements optional caching and retry methods.
    /// </summary>
    public class MusicGenieClient
    {
        public const string UserAgent = "MusicGenie/1.0 (gabrwagn@gmail.com)";

        private HttpClient _httpClient;
        private ICacheService _cache;

        private readonly int _maxRetries;
        private readonly int _delayMilliseconds;
        private readonly int _maxDelayMilliseconds;


        public MusicGenieClient(ICacheService cache)
        {
            _cache = cache;
            _httpClient = new HttpClient();

            // MusicBrainz unclear on rules
            // Either 50 or 1 request per second.
            // Values chosen somewhere inbetween, backoff should smooth out any problems.
            _maxRetries = 8;
            _delayMilliseconds = 50; // 20 requests per second
            _maxDelayMilliseconds = 5000; // 1 request per 5 seconds max

            // Future idea: Scale delay by current load / pending requests
           
        }

        /// <summary>
        /// Method for checking cached responses to requests, otherwise sending a new request.
        /// </summary>
        /// <param name="url"> Request destination </param>
        /// <returns> String with content of the response or cache, failed requests return empty strings. </returns>
        public async Task<string> SendRequestAsync(string url)
        {
            return await _cache.GetOrSet(url, () => SendRequestAsyncUncached(url));
        }

        /// <summary>
        /// Method for sending a request.
        /// </summary>
        /// <param name="url"> Request destination </param>
        /// <returns> String with content of the response, failed requests return empty strings </returns>
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

        /// <summary>
        /// Method for sending a request with cache and retries on failed requests.
        /// Uses exponential backoff until max retries is reached.
        /// Uses cached request method.
        /// </summary>
        /// <param name="url"> Destination url </param>
        /// <returns> String with content of the response or cache, failed requests return empty strings. </returns>
        public async Task<string> SendResilientRequestAsync(string url)
        {
            ExponentialBackoff backoff = new ExponentialBackoff(_maxRetries, _delayMilliseconds, _maxDelayMilliseconds);

            retry:
            try
            {
                return await SendRequestAsync(url);
            }
            catch (HttpResponseException e) when (!backoff.MaxReached)
            {
                // Dont retry on non-existent result
                if (e.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return "";
                }

                Debug.WriteLine("HttpResponseException StatusCode: " + e.Response.StatusCode);
                await backoff.Delay();
                goto retry;
            }
            catch(Exception e)
            {
                // Max tries reached or non-httpresponseexception
                return "";
            }
        }
    }

    /// <summary>
    /// Helper class for creating exponential backoff delays.
    /// </summary>
    public struct ExponentialBackoff
    {
        private readonly int m_maxRetries, m_delayMilliseconds, m_maxDelayMilliseconds;
        private int m_retries, m_pow;

        public bool MaxReached
        {
            get { return m_retries >= m_maxRetries;  }
        }

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
