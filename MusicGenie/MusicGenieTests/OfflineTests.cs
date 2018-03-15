using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicGenie;
using MusicGenie.Controllers;
using Newtonsoft.Json.Linq;

namespace MusicGenieTests
{
    [TestClass]
    public class OfflineTests
    {
        [TestMethod]
        public void GetTokenContent_ShouldReturnNonEmptyString()
        {
            string expectedContent = "abc";
            string key = "key";
            int index = 1;
            string path = String.Format("{0}[{1}]", key, index);

            JObject json = JObject.Parse("{\"key\":[\"123\",\"abc\"]}");

            string result = JsonHelper.TrySelectToken(json, path);

            Assert.AreEqual(expectedContent, result);
        }

        [TestMethod]
        public void GetTokenContentIncorrectPath_ShouldReturnTokenEmpyString()
        {
            int index = 0;
            string path = String.Format("keyx[{0}]", index);

            string expectedResult = "";

            JObject json = JObject.Parse("{\"key\":[\"123\",\"abc\"]}");

            string result = JsonHelper.TrySelectToken(json, path);


            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void TestExponentialBackoffDelays()
        {

            int maxRetries = 5;
            int delayMilliseconds = 100;
            int maxDelayMilliseconds = 1000;

            var backoff = new ExponentialBackoff(maxRetries, delayMilliseconds, maxDelayMilliseconds);

            int[] expectedBackoffDelays = { 100, 300, 700, 1000, 1000 };

            int[] resultingBackoffDelays = new int[maxRetries];
            for (int i = 0; i < maxRetries; i++)
            {
                resultingBackoffDelays[i] = backoff.GetNextDelayLength();
            }

            Assert.IsTrue(expectedBackoffDelays.SequenceEqual(resultingBackoffDelays));
        }

        [TestMethod]
        public void TestExponentialBackoffMaxRetries()
        {

            int maxRetries = 5;
            int delayMilliseconds = 1;
            int maxDelayMilliseconds = 1;

            var backoff = new ExponentialBackoff(maxRetries, delayMilliseconds, maxDelayMilliseconds);

            int expectedDelaysCount = maxRetries;

            int resultingDelayCount = 0;
            while (!backoff.MaxReached)
            {
                resultingDelayCount++;
                backoff.GetNextDelayLength();
            }

            Assert.AreEqual(expectedDelaysCount, resultingDelayCount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Invalid arguments were accepted in backoff constructor.")]
        public void TestInvalidDelayTimes()
        {
            int maxRetries = 0;
            int delayMilliseconds = 100;
            int maxDelayMilliseconds = -1000;

            var backoff = new ExponentialBackoff(maxRetries, delayMilliseconds, maxDelayMilliseconds);
        }

        [TestMethod]
        public async Task GetRequestFromFromCache_ShouldReturnNonEmptyString()
        {
            string uri = "https://fake.url.irrelevant.org/shouldstillwork";
            string expectedSummary = await GetGenericSummary();
        
            var memory = new InMemoryCache(1, new MemoryCache("unit-test-cache"));
            await memory.GetOrSet(uri, () => GetGenericSummary());
        
            var client = new MusicGenieClient(memory);
            var result = await client.SendRequestAsyncCached(uri);
        
            Assert.AreEqual(expectedSummary, result);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException), "Invalid url turned into a valid request.")]
        public async Task TestFailedRequest_ShouldThrowHttpRequestException()
        {
            var memory = new InMemoryCache(1, new MemoryCache("unit-test-cache3"));
            MusicGenieClient client = new MusicGenieClient(memory);
            var result = "";
            result = await client.SendRequestAsync("https://thisurlisgoingtofail.org/fortestingpurposes");
        }

        private async Task<String> GetGenericSummary()
        {
            return "This is a short summary";
        }
    }
}
