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
                Write(string.Format("//Exile Game file ops commenced at: {0}", DateTime.Now.ToString(culture)), true, ConsoleColor.Red);
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
        /// <param name="color">Color that text will output to Console</param>
        /// <param name="writeToConsole">If true, dual output to file & console, otherwise file only</param>
        public void Write(string text, bool writeToConsole = true, ConsoleColor color = ConsoleColor.Gray)
        {
            if (String.IsNullOrEmpty(text) == false)
            {
                //write to file
                if (File.Exists(path) == true)
                {
                    writer.Write(text);
                    writer.Write(Environment.NewLine);
                }
                else { Game.SetError(new Error(192, string.Format("File does not exist -> Write (\"{0}\") -> Text not written", path))); }
                //write to Console
                if (writeToConsole == true)
                {
                    Console.ForegroundColor = color;
                    Console.WriteLine(text);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
        }

        /// <summary>
        /// Open and existing, but closed, file for write operations
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
                    Write(string.Format("//Exile Game File opened at: {0}", DateTime.Now.ToString(culture)), true, ConsoleColor.Red);
                }
                catch (PathTooLongException)
                { Game.SetError(new Error(192, string.Format("Path Too Long (\"{0}\") -> file not created", path))); }
                catch (DirectoryNotFoundException)
                { Game.SetError(new Error(192, string.Format("Directory Not Found (\"{0}\") -> file not created", path))); }
                catch (FileLoadException)
                { Game.SetError(new Error(192, "File Failed to Load (Logger) -> file not created")); }
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
                Write(string.Format("//Exile Game Initialisation completed at: {0}", DateTime.Now.ToString(culture)), true, ConsoleColor.Red);
                timer.Stop();
                TimeSpan ts = timer.Elapsed;
                Write(string.Format("//Exile Elapsed Time {0} ms", ts.Milliseconds), true, ConsoleColor.Red);
                writer.Close();
            }
            else { Game.SetError(new Error(192, string.Format("File does not exist -> Close (\"{0}\") -> Text not written", path))); }
        }

        /// <summary>
        /// Dispose of Logger object
        /// </summary>
        public void Dispose()
        {
            if (File.Exists(path) == true)
            {
                Write(string.Format("//Exile Game file ops completed at: {0}", DateTime.Now.ToString(culture)), true, ConsoleColor.Red);
                timer.Stop();
                TimeSpan ts = timer.Elapsed;
                Write(string.Format("//Elapsed Time {0} ms", ts.Milliseconds), true, ConsoleColor.Red);
                writer.Close();
            }
            else { Game.SetError(new Error(192, string.Format("File does not exist -> Dispose (\"{0}\") -> Text not written", path))); }
        }
    }
}
