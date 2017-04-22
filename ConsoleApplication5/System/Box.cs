using System;
using System.Collections.Generic;
using RLNET;


namespace Next_Game
{
    /// <summary>
    /// Special Display boxes -> all centred at top of screen with text possible underneath (not yet implemented)
    /// </summary>
    class Box
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Offset_x { get; set; }
        public int Offset_y { get; set; }
        private RLColor backColor;
        private int[,] arrayOfCells; //cell array for box and text
        private RLColor[,] arrayOfColors; //foreground color for cell contents

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="offset_y"></param>
        /// <param name="backColor"></param>
        /// <param name="borderColor"></param>
        public Box(int width, int height, int offset_y, RLColor backColor, RLColor borderColor)
        {
            this.Width = width;
            this.Height = height;
            this.Offset_y = offset_y;
            this.backColor = backColor;
            int multiWidth = Game._multiWidth;
            int multiHeight = Game._multiHeight;
            //work out X offset to have box centred
            Offset_x = (multiWidth - Width) / 2;
            //error check dimensions to see that they'll fit into the multi-console (130 x 100) -> Boxes are assumed to be centred horizontally in the multiconsole
            if (Width > multiWidth) { Width = multiWidth; Offset_x = 0; }
            if (Height > multiHeight) { Height = multiHeight; Offset_y = 0; }
            
            //initialise border and colors
            arrayOfCells = new int[Width, Height];
            arrayOfColors = new RLColor[Width, Height];
            //populate array - corners
            arrayOfCells[0, 0] = 218; arrayOfColors[0, 0] = borderColor;
            arrayOfCells[0, Height - 1] = 192; arrayOfColors[0, Height - 1] = borderColor;
            arrayOfCells[Width - 1, 0] = 191; arrayOfColors[Width - 1, 0] = borderColor;
            arrayOfCells[Width - 1, Height - 1] = 217; arrayOfColors[Width - 1, Height - 1] = borderColor;
            //Top & bottom rows
            for (int i = 1; i < Width - 1; i++)
            { arrayOfCells[i, 0] = 196; arrayOfCells[i, Height - 1] = 196; arrayOfColors[i, 0] = borderColor; arrayOfColors[0, Height - 1] = borderColor; }
            //left and right sides
            for (int i = 1; i < Height - 1; i++)
            { arrayOfCells[0, i] = 179; arrayOfCells[Width - 1, i] = 179; arrayOfColors[0, i] = borderColor; arrayOfColors[Width - 1, i] = borderColor; }
            
        }

        /// <summary>
        /// Change background color
        /// </summary>
        /// <param name="color"></param>
        public void SetBackColor(RLColor color)
        { backColor = color; }

        /// <summary>
        /// Populate arrayOfCells with centred text of the correct color
        /// </summary>
        /// <param name="listOfText"></param>
        public void SetText(List<Snippet> listOfText)
        {
            int outerLimit = Math.Min(Height - 2, listOfText.Count);
            int innerLimit;
            string text;
            int textLength;
            int textCounter;
            RLColor foreColor;
            int offset; // space on either side of a line of text to have it centred within the box
            for (int i = 0; i < outerLimit; i++)
            {
                text = listOfText[i].GetText();
                if (text == null) { textLength = 0; }
                else { textLength = text.Length; }

                offset = (Width - textLength) / 2;
                offset = Math.Max(offset, 1);
                foreColor = listOfText[i].GetForeColor();
                textCounter = 0;
                innerLimit = offset + textLength;
                //place text characters in array, one by one, for this line
                for (int k = offset; k < innerLimit; k++)
                {
                    arrayOfCells[k, i + 1] = text[textCounter++];
                    arrayOfColors[k, i + 1] = foreColor;
                }
            }
        }


        /// <summary>
        /// Draw box to multiConsole
        /// </summary>
        /// <param name="multiConsole"></param>
        public void Draw(RLConsole multiConsole)
        {
            multiConsole.Clear();
            for (int height = 0; height < Height; height++)
            {
                for (int width = 0; width < Width; width++)
                { multiConsole.Set(Offset_x + width, Offset_y + height, arrayOfColors[width, height], backColor, arrayOfCells[width, height]); }
            }
            
        }


    }
}
