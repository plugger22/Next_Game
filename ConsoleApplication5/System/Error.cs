using System;
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
        public string Text { get; } //description
        public string Method { get; } //calling method
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
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            errorID = errorIndex++;
            Code = errorCode;
            this.Text = text;
            Method = memberName;
            Line = sourceLineNumber;
        }


    }
}
