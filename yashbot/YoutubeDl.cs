using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yashbot
{
    /// <summary>
    /// youtube-dl handler
    /// </summary>
    class YoutubeDl
    {
        readonly static string OUTPUT_FOLDER = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "yashbot");

        /// <summary>
        /// Downloads an audio track from a YouTube video as m4a.
        /// </summary>
        /// <param name="videoId">The ID of the video.</param>
        /// <returns>The path to the downloaded file.</returns>
        public static string CallYoutubeDl(string videoId)
        {
            string tempfile = Path.Combine(OUTPUT_FOLDER, videoId + ".m4a");
            ProcessStartInfo info = new ProcessStartInfo(
                 "youtube-dl.exe", "--ignore-config -f bestaudio -x --audio-format m4a --add-metadata -o " +
                 OUTPUT_FOLDER + "/%(id)s.%(ext)s " + videoId
                );
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            Process process = new Process();
            process.StartInfo = info;
            process.Start();
            process.WaitForExit();
            process.Dispose();
            return tempfile;
        }

    }
}
