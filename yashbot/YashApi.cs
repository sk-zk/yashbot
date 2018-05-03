using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace yashbot
{

    /// <summary>
    /// For downloading & uploading Youtube ASHes.
    /// </summary>
    class YashApi
    {
        const string DOWNLOAD_URL = "http://audiosurf2.com/shield/download_yash.php";
        const string UPLOAD_URL = "http://audiosurf2.com/shield/upload_yashW.php";

        /// <summary>
        /// Fetches the ash sums of a Youtube video from the AS2 server. Returns null if none exists.
        /// </summary>
        /// <param name="videoId">The ID of the video.</param>
        /// <returns>The ash sums of this video.</returns>
        public static Yash GetYash(string videoId)
        {
            using (WebClient client = new WebClient())
            {
                byte[] response = DownloadYash(videoId);

                if (response.Length == 0)
                {
                    return null;
                }

                // first word: just 1 as int

                // second word: song duration
                float duration = BitConverter.ToSingle(response, 4); // in seconds

                // third word: amount of sums
                int sumAmount = BitConverter.ToInt32(response, 8);

                // after that: sums
                List<float> sums = new List<float>();
                for (int i = 32 * 3; i < response.Length; i += 4)
                {
                    sums.Add(BitConverter.ToSingle(response, i));
                }

                return new Yash(sums, duration);
            }
        }

        private static byte[] DownloadYash(string videoId)
        {
            using (WebClient client = new WebClient())
            {
                byte[] response = client.UploadValues(DOWNLOAD_URL,
                    new NameValueCollection() { { "id", videoId } }
                );
                return response;
            }
        }

        /// <summary>
        /// Checks if a video has been processed yet,
        /// </summary>
        /// <param name="videoId">The ID of the video.</param>
        /// <returns>Whether or not this video has been processed.</returns>
        public static bool YashExists(string videoId)
        {
            byte[] response = DownloadYash(videoId);
            return response.Length != 0;
        }

        /// <summary>
        /// Uploads ash sums of a video to the AS2 server.
        /// </summary>
        /// <param name="videoId">The ID of the video.</param>
        /// <param name="sums">The ash sums of the video.</param>
        /// <param name="duration">The duration of the video in seconds.</param>
        public static async Task UploadYash(string videoId, List<float> sums, float duration, AuthInfo authInfo)
        {
            const int HEADER_SIZE = 3 * 4;
            var sumBytes = new byte[HEADER_SIZE + (sums.Count() * 4)];
            Buffer.BlockCopy(BitConverter.GetBytes(1), 0, sumBytes, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(duration), 0, sumBytes, 1 * 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(sums.Count), 0, sumBytes, 2 * 4, 4);
            Buffer.BlockCopy(sums.ToArray(), 0, sumBytes, 3 * 4, sums.Count * 4);

            HttpClient httpClient = new HttpClient();
            MultipartFormDataContent form = new MultipartFormDataContent();

            form.Add(new StringContent(videoId), "id");
            StreamContent sc = new StreamContent(new MemoryStream(sumBytes));
            sc.Headers.Add("Content-Type", "application/octet-stream");
            sc.Headers.Add("Content-Disposition", "form-data; name=\"yash\"; filename=\"yash.dat\"");
            form.Add(sc);
            form.Add(new StringContent(authInfo.Username), "username");
            form.Add(new StringContent(authInfo.SteamId), "steamid");
            form.Add(new StringContent(authInfo.Session), "session");
            form.Add(new StringContent(authInfo.SteamTicket), "steamticket");
            HttpResponseMessage response = await httpClient.PostAsync(UPLOAD_URL, form);

            response.EnsureSuccessStatusCode();
            byte[] bodyBytes = await response.Content.ReadAsByteArrayAsync();
            string body = Encoding.UTF8.GetString(bodyBytes); // workaround for github.com/dotnet/corefx/issues/5014
            if (!body.Contains("</emptyisgood>"))
            {
                throw new WebException(body);
            }
            httpClient.Dispose();

        }


    }
}
