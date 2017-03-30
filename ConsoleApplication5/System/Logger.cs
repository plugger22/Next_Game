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
        string fileName;
        FileStream fileStream;
        StreamWriter writer;
        CultureInfo culture;
        Stopwatch timer;
        bool useTimer;

        /// <summary>
        /// constructor -> create file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="timer">true if you want the file ops to be timed, false otherise</param>
        public Logger(string path, bool timedOp = false)
        {
            string[] tokens = path.Split('/'); 
            fileName = tokens[tokens.Length -1];
            this.path = path;
            useTimer = timedOp;
            //create file, if exists overwrite
            try
            {
                fileStream = File.Create(path);
                writer = new StreamWriter(fileStream);
                if (useTimer == true)
                { timer = new Stopwatch(); timer.Start(); }
                //write header (time & data stamp
                culture = new CultureInfo("en-GB");
                Write(string.Format("//Exile Game file ops \"{0}\" commenced at: {1}", fileName, DateTime.Now.ToString(culture)), true, ConsoleColor.Red);
            }
            catch(PathTooLongException)
            { Game.SetError(new Error(192, string.Format("Path Too Long (\"{0}\") -> file \"{1}\" not created", path, fileName))); }
            catch(DirectoryNotFoundException)
            { Game.SetError(new Error(192, string.Format("Directory Not Found (\"{0}\") -> file \"{1}\" not created", path, fileName))); }
            catch(FileLoadException)
            { Game.SetError(new Error(192, "File Failed to Load (Logger) -> file not created")); }
        }

        /// <summary>
        /// write to file and Console. NOTE: IF a header, eg. "--- Process..." then new line auto inserted prior and color auto White -> no need to specify either
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color">Color that text will output to Console</param>
        /// <param name="writeToConsole">If true, dual output to file & console, otherwise file only</param>
        public void Write(string text, bool writeToConsole = true, ConsoleColor color = ConsoleColor.Gray)
        {
            if (String.IsNullOrEmpty(text) == false)
            {
                //write to file
                if (File.Exists(path) == true && fileStream != null)
                {
                    try
                    {
                        writer.Write(text);
                        writer.Write(Environment.NewLine);
                    }
                    catch(ObjectDisposedException)
                    { Game.SetError(new Error(192, string.Format("File \"{0}\" has been Disposed -> Write (\"{1}\") -> Text not written",fileName, text))); }
                }
                else { Game.SetError(new Error(192, string.Format("File \"{0}\" does not exist -> Write (\"{1}\") -> Text not written", fileName, text))); }
                //write to Console
                if (writeToConsole == true)
                {
                    //if a header, eg. "--- ProcessStartTurn ---", then auo insert new line prior and make Color white
                    if (text.Contains("---") == true)
                    { Console.WriteLine(); color = ConsoleColor.White; }
                    Console.ForegroundColor = color;
                    Console.WriteLine(text);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
        }

        /// <summary>
        /// Open an existing, but closed, file for write operations (any existing text is overwritten)
        /// </summary>
        public void Open()
        {
            //Open file for write operations
            if (File.Exists(path) == true)
            {
                try
                {
                    fileStream = File.OpenWrite(path);
                    //write header (time & data stamp
                    culture = new CultureInfo("en-GB");
                    Write(string.Format("//Exile Game File \"{0}\" opened at: {1}", fileName, DateTime.Now.ToString(culture)), true, ConsoleColor.Red);
                }
                catch (PathTooLongException)
                { Game.SetError(new Error(192, string.Format("Path Too Long (\"{0}\") -> file not created", path))); }
                catch (DirectoryNotFoundException)
                { Game.SetError(new Error(192, string.Format("Directory Not Found (\"{0}\") -> file not created", path))); }
                catch (FileLoadException)
                { Game.SetError(new Error(192, string.Format("File \"{0}\" Failed to Load (Logger) -> file not created"), fileName)); }
            }
            else { Game.SetError(new Error(192, string.Format("File does not exist -> Open (\"{0}\") -> Text not written", path))); }
        }

        /// <summary>
        /// close file -> use Dispose if you no longer want to use the file again
        /// </summary>
        public void Close()
        {
            if (File.Exists(path) == true)
            {
                Write(string.Format("//Exile Game File \"{0}\" Close completed at: {1}", fileName, DateTime.Now.ToString(culture)), true, ConsoleColor.Red);
                if (useTimer == true)
                {
                    timer.Stop();
                    TimeSpan ts = timer.Elapsed;
                    Write(string.Format("//Exile Elapsed Time {0} ms", ts.Milliseconds), true, ConsoleColor.Red);
                }
                //Closing the adapter also closes the underlying filestream
                writer.Close();
                //fileStream.Close();
                fileStream = null;
            }
            else { Game.SetError(new Error(192, string.Format("File does not exist -> Close (\"{0}\")", path))); }
        }

        /// <summary>
        /// Dispose of Logger object
        /// </summary>
        public void Dispose()
        {
            if (File.Exists(path) == true && fileStream != null)
            {
                Write(string.Format("//Exile Game file ops \"{0}\" Completed at: {1}", fileName, DateTime.Now.ToString(culture)), true, ConsoleColor.Red);
                if (useTimer == true)
                {
                    timer.Stop();
                    TimeSpan ts = timer.Elapsed;
                    Write(string.Format("//Elapsed Time {0} ms", ts.Milliseconds), true, ConsoleColor.Red);
                }
                //closing the adapter also closes the underlying filestream
                writer.Dispose(); 
                //fileStream.Dispose();
                fileStream = null;
               
            }
            else { Game.SetError(new Error(192, string.Format("File does not exist -> Dispose (\"{0}\")", path))); }
        }
    }
}
