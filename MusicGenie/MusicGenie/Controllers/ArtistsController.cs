using MusicGenie.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Hqub.MusicBrainz.API;
using Newtonsoft.Json;

namespace MusicGenie.Controllers
{
    public class ArtistController : ApiController
    {
        private MyHttpClient _client;


        public ArtistController()
        {
            int chacheTimeMinutes = 10;
            _client = new MyHttpClient(new InMemoryCache(chacheTimeMinutes));
        }


        /// <summary>
        /// Method that is called upon a GET request.
        /// </summary>
        /// <param name="id"> MBID of requested artist. </param>
        /// <returns> JSON containing artist information. </returns>
        [HttpGet]
        [Route("genie/artist/{id}")]
        public async Task<IHttpActionResult> GetArtistInfo(string id)
        {
            ArtistInfo info =  await BuildArtistInfo(id);

            // Failed to find artist
            if (info == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            return Ok(info);
        }

        /// <summary>
        /// Method that constructs a Artist Info instance.
        /// Uses MusicBrainz information for albums and cover art, and Wikipedia for summary text.
        /// </summary>
        /// <param name="artistMbid"> MBID of requested artist. </param>
        /// <returns> ArtistInfo container for the requested artist. </returns>
        private async Task<ArtistInfo> BuildArtistInfo(string artistMbid)
        {
            // Attempt to get artist information from MusicBrainz
            // Await request immediately as it is needed for subsequent requests
            JToken musicBrainzJson = null;

            musicBrainzJson = await GetMusicBrainzJson(artistMbid);

            // Failed JSON parse
            if (musicBrainzJson == null)
                return null;

            // Extract wikipedia lookup name and send request
            string wikipedia_name = "";
            foreach (JToken token in musicBrainzJson.SelectToken("relations"))
            {
                string type = TrySelectToken(token, "type");
                if (type == "wikipedia")
                {
                    string wiki_url = TrySelectToken(token, "url.resource");
                    wikipedia_name = wiki_url.Split('/').Last();
                }
            }
            Task<string> wikiDescriptionTask = GetWikipediaInfo(wikipedia_name);

            // Create cover art requests for each album in MusicBrainz json
            // Keep track of which request belongs to which album id
            Dictionary<AlbumInfo, Task<string>> albumCoverTasks = new Dictionary<AlbumInfo, Task<string>>();
            foreach (JToken token in musicBrainzJson.SelectToken("release-groups"))
            {
                string albumId = TrySelectToken(token, "id");
                AlbumInfo albumInfo = new AlbumInfo()
                {
                    Id = albumId,
                    Title = TrySelectToken(token, "title"),
                };

                albumCoverTasks[albumInfo] = GetCoverArtLink(albumId);
            }

            // Await each Cover Art Link and finalize each album info
            List<AlbumInfo> albumsInfo = new List<AlbumInfo>();
            foreach(KeyValuePair<AlbumInfo, Task<string>> entry in albumCoverTasks)
            {
                AlbumInfo albumInfo = entry.Key;
                albumInfo.Image = await entry.Value;
                albumsInfo.Add(albumInfo);
            }

            // Construct artist info (make sure wikipedia request replied)
            ArtistInfo artistInfo = new ArtistInfo()
            {
                Name = TrySelectToken(musicBrainzJson, "name"),
                Mbid = artistMbid,
                Albums = albumsInfo,
                Description = await wikiDescriptionTask,
            };

            return artistInfo;
        }


        /// <summary>
        /// Method for generating a generic request that return a JSON formatted response.
        /// </summary>
        /// <param name="url"> URL of the request. </param>
        /// <returns> JObject response if successfull, else null. </returns>
        private async Task<JToken> CreateGetRequest(string url)
        {
            string content = "";
            try
            {
                content = await _client.RetryWithExponentialBackoff(url);

                if ((content.StartsWith("{") && content.EndsWith("}")) ||
                    (content.StartsWith("[") && content.EndsWith("]")))
                {
                    return JToken.Parse(content);
                }
                else
                {
                    // Non-valid json format in response
                    return null;
                }
            }
            catch (TimeoutException e)
            {
                // Max retries exceeded
                return null;
            }
            catch (JsonReaderException e)
            {
                // Non-valid json format in response
                return null;
            }
        }

        /// <summary>
        /// Method for requesting artist information from MusicBrainz.
        /// </summary>
        /// <param name="artistMbid"> MBID of requested artist. </param>
        /// <returns> JObject if request is successful, else null. </returns>
        private async Task<JToken> GetMusicBrainzJson(string artistMbid)
        {   
            string musicBrainzRequestUrl = String.Format("http://musicbrainz.org/ws/2/artist/{0}?&fmt=json&inc=url-rels+release-groups", artistMbid);

            JToken json = await CreateGetRequest(musicBrainzRequestUrl);
            if (json != null)
                return json;
            else
            {
                // Request failed
                Debug.WriteLine("Failed to get MusicBrainz artist information.");
                return null;
            }
            
        }

        /// <summary>
        /// Method for requesting cover art information from MusicBrainz.
        /// </summary>
        /// <param name="albumMbid"> MBID of album to be requested. </param>
        /// <returns> string with image link if successfull request, else empty string. </returns>
        private async Task<string> GetCoverArtLink(string albumMbid)
        {
            string coverArtRequestUrl = "http://coverartarchive.org/release-group/" + albumMbid;

            // Wait before making request (MusicBrainz max 50 req per sec for anon user agents)
            JToken coverArtJson = await CreateGetRequest(coverArtRequestUrl);
            if (coverArtJson != null)
                return TrySelectToken(coverArtJson, "images[0].image");
            else
            {
                // Request failed
                Debug.WriteLine("Failed to get cover art link.");
                return ""; 
            }
        }

        /// <summary>
        /// Method for requesting artist summary from Wikipedia.
        /// </summary>
        /// <param name="artist"> artist/band string as specified in wikipedia url. </param>
        /// <returns> string containing html summary if successfull, else empty string. </returns>
        private async Task<string> GetWikipediaInfo(string artist)
        {
            string uri = "https://en.wikipedia.org/w/api.php?action=query&format=json&prop=extracts&exintro=true&redirects=true&titles={0}";
            uri = String.Format(uri, artist);

            JToken json = await CreateGetRequest(uri);
            if (json != null)
            {
                return TrySelectToken(json, "query.pages.*.extract");
            }
            else
            {
                // Request failed
                Debug.WriteLine("Failed to get wiki artist information");
                return ""; 
            }
           
        }
       

        /// <summary>
        /// Methods for trying to get a token from a JObject/JToken and casts it to a string.
        /// </summary>
        /// <param name="json"> JSON object to attempt selection from. </param>
        /// <param name="path"> key/index path to the selected token. </param>
        /// <returns> string containing content of selected token if present, else empty string. </returns>
        private string TrySelectToken(JToken json, string path)
        {
            JToken token = json.SelectToken(path);
            if (token != null)
                return token.Value<string>();

            return "";
        }

    }
}
