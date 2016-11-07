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
        //multiConsole window
        public int Width { get; set; }
        public int Height { get; set; }
        public int Offset_x { get; set; } //offset from right hand side of screen (cells)
        public int Offset_y { get; set; } //offset from top and bottom of screen (cells)
        private RLColor backColor;
        //card layout
        int ca_left_outer; //left side of status boxes (y_coord)
        int ca_right_align;
        int ca_card_width;
        int ca_card_height;
        int ca_left_inner;
        int ca_status_width;
        int ca_status_height;
        int ca_top_align; //top of card (y_coord) & top y_coord for upper status boxes
        int ca_spacer; //space between bottom of card and top of text boxes below
        int ca_message_height; //upper text box
        int ca_instruct_height; //lower text box
        int ca_score_height;
        int ca_score_vert_align;
        int ca_bar_offset_x; //score internal bar
        int ca_bar_offset_y;
        //strategy layout
        int st_left_outer; //left margin (y_coord)
        int st_middle_align; //middle box margin (y_coord)
        int st_right_outer;
        int st_upper_box_width;
        int st_upper_box_height;
        int st_top_align;
        int st_spacer; //vertical space between frames
        int st_strategy_width; //strategy box, uppper middle
        int st_instruct_height;
        //Outcome layout
        int ou_left_align;
        int ou_spacer;
        int ou_outcome_height;
        int ou_instruct_height;
        //Confirm Layout
        RLColor Confirm_FillColor { get; set; }
        //AutoResolve Layout
        RLColor Resolve_FillColor { get; set; }

        //Conflict Cards
        private int[,] arrayOfCells_Cards; //cell array for box and text
        private RLColor[,] arrayOfForeColors_Cards; //foreground color for cell contents
        private RLColor[,] arrayOfBackColors_Cards; //background color for cell's
        //Conflict Strategy
        private int[,] arrayOfCells_Strategy;
        private RLColor[,] arrayOfForeColors_Strategy;
        private RLColor[,] arrayOfBackColors_Strategy;
        //Conflict Outcome
        private int[,] arrayOfCells_Outcome;
        private RLColor[,] arrayOfForeColors_Outcome;
        private RLColor[,] arrayOfBackColors_Outcome;
        //Confirmation
        private int[,] arrayOfCells_Confirm;
        private RLColor[,] arrayOfForeColors_Confirm;
        private RLColor[,] arrayOfBackColors_Confirm;

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
            arrayOfCells_Strategy = new int[Width - Offset_x, Height - Offset_y * 2];
            arrayOfForeColors_Strategy = new RLColor[Width - Offset_x, Height - Offset_y * 2];
            arrayOfBackColors_Strategy = new RLColor[Width - Offset_x, Height - Offset_y * 2];
            arrayOfCells_Outcome = new int[Width - Offset_x, Height - Offset_y * 2];
            arrayOfForeColors_Outcome = new RLColor[Width - Offset_x, Height - Offset_y * 2];
            arrayOfBackColors_Outcome = new RLColor[Width - Offset_x, Height - Offset_y * 2];
            arrayOfCells_Confirm = new int[Width - Offset_x, Height - Offset_y * 2];
            arrayOfForeColors_Confirm = new RLColor[Width - Offset_x, Height - Offset_y * 2];
            arrayOfBackColors_Confirm = new RLColor[Width - Offset_x, Height - Offset_y * 2];

            //debug data set
            for (int height_index = 0; height_index < Height - Offset_y * 2; height_index++)
            {
                for (int width_index = 0; width_index < Width - Offset_x; width_index++)
                {
                    arrayOfBackColors_Cards[width_index, height_index] = fillColor;
                    arrayOfForeColors_Cards[width_index, height_index] = RLColor.White;
                    arrayOfCells_Cards[width_index, height_index] = 255;
                    arrayOfBackColors_Strategy[width_index, height_index] = fillColor;
                    arrayOfForeColors_Strategy[width_index, height_index] = RLColor.White;
                    arrayOfCells_Strategy[width_index, height_index] = 255;
                    arrayOfBackColors_Outcome[width_index, height_index] = fillColor;
                    arrayOfForeColors_Outcome[width_index, height_index] = RLColor.White;
                    arrayOfCells_Outcome[width_index, height_index] = 255;
                    arrayOfBackColors_Confirm[width_index, height_index] = fillColor;
                    arrayOfForeColors_Confirm[width_index, height_index] = RLColor.White;
                    arrayOfCells_Confirm[width_index, height_index] = 255;
                }
            }
            //Card Layout
            ca_left_outer = 8; //left side of status boxes (y_coord)
            ca_right_align = 120;
            ca_card_width = 40;
            ca_card_height = 41;
            ca_left_inner = 44;
            ca_status_width = 33;
            ca_status_height = 11;
            ca_top_align = 22; //top of card (y_coord) & top y_coord for upper status boxes
            ca_spacer = 6; //space between bottom of card and top of text boxes below
            ca_message_height = 12; //upper text box
            ca_instruct_height = 9; //lower text box
            ca_score_height = 12;
            ca_score_vert_align = 6;
            ca_bar_offset_x = 4; //score internal bar
            ca_bar_offset_y = 3;
            //Strategy Layout
            st_left_outer = 8; //left margin (y_coord)
            st_middle_align = 44; //middle box margin (y_coord)
            st_right_outer = 120;
            st_upper_box_width = 33;
            st_upper_box_height = 11;
            st_top_align = 6;
            st_spacer = 3; //vertical space between frames
            st_strategy_width = 40; //strategy box, uppper middle
            st_instruct_height = 9;
            //Outcome Layout
            ou_left_align = 10;
            ou_spacer = 4;
            ou_outcome_height = 20;
            ou_instruct_height = 7;
            //Confirm layout
            Confirm_FillColor = RLColor.White;
            //AutoResolve Layout
            Resolve_FillColor = RLColor.White;
        }

        /// <summary>
        /// Initialise the layouts required for the Conflict system
        /// </summary>
        public void Initialise()
        {
            InitialiseStrategy();
            InitialiseCards();
            InitialiseOutcome();
            InitialiseConfirm();
        }

        /// <summary>
        /// Initialise Cards layout
        /// </summary>
        public void InitialiseCards()
        {
            int card_vert_1 = ca_top_align + 2; //type of card, eg. 'skill card', y_coord
            int card_vert_2 = ca_top_align + 4; //spacer
            int card_vert_3 = ca_top_align + ca_card_height / 4; //eg. "King's Leadship"
            int card_vert_4 = ca_top_align + ca_card_height / 4 + 3; //Immersion text eg. "Forward Men!"
            int card_vert_5 = ca_top_align + ca_card_height / 4 * 3; //advantage / disadvantage
            int card_vert_6 = ca_top_align + ca_card_height / 4 * 3 + 3; //"Play for 2 points"
            int card_vert_7 = ca_top_align + ca_card_height / 4 * 3 + 6; //"Ignore for 1 point"
            int middle_align = ca_top_align + ca_card_height / 2 - ca_status_height / 2; // top y_coord for middle status boxes
            int bottom_align = ca_top_align + ca_card_height; //bottom y_coord for lower status boees
            int horizontal_align = ca_right_align - ca_status_width; //status boxes, left side of status boxes for those on the right hand side
            int vertical_align = bottom_align - ca_status_height; //status boxes
            int vertical_pos = ca_top_align + ca_card_height + ca_spacer; //used for message boxes down the bottom
            int text_box_width = horizontal_align + ca_status_width - ca_left_outer; //two boxes under the card display
            int score_width = horizontal_align - ca_left_outer;
            int score_left_align = ca_left_outer + ca_status_width / 2;
            int bar_width = score_width - (ca_bar_offset_x * 2);
            int bar_middle = score_left_align + ca_bar_offset_x + (bar_width / 2); //x_coord of mid point
            int bar_segment = Convert.ToInt32((float)bar_width / 8); //scoring marker segments on bar (4 either side of the zero mid point)
            int bar_top = ca_score_vert_align + ca_bar_offset_y; //y_coord of bar
            int bar_height = ca_score_height - (ca_bar_offset_y * 2);
            //Card
            DrawBox(ca_left_inner, ca_top_align, ca_card_width, ca_card_height, RLColor.Yellow, RLColor.White, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            DrawCenteredText("Skill Card", ca_left_inner, card_vert_1, ca_card_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("--- 0 ---", ca_left_inner, card_vert_2, ca_card_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("The King's Leadership", ca_left_inner, card_vert_3, ca_card_width, RLColor.Red, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("For the King! Long Live the King!", ca_left_inner, card_vert_4, ca_card_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("Disadvantage", ca_left_inner, card_vert_5, ca_card_width, RLColor.Red, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("Play for -2 Points", ca_left_inner, card_vert_6, ca_card_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("Ignore for -8 Points", ca_left_inner, card_vert_7, ca_card_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            //Score track
            DrawBox(score_left_align, ca_score_vert_align, score_width, ca_score_height, RLColor.Yellow, RLColor.LightGray, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            //...bar
            DrawBox(score_left_align + ca_bar_offset_x, bar_top, bar_width, bar_height, RLColor.Gray, RLColor.Gray, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            //...coloured bars
            DrawBox(bar_middle, bar_top, bar_segment * 2, bar_height, RLColor.Gray, RLColor.Green, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            //...bar number markings (top)
            DrawText("0", bar_middle, ca_score_vert_align + ca_bar_offset_y - 1, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("+5", bar_middle + bar_segment - 1, ca_score_vert_align + ca_bar_offset_y - 1, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("+10", bar_middle + bar_segment * 2 - 2, ca_score_vert_align + ca_bar_offset_y - 1, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("+15", bar_middle + bar_segment * 3 - 2, ca_score_vert_align + ca_bar_offset_y - 1, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("-5", bar_middle - bar_segment - 1, ca_score_vert_align + ca_bar_offset_y - 1, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("-10", bar_middle - bar_segment * 2 - 2, ca_score_vert_align + ca_bar_offset_y - 1, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("-15", bar_middle - bar_segment * 3 - 2, ca_score_vert_align + ca_bar_offset_y - 1, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            //...current score
            DrawText("+10", bar_middle - 1, bar_top + bar_height, RLColor.Blue, arrayOfCells_Cards, arrayOfForeColors_Cards);
            //upper text box - messages
            DrawBox(ca_left_outer, vertical_pos, text_box_width, ca_message_height, RLColor.Yellow, RLColor.LightGray, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            //lower text box - instructions
            vertical_pos += ca_message_height + ca_spacer / 2;
            DrawBox(ca_left_outer, vertical_pos, text_box_width, ca_instruct_height, RLColor.Yellow, RLColor.White, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            DrawCenteredText("[F1] to Play a Card", ca_left_outer, vertical_pos + 2, text_box_width, RLColor.Blue, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("[SPACE] or [ENTER] to Ignore a Card", ca_left_outer, vertical_pos + 4, text_box_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("[ESC] to Auto Resolve", ca_left_outer, vertical_pos + 6, text_box_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            //Remaining Influence (top left in relation to card display)
            DrawBox(ca_left_outer, ca_top_align, ca_status_width, ca_status_height, RLColor.Yellow, RLColor.LightGray, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            DrawCenteredText("Remaining", ca_left_outer, ca_top_align + 2, ca_status_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("Influence", ca_left_outer, ca_top_align + 4, ca_status_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("0", ca_left_outer, ca_top_align + 7, ca_status_width, RLColor.Blue, arrayOfCells_Cards, arrayOfForeColors_Cards);
            //Card Pool (middle left)
            DrawBox(ca_left_outer, middle_align, ca_status_width, ca_status_height, RLColor.Yellow, RLColor.LightGray, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            DrawText("Good cards", ca_left_outer + 7, middle_align + 2, RLColor.Blue, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("Neutral cards", ca_left_outer + 7, middle_align + 4, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("Bad cards", ca_left_outer + 7, middle_align + 6, RLColor.Red, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("Total cards", ca_left_outer + 7, middle_align + 8, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("12", ca_left_outer + 24, middle_align + 2, RLColor.Blue, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("4", ca_left_outer + 24, middle_align + 4, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("8", ca_left_outer + 24, middle_align + 6, RLColor.Red, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("24", ca_left_outer + 24, middle_align + 8, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            //Remaining Cards (bottom left)
            DrawBox(ca_left_outer, vertical_align, ca_status_width, ca_status_height, RLColor.Yellow, RLColor.LightGray, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            DrawCenteredText("Remaining", ca_left_outer, vertical_align + 2, ca_status_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("Cards", ca_left_outer, vertical_align + 4, ca_status_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("0", ca_left_outer, vertical_align + 7, ca_status_width, RLColor.Red, arrayOfCells_Cards, arrayOfForeColors_Cards);
            //Situation (top right)
            DrawBox(horizontal_align, ca_top_align, ca_status_width, ca_status_height, RLColor.Yellow, RLColor.LightGray, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            DrawCenteredText("Situation", horizontal_align, ca_top_align + 2, ca_status_width,  RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("Defendable Hill (2 cards)", horizontal_align, ca_top_align + 4, ca_status_width, RLColor.Blue, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("Muddy Ground (1 card)", horizontal_align, ca_top_align + 6, ca_status_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("Army Size (3 cards)", horizontal_align, ca_top_align + 8, ca_status_width, RLColor.Red, arrayOfCells_Cards, arrayOfForeColors_Cards);
            //Secrets box (middle right)
            DrawBox(horizontal_align, middle_align, ca_status_width, ca_status_height, RLColor.Yellow, RLColor.LightGray, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            DrawCenteredText("Secrets", horizontal_align, middle_align + 2, ca_status_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("No Relevant Secrets Found", horizontal_align, middle_align + 4, ca_status_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            //Strategy Info (bottom right)
            DrawBox(horizontal_align, vertical_align, ca_status_width, ca_status_height, RLColor.Yellow, RLColor.LightGray, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            DrawCenteredText("Your Strategy", horizontal_align, vertical_align + 2, ca_status_width,  RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("All Out Assault 8/2", horizontal_align, vertical_align + 4, ca_status_width,  RLColor.Blue, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("Opponent's Strategy", horizontal_align, vertical_align + 6, ca_status_width,  RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("Hold the Ground 4/0", horizontal_align, vertical_align + 8, ca_status_width,  RLColor.Red, arrayOfCells_Cards, arrayOfForeColors_Cards);
        }

        /// <summary>
        /// Initialise Choose a Strategy Layout
        /// </summary>
        public void InitialiseStrategy()
        {
            int strategy_box_height = st_upper_box_height;
            int right_inner = st_right_outer - st_upper_box_width;
            int vertical_middle = st_top_align + st_upper_box_height + st_spacer; //y_coord of breakdown box
            int instruction_box_width = st_right_outer - st_left_outer;
            int vertical_bottom = Height - Offset_y * 2 - st_spacer - st_instruct_height; //y-coord of instruction box
            int breakdown_box_height = vertical_bottom - vertical_middle - st_spacer;
            int breakdown_box_width = st_right_outer - st_left_outer;
            //Card Pool (top left)
            DrawBox(st_left_outer, st_top_align, st_upper_box_width, st_upper_box_height, RLColor.Yellow, RLColor.LightGray, arrayOfCells_Strategy, arrayOfForeColors_Strategy, arrayOfBackColors_Strategy);
            DrawText("Good cards", st_left_outer + 7, st_top_align + 2, RLColor.Blue, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawText("Neutral cards", st_left_outer + 7, st_top_align + 4, RLColor.Black, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawText("Bad cards", st_left_outer + 7, st_top_align + 6, RLColor.Red, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawText("Total cards", st_left_outer + 7, st_top_align + 8, RLColor.Black, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawText("12", st_left_outer + 24, st_top_align + 2, RLColor.Blue, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawText("4", st_left_outer + 24, st_top_align + 4, RLColor.Black, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawText("8", st_left_outer + 24, st_top_align + 6, RLColor.Red, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawText("24", st_left_outer + 24, st_top_align + 8, RLColor.Black, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            //Situation (top right)
            DrawBox(right_inner, st_top_align, st_upper_box_width, st_upper_box_height, RLColor.Yellow, RLColor.LightGray, arrayOfCells_Strategy, arrayOfForeColors_Strategy, arrayOfBackColors_Strategy);
            DrawCenteredText("Situation", right_inner, st_top_align + 2, st_upper_box_width, RLColor.Black, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawCenteredText("Defendable Hill (2 cards)", right_inner, st_top_align + 4, st_upper_box_width, RLColor.Blue, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawCenteredText("Muddy Ground (1 card)", right_inner, st_top_align + 6, st_upper_box_width, RLColor.Black, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawCenteredText("Army Size (3 cards)", right_inner, st_top_align + 8, st_upper_box_width, RLColor.Red, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            //Choose Strategy (top middle)
            DrawBox(st_middle_align, st_top_align, st_strategy_width, strategy_box_height, RLColor.Yellow, RLColor.White, arrayOfCells_Strategy, arrayOfForeColors_Strategy, arrayOfBackColors_Strategy);
            DrawCenteredText("Choose a Strategy", st_middle_align, st_top_align + 2, st_strategy_width, RLColor.Blue, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawText("[F1] Take the Fight to the Enemy", st_middle_align + 4, st_top_align + 4, RLColor.Black, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawText("[F2] Aggressive Defensive", st_middle_align + 4, st_top_align + 6, RLColor.Black, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawText("[F1] Stand Firm", st_middle_align + 4, st_top_align + 8, RLColor.Black, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            //Breakdown of Cards box 
            DrawBox(st_left_outer, vertical_middle, breakdown_box_width, breakdown_box_height, RLColor.Yellow, RLColor.LightGray, arrayOfCells_Strategy, arrayOfForeColors_Strategy, arrayOfBackColors_Strategy);
            //Instruction box
            DrawBox(st_left_outer, vertical_bottom, instruction_box_width, st_instruct_height, RLColor.Yellow, RLColor.White, arrayOfCells_Strategy, arrayOfForeColors_Strategy, arrayOfBackColors_Strategy);
            DrawCenteredText("Choose a Strategy [F1], [F2] or [F3]", st_left_outer, vertical_bottom + 2, instruction_box_width, RLColor.Black, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawCenteredText("[ESC] to Auto Resolve", st_left_outer, vertical_bottom + 6, instruction_box_width, RLColor.Black, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
        }

        /// <summary>
        /// Initialise the Outcome Layout
        /// </summary>
        public void InitialiseOutcome()
        {
            int box_width = Width - Offset_x - ou_left_align * 2;
            int history_height = Height - Offset_y * 2 - ou_outcome_height - ou_instruct_height - ou_spacer * 4;
            int vertical_top = ou_spacer;
            int vertical_middle = vertical_top + ou_outcome_height + ou_spacer;
            int vertical_bottom = vertical_middle + history_height + ou_spacer;
            //Outcome
            DrawBox(ou_left_align, vertical_top, box_width, ou_outcome_height, RLColor.Yellow, RLColor.White, arrayOfCells_Outcome, arrayOfForeColors_Outcome, arrayOfBackColors_Outcome);
            DrawCenteredText("Outcome", ou_left_align, vertical_top + 2, box_width, RLColor.Black, arrayOfCells_Outcome, arrayOfForeColors_Outcome);
            //History
            DrawBox(ou_left_align, vertical_middle, box_width, history_height, RLColor.Yellow, RLColor.LightGray, arrayOfCells_Outcome, arrayOfForeColors_Outcome, arrayOfBackColors_Outcome);
            DrawCenteredText("History", ou_left_align, vertical_middle + 2, box_width, RLColor.Black, arrayOfCells_Outcome, arrayOfForeColors_Outcome);
            //Instruction
            DrawBox(ou_left_align, vertical_bottom, box_width, ou_instruct_height, RLColor.Yellow, RLColor.White, arrayOfCells_Outcome, arrayOfForeColors_Outcome, arrayOfBackColors_Outcome);
            DrawCenteredText("Press [SPACE] or [ENTER] to Continue", ou_left_align, vertical_bottom + 2, box_width, RLColor.Black, arrayOfCells_Outcome, arrayOfForeColors_Outcome);
        }

        /// <summary>
        /// Set up a General purpose confirmation box
        /// </summary>
        public void InitialiseConfirm()
        {
            int left_align = ( Width - Offset_x ) / 3;
            int top_align = ( Height - Offset_y * 2 ) / 3;
            int box_width = left_align;
            int box_height = top_align;
            //confirmation box (centred)
            DrawBox(left_align, top_align, box_width, box_height, RLColor.Yellow, Confirm_FillColor, arrayOfCells_Confirm, arrayOfForeColors_Confirm, arrayOfBackColors_Confirm);
        }


        /// <summary>
        /// Draw Cards Layout
        /// </summary>
        /// <param name="multiConsole"></param>
        public void DrawCards(RLConsole multiConsole)
        { Draw(multiConsole, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards); }

        /// <summary>
        /// Draw Strategy Layout
        /// </summary>
        /// <param name="multiConsole"></param>
        public void DrawStrategy(RLConsole multiConsole)
        { Draw(multiConsole, arrayOfCells_Strategy, arrayOfForeColors_Strategy, arrayOfBackColors_Strategy); }

        /// <summary>
        /// Draw Confirm Layout (internals filled and new text written)
        /// </summary>
        /// <param name="multiConsole"></param>
        public void DrawConfirm(RLConsole multiConsole, List<Snippet> listOfSnippets)
        {
            int left_align = (Width - Offset_x) / 3;
            int top_align = (Height - Offset_y * 2) / 3;
            int box_width = left_align;
            int box_height = top_align;
            //Clear box
            for (int width_index = left_align + 1; width_index < left_align + box_width - 1; width_index++)
            {
                for (int height_index = top_align + 1; height_index < top_align + box_height - 1; height_index++)
                { arrayOfBackColors_Confirm[width_index, height_index] = Confirm_FillColor; }
            }
            //Add new text
            string text;
            RLColor foreColor;
            //max four lines of text
            int limit = Math.Min(4, listOfSnippets.Count);
            for (int i = 0; i < limit; i++)
            {
                text = listOfSnippets[i].GetText();
                foreColor = listOfSnippets[i].GetForeColor();
                DrawCenteredText(text, left_align, top_align + (i + 1) * 3, box_width, foreColor, arrayOfCells_Confirm, arrayOfForeColors_Confirm);
            }
            DrawCenteredText("Press [SPACE] or [ENTER] to Continue", left_align, top_align + 28, box_width, RLColor.Black, arrayOfCells_Confirm, arrayOfForeColors_Confirm);
            //Draw
            Draw(multiConsole, arrayOfCells_Confirm, arrayOfForeColors_Confirm, arrayOfBackColors_Confirm);
        }

        /// <summary>
        /// Draw AutoResolve Layout
        /// </summary>
        /// <param name="multiConsole"></param>
        public void DrawAutoResolve(RLConsole multiConsole, List<Snippet> listOfSnippets)
        {
            int left_align = (Width - Offset_x) / 3;
            int top_align = (Height - Offset_y * 2) / 3;
            int box_width = left_align;
            int box_height = top_align;
            //Clear box
            for (int width_index = left_align + 1; width_index < left_align + box_width - 1; width_index++)
            {
                for (int height_index = top_align + 1; height_index < top_align + box_height - 1; height_index++)
                { arrayOfBackColors_Confirm[width_index, height_index] = Resolve_FillColor; }
            }
            //Add new text
            string text;
            RLColor foreColor;
            //max four lines of text
            int limit = Math.Min(4, listOfSnippets.Count);
            for (int i = 0; i < limit; i++)
            {
                text = listOfSnippets[i].GetText();
                foreColor = listOfSnippets[i].GetForeColor();
                DrawCenteredText(text, left_align, top_align + (i + 1) * 3, box_width, foreColor, arrayOfCells_Confirm, arrayOfForeColors_Confirm);
            }
            DrawCenteredText("Press [SPACE] or [ENTER] to Continue", left_align, top_align + 28, box_width, RLColor.Black, arrayOfCells_Confirm, arrayOfForeColors_Confirm);
            //Draw
            //Draw
            Draw(multiConsole, arrayOfCells_Confirm, arrayOfForeColors_Confirm, arrayOfBackColors_Confirm);
        }

        /// <summary>
        /// Draw Outcome Layout
        /// </summary>
        /// <param name="multiConsole"></param>
        public void DrawOutcome(RLConsole multiConsole)
        { Draw(multiConsole, arrayOfCells_Outcome, arrayOfForeColors_Outcome, arrayOfBackColors_Outcome); }

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
        internal void DrawBox(int coord_X, int coord_Y, int width, int height, RLColor borderColor, RLColor fillColor, int[,] arrayOfCells, RLColor[,] arrayOfForeColors, RLColor[,] arrayOfBackColors)
        {
            
            //error check input, exit on bad data
            if (coord_X < 0 || coord_X + width > Width - Offset_x)
            { Game.SetError(new Error(81, string.Format("Invalid coord_X input \"{0}\"", coord_X))); return; }
            if (coord_Y < 0 || coord_Y + height > Height - Offset_y * 2)
            { Game.SetError(new Error(81, string.Format("Invalid coord_Y input \"{0}\"", coord_Y))); return; }
            //populate array - corners
            arrayOfCells[coord_X, coord_Y] = 218; arrayOfForeColors[coord_X, coord_Y] = borderColor; //top left
            arrayOfCells[coord_X, coord_Y + height - 1] = 192; arrayOfForeColors[coord_X, coord_Y + height - 1] = borderColor; //bottom left
            arrayOfCells[coord_X + width - 1, coord_Y] = 191; arrayOfForeColors[coord_X + width - 1, coord_Y] = borderColor; //top right
            arrayOfCells[coord_X + width - 1, coord_Y + height - 1] = 217; arrayOfForeColors[coord_X + width - 1, coord_Y + height - 1] = borderColor; //bottom right
            //Top & bottom rows
            for (int i = coord_X + 1; i < coord_X + width - 1; i++)
            {
                arrayOfCells[i, coord_Y] = 196;
                arrayOfCells[i, coord_Y + height - 1] = 196;
                arrayOfForeColors[i, coord_Y] = borderColor;
                arrayOfForeColors[i, coord_Y + height - 1] = borderColor;
            }
            //left and right sides
            for (int i = coord_Y + 1; i < coord_Y + height - 1; i++)
            {
                arrayOfCells[coord_X, i] = 179;
                arrayOfCells[coord_X + width - 1, i] = 179;
                arrayOfForeColors[coord_X, i] = borderColor;
                arrayOfForeColors[coord_X + width - 1, i] = borderColor;
            }
            //fill backcolor
            for (int width_index = coord_X + 1; width_index < coord_X + width - 1; width_index++)
            {
                for (int height_index = coord_Y + 1; height_index < coord_Y + height - 1; height_index++)
                { arrayOfBackColors[width_index, height_index] = fillColor; }
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
        /// Draws text centered between two points (x1, y1 & width -> x2, y1)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="coord_X_Left"></param>
        /// <param name="coord_X_Right"></param>
        /// <param name="coord_Y"></param>
        /// <param name="foreColor"></param>
        /// <param name="arrayOfCells"></param>
        /// <param name="arrayOfForeColors"></param>
        internal void DrawCenteredText(string text, int coord_X, int coord_Y, int width, RLColor foreColor, int[,] arrayOfCells, RLColor[,] arrayOfForeColors)
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

        /// <summary>
        /// Debug method
        /// </summary>
        /// <returns></returns>
        public List<Snippet> GetTestDataConfirm()
        {
            List<Snippet> listOfSnippets = new List<Snippet>();
            listOfSnippets.Add(new Snippet("You have Chosen...", RLColor.Black, Confirm_FillColor));
            listOfSnippets.Add(new Snippet("Take the Fight to the Enemy", RLColor.Blue, Confirm_FillColor));
            listOfSnippets.Add(new Snippet("Your Opponent has Chosen...", RLColor.Black, Confirm_FillColor));
            listOfSnippets.Add(new Snippet("Aggressive Probe", RLColor.Red, Confirm_FillColor));
            return listOfSnippets;
        }

        /// <summary>
        /// Debug method
        /// </summary>
        /// <returns></returns>
        public List<Snippet> GetTestDataAutoResolve()
        {
            List<Snippet> listOfSnippets = new List<Snippet>();
            listOfSnippets.Add(new Snippet("You have chosen to AutoResolve", RLColor.Black, Confirm_FillColor));
            listOfSnippets.Add(new Snippet("Calculating...", RLColor.Blue, Confirm_FillColor));
            listOfSnippets.Add(new Snippet("Your Opponent chose...", RLColor.Black, Confirm_FillColor));
            listOfSnippets.Add(new Snippet("Aggressive Probe", RLColor.Red, Confirm_FillColor));
            return listOfSnippets;
        }

        //new methods above here
    }
}
