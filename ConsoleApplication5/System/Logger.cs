using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    /// <summary>
    /// Writes debug info to console and files for logging purposes
    /// </summary>
    public class Logger
    {

        string path;
        FileStream fileStream;
        StreamWriter writer;
        CultureInfo culture;
        Stopwatch timer;

        /// <summary>
        /// constructor -> create file
        /// </summary>
        /// <param name="path"></param>
        public Logger(string path)
        {
            this.path = path;
            //create file, if exists overwrite
            try
            {
                fileStream = File.Create(path);
                writer = new StreamWriter(fileStream);
                timer = new Stopwatch();
                timer.Start();
                //write header (time & data stamp
                culture = new CultureInfo("en-GB");
                Write(string.Format("//Exile Game Initialisation commenced at: {0}", DateTime.Now.ToString(culture)), ConsoleColor.Red);
            }
            catch(PathTooLongException)
            { Game.SetError(new Error(192, string.Format("Path Too Long (\"{0}\") -> file not created", path))); }
            catch(DirectoryNotFoundException)
            { Game.SetError(new Error(192, string.Format("Directory Not Found (\"{0}\") -> file not created", path))); }
            catch(FileLoadException)
            { Game.SetError(new Error(192, "File Failed to Load (Logger) -> file not created")); }
        }

        /// <summary>
        /// write to file and Console
        /// </summary>
        /// <param name="text"></param>
        public void Write(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            if (String.IsNullOrEmpty(text) == false)
            {
                //write to file
                writer.Write(text);
                writer.Write(Environment.NewLine);
                //write to Console
                Console.ForegroundColor = color;
                Console.WriteLine(text);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        /// <summary>
        /// close file
        /// </summary>
        public void Close()
        {
            if (File.Exists(path) == true)
            {
                Write(string.Format("//Exile Game Initialisation completed at: {0}", DateTime.Now.ToString(culture)), ConsoleColor.Red);
                timer.Stop();
                TimeSpan ts = timer.Elapsed;
                Write(string.Format("//Exile Elapsed Time {0} ms", ts.Milliseconds), ConsoleColor.Red);
                writer.Close();
            }
        }

        /// <summary>
        /// Dispose of Logger object
        /// </summary>
        public void Dispose()
        {
            if (File.Exists(path) == true)
            {
                Write(string.Format("//Exile Game Initialisation completed at: {0}", DateTime.Now.ToString(culture)), ConsoleColor.Red);
                timer.Stop();
                TimeSpan ts = timer.Elapsed;
                Write(string.Format("//Elapsed Time {0} ms", ts.Milliseconds), ConsoleColor.Red);
                writer.Close();
            }
        }
    }
}
