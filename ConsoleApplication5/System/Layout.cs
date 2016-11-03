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

    }
}
