using System;
using System.Collections.Generic;
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

        /// <summary>
        /// constructor -> create file
        /// </summary>
        /// <param name="path"></param>
        public Logger(string path)
        {
            this.path = path;
            //create file, if exists overwrite
            fileStream = File.OpenWrite(path);
            writer = new StreamWriter(fileStream);
        }

        /// <summary>
        /// write to file and Console
        /// </summary>
        /// <param name="text"></param>
        public void Write(string text)
        {
            if (String.IsNullOrEmpty(text) == false)
            {
                //write to file
                writer.Write(text);
                writer.Write(Environment.NewLine);
                //write to Console
                Console.WriteLine(text);
            }
        }

        /// <summary>
        /// close file
        /// </summary>
        public void Close()
        {
            writer.Close();
        }
    }
}
