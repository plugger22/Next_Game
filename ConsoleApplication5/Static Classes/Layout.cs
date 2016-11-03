using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;

namespace Next_Game
{
    /// <summary>
    /// handles specific multi console screen layouts for the conflict system
    /// </summary>
    public class Layout
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Offset_x { get; set; } //offset from right hand side of screen (cells)
        public int Offset_y { get; set; } //offset from top and bottom of screen (cells)
        private RLColor backColor;
        private int[,] arrayOfCells_Cards; //cell array for box and text
        private RLColor[,] arrayOfForeColors_Cards; //foreground color for cell contents
        private RLColor[,] arrayOfBackColors_Cards; //background color for cell's

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="fillColor"></param>
        /// <param name="borderColor"></param>
        public Layout(int width, int height, int offset_x, int offset_y, RLColor fillColor, RLColor borderColor)
        {
            this.Width = width;
            this.Height = height;
            this.backColor = fillColor;
            this.Offset_x = offset_x;
            this.Offset_y = offset_y;
            //error check dimensions to see that they'll fit into the multi-console (130 x 100)
            if (Width > 130) { Width = 130; Game.SetError(new Error(80, string.Format("Invalid Width input \"{0}\", changed to 130", width))); }
            if (Height > 100) { Height = 100; Game.SetError(new Error(80, string.Format("Invalid Height input \"{0}\", changed to 130", height))); }
            //initialise border and colors
            arrayOfCells_Cards = new int[Width - Offset_x, Height - Offset_y * 2];
            arrayOfForeColors_Cards = new RLColor[Width - Offset_x, Height - Offset_y * 2];
            arrayOfBackColors_Cards = new RLColor[Width - Offset_x, Height - Offset_y * 2];

            //debug data set
            for (int height_index = 0; height_index < Height - Offset_y * 2; height_index++)
            {
                for (int width_index = 0; width_index < Width - Offset_x; width_index++)
                {
                    arrayOfBackColors_Cards[width_index, height_index] = fillColor;
                    arrayOfForeColors_Cards[width_index, height_index] = RLColor.White;
                    arrayOfCells_Cards[width_index, height_index] = 255;
                }
            }

        }

        /// <summary>
        /// Initialise Cards layout
        /// </summary>
        public void InitialiseCards()
        {
            int left_align = 11; //left side of status boxes (y_coord)
            int right_align = 117;
            int top_align = 24; //top of card (y_coord)
            int card_width = 40;
            int card_height = 40;
            int bottom_align = top_align + card_height;
            int status_box_width = 26;
            int status_box_height = 11;
            int text_box_width = 106; //two boxes under the card display
            
            //Card
            DrawBox(44, top_align, card_width, card_height, RLColor.Yellow, RLColor.LightGray);
            //message box under card
            DrawBox(left_align, 70, text_box_width, 12, RLColor.Yellow, RLColor.LightGray);
            //instruction box
            DrawBox(left_align, 86, text_box_width, 6, RLColor.Yellow, RLColor.LightGray);
            //Remaining Influence (top left in relation to card display)
            DrawBox(left_align, top_align, status_box_width, status_box_height, RLColor.Yellow, RLColor.LightGray);
            DrawCenteredText("Remaining", left_align, status_box_width, top_align + 2, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("Influence", left_align, status_box_width, top_align + 4, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("0", left_align, status_box_width, top_align + 7, RLColor.Blue, arrayOfCells_Cards, arrayOfForeColors_Cards);
            //Remaining Cards (bottom left)
            int vertical_align = bottom_align - status_box_height;
            DrawBox(left_align, vertical_align, status_box_width, status_box_height, RLColor.Yellow, RLColor.LightGray);
            DrawCenteredText("Remaining", left_align, status_box_width, vertical_align + 2, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("Cards", left_align, status_box_width, vertical_align + 4, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("0", left_align, status_box_width, vertical_align + 7, RLColor.Red, arrayOfCells_Cards, arrayOfForeColors_Cards);
            //Pool info (top right)
            int horizontal_align = right_align - status_box_width;
            DrawBox(horizontal_align, top_align, status_box_width, status_box_height, RLColor.Yellow, RLColor.LightGray);
            DrawText("Good cards", horizontal_align + 3, top_align + 2, RLColor.Blue, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("Neutral cards", horizontal_align + 3, top_align + 4, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("Bad cards", horizontal_align + 3, top_align + 6, RLColor.Red, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("Total cards", horizontal_align + 3, top_align + 8, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            //Strategy Info (bottom right)
            DrawBox(horizontal_align, vertical_align, status_box_width, status_box_height, RLColor.Yellow, RLColor.LightGray);
            DrawCenteredText("Your Strategy", horizontal_align, status_box_width, vertical_align + 2, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("All Out Assault", horizontal_align, status_box_width, vertical_align + 4, RLColor.Blue, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("Opponent's Strategy", horizontal_align, status_box_width, vertical_align + 6, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("Hold the Ground", horizontal_align, status_box_width, vertical_align + 8, RLColor.Red, arrayOfCells_Cards, arrayOfForeColors_Cards);
        }

        /// <summary>
        /// Draw Cards Layout
        /// </summary>
        /// <param name="multiConsole"></param>
        public void DrawCards(RLConsole multiConsole)
        { Draw(multiConsole, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards); }

        /// <summary>
        /// Draw box to multiConsole
        /// </summary>
        /// <param name="multiConsole"></param>
        private void Draw(RLConsole multiConsole, int[,] arrayOfCells, RLColor[,] arrayOfForeColors, RLColor[,] arrayOfBackColors )
        {
            multiConsole.Clear();
            for (int height_index = Offset_y; height_index < Height - Offset_y * 2; height_index++)
            {
                for (int width_index = 0; width_index < Width - Offset_x; width_index++)
                { multiConsole.Set(width_index, height_index, arrayOfForeColors[width_index, height_index], arrayOfBackColors[width_index, height_index], 
                    arrayOfCells[width_index, height_index]); }
            }

        }

        /// <summary>
        /// Draw a bordered, filled box of any size. Used by all layout classes to initialise
        /// </summary>
        /// <param name="coord_X"></param>
        /// <param name="coord_Y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="borderColor"></param>
        /// <param name="fillColor"></param>
        internal void DrawBox(int coord_X, int coord_Y, int width, int height, RLColor borderColor, RLColor fillColor)
        {
            //error check input, exit on bad data
            if (coord_X < 0 || coord_X + width > Width - Offset_x)
            { Game.SetError(new Error(81, string.Format("Invalid coord_X input \"{0}\"", coord_X))); return; }
            if (coord_Y < 0 || coord_Y + height > Height - Offset_y * 2)
            { Game.SetError(new Error(81, string.Format("Invalid coord_Y input \"{0}\"", coord_Y))); return; }
            //populate array - corners
            arrayOfCells_Cards[coord_X, coord_Y] = 218; arrayOfForeColors_Cards[coord_X, coord_Y] = borderColor; //top left
            arrayOfCells_Cards[coord_X, coord_Y + height - 1] = 192; arrayOfForeColors_Cards[coord_X, coord_Y + height - 1] = borderColor; //bottom left
            arrayOfCells_Cards[coord_X + width - 1, coord_Y] = 191; arrayOfForeColors_Cards[coord_X + width - 1, coord_Y] = borderColor; //top right
            arrayOfCells_Cards[coord_X + width - 1, coord_Y + height - 1] = 217; arrayOfForeColors_Cards[coord_X + width - 1, coord_Y + height - 1] = borderColor; //bottom right
            //Top & bottom rows
            for (int i = coord_X + 1; i < coord_X + width - 1; i++)
            {
                arrayOfCells_Cards[i, coord_Y] = 196;
                arrayOfCells_Cards[i, coord_Y + height - 1] = 196;
                arrayOfForeColors_Cards[i, coord_Y] = borderColor;
                arrayOfForeColors_Cards[i, coord_Y + height - 1] = borderColor;
            }
            //left and right sides
            for (int i = coord_Y + 1; i < coord_Y + height - 1; i++)
            {
                arrayOfCells_Cards[coord_X, i] = 179;
                arrayOfCells_Cards[coord_X + width - 1, i] = 179;
                arrayOfForeColors_Cards[coord_X, i] = borderColor;
                arrayOfForeColors_Cards[coord_X + width - 1, i] = borderColor;
            }
            //fill backcolor
            for (int width_index = coord_X + 1; width_index < coord_X + width - 1; width_index++)
            {
                for (int height_index = coord_Y + 1; height_index < coord_Y + height - 1; height_index++)
                { arrayOfBackColors_Cards[width_index, height_index] = fillColor; }
            }
        }

        /// <summary>
        /// Draws text on a layout (labels)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="coord_X"></param>
        /// <param name="coord_Y"></param>
        /// <param name="foreColor"></param>
        internal void DrawText(string text, int coord_X, int coord_Y, RLColor foreColor, int [,] arrayOfCells, RLColor [,] arrayOfForeColors)
        {
            for (int i = 0; i < text.Length; i++)
            {
                arrayOfCells[coord_X + i, coord_Y] = text[i];
                arrayOfForeColors[coord_X + i, coord_Y] = foreColor;
            }
        }

        /// <summary>
        /// Draws text centered between two x coords (Left and Right boundaries)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="coord_X_Left"></param>
        /// <param name="coord_X_Right"></param>
        /// <param name="coord_Y"></param>
        /// <param name="foreColor"></param>
        /// <param name="arrayOfCells"></param>
        /// <param name="arrayOfForeColors"></param>
        internal void DrawCenteredText(string text, int coord_X, int width, int coord_Y, RLColor foreColor, int[,] arrayOfCells, RLColor[,] arrayOfForeColors)
        {
            int length = text.Length;
            //error check
            if (length >= width)
            { Game.SetError(new Error(82, string.Format("String input \"{0}\" is to wide to fit in the box (max {1})", text, width))); return; }
            //work out start position
            int start = (width - length) / 2;
            //place text
            for (int i = 0; i < text.Length; i++)
            {
                arrayOfCells[coord_X + i + start, coord_Y] = text[i];
                arrayOfForeColors[coord_X + i + start, coord_Y] = foreColor;
            }
        }

    }
}
