using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.IO;
using Steamworks;

namespace yashbot
{
    class Program
    {
        static SongLoader songLoader = new SongLoader();
        static AuthInfo authInfo;

        static void Main(string[] args)
        {           
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide at least one video ID or a text file containing video IDs.");
                Environment.Exit(1);
            }

            authInfo = Login();
            Console.WriteLine();

            var isProtocolHandler = false;
            string[] videoIds;

            // protocol handler
            if (args[0].StartsWith("yashbot://"))
            {
                isProtocolHandler = true;
                var videoIdStr = args[0].Substring(10).Replace("/", "");
                videoIds = videoIdStr.Split(',');
            }
            else
            {
                videoIds = args;
            }

            foreach (string arg in videoIds)
            {
                try
                {
                    if (IsYoutubeId(arg))
                    {
                        ProcessVideo(arg).Wait();
                    }
                    else if (!isProtocolHandler && File.Exists(arg))
                    {
                        string[] ids = File.ReadAllText(arg).Split();
                        ProcessVideos(ids);
                    }
                    else
                    {
                        Console.Error.WriteLine("Don't know what to do with \"{0}\"\n", arg);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Something went wrong:");
                    Console.Error.WriteLine(ex.ToString());
                }
            }

            if(isProtocolHandler)
            {
                Console.WriteLine("Press any key to exit ...");
                Console.ReadKey();
            }

            #if DEBUG
                Console.ReadLine();
            #endif
        }

        static void ProcessVideos(string[] videoIds)
        {
            foreach (string videoId in videoIds)
            {
                CheckAndProcessVideo(videoId);
            }
        }

        static void CheckAndProcessVideo(string videoId)
        {
            if (videoId == "")
            {
                return;
            }
            if (IsYoutubeId(videoId))
            {
                ProcessVideo(videoId).Wait();
            }
            else
            {
                Console.Error.WriteLine("Don't know what to do with \"{0}\"\n", videoId);
            }
        }

        static bool IsYoutubeId(string input)
        {
            if (input.Length != 11)
            {
                return false;
            }

            if (input.Contains("/") || input.Contains("\\"))
            {
                return false;
            }

            if (File.Exists(input))
            {
                return false;
            }

            return true;
        }

        static AuthInfo Login()
        {
            // Unfortunately, youtube ASHes can only be submitted with a valid steam ticket
            // and a session ID from the AS2 server.

            // 1) Login with Steamworks
            Console.WriteLine("Getting Steam session");
            if (!SteamAPI.Init())
            {
                Console.Error.WriteLine("SteamAPI.Init() failed");
                Environment.Exit(1);
            }

            byte[] ticket = new byte[1024];
            uint ticketLength = 0;
            HAuthTicket authTicket = SteamUser.GetAuthSessionTicket(ticket, ticket.Length, out ticketLength);
            if (authTicket == HAuthTicket.Invalid)
            {
                Console.Error.WriteLine("Got invalid ticket");
                Environment.Exit(1);
            }
            string steamTicket = BitConverter.ToString(ticket, 0, (int)ticketLength).Replace("-", "").ToLowerInvariant();

            // 2) Get a session ID from the AS2 server
            string result = "";
            using (WebClient client = new WebClient())
            {
                try
                {
                    byte[] response = client.UploadValues("http://audiosurf2.com/as/airgame_steamAuthenticate4.php",
                        new NameValueCollection()
                        {
                        {"username", "yashbot"},
                        {"session", ""},
                        {"steamid", SteamUser.GetSteamID().ToString()},
                        {"steamticket", steamTicket},
                        {"steamfriends", ""},

                        });

                    result = Encoding.UTF8.GetString(response);
                }
                catch (WebException wex)
                {
                    Console.Error.WriteLine("Couldn't get AS2 session ID:");
                    Console.Error.WriteLine(wex.ToString());
                    Environment.Exit(1);
                }
            }
            string session = Regex.Match(result, "token='(.+?)'").Groups[1].Value; // cthulhu > xml parsers
            return new AuthInfo(SteamUser.GetSteamID().ToString(), steamTicket, session);
        }

        static async Task ProcessVideo(string videoId)
        {
            Console.WriteLine("Looking up " + videoId);
            var yash = YashApi.GetYash(videoId);
            if (yash == null || yash.Sums.Count == 0)
            {
                await CreateAndUploadYash(videoId);
            }
            else
            {
                Console.WriteLine("{0} has already been processed.\nChecking duration", videoId);
                // The game ignores existing yashes and forces a resync if the length doesn't match
                // so we'll deal with that, too.
                // Hopefully this won't accidentally overwrite working yashes
                var ytDuration = YoutubeApi.GetVideoDuration(videoId, authInfo);
                if (Math.Abs(yash.Duration - ytDuration) < 1.05f)
                {
                    Console.WriteLine("Duration is correct");
                }
                else
                {
                    Console.WriteLine("Existing yash seems corrupt; overwriting");
                    await CreateAndUploadYash(videoId);
                }
            }

            Console.WriteLine("Done\n");
        }

        static async Task CreateAndUploadYash(string videoId)
        {
            Console.WriteLine("Processing {0} now", videoId);
            Console.WriteLine("Downloading audio");
            string ytAudioFile;
            try
            {
                ytAudioFile = YoutubeDl.CallYoutubeDl(videoId);
            }
            catch (YoutubeDlException yex)
            {
                Console.Error.WriteLine("youtube-dl encountered an error:");
                Console.Error.WriteLine(yex.Message);
                return;
            }
            Console.WriteLine("Analyzing song");
            TagLib.File file = TagLib.File.Create(ytAudioFile);
            float duration = (float)file.Properties.Duration.TotalSeconds;
            file.Dispose();
            List<float> sums = songLoader.DecodeSongSums(ytAudioFile, duration);
            Console.WriteLine("Uploading yash");
            try
            {
                await YashApi.UploadYash(videoId, sums, duration, authInfo);
                Console.WriteLine("Upload successful");
            }
            catch (WebException wex)
            {
                Console.Error.WriteLine("Something went wrong, here's the response:");
                Console.Error.WriteLine(wex.ToString());
            }
        }

    }
}
