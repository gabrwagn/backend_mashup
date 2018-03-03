// Author: gabrwagn

using MusicGenie.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Hqub.MusicBrainz.API;


namespace MusicGenie.Controllers
{
    public class ArtistController : ApiController
    {
        /// <summary>
        /// Method that is triggered upon a GET request from /api/artist/MBID
        /// </summary>
        /// <param name="id"> MBID of requested artist. </param>
        /// <returns> JSON containing artist information. </returns>
        public async Task<IHttpActionResult> GetArtistInfo(string id)
        {
            ArtistInfo info =  await BuildArtistInfo(id);

            // Failed to find artist
            if (info == null)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            return Ok(info);
        }

        /// <summary>
        /// Method that constructs a Artist Info container.
        /// </summary>
        /// <param name="mbid"> MBID of requested artist. </param>
        /// <returns> ArtistInfo container for the requested artist. </returns>
        private async Task<ArtistInfo> BuildArtistInfo(string mbid)
        {
            // Attempt to get artist information from MusicBrainz
            JObject musicBrainzJson = null;
            try
            {
                musicBrainzJson = await GetMusicBrainzJson(mbid);

                // Failed to parse request to JSON
                if (musicBrainzJson == null)
                    return null;
            }
            catch (HttpClientException e)
            {
                // Bad request of some kind
                return null;
            }


            // Extract wikipedia lookup name
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

            // Parse albums from JSON
            List<AlbumInfo> albumsInfo = new List<AlbumInfo>();
            foreach (JToken token in musicBrainzJson.SelectToken("release-groups"))
            {
                // Sleep 1 second to avoid exceeding 1 request per second.
                Thread.Sleep(1000);

                // Construct album info
                string albumId = TrySelectToken(token, "id");
                AlbumInfo albumInfo = new AlbumInfo()
                {
                    Id = albumId,
                    Title = TrySelectToken(token, "title"),
                    Image = await GetCoverArtLink(albumId),
                };
                albumsInfo.Add(albumInfo);
            }

            // Construct artist info
            ArtistInfo artistInfo = new ArtistInfo()
            {
                Name = TrySelectToken(musicBrainzJson, "name"),
                Mbid = mbid,
                Albums = albumsInfo,
                Description = await GetWikipediaInfo(wikipedia_name),
            };

            return artistInfo;
        }

        /// <summary>
        /// Method for requesting artist information from MusicBrainz.
        /// </summary>
        /// <param name="mbid"> MBID of requested artist. </param>
        /// <returns> JObject if request is successful, else null. </returns>
        private async Task<JObject> GetMusicBrainzJson(string mbid)
        {   
            string musicBrainzRequestUrl = String.Format("http://musicbrainz.org/ws/2/artist/{0}?&fmt=json&inc=url-rels+release-groups", mbid);

            JObject json = await CreateGetRequest(musicBrainzRequestUrl);
            if (json != null)
                return json;
            else
            {
                // Request failed
                Debug.WriteLine("Failed to get MB artist information.");
                return null;
            }
            
        }

        /// <summary>
        /// Method for requesting cover art information from MusicBrainz.
        /// </summary>
        /// <param name="id"> MBID of album to be requested. </param>
        /// <returns> string with image link if successfull request, else empty string. </returns>
        private async Task<string> GetCoverArtLink(string id)
        {
            string coverArtRequestUrl = "http://coverartarchive.org/release-group/" + id;

            JObject coverArtJson = await CreateGetRequest(coverArtRequestUrl);
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

            JObject json = await CreateGetRequest(uri);
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
        /// Method for generating a generic request that return a JSON formatted response.
        /// </summary>
        /// <param name="url"> URL of the request. </param>
        /// <returns> JObject response if successfull, else null. </returns>
        private async Task<JObject> CreateGetRequest(string url)
        {
            MyHttpClient client = new MyHttpClient(url);
            try
            {
                HttpResponseMessage respose = await client.SendRequestAsync();
                string content = await respose.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(content);

                return json;
            }
            catch (HttpClientException e)
            {
                // Request failed
                return null;
            }
        }

        /// <summary>
        /// Methods for trying to get a token from a JObject/JToken and casts it to a string.
        /// </summary>
        /// <param name="json"> JSON object to attempt selection from. </param>
        /// <param name="path"> key/index path to the selected token. </param>
        /// <returns> string containing content of selected token if present, else empty string. </returns>
        private string TrySelectToken(JObject json, string path)
        {
            return TrySelectToken((JToken)json, path);
        }
        private string TrySelectToken(JToken json, string path)
        {
            JToken token = json.SelectToken(path);
            if (token != null)
                return token.ToObject<string>();

            return "";
        }

    }
}
