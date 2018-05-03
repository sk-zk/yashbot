using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml;

namespace yashbot
{
    class YoutubeApi
    {
        /// <summary>
        /// Gets the duration of a YouTube video from the YouTube API.
        /// </summary>
        /// <param name="videoId"></param>
        /// <param name="authInfo"></param>
        /// <returns></returns>
        public static double GetVideoDuration(string videoId, AuthInfo authInfo)
        {
            // we're just gonna use Dylan's wrapper for it, just like as2
            var client = new WebClient();
            var response = client.UploadValues("http://www.audiosurf2.com/as/as2_youtube2.php",
               new NameValueCollection() {
                       { "username", authInfo.Username },
                       { "steamid", authInfo.SteamId },
                       { "session", authInfo.Session },
                       { "steamticket", authInfo.SteamTicket },
                       { "steamfriends", "0" },
                       { "videoids", videoId }
               }
            );
            var responseStr = Encoding.UTF8.GetString(response);
            return GetDurationFromJson(responseStr);
        }

        /// <summary>
        /// Extracts the duration of a video from YouTube's JSON response.
        /// </summary>
        /// <param name="responseStr"></param>
        /// <returns></returns>
        private static double GetDurationFromJson(string responseStr)
        {
            var iso8601Duration = Regex.Match(responseStr, "\"duration\": \"(.+?)\"").Groups[1].Value;

            return XmlConvert.ToTimeSpan(iso8601Duration).TotalSeconds;
        }
    }
}
