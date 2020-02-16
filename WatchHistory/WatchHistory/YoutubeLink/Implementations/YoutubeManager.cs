namespace DoenaSoft.WatchHistory.YoutubeLink.Implementations
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Web;
    using MediaInfoHelper.Youtube;
    using Newtonsoft.Json;

    internal class YoutubeManager : IYoutubeManager
    {
        private static readonly Regex _youtubeIdRegex;

        private static readonly Regex _runningTimeRegex;

        private static readonly HttpClient _httpClient;

        static YoutubeManager()
        {
            const string YoutubeId = "[^\"&?\\/ ]{11}";

            _youtubeIdRegex = new Regex($"v=(?'YoutubeId'{YoutubeId})", RegexOptions.Compiled);

            const string Number = "[0-9]{1,2}";

            _runningTimeRegex = new Regex($"PT((?'Hours'{Number})H)?((?'Minutes'{Number})M)?((?'Seconds'{Number})S)?", RegexOptions.Compiled);

            _httpClient = new HttpClient();
        }

        public YoutubeVideoInfo GetInfo(string youtubeUrl)
        {
            var id = GetYoutubeId(youtubeUrl);

            string url = GetUrl(id);

            var responseItem = GetResponse(url);

            var info = new YoutubeVideoInfo()
            {
                Id = id,
                Title = responseItem?.snippet?.title ?? id,
                RunningTime = GetRunningTime(responseItem?.contentDetails?.duration),
                Published = responseItem?.snippet?.publishedAt ?? DateTime.UtcNow,
            };

            return info;
        }

        private static string GetYoutubeId(string youtubeUrl)
        {
            if (string.IsNullOrEmpty(youtubeUrl) || !youtubeUrl.Contains(".youtube."))
            {
                throw new YoutubeUrlException("Youtube URL is invalid");
            }

            var match = _youtubeIdRegex.Match(youtubeUrl);

            if (!match.Success)
            {
                throw new YoutubeUrlException("Youtube ID is invalid");
            }

            var id = match.Groups["YoutubeId"].Value;

            return id;
        }

        private static string GetUrl(string id)
        {
            var builder = new UriBuilder("https://www.googleapis.com/youtube/v3/videos");

            var query = HttpUtility.ParseQueryString(builder.Query);

            query["part"] = "snippet,contentDetails";
            query["id"] = id;
            query["key"] = YoutubeApiConstants.ApiKey;

            builder.Query = query.ToString();

            var url = builder.ToString();

            return url;
        }

        private static YoutubeApi.Item GetResponse(string url)
        {
            var responseMessage = _httpClient.GetAsync(url).Result;

            var jason = responseMessage.Content.ReadAsStringAsync().Result;

            var response = JsonConvert.DeserializeObject<YoutubeApi.RootObject>(jason);

            var responseItem = response?.items.FirstOrDefault();

            return responseItem;
        }

        private uint GetRunningTime(string duration)
        {
            if (string.IsNullOrEmpty(duration))
            {
                return 0;
            }

            var match = _runningTimeRegex.Match(duration);

            if (!match.Success)
            {
                return 0;
            }

            var hours = TryGetNumber(match.Groups["Hours"]);

            var minutes = TryGetNumber(match.Groups["Minutes"]);

            var seconds = TryGetNumber(match.Groups["Seconds"]);

            var runningTime = (uint)(new TimeSpan(hours, minutes, seconds)).TotalSeconds;

            return runningTime;
        }

        private int TryGetNumber(Group group)
        {
            var value = group?.Value;

            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            var number = int.Parse(value);

            return number;
        }
    }
}