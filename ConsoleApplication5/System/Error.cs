﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Game
{
    /// <summary>
    /// logs internal error messages
    /// </summary>
    class Error
    {
        private static int errorIndex = 1;
        public int errorID { get; }
        public int Code { get; } //3 digit error code
        public int Turn { get; } //game turn
        public string Time { get; } //time of occurrence (local)
        public string TimeZone { get; } //time zone
        public string Text { get; } //description
        public string Method { get; } //calling method
        public string Object { get; } //calling object
        public int Line { get; } //line of code

        public Error()
        { }

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="text"></param>
        /// <param name="memberName"></param>
        /// <param name="sourceLineNumber"></param>
        public Error(int errorCode, string text = "not specified",
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            errorID = errorIndex++;
            Code = errorCode;
            this.Text = text;
            Method = memberName;
            Line = sourceLineNumber;
            string[] tokens = sourceFilePath.Split('\\');
            Object = tokens[tokens.Length - 1];
            Turn = Game.gameTurn; //remember that Day # is gameTurn + 1
            Time = DateTime.Now.ToString("HH:mm:ss");
            TimeZone = DateTime.Now.ToString("%K");
        }


    }
}
