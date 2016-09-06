using System;
using System.Collections.Generic;
using RLNET;


namespace Next_Game
{
    class Box
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Offset_x { get; set; }
        public int Offset_y { get; set; }
        private RLColor backColor;
        private int[,] arrayOfCells; //cell array for box and text
        private RLColor[,] arrayOfColors; //foreground color for cell contents

        public Box(int width, int height, int offset_x, int offset_y, RLColor backColor, RLColor borderColor)
        {
            this.Width = width;
            this.Height = height;
            this.Offset_x = offset_x;
            this.Offset_y = offset_y;
            this.backColor = backColor;

            //error check dimensions to see that they'll fit into the multi-console (130 x 100) -> Boxes are assumed to be centred horizontally in the multiconsole
            if (Width + Offset_x * 2 > 130) { Width = 130 - (Offset_x * 2); }
            if (Height + Offset_y * 2 > 100) { Height = 100 - (Offset_y * 2); }
            Width = Math.Max(Width, 8);
            Height = Math.Max(Height, 8);

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
