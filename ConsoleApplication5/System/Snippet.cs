using RLNET;

namespace Next_Game
{
    //Text snippets that can be displayed in multiple colors. Can be part line or full line texts
    public class Snippet
    {
        private string textString; //string to be displayed
        private RLColor foreColor;
        private RLColor backColor;
        private bool newLine; //if true a newline is inserted after this snippet, false it continues on the same line

        /// <summary>
        /// add a blank line
        /// </summary>
        public Snippet()
        { textString = null; newLine = true; }

        /// <summary>
        /// Simple Construstor that takes a string only and converts to a default gray on black, newline, snippet
        /// </summary>
        /// <param name="textString"></param>
        /// <param name="newLine"></param>
        public Snippet(string textString, bool newLine = true)
        {
            this.textString = textString;
            this.foreColor = RLColor.White;
            this.backColor = RLColor.Black;
            this.newLine = newLine;
        }

        /// <summary>
        /// Standard Constructor that allows customisation of colors
        /// </summary>
        /// <param name="textString"></param>
        /// <param name="foreColor"></param>
        /// <param name="backColor"></param>
        /// <param name="newLine">Set as false if you want the NEXT snippet to be on the same line</param>
        public Snippet(string textString, RLColor foreColor, RLColor backColor, bool newLine = true)
        {
            this.textString = textString;
            this.foreColor = foreColor;
            this.backColor = backColor;
            this.newLine = newLine;
        }

        public string GetText()
        { return textString; }

        public RLColor GetForeColor()
        { return foreColor; }

        public RLColor GetBackColor()
        { return backColor; }

        public bool GetNewLine()
        { return newLine; }

        public void SetText(string snippetText)
        { this.textString = snippetText; }
       
    }
}