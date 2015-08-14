using System;

namespace Helix.Utility
{
    using System.IO;

    public sealed class FileOps
    {
        /// <summary>
        /// Loads File; returns string of contents 
        /// </summary>
        /// <param name="filenameandPath"></param>
        /// <returns></returns>
        public static string Loadfile(string filenameandPath)
        {
            using (var sr = new StreamReader(filenameandPath))
            {
                var text = sr.ReadToEnd();
                sr.Close();
                return text;
            }
        }

        /// <summary>
        /// Writes File to passed directory
        /// </summary>
        /// <param name="filenameandPath"></param>
        /// <param name="message"></param>
        /// <param name="append"> </param>
        /// <param name="objects"></param>
        public static void Writefile(string filenameandPath, string message, bool append = true, params object[] objects)
        {
            using (var wr = new StreamWriter(filenameandPath, append))
            {
                wr.WriteLine(message, objects);
                wr.Flush();
                wr.Close();
            }

        }

    }
}
