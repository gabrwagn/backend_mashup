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
    public class OnlineTests
    {
        [TestMethod]
        public async Task TestWikipediaRequest_ShouldReturnValidJsonString()
        {
            // Proper test? Fails if content changes
            string uri = "https://en.wikipedia.org/w/api.php?action=query&format=json&prop=extracts&exintro=true&redirects=true&titles=pink_floyd";
            string expectedResponse = GetPinkFloydSummary;

            var memory = new InMemoryCache(1, new MemoryCache("unit-test-cache2"));
            var controller = new ArtistController();
            var result = await controller.MakeRequest(uri);

            Assert.AreEqual(expectedResponse, result);

        }

        [TestMethod]
        public async Task TestInvalidWikipediaRequest_ShouldEmptyString()
        {
            // Proper test? Fails if content changes
            string uri = "https://en.wikipedia.org/w/api.php?action=query&format=json&prop=extracts&exintro=true&redirects=true&titles=";
            string expectedResponse = "{\"batchcomplete\":\"\"}";

            var memory = new InMemoryCache(1, new MemoryCache("unit-test-cache2"));
            var controller = new ArtistController();
            var result = await controller.MakeRequest(uri);

            Assert.AreEqual(expectedResponse, result);

        }

        [TestMethod]
        public async Task TestMusicBrainzRequest_ShouldReturnValidJsonString()
        {
            // Proper test? Fails if content changes
            string uri = "http://musicbrainz.org/ws/2/artist/53b106e7-0cc6-42cc-ac95-ed8d30a3a98e?&fmt=json&inc=url-rels+release-groups";
            int expectedResponseLength = 14382;

            var memory = new InMemoryCache(1, new MemoryCache("unit-test-cache2"));
            var controller = new ArtistController();
            var result = await controller.MakeRequest(uri);

            Assert.AreEqual(expectedResponseLength, result.Length);

        }

        [TestMethod]
        public async Task TestInvalidMusicBrainzRequest_ShouldReturnValidJsonString()
        {
            // Proper test? Fails if content changes
            string uri = "http://musicbrainz.org/ws/2/artist/X53b106e7-0cc6-42cc-ac95-ed8d30a3a98e?&fmt=json&inc=url-rels+release-groups";
            string expectedResponse = "";

            var memory = new InMemoryCache(1, new MemoryCache("unit-test-cache2"));
            var controller = new ArtistController(new MusicGenieClient(memory, 1, 1, 1));
            var result = await controller.MakeRequest(uri);

            Assert.AreEqual(expectedResponse, result);

        }

        private string GetPinkFloydSummary =>
            @"{""batchcomplete"":"""",""query"":{""normalized"":[{""from"":""pink_floyd"",""to"":""Pink floyd""}],""redirects"":[{""from"":""Pink floyd"",""to"":""Pink Floyd""}],""pages"":{""5079506"":{""pageid"":5079506,""ns"":0,""title"":""Pink Floyd"",""extract"":""<p><b>Pink Floyd</b> were an English rock band formed in London in 1965. They achieved international acclaim with their progressive and psychedelic music. Distinguished by their use of philosophical lyrics, sonic experimentation, extended compositions, and elaborate live shows, they are one of the most commercially successful and influential groups in popular music history.</p>\n<p>Pink Floyd were founded by students Syd Barrett on guitar and lead vocals, Nick Mason on drums, Roger Waters on bass and vocals, and Richard Wright on keyboards and vocals. They gained popularity performing in London's underground music scene during the late 1960s, and under Barrett's leadership released two charting singles and a successful debut album, <i>The Piper at the Gates of Dawn</i> (1967). Guitarist and vocalist David Gilmour joined in December 1967; Barrett left in April 1968 due to deteriorating mental health. Waters became the band's primary lyricist and conceptual leader, devising the concepts behind their albums <i>The Dark Side of the Moon</i> (1973), <i>Wish You Were Here</i> (1975), <i>Animals</i> (1977), <i>The Wall</i> (1979) and <i>The Final Cut</i> (1983). <i>The Dark Side of the Moon</i> and <i>The Wall</i> became two of the best-selling albums of all time.</p>\n<p>Following creative tensions, Wright left Pink Floyd in 1979, followed by Waters in 1985. Gilmour and Mason continued as Pink Floyd; Wright rejoined them as a session musician and, later, band member. The three produced two more albums\u2014<i>A Momentary Lapse of Reason</i> (1987) and <i>The Division Bell</i> (1994)\u2014and toured through 1994. After nearly two decades of enmity, Gilmour, Wright, and Mason reunited with Waters in 2005 to perform as Pink Floyd in London as part of the global awareness event Live 8; Gilmour and Waters later stated they had no further plans to reunite the band. Barrett died in 2006, and Wright in 2008. The final Pink Floyd studio album, <i>The Endless River</i> (2014), was recorded without Waters and based almost entirely on unreleased material.</p>\n<p>Pink Floyd were inducted into the American Rock and Roll Hall of Fame in 1996 and the UK Music Hall of Fame in 2005. By 2013, the band had sold more than 250 million records worldwide.</p>\n<p></p>""}}}}";
    }
}
