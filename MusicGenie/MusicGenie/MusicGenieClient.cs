using System;
using System.Diagnostics;
using System.Net;
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
            :this(cache, 8, 50, 5000)
        {
        }

        public MusicGenieClient(ICacheService cache, int maxRetries, int delayMilliseconds, int maxDelayMilliseconds)
        {
            _cache = cache;
            _httpClient = new HttpClient();

            _maxRetries = maxRetries;
            _delayMilliseconds = delayMilliseconds;
            _maxDelayMilliseconds = maxDelayMilliseconds;
        }


        /// <summary>
        /// Method for sending a request.
        /// </summary>
        /// <param name="url"> Request destination </param>
        /// <returns> String with content of the response, failed requests throw HttpRequestException </returns>
        public async Task<string> SendRequestAsync(string url)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            request.Headers.Add("User-Agent", UserAgent);

            HttpResponseMessage response;

            response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpResponseException(response);
            }

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Method for checking cached responses to requests, otherwise sending a new request.
        /// </summary>
        /// <param name="url"> Request destination </param>
        /// <returns> String with content of the response or cache, failed requests throw HttpRequestException. </returns>
        public async Task<string> SendRequestAsyncCached(string url)
        {
            return await _cache.GetOrSet(url, () => SendRequestAsync(url));
        }

        /// <summary>
        /// Method for sending a request with cache and retries on failed requests.
        /// Uses exponential backoff until max retries is reached.
        /// Uses cached request method.
        /// </summary>
        /// <param name="url"> Destination url </param>
        /// <returns> String with content of the response or cache, failed requests return empty strings. </returns>
        public async Task<string> SendResilientRequestAsyncCached(string url)
        {
            ExponentialBackoff backoff = new ExponentialBackoff(_maxRetries, _delayMilliseconds, _maxDelayMilliseconds);

            retry:
            try
            {
                return await SendRequestAsyncCached(url);
            }
            catch (HttpResponseException e) when (!backoff.MaxReached)
            {
                if (e.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    return "";
                }

                await Task.Delay(backoff.GetNextDelayLength());
                goto retry;
            }
            catch (Exception e)
            {
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
            if (maxRetries <= 0 ||
                delayMilliseconds <= 0 ||
                maxDelayMilliseconds <= 0)
            {
                throw new ArgumentException("All parameters must be positive and non-zero.");
            }

            m_maxRetries = maxRetries;
            m_delayMilliseconds = delayMilliseconds;
            m_maxDelayMilliseconds = maxDelayMilliseconds;
            m_retries = 0;
            m_pow = 1;
        }

        /// <summary>
        /// Increments and returns exponential backoff delay length.
        /// </summary>
        /// <returns></returns>
        public int GetNextDelayLength()
        {
            ++m_retries;
            if (m_retries < 31)
            {
                m_pow = m_pow << 1; // m_pow = Pow(2, m_retries - 1)
            }
            int delay = Math.Min(m_delayMilliseconds * (m_pow - 1), m_maxDelayMilliseconds);
            return delay;
        }
    }

}
