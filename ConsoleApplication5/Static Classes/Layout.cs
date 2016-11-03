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
        private int[,] arrayOfCells; //cell array for box and text
        private RLColor[,] arrayOfColors; //foreground color for cell contents

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="backColor"></param>
        /// <param name="borderColor"></param>
        public Layout(int width, int height, int offset_x, int offset_y, RLColor backColor, RLColor borderColor)
        {
            this.Width = width;
            this.Height = height;
            this.backColor = backColor;
            this.Offset_x = offset_x;
            this.Offset_y = offset_y;
            //error check dimensions to see that they'll fit into the multi-console (130 x 100)
            if (Width > 130) { Width = 130; Game.SetError(new Error(80, string.Format("Invalid Width input \"{0}\", changed to 130", width))); }
            if (Height > 100) { Height = 100; Game.SetError(new Error(80, string.Format("Invalid Height input \"{0}\", changed to 130", height))); }
            //initialise border and colors
            arrayOfCells = new int[Width - Offset_x, Height - Offset_y * 2];
            arrayOfColors = new RLColor[Width, Height];

            //debug data set
            for (int height_index = 0; height_index < Height - Offset_y * 2; height_index++)
            {
                for (int width_index = 0; width_index < Width - Offset_x; width_index++)
                { arrayOfColors[width_index, height_index] = backColor; arrayOfCells[width_index, height_index] = 177; }
            }

        }
    
                
        /// <summary>
        /// Draw box to multiConsole
        /// </summary>
        /// <param name="multiConsole"></param>
        public void Draw(RLConsole multiConsole)
        {
            multiConsole.Clear();
            for (int height_index = Offset_y; height_index < Height - Offset_y * 2; height_index++)
            {
                for (int width_index = 0; width_index < Width - Offset_x; width_index++)
                { multiConsole.Set(width_index, height_index, arrayOfColors[width_index, height_index], backColor, arrayOfCells[width_index, height_index]); }
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
        private void DrawBox(int coord_X, int coord_Y, int width, int height, RLColor borderColor, RLColor fillColor)
        {
            //error check input, exit on bad data
            if (coord_X < 0 || coord_X + width > Width - Offset_x)
            { Game.SetError(new Error(81, string.Format("Invalid coordX input \"{0}\"", coord_X))); return; }
            if (coord_Y < 0 || coord_Y + height > Height - Offset_y * 2)
            //populate array - corners
            arrayOfCells[coord_X, coord_Y] = 218; arrayOfColors[coord_X, coord_Y] = borderColor; //top left
            arrayOfCells[coord_X, coord_Y + height - 1] = 192; arrayOfColors[coord_X, coord_Y + height - 1] = borderColor; //bottom left
            arrayOfCells[coord_X + width - 1, coord_Y] = 191; arrayOfColors[coord_X + width - 1, coord_Y] = borderColor; //top right
            arrayOfCells[coord_X + width - 1, coord_Y + height - 1] = 217; arrayOfColors[coord_X + width - 1, coord_Y + height - 1] = borderColor; //bottom right
            //Top & bottom rows
            for (int i = coord_X; i < coord_X + width - 1; i++)
            { arrayOfCells[i, coord_Y] = 196; arrayOfCells[i, coord_Y + height - 1] = 196; arrayOfColors[i, coord_Y] = borderColor; arrayOfColors[i, coord_Y + height - 1] = borderColor; }
            //left and right sides
            for (int i = coord_Y; i < coord_Y + height - 1; i++)
            { arrayOfCells[coord_X, i] = 179; arrayOfCells[coord_X + width - 1, i] = 179; arrayOfColors[coord_X, i] = borderColor; arrayOfColors[coord_X + width - 1, i] = borderColor; }
        }

    }

    /// <summary>
    /// Decide Strategy layout
    /// </summary>
    public class LayoutStrategy : Layout
    {

        public LayoutStrategy(int width, int height, int offset_x, int offset_y, RLColor backColor, RLColor borderColor) : base(width, height, offset_x, offset_y, backColor, borderColor)
        { }
    }



    /// <summary>
    /// Card Play layout
    /// </summary>
    public class LayoutCards : Layout
    {
       
        public LayoutCards(int width, int height, int offset_x, int offset_y, RLColor backColor, RLColor borderColor) : base (width, height, offset_x, offset_y, backColor, borderColor)
        { }

        /// <summary>
        /// Set up layout
        /// </summary>
        public void Initialise()
        {

        }
    }


    /// <summary>
    /// End of Conflict Outcome layout
    /// </summary>
    public class LayoutOutcome : Layout
    {

        public LayoutOutcome(int width, int height, int offset_x, int offset_y, RLColor backColor, RLColor borderColor) : base(width, height, offset_x, offset_y, backColor, borderColor)
        { }
    }

}
