﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Next_Game.Event_System;
using RLNET;

namespace Next_Game
{
    /// <summary>
    /// handles specific multi console screen layouts for the conflict system
    /// </summary>
    public class Layout
    {
        static Random rnd;
        //multiConsole window
        public int Width { get; set; }
        public int Height { get; set; }
        public int Offset_x { get; set; } //offset from right hand side of screen (cells)
        public int Offset_y { get; set; } //offset from top and bottom of screen (cells)
        private RLColor backColor;
        //assorted
        public bool NextCard { get; set; } //flag indicating next card is ready to draw
        public bool CardFirstFlag { get; set; } //used to update card pool and hand on first run through UpdateCards
        public bool PopupFlag { get; set; } //flag to prevent popup overwriting itself
        public bool Challenger { get; set; } //is the Player the Challenger?
        public int InfluenceRemaining { get; set; }
        public int DefenderStrategy { get; set; } //chosen strategy of the Defender
        public CardType GameSituation { get; set; } //is the game specific situation good, bad or neutral?
        public ConflictResult Result { get; set; }
        int result_factor; //multiple for outcomes, eg. 1X is a minor win, 2X is a win, 3X is a major win
        int cardsRemaining;
        //int cardsPlayed;
        int handSize;
        int score;
        int points_play; //points if card played
        int points_ignore; //points if card ignored
        int cardCounter; //number of cards played
        int neutralEffect; //points value of neutral cards
        Card_Conflict currentCard;
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
        int ca_message_max; 
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
        int ou_width;
        //Popup layout
        int po_left_align;
        int po_top_align;
        int po_box_width;
        int po_box_height;
        //Layout Fill colors
        public RLColor Bar_FillColor { get; set; } 
        public RLColor Back_FillColor { get; set; } //for all non-highlighted boxes
        public RLColor Confirm_FillColor { get; set; }
        public RLColor Resolve_FillColor { get; set; }
        public RLColor Error_FillColor { get; set; }
        public RLColor Intro_FillColor { get; set; }
        public RLColor Outcome_FillColor { get; set; }
        //Dynamic Data Sets
        private int[] arrayCardPool { get; set; } //0 - # Good cards, 1 - # Neutral Cards, 2 - # Bad Cards, 3 - # Defender Specific
        private int[,,] arrayPoints { get; set; } //0 -> Good card Influence, 1 - Good card Ignore, 2 Bad card Influence, 3 Bad card Ignore (3 x 3 is Player vs. Opponent strategies)
        private string[,] arraySituation { get; set; } // up to 3 situation factors
        private string[] arrayStrategy { get; set; } //two lots of 3 strategies - always variations of Attack, Balanced & Defend (0/1/2 - Player, 3/4/5 - Opponent)
        private string[] arrayOutcome { get; set; } // 0 is Conflict Type, 1/2/3 are Wins (minor/normal/major), 4/5/6 are Losses, 7/8 are advantage/disadvantage and recommendation, 9 Actors, 10 No result
        private List<Snippet> listCardBreakdown; //breakdown of card pool by Your cards, opponents & situation
        private List<Card_Conflict> listCardHand; //cards in playable hand
        private List<Snippet> listHistory; //record of how the hand was played
        private Queue<Snippet> messageQueue;
        //Cards
        private int[,] arrayOfCells_Cards; //cell array for box and text
        private RLColor[,] arrayOfForeColors_Cards; //foreground color for cell contents
        private RLColor[,] arrayOfBackColors_Cards; //background color for cell's
        //Strategy
        private int[,] arrayOfCells_Strategy;
        private RLColor[,] arrayOfForeColors_Strategy;
        private RLColor[,] arrayOfBackColors_Strategy;
        //Outcome
        private int[,] arrayOfCells_Outcome;
        private RLColor[,] arrayOfForeColors_Outcome;
        private RLColor[,] arrayOfBackColors_Outcome;
        //Message
        private int[,] arrayOfCells_Message;
        private RLColor[,] arrayOfForeColors_Message;
        private RLColor[,] arrayOfBackColors_Message;
        //Introduction
        private int[,] arrayOfCells_Intro;
        private RLColor[,] arrayOfForeColors_Intro;
        private RLColor[,] arrayOfBackColors_Intro;
        //Pop-up -> used to store & restore Card layout data before & after a pop-up
        private int[,] arrayOfCells_Popup;
        private RLColor[,] arrayOfForeColors_Popup;
        private RLColor[,] arrayOfBackColors_Popup;
        //strategy
        public int Strategy_Player { get; set; } //0 -> Aggressive, 1 -> Balanced, 2 -> Defend (set in Game.SetSpecialMode)
        public int Strategy_Opponent { get; set; } //0 -> Aggressive, 1 -> Balanced, 2 -> Defend
        public int Strategy_Recommended { get; set; } //as above, used for player if autoresolve and they didn't choose


        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="fillColor"></param>
        /// <param name="borderColor"></param>
        public Layout(int seed, int width, int height, int offset_x, int offset_y, RLColor fillColor, RLColor borderColor)
        {
            rnd = new Random(seed);
            this.Width = width;
            this.Height = height;
            this.backColor = fillColor;
            this.Offset_x = offset_x;
            this.Offset_y = offset_y;
            //Popup layout (overlays Card layout)
            po_left_align = (Width - Offset_x) / 8;
            po_top_align = (Height - Offset_y * 2) / 4;
            po_box_width = (Width - Offset_x) * 6 / 8;
            po_box_height = (Height - Offset_y * 2) / 4;
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
            arrayOfCells_Message = new int[Width - Offset_x, Height - Offset_y * 2];
            arrayOfForeColors_Message = new RLColor[Width - Offset_x, Height - Offset_y * 2];
            arrayOfBackColors_Message = new RLColor[Width - Offset_x, Height - Offset_y * 2];
            arrayOfCells_Intro = new int[Width - Offset_x, Height - Offset_y * 2];
            arrayOfForeColors_Intro = new RLColor[Width - Offset_x, Height - Offset_y * 2];
            arrayOfBackColors_Intro = new RLColor[Width - Offset_x, Height - Offset_y * 2];
            arrayOfCells_Popup = new int[po_box_width, po_box_height];
            arrayOfForeColors_Popup = new RLColor[po_box_width, po_box_height];
            arrayOfBackColors_Popup = new RLColor[po_box_width, po_box_height];
            //Default data for arrays
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
                    arrayOfBackColors_Message[width_index, height_index] = fillColor;
                    arrayOfForeColors_Message[width_index, height_index] = RLColor.White;
                    arrayOfCells_Message[width_index, height_index] = 255;
                    arrayOfBackColors_Intro[width_index, height_index] = fillColor;
                    arrayOfForeColors_Intro[width_index, height_index] = RLColor.White;
                    arrayOfCells_Intro[width_index, height_index] = 255;
                }
            }
            //Dynamic Data Sets
            arrayCardPool = new int[4];
            arrayPoints = new int[3, 3, 4];
            arraySituation = new string[3, 3];
            arrayStrategy = new string[6];
            arrayOutcome = new string[11];
            listCardBreakdown = new List<Snippet>();
            listCardHand = new List<Card_Conflict>();
            listHistory = new List<Snippet>();
            messageQueue = new Queue<Snippet>();
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
            ca_message_max = 3; //number of lines of text in message box
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
            ou_outcome_height = 24;
            ou_instruct_height = 5;
            ou_width = Width - Offset_x - ou_left_align * 2;
            //Layout fillcolors
            Bar_FillColor = RLColor.Gray;
            Back_FillColor = Color._background0;
            Confirm_FillColor = RLColor.White;
            Resolve_FillColor = RLColor.White;
            Outcome_FillColor = RLColor.White;
            Error_FillColor = Color._errorFill;
            Intro_FillColor = Color._background0;
            //assorted
            score = 0;
            currentCard = null;
            PopupFlag = false;
            result_factor = Game.constant.GetValue(Global.RESULT_FACTOR);
            neutralEffect = Game.constant.GetValue(Global.NEUTRAL_EFFECT);
            handSize = Game.constant.GetValue(Global.HAND_CARDS_NUM);
        }

        /// <summary>
        /// Initialise the layouts required for the Conflict system
        /// </summary>
        public void Initialise()
        {
            //SetTestData();
            InitialiseIntro();
            InitialiseStrategy();
            InitialiseCards();
            InitialiseOutcome();
            InitialiseMessage();
        }

        /// <summary>
        /// Set up layout data ready for next challenge. Called by InitialiseConflict
        /// </summary>
        public void InitialiseData()
        {
            InfluenceRemaining = Game.constant.GetValue(Global.PLAYER_INFLUENCE);
            cardsRemaining = handSize;
            cardCounter = 0;
            NextCard = true;
            Strategy_Player = -1;
        }

        /// <summary>
        /// resets all relevant data at the end of a challenge
        /// </summary>
        internal void ResetLayout()
        {
            cardCounter = 0;
            score = 0;
            points_play = 0;
            points_ignore = 0;
            listCardHand.Clear();
            listHistory.Clear();
            messageQueue.Clear();
            currentCard = null;
        }

        /// <summary>
        /// Initialise Introduction Layout
        /// </summary>
        public void InitialiseIntro()
        {
            int left_align = (Width - Offset_x) / 8;
            int vertical_segment = (Height - Offset_y * 2) / 8;
            int top_align = vertical_segment * 2;
            int bottom_align = vertical_segment * 5;
            int box_width = (Width - Offset_x) * 6 / 8;
            int box_height = vertical_segment * 2;
            //confirmation box (centred)
            DrawBox(left_align, top_align, box_width, box_height, RLColor.Yellow, Intro_FillColor, arrayOfCells_Intro, arrayOfForeColors_Intro, arrayOfBackColors_Intro);
            DrawBox(left_align, bottom_align, box_width, box_height, RLColor.Yellow, Intro_FillColor, arrayOfCells_Intro, arrayOfForeColors_Intro, arrayOfBackColors_Intro);
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
            DrawBox(st_left_outer, st_top_align, st_upper_box_width, st_upper_box_height, RLColor.Yellow, Back_FillColor, arrayOfCells_Strategy, arrayOfForeColors_Strategy, arrayOfBackColors_Strategy);
            DrawText("Good cards", st_left_outer + 7, st_top_align + 2, RLColor.Blue, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawText("Neutral cards", st_left_outer + 7, st_top_align + 4, RLColor.Magenta, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawText("Bad cards", st_left_outer + 7, st_top_align + 6, RLColor.Red, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawText("Total cards", st_left_outer + 7, st_top_align + 8, RLColor.Black, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            //Situation (top right)
            DrawBox(right_inner, st_top_align, st_upper_box_width, st_upper_box_height, RLColor.Yellow, Back_FillColor, arrayOfCells_Strategy, arrayOfForeColors_Strategy, arrayOfBackColors_Strategy);
            //Choose Strategy (top middle)
            DrawBox(st_middle_align, st_top_align, st_strategy_width, strategy_box_height, RLColor.Yellow, RLColor.White, arrayOfCells_Strategy, arrayOfForeColors_Strategy, arrayOfBackColors_Strategy);
            //Breakdown of Cards box 
            DrawBox(st_left_outer, vertical_middle, breakdown_box_width, breakdown_box_height, RLColor.Yellow, Back_FillColor, arrayOfCells_Strategy, arrayOfForeColors_Strategy, arrayOfBackColors_Strategy);
            //Instruction box
            DrawBox(st_left_outer, vertical_bottom, instruction_box_width, st_instruct_height, RLColor.Yellow, RLColor.White, arrayOfCells_Strategy, arrayOfForeColors_Strategy, arrayOfBackColors_Strategy);
            DrawCenteredText("Choose a Strategy [F1], [F2] or [F3]", st_left_outer, vertical_bottom + 2, instruction_box_width, RLColor.Black, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawCenteredText("[ESC] to Auto Resolve", st_left_outer, vertical_bottom + 6, instruction_box_width, RLColor.Black, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
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
            int bar_segment = Convert.ToInt32((float)(bar_width / 8)); //scoring marker segments on bar (4 either side of the zero mid point)
            int bar_top = ca_score_vert_align + ca_bar_offset_y; //y_coord of bar
            int bar_height = ca_score_height - (ca_bar_offset_y * 2);
            //Card
            DrawBox(ca_left_inner, ca_top_align, ca_card_width, ca_card_height, RLColor.Yellow, RLColor.White, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            //Score track
            DrawBox(score_left_align, ca_score_vert_align, score_width, ca_score_height, RLColor.Yellow, Back_FillColor, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            //...bar
            DrawBox(score_left_align + ca_bar_offset_x, bar_top, bar_width, bar_height, Bar_FillColor, Bar_FillColor, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            //...bar number markings (top)
            DrawText("0", bar_middle, ca_score_vert_align + ca_bar_offset_y - 1, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText(string.Format("+{0}", result_factor), bar_middle + bar_segment - 1, ca_score_vert_align + ca_bar_offset_y - 1, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText(string.Format("+{0}", result_factor * 2), bar_middle + bar_segment * 2 - 2, ca_score_vert_align + ca_bar_offset_y - 1, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText(string.Format("+{0}", result_factor * 3), bar_middle + bar_segment * 3 - 2, ca_score_vert_align + ca_bar_offset_y - 1, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText(string.Format("-{0}", result_factor), bar_middle - bar_segment - 1, ca_score_vert_align + ca_bar_offset_y - 1, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText(string.Format("-{0}", result_factor * 2), bar_middle - bar_segment * 2 - 2, ca_score_vert_align + ca_bar_offset_y - 1, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText(string.Format("-{0}", result_factor * 3), bar_middle - bar_segment * 3 - 2, ca_score_vert_align + ca_bar_offset_y - 1, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            
            //upper text box - messages
            DrawBox(ca_left_outer, vertical_pos, text_box_width, ca_message_height, RLColor.Yellow, Back_FillColor, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            //lower text box - instructions
            vertical_pos += ca_message_height + ca_spacer / 2;
            DrawBox(ca_left_outer, vertical_pos, text_box_width, ca_instruct_height, RLColor.Yellow, RLColor.White, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            DrawCenteredText("[F1] to Play a Card, [SPACE] or [ENTER] to Ignore a Card", ca_left_outer, vertical_pos + 2, text_box_width, RLColor.Blue, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("[F2] to see Outcomes", ca_left_outer, vertical_pos + 4, text_box_width, RLColor.Gray, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("[ESC] to Auto Resolve", ca_left_outer, vertical_pos + 6, text_box_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            //Remaining Influence (top left in relation to card display)
            DrawBox(ca_left_outer, ca_top_align, ca_status_width, ca_status_height, RLColor.Yellow, Back_FillColor, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            DrawCenteredText("Remaining", ca_left_outer, ca_top_align + 2, ca_status_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("Influence", ca_left_outer, ca_top_align + 4, ca_status_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            
            //Card Pool (middle left)
            DrawBox(ca_left_outer, middle_align, ca_status_width, ca_status_height, RLColor.Yellow, Back_FillColor, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            DrawText("Good cards", ca_left_outer + 7, middle_align + 2, RLColor.Blue, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("Neutral cards", ca_left_outer + 7, middle_align + 4, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("Bad cards", ca_left_outer + 7, middle_align + 6, RLColor.Red, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("Total cards", ca_left_outer + 7, middle_align + 8, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            //Remaining Cards (bottom left)
            DrawBox(ca_left_outer, vertical_align, ca_status_width, ca_status_height, RLColor.Yellow, Back_FillColor, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            DrawCenteredText("Remaining", ca_left_outer, vertical_align + 2, ca_status_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("Cards", ca_left_outer, vertical_align + 4, ca_status_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            //Situation (top right)
            DrawBox(horizontal_align, ca_top_align, ca_status_width, ca_status_height, RLColor.Yellow, Back_FillColor, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            //Secrets box (middle right)
            DrawBox(horizontal_align, middle_align, ca_status_width, ca_status_height, RLColor.Yellow, Back_FillColor, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            //Strategy Info (bottom right)
            DrawBox(horizontal_align, vertical_align, ca_status_width, ca_status_height, RLColor.Yellow, Back_FillColor, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
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
            DrawBox(ou_left_align, vertical_middle, box_width, history_height, RLColor.Yellow, Back_FillColor, arrayOfCells_Outcome, arrayOfForeColors_Outcome, arrayOfBackColors_Outcome);
            DrawCenteredText("History", ou_left_align, vertical_middle + 2, box_width, RLColor.Black, arrayOfCells_Outcome, arrayOfForeColors_Outcome);
            //Instruction
            DrawBox(ou_left_align, vertical_bottom, box_width, ou_instruct_height, RLColor.Yellow, Back_FillColor, arrayOfCells_Outcome, arrayOfForeColors_Outcome, arrayOfBackColors_Outcome);
            DrawCenteredText("Press [SPACE] or [ENTER] to Continue", ou_left_align, vertical_bottom + 2, box_width, RLColor.Gray, arrayOfCells_Outcome, arrayOfForeColors_Outcome);
        }

        /// <summary>
        /// Set up a General purpose Message box
        /// </summary>
        public void InitialiseMessage()
        {
            int left_align = ( Width - Offset_x ) / 3;
            int top_align = ( Height - Offset_y * 2 ) / 3;
            int box_width = left_align;
            int box_height = top_align + 2;
            //confirmation box (centred)
            DrawBox(left_align, top_align, box_width, box_height, RLColor.Yellow, Confirm_FillColor, arrayOfCells_Message, arrayOfForeColors_Message, arrayOfBackColors_Message);
        }

        /// <summary>
        /// Draw Introduction Layout
        /// </summary>
        /// <param name="multiConsole"></param>
        public void DrawIntro(RLConsole multiConsole)
        { Draw(multiConsole, arrayOfCells_Intro, arrayOfForeColors_Intro, arrayOfBackColors_Intro); }

        /// <summary>
        /// Update Introduction layout contents
        /// </summary>
        public void UpdateIntro()
        {
            int left_align = (Width - Offset_x) / 8;
            int vertical_segment = (Height - Offset_y * 2) / 8;
            int top_align = vertical_segment * 2;
            int bottom_align = vertical_segment * 5;
            int box_width = (Width - Offset_x) * 6 / 8;
            int box_height = vertical_segment * 2;
            //Clear top box & change background color
            for (int width_index = left_align + 1; width_index < left_align + box_width - 1; width_index++)
            {
                for (int height_index = top_align + 1; height_index < top_align + box_height - 1; height_index++)
                { arrayOfCells_Intro[width_index, height_index] = 255; }
            }
            //Add new text -> Type of Conflict
            DrawCenteredText(arrayOutcome[0], left_align, top_align + 3, box_width, RLColor.Blue, arrayOfCells_Intro, arrayOfForeColors_Intro);
            //protagonists -> Word Wrap
            List<string> tempList = new List<string>();
            tempList.AddRange(Game.utility.WordWrap(arrayOutcome[9], box_width - 4));
            for (int i = 0; i < tempList.Count(); i++)
            { DrawCenteredText(tempList[i], left_align, top_align + 6 + i, box_width, RLColor.Black, arrayOfCells_Intro, arrayOfForeColors_Intro); }
            //who has the advantage
            RLColor foreColor = RLColor.Green;
            if (Challenger == false) { foreColor = RLColor.Red; }
            DrawCenteredText(arrayOutcome[7], left_align, top_align + 10, box_width, foreColor, arrayOfCells_Intro, arrayOfForeColors_Intro);
            //recommendation
            DrawCenteredText(arrayOutcome[8], left_align, top_align + 13, box_width, RLColor.Black, arrayOfCells_Intro, arrayOfForeColors_Intro);
            //clear bottom box
            for (int width_index = left_align + 1; width_index < left_align + box_width - 1; width_index++)
            {
                for (int height_index = bottom_align + 1; height_index < bottom_align + box_height - 1; height_index++)
                {
                    arrayOfCells_Intro[width_index, height_index] = 255;
                    //arrayOfBackColors_Intro[width_index, height_index] = Intro_FillColor;
                }
            }
            //outcomes
            DrawCenteredText(string.Format("Win Outcomes (+{0}/+{1}/+{2})", result_factor, result_factor * 2, result_factor * 3), left_align + 3, bottom_align + 2, box_width, 
                RLColor.Blue, arrayOfCells_Intro, arrayOfForeColors_Intro);
            DrawText("Minor Win:  " + arrayOutcome[1], left_align + 3, bottom_align + 4, RLColor.Black, arrayOfCells_Intro, arrayOfForeColors_Intro);
            DrawText("Win:        " + arrayOutcome[2], left_align + 3, bottom_align + 6, RLColor.Black, arrayOfCells_Intro, arrayOfForeColors_Intro);
            DrawText("Major Win:  " + arrayOutcome[3], left_align + 3, bottom_align + 8, RLColor.Black, arrayOfCells_Intro, arrayOfForeColors_Intro);
            DrawCenteredText(string.Format("Lose Outcomes (-{0}/-{1}/-{2})", result_factor, result_factor * 2, result_factor * 3), left_align + 3, bottom_align + 12, box_width, 
                RLColor.Red, arrayOfCells_Intro, arrayOfForeColors_Intro);
            DrawText("Minor Loss: " + arrayOutcome[4], left_align + 3, bottom_align + 14, RLColor.Black, arrayOfCells_Intro, arrayOfForeColors_Intro);
            DrawText("Loss:       " + arrayOutcome[5], left_align + 3, bottom_align + 16, RLColor.Black, arrayOfCells_Intro, arrayOfForeColors_Intro);
            DrawText("Major Loss: " + arrayOutcome[6], left_align + 3, bottom_align + 18, RLColor.Black, arrayOfCells_Intro, arrayOfForeColors_Intro);
            //top box instructions
            DrawCenteredText("Press [SPACE] or [ENTER] to Continue", left_align, top_align + box_height - 3, box_width, RLColor.Gray, arrayOfCells_Intro, arrayOfForeColors_Intro);
            //set flag to Update Card pool and deal hand on first run through of each conflict
            CardFirstFlag = true;
        }

        /// <summary>
        /// Update Outcome layout contents
        /// </summary>
        public void UpdateOutcome()
        {
            int box_width = Width - Offset_x - ou_left_align * 2;
            int history_height = Height - Offset_y * 2 - ou_outcome_height - ou_instruct_height - ou_spacer * 4;
            int vertical_top = ou_spacer;
            int vertical_middle = vertical_top + ou_outcome_height + ou_spacer;
            int vertical_bottom = vertical_middle + history_height + ou_spacer;
            List<Snippet> listOutcome = new List<Snippet>();
            //outcome
            string textDescription = arrayOutcome[10]; //default 'no result'
            string textOutcome = "No Result";
            int resultOutcome = 0; // 1/2/3 for ascending level of wins and 4/5/6 for ascending level of losses. '0' for no result
            Result = ConflictResult.None;
            RLColor forecolor = RLColor.Black;
            //determine level of win or loss
            if (score > 0)
            {
                if (score >= result_factor) { resultOutcome = 1; textOutcome = "A Minor Win"; Result = ConflictResult.MinorWin; forecolor = RLColor.Green; }
                if (score >= result_factor * 2) { resultOutcome = 2; textOutcome = "A Win"; Result = ConflictResult.Win; forecolor = RLColor.Green; }
                if (score >= result_factor * 3) { resultOutcome = 3; textOutcome = "A Major Win"; Result = ConflictResult.MajorWin; forecolor = RLColor.Green; }
            }
            else if (score < 0)
            {
                if (score <= -1 * result_factor) { resultOutcome = 4; textOutcome = "A Minor Loss"; Result = ConflictResult.MinorLoss; forecolor = RLColor.Red; }
                if (score <= -1 * result_factor * 2) { resultOutcome = 5; textOutcome = "A Loss"; Result = ConflictResult.Loss; forecolor = RLColor.Red; }
                if (score <= -1 * result_factor * 3) { resultOutcome = 6; textOutcome = "A Major Loss"; Result = ConflictResult.MajorLoss; forecolor = RLColor.Red; }
            }
            //text version of win/loss outcome
            if (resultOutcome != 0) { textDescription = arrayOutcome[resultOutcome]; }
            //message
            string protagonistText = Game.conflict.GetProtagonists();
            string tempText = string.Format("{0}, a {1} Conflict. {2}, score of {3}{4}", protagonistText, Game.conflict.Conflict_Type, textOutcome,
                score > 0 ? "+" : "", score);
            Message message = new Message(tempText, MessageType.Conflict);
            Game.world.SetMessage(message);
            //record
            Active player = Game.world.GetActiveActor(1);
            if (player != null)
            {
                //int refID = Game.world.ConvertLocToRef(player.LocID);
                Game.world.SetPlayerRecord(new Record(tempText, player.ActID, player.LocID, CurrentActorEvent.Challenge));
            }
            //clear out outcome box
            for (int width_index = ou_left_align + 1; width_index < ou_left_align + ou_width - 1; width_index++)
            {
                for (int height_index = vertical_top + 1; height_index < vertical_top + ou_outcome_height - 1; height_index++)
                { arrayOfCells_Outcome[width_index, height_index] = 255; }
            }
            //fill list to display in outcome box, top
            listOutcome.Add(new Snippet("Outcome", RLColor.Blue, Outcome_FillColor));
            //...protagonists -> Word Wrap
            List<string> tempList = new List<string>();
            tempList.AddRange(Game.utility.WordWrap(arrayOutcome[9], box_width - 4));
            for (int i = 0; i < tempList.Count(); i++)
            { listOutcome.Add(new Snippet(tempList[i], RLColor.Magenta, Outcome_FillColor)); }
            listOutcome.Add(new Snippet(arrayOutcome[0], RLColor.Black, Outcome_FillColor));
            listOutcome.Add(new Snippet(string.Format("Final Score {0}{1}", score > 0 ? "+" : "", score), RLColor.Black, Outcome_FillColor));
            listOutcome.Add(new Snippet(textOutcome, RLColor.Magenta, Outcome_FillColor));
            listOutcome.Add(new Snippet(textDescription, RLColor.Black, Outcome_FillColor));
            //Results
            List<Snippet> resultList = Game.conflict.ResolveResults(Result);
            if (resultList.Count > 0)
            { listOutcome.AddRange(resultList); }
            //...display outcome box text
            for(int i = 0; i < listOutcome.Count(); i++)
            { DrawCenteredText(listOutcome[i].GetText(), ou_left_align, vertical_top + 2 + i * 2, ou_width, listOutcome[i].GetForeColor(), arrayOfCells_Outcome, arrayOfForeColors_Outcome); }
            //history
            for (int width_index = ou_left_align + 1; width_index < ou_left_align + ou_width - 1; width_index++)
            {
                for (int height_index = vertical_middle + 1; height_index < vertical_middle + history_height - 1; height_index++)
                { arrayOfCells_Outcome[width_index, height_index] = 255; }
            }
            DrawCenteredText("Hand History", ou_left_align, vertical_middle + 2, ou_width, RLColor.Blue, arrayOfCells_Outcome, arrayOfForeColors_Outcome);
            for (int i = 0; i < listHistory.Count; i++)
            { DrawText(listHistory[i].GetText(), ou_left_align + 4, vertical_middle + 5 + i * 2, listHistory[i].GetForeColor(), arrayOfCells_Outcome, arrayOfForeColors_Outcome); }
        }

        /// <summary>
        /// Draw Cards Layout
        /// </summary>
        /// <param name="multiConsole"></param>
        public void DrawCards(RLConsole multiConsole)
        {
            Draw(multiConsole, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            NextCard = false;
        }

        /// <summary>
        /// Update Card Layout contents (assumes that all card data sets are up to data)
        /// </summary>
        public void UpdateCards()
        {
            int card_vert_1 = ca_top_align + 2; //type of card, eg. 'skill card', y_coord
            int card_vert_2 = ca_top_align + 4; //spacer
            int card_vert_3 = ca_top_align + ca_card_height / 4; //eg. "King's Leadship" + "Immersion text"
            int card_vert_4 = ca_top_align + ca_card_height / 4 + 3; //Unique text
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
            int bar_width = score_width - (ca_bar_offset_x * 2); //scoring colored bar
            int bar_middle = score_left_align + ca_bar_offset_x + (bar_width / 2); //x_coord of mid point
            int bar_top = ca_score_vert_align + ca_bar_offset_y; //y_coord of bar
            int bar_height = ca_score_height - (ca_bar_offset_y * 2);
            RLColor foreColor;
            string textPlay, textIgnore;
            //Update Card Pool and Deal Hand if first run through
            if (CardFirstFlag == true)
            {
                //is the defender using a Defence [F3] strategy?
                if (Challenger == true) { DefenderStrategy = Strategy_Opponent; }
                else { DefenderStrategy = Strategy_Player; }
                Game.conflict.UpdateCardPool(DefenderStrategy);
                Game.conflict.SetHand();
                CardFirstFlag = false;
            }
            //Situation -> Defender Advantage at top, Neutral middle, Game Specific bottom
            for (int width_index = horizontal_align + 1; width_index < horizontal_align + ca_status_width - 1; width_index++)
            {
                for (int height_index = ca_top_align + 1; height_index < ca_top_align + ca_status_height - 1; height_index++)
                { arrayOfCells_Cards[width_index, height_index] = 255; }
            }
            DrawCenteredText("Situation", horizontal_align, ca_top_align + 2, ca_status_width, RLColor.Blue, arrayOfCells_Cards, arrayOfForeColors_Cards);
            if (DefenderStrategy != 2) { foreColor = RLColor.LightGray; } else { foreColor = RLColor.Black; }
            DrawCenteredText(arraySituation[0, 0], horizontal_align, ca_top_align + 4, ca_status_width, foreColor, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText(arraySituation[1, 0], horizontal_align, ca_top_align + 6, ca_status_width, RLColor.Magenta, arrayOfCells_Cards, arrayOfForeColors_Cards);
            foreColor = RLColor.Green;
            if (GameSituation == CardType.Bad) { foreColor = RLColor.Red; } else if (GameSituation == CardType.Neutral) { foreColor = RLColor.Magenta; }
            DrawCenteredText(arraySituation[2, 0], horizontal_align, ca_top_align + 8, ca_status_width, foreColor, arrayOfCells_Cards, arrayOfForeColors_Cards);
            //Secrets box (middle right)
            for (int width_index = horizontal_align + 1; width_index < horizontal_align + ca_status_width - 1; width_index++)
            {
                for (int height_index = middle_align + 1; height_index < middle_align + ca_status_height - 1; height_index++)
                { arrayOfCells_Cards[width_index, height_index] = 255; }
            }
            DrawCenteredText("Secrets", horizontal_align, middle_align + 2, ca_status_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("No Relevant Secrets Found", horizontal_align, middle_align + 4, ca_status_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            //Strategy
            for (int width_index = horizontal_align + 1; width_index < horizontal_align + ca_status_width - 1; width_index++)
            {
                for (int height_index = vertical_align + 1; height_index < vertical_align + ca_status_height - 1; height_index++)
                { arrayOfCells_Cards[width_index, height_index] = 255; }
            }
            DrawCenteredText("Your Strategy", horizontal_align, vertical_align + 2, ca_status_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText(string.Format("{0} [F{1}]", arrayStrategy[Strategy_Player], Strategy_Player + 1), horizontal_align, vertical_align + 4, ca_status_width, 
                RLColor.Blue, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText("Opponent's Strategy", horizontal_align, vertical_align + 6, ca_status_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText(string.Format("{0} [F{1}]", arrayStrategy[Strategy_Opponent + 3], Strategy_Opponent + 1), horizontal_align, vertical_align + 8, ca_status_width, 
                RLColor.Red, arrayOfCells_Cards, arrayOfForeColors_Cards);
            //Message
            for (int width_index = ca_left_outer + 1; width_index < ca_left_outer + text_box_width - 1; width_index++)
            {
                for (int height_index = vertical_pos + 1; height_index < vertical_pos + ca_message_height - 1; height_index++)
                { arrayOfCells_Cards[width_index, height_index] = 255; }
            }
            Snippet[] messageArray = messageQueue.ToArray();
            for (int i = 0; i < messageQueue.Count(); i++)
            { DrawCenteredText(messageArray[i].GetText(), ca_left_outer + 4, vertical_pos + 3 + i * 2, text_box_width, messageArray[i].GetForeColor(), arrayOfCells_Cards, arrayOfForeColors_Cards); }
            //Card
            points_play = 0;
            points_ignore = 0;
            if (listCardHand.Count > 0)
            {
                //get next card, delete once done
                Card_Conflict card = listCardHand[0];
                currentCard = listCardHand[0];
                RLColor backColor = RLColor.White;
                string textType = "Unknown";
                switch (card.Type)
                {
                    case CardType.Good:
                        backColor = Color._goodCard;
                        textType = "Advantage";
                        arrayCardPool[0]--;
                        points_play = arrayPoints[Strategy_Player, Strategy_Opponent, 0] * card.Effect;
                        points_ignore = arrayPoints[Strategy_Player, Strategy_Opponent, 1] * card.Effect;
                        break;
                    case CardType.Neutral:
                        backColor = Color._neutralCard;
                        textType = "Could Go Either Way";
                        int rndNum = rnd.Next(1, neutralEffect + 1) + rnd.Next(1, neutralEffect + 1); //2d6
                        points_play = rndNum; points_ignore = rndNum * -1; 
                        arrayCardPool[1]--;
                        break;
                    case CardType.Bad:
                        backColor = Color._badCard;
                        textType = "Disadvantage";
                        arrayCardPool[2]--;
                        points_play = arrayPoints[Strategy_Player, Strategy_Opponent, 2] * card.Effect;
                        points_ignore = arrayPoints[Strategy_Player, Strategy_Opponent,3] * card.Effect;
                        break;
                    default:
                        Game.SetError(new Error(95, "Invalid Card Type"));
                        break;
                }
                //blank out previous text and redraw new card background
                for (int width_index = ca_left_inner + 1; width_index < ca_left_inner + ca_card_width - 1; width_index++)
                {
                    for (int height_index = ca_top_align + 1; height_index < ca_top_align + ca_card_height - 1; height_index++)
                    {
                        arrayOfCells_Cards[width_index, height_index] = 255;
                        arrayOfBackColors_Cards[width_index, height_index] = backColor;
                    }
                }
                //card text
                DrawCenteredText(string.Format("{0} Card", card.Conflict_Type), ca_left_inner, card_vert_1, ca_card_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
                DrawCenteredText("--- 0 ---", ca_left_inner, card_vert_2, ca_card_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
                //...word wrap title & description
                List<string> tempList = new List<string>();
                tempList.AddRange(Game.utility.WordWrap(card.Title, ca_card_width - 4));
                tempList.Add("");
                tempList.AddRange(Game.utility.WordWrap(card.Description, ca_card_width - 4));
                for (int i = 0; i < tempList.Count; i++)
                { DrawCenteredText(tempList[i], ca_left_inner, card_vert_3 + i * 2, ca_card_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards); }
                //...unique ability
                if (card.Unique > CardUnique.None)
                { DrawCenteredText(card.UniqueText, ca_left_inner, card_vert_4 + 8, ca_card_width, RLColor.Yellow, arrayOfCells_Cards, arrayOfForeColors_Cards); }
                //...points and good/bad card
                DrawCenteredText(textType, ca_left_inner, card_vert_5, ca_card_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
                if (card.Type != CardType.Neutral)
                {
                    textPlay = string.Format("Play for{0}{1} Points", points_play > 0 ? " +" : " ", points_play);
                    textIgnore = string.Format("Ignore for{0}{1} Points", points_ignore > 0 ? " +" : " ", points_ignore);
                }
                else { textPlay = string.Format("Play for +2d{0} Points", neutralEffect);  textIgnore = string.Format("Ignore for -2d{0} Points", neutralEffect); }
                DrawCenteredText(textPlay, ca_left_inner, card_vert_6, ca_card_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
                DrawCenteredText(textIgnore, ca_left_inner, card_vert_7, ca_card_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
                //delete card
                listCardHand.RemoveAt(0);
                //update remaining cards
                DrawCenteredText(Convert.ToString(cardsRemaining), ca_left_outer, vertical_align + 7, ca_status_width, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
                cardsRemaining--;
                cardsRemaining = Math.Max(0, cardsRemaining);
                //update remaining influence
                DrawCenteredText(Convert.ToString(InfluenceRemaining), ca_left_outer, ca_top_align + 7, ca_status_width, RLColor.Blue, arrayOfCells_Cards, arrayOfForeColors_Cards);
            }
            else { Game.SetError(new Error(95, "No Cards Remaining in listCardHand")); }
            //Card Pool
            for (int width_index = ca_left_outer + 24; width_index < ca_left_outer + 28; width_index++)
            {
                for (int height_index = middle_align + 2; height_index < middle_align + ca_status_height - 1; height_index++)
                { arrayOfCells_Cards[width_index, height_index] = 255; }
            }
            DrawText(Convert.ToString(arrayCardPool[0]), ca_left_outer + 24, middle_align + 2, RLColor.Blue, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText(Convert.ToString(arrayCardPool[1]), ca_left_outer + 24, middle_align + 4, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText(Convert.ToString(arrayCardPool[2]), ca_left_outer + 24, middle_align + 6, RLColor.Red, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText(Convert.ToString(arrayCardPool.Sum()), ca_left_outer + 24, middle_align + 8, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            //Score Bar
            for (int width_index = score_left_align + ca_bar_offset_x; width_index < score_left_align + ca_bar_offset_x + bar_width; width_index++)
            {
                for (int height_index = bar_top; height_index < bar_top + bar_height + 1; height_index++)
                { arrayOfCells_Cards[width_index, height_index] = 255; }
            }
            //...bar
            DrawBox(score_left_align + ca_bar_offset_x, bar_top, bar_width, bar_height, Bar_FillColor, Bar_FillColor, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            //...coloured bars
            int tempScore = Math.Min(20, score);
            tempScore = Math.Max(-20, tempScore);
            int bar_length = Math.Abs(Convert.ToInt32((float)(bar_width / 2 * tempScore / 20))); //scoring marker segments on bar (max display of bar is +/- 20 points)
            if (score > 0)
            { DrawBox(bar_middle, bar_top, bar_length, bar_height, RLColor.Green, RLColor.Green, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards); }
            else if (score < 0)
            { DrawBox(bar_middle - bar_length, bar_top, bar_length, bar_height, RLColor.Red, RLColor.Red, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards); }
            //...current score
            if (score != 0)
            { DrawText(string.Format("{0}{1}", score > 0 ? "+" : "", score), bar_middle - 1, bar_top + bar_height, RLColor.Blue, arrayOfCells_Cards, arrayOfForeColors_Cards); }
            else { DrawText(string.Format(" {0}", score), bar_middle - 1, bar_top + bar_height, RLColor.Blue, arrayOfCells_Cards, arrayOfForeColors_Cards); }
        }

        /// <summary>
        /// handles autoresolution of challenges, either at the start or part way through
        /// </summary>
        public void HandAutoResolve()
        {
            //Player strategy
            if (Strategy_Player < 0) { Strategy_Player = Strategy_Recommended; }
            //Update Card Pool and Deal Hand if first run through
            if (CardFirstFlag == true)
            {
                //is the defender using a Defence [F3] strategy?
                if (Challenger == true) { DefenderStrategy = Strategy_Opponent; }
                else { DefenderStrategy = Strategy_Player; }
                Game.conflict.UpdateCardPool(DefenderStrategy);
                Game.conflict.SetHand();
                CardFirstFlag = false;
            }
            cardsRemaining--;
            if (currentCard != null)
            {
                //handles autoExec part way through hand
                listCardHand.Insert(0, currentCard);
                cardsRemaining++;
            } 
            for (int i = cardCounter; i <= handSize; i++)
            {
                //Card
                points_play = 0;
                points_ignore = 0;
                if (listCardHand.Count > 0)
                {
                    //get next card, delete once done
                    Card_Conflict card = listCardHand[0];
                    currentCard = listCardHand[0];
                    switch (card.Type)
                    {
                        case CardType.Good:
                            points_play = arrayPoints[Strategy_Player, Strategy_Opponent, 0] * card.Effect;
                            points_ignore = arrayPoints[Strategy_Player, Strategy_Opponent, 1] * card.Effect;
                            break;
                        case CardType.Neutral:
                            int rndNum = rnd.Next(1, neutralEffect + 1) + rnd.Next(1, neutralEffect + 1); //2d6
                            points_play = rndNum; points_ignore = rndNum * -1;
                            break;
                        case CardType.Bad:
                            points_play = arrayPoints[Strategy_Player, Strategy_Opponent, 2] * card.Effect;
                            points_ignore = arrayPoints[Strategy_Player, Strategy_Opponent, 3] * card.Effect;
                            break;
                        default:
                            Game.SetError(new Error(95, "Invalid Card Type"));
                            break;
                    }
                    //resolve card
                    ResolveCard();
                    listCardHand.RemoveAt(0);
                    cardsRemaining--;
                }
            }
        }

        /// <summary>
        /// Update card layout to show an overlaid popup, call DrawCard() to display Popup
        /// </summary>
        public void UpdatePopup()
        {
            //copy relevant section of card arrays to Popup arrays
            int width = 0;
            int height = 0;
            for(int width_index = po_left_align; width_index < po_left_align + po_box_width; width_index++)
            {
                for(int height_index = po_top_align; height_index < po_top_align + po_box_height; height_index++)
                {
                    //copy data
                    arrayOfCells_Popup[width, height] = arrayOfCells_Cards[width_index, height_index];
                    arrayOfForeColors_Popup[width, height] = arrayOfForeColors_Cards[width_index, height_index];
                    arrayOfBackColors_Popup[width, height] = arrayOfBackColors_Cards[width_index, height_index];
                    //blank card cellarray
                    arrayOfCells_Cards[width_index, height_index] = 255;
                    height++;
                }
                width++; height = 0;
            }
            //draw popUp box in card layout
            DrawBox(po_left_align, po_top_align, po_box_width, po_box_height, RLColor.Magenta, Color._popup, arrayOfCells_Cards, arrayOfForeColors_Cards, arrayOfBackColors_Cards);
            //populate with relevant data
            DrawCenteredText(string.Format("Win Outcomes (+{0}/+{1}/+{2})", result_factor, result_factor * 2, result_factor * 3), po_left_align + 3, po_top_align + 2, po_box_width, 
                RLColor.Blue, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("Minor Win:  " + arrayOutcome[1], po_left_align + 3, po_top_align + 4, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("Win:        " + arrayOutcome[2], po_left_align + 3, po_top_align + 6, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("Major Win:  " + arrayOutcome[3], po_left_align + 3, po_top_align + 8, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawCenteredText(string.Format("Lose Outcomes (-{0}/-{1}/-{2})", result_factor, result_factor * 2, result_factor * 3), po_left_align + 3, po_top_align + 10, po_box_width, 
                RLColor.Red, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("Minor Loss: " + arrayOutcome[4], po_left_align + 3, po_top_align + 12, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("Loss:       " + arrayOutcome[5], po_left_align + 3, po_top_align + 14, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            DrawText("Major Loss: " + arrayOutcome[6], po_left_align + 3, po_top_align + 16, RLColor.Black, arrayOfCells_Cards, arrayOfForeColors_Cards);
            //top box instructions
            DrawCenteredText("Press [SPACE] or [ENTER] to Continue", po_left_align, po_top_align + po_box_height - 3, po_box_width, RLColor.Gray, arrayOfCells_Cards, arrayOfForeColors_Cards);
            PopupFlag = true;
        }

        /// <summary>
        /// Removes Popup by restoring original card layout from data within Popup arrays. Run DrawCard afterwards
        /// </summary>
        public void RemovePopup()
        {
            int width = 0;
            int height = 0;
            for (int width_index = po_left_align; width_index < po_left_align + po_box_width; width_index++)
            {
                for (int height_index = po_top_align; height_index < po_top_align + po_box_height; height_index++)
                {
                    //blank card cellarray
                    arrayOfCells_Cards[width_index, height_index] = 255;
                    //swap data
                    arrayOfCells_Cards[width_index, height_index] = arrayOfCells_Popup[width, height];
                    arrayOfForeColors_Cards[width_index, height_index] = arrayOfForeColors_Popup[width, height];
                    arrayOfBackColors_Cards[width_index, height_index] = arrayOfBackColors_Popup[width, height];
                    height++;
                }
                width++; height = 0;
            }
            //clear popup arrays
            Array.Clear(arrayOfCells_Popup, 0, arrayOfCells_Popup.Length);
            Array.Clear(arrayOfForeColors_Popup, 0, arrayOfForeColors_Popup.Length);
            Array.Clear(arrayOfBackColors_Popup, 0, arrayOfBackColors_Popup.Length);
            PopupFlag = false;
        }

        /// <summary>
        /// Draw Strategy Layout
        /// </summary>
        /// <param name="multiConsole"></param>
        public void DrawStrategy(RLConsole multiConsole)
        { Draw(multiConsole, arrayOfCells_Strategy, arrayOfForeColors_Strategy, arrayOfBackColors_Strategy); }

        /// <summary>
        /// Update Strategy Layout contents (assumes that all strategy data sets are up to date)
        /// </summary>
        public void UpdateStrategy()
        {
            RLColor foreColor;
            int strategy_box_height = st_upper_box_height;
            int right_inner = st_right_outer - st_upper_box_width;
            int vertical_middle = st_top_align + st_upper_box_height + st_spacer; //y_coord of breakdown box
            int instruction_box_width = st_right_outer - st_left_outer;
            int vertical_bottom = Height - Offset_y * 2 - st_spacer - st_instruct_height; //y-coord of instruction box
            int breakdown_box_height = vertical_bottom - vertical_middle - st_spacer;
            int breakdown_box_width = st_right_outer - st_left_outer;
            //Card Pool dynamic data
            for (int width_index = st_left_outer + 24; width_index < st_left_outer + 26; width_index++)
            {
                for (int height_index = st_top_align + 1; height_index < st_top_align + st_upper_box_height - 1; height_index++)
                { arrayOfCells_Strategy[width_index, height_index] = 255; }
            }
            int totalNumCards = arrayCardPool[0] + arrayCardPool[1] + arrayCardPool[2];
            DrawText(Convert.ToString(arrayCardPool[0]), st_left_outer + 24, st_top_align + 2, RLColor.Blue, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawText(Convert.ToString(arrayCardPool[1]), st_left_outer + 24, st_top_align + 4, RLColor.Magenta, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawText(Convert.ToString(arrayCardPool[2]), st_left_outer + 24, st_top_align + 6, RLColor.Red, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawText(Convert.ToString(totalNumCards), st_left_outer + 24, st_top_align + 8, RLColor.Black, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            //Situation
            for (int width_index = right_inner + 1; width_index < right_inner + st_upper_box_width - 1; width_index++)
            {
                for (int height_index = st_top_align + 1; height_index < st_top_align + st_upper_box_height - 1; height_index++)
                { arrayOfCells_Strategy[width_index, height_index] = 255; }
            }
            DrawCenteredText("Situation", right_inner, st_top_align + 2, st_upper_box_width, RLColor.Blue, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawCenteredText(arraySituation[0, 0], right_inner, st_top_align + 4, st_upper_box_width, RLColor.Black, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawCenteredText(arraySituation[1, 0], right_inner, st_top_align + 6, st_upper_box_width, RLColor.Magenta, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            foreColor = RLColor.Green;
            if (GameSituation == CardType.Bad) { foreColor = RLColor.Red; } else if (GameSituation == CardType.Neutral) { foreColor = RLColor.Magenta; }
            DrawCenteredText(arraySituation[2, 0], right_inner, st_top_align + 8, st_upper_box_width, foreColor, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            //Strategy
            for (int width_index = st_middle_align + 1; width_index < st_middle_align + st_strategy_width - 1; width_index++)
            {
                for (int height_index = st_top_align + 1; height_index < st_top_align + strategy_box_height - 1; height_index++)
                { arrayOfCells_Strategy[width_index, height_index] = 255; }
            }
            DrawCenteredText("Choose a Strategy", st_middle_align, st_top_align + 2, st_strategy_width, RLColor.Blue, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawText(string.Format("[F1] {0}", arrayStrategy[0]), st_middle_align + 4, st_top_align + 4, RLColor.Black, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawText(string.Format("[F2] {0}", arrayStrategy[1]), st_middle_align + 4, st_top_align + 6, RLColor.Black, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawText(string.Format("[F3] {0}", arrayStrategy[2]), st_middle_align + 4, st_top_align + 8, RLColor.Black, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            //Breakdown of Card Pool
            for (int width_index = st_left_outer + 1; width_index < st_left_outer + breakdown_box_width - 1; width_index++)
            {
                for (int height_index = vertical_middle + 1; height_index < vertical_middle + breakdown_box_height - 1; height_index++)
                { arrayOfCells_Strategy[width_index, height_index] = 255; }
            }
            //header first then card breakdown
            DrawCenteredText(arrayOutcome[0], st_left_outer, vertical_middle + 2, breakdown_box_width, RLColor.Blue, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            DrawCenteredText(arrayOutcome[9], st_left_outer, vertical_middle + 4, breakdown_box_width, RLColor.Blue, arrayOfCells_Strategy, arrayOfForeColors_Strategy);
            int limit = Math.Min((breakdown_box_height - 8) / 2, listCardBreakdown.Count);
            for (int i = 0; i < limit; i++)
            { DrawText(listCardBreakdown[i].GetText(), st_left_outer + 4, vertical_middle + 6 + (i + 1) * 2, listCardBreakdown[i].GetForeColor(), arrayOfCells_Strategy, arrayOfForeColors_Strategy); }
        }

        /// <summary>
        /// Draw Confirm Layout (internals filled and new text written)
        /// </summary>
        /// <param name="multiConsole"></param>
        public void DrawConfirm(RLConsole multiConsole)
        { Draw(multiConsole, arrayOfCells_Message, arrayOfForeColors_Message, arrayOfBackColors_Message); }


        /// <summary>
        /// Draw AutoResolve Layout
        /// </summary>
        /// <param name="multiConsole"></param>
        public void DrawMessage(RLConsole multiConsole )
        { Draw(multiConsole, arrayOfCells_Message, arrayOfForeColors_Message, arrayOfBackColors_Message); }


        /// <summary>
        /// Update Message Layout
        /// </summary>
        /// <param name="listOfSnippets"></param>
        public void UpdateMessage(List<Snippet> listOfSnippets, RLColor fillColor)
        {
            int left_align = (Width - Offset_x) / 3;
            int top_align = (Height - Offset_y * 2) / 3;
            int box_width = left_align;
            int box_height = top_align + 2;
            //Clear box & change background color
            for (int width_index = left_align + 1; width_index < left_align + box_width - 1; width_index++)
            {
                for (int height_index = top_align + 1; height_index < top_align + box_height - 1; height_index++)
                {
                    arrayOfCells_Message[width_index, height_index] = 255;
                    arrayOfBackColors_Message[width_index, height_index] = fillColor;
                }
            }
            //Add new text
            string text;
            RLColor foreColor;
            //max ines of text
            int limit = Math.Min((top_align - 6) / 3, listOfSnippets.Count);
            for (int i = 0; i < limit; i++)
            {
                text = listOfSnippets[i].GetText();
                foreColor = listOfSnippets[i].GetForeColor();
                DrawCenteredText(text, left_align, top_align + (i + 1) * 3, box_width, foreColor, arrayOfCells_Message, arrayOfForeColors_Message);
            }
            DrawCenteredText("Press [SPACE] or [ENTER] to Continue", left_align, top_align + box_height - 3, box_width, RLColor.Gray, arrayOfCells_Message, arrayOfForeColors_Message);
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
            if (text != null)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    //truncates text if overlength (otherwise index out of bounds error)
                    if ((i + coord_X) <= (arrayOfCells.GetUpperBound(0) - coord_X) )
                    {
                        arrayOfCells[coord_X + i, coord_Y] = text[i];
                        arrayOfForeColors[coord_X + i, coord_Y] = foreColor;
                    }
                    else
                    { break; }
                }
            }
            else { Game.SetError(new Error(87, "String input is invalid (null)")); }
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
            if (text != null)
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
            else { Game.SetError(new Error(82, "String input is invalid (null)")); }
        }

        /// <summary>
        /// pop-up confirmation
        /// </summary>
        /// <returns></returns>
        public List<Snippet> GetDataConfirm()
        {
            int box_width = (Width - Offset_x) / 3;
            List<Snippet> listOfSnippets = new List<Snippet>();
            listOfSnippets.Add(new Snippet("You have Chosen...", RLColor.Black, Confirm_FillColor));
            listOfSnippets.Add(new Snippet(string.Format("{0} [F{1}]", arrayStrategy[Strategy_Player], Strategy_Player + 1), RLColor.Blue, Confirm_FillColor));
            listOfSnippets.Add(new Snippet("Your Opponent has Chosen...", RLColor.Black, Confirm_FillColor));
            listOfSnippets.Add(new Snippet(string.Format("{0} [F{1}]", arrayStrategy[Strategy_Opponent + 3], Strategy_Opponent + 1), RLColor.Red, Confirm_FillColor));
            listOfSnippets.Add(new Snippet(""));
            //has defender forfeited their advantage?
            if (Challenger == true)
            {
                if (Strategy_Opponent != 2)
                {
                    //word wrap
                    string text = string.Format("Your Opponent has forfeited the advantage of \"{0}\" by not defending", arraySituation[0, 0]);
                    List<string> tempList = new List<string>();
                    tempList.AddRange(Game.utility.WordWrap(text, box_width - 4));
                    for (int i = 0; i < tempList.Count(); i++)
                    { listOfSnippets.Add(new Snippet(tempList[i], RLColor.Magenta, Confirm_FillColor)); }
                }
            }
            else
            {
                if (Strategy_Player != 2)
                {
                    //word wrap
                    string text = string.Format("You have forfeited the advantage of \"{0}\" by not defending", arraySituation[0, 0]);
                    List<string> tempList = new List<string>();
                    tempList.AddRange(Game.utility.WordWrap(text, box_width - 4));
                    for (int i = 0; i < tempList.Count(); i++)
                    { listOfSnippets.Add(new Snippet(tempList[i], RLColor.LightMagenta, Confirm_FillColor)); }
                }
            }
            return listOfSnippets;
        }

        /// <summary>
        /// Warning message for Strategy Layout
        /// </summary>
        /// <returns></returns>
        public List<Snippet> GetDataErrorStrategy()
        {

            List<Snippet> listOfSnippets = new List<Snippet>();
            listOfSnippets.Add(new Snippet("You need to choose a Strategy", RLColor.Black, Error_FillColor));
            listOfSnippets.Add(new Snippet("or press [ESC] to AutoResolve", RLColor.Black, Error_FillColor));
            return listOfSnippets;
        }

        /// <summary>
        /// Provides text for AutoResolve message
        /// </summary>
        /// <returns></returns>
        public List<Snippet> GetAutoResolveText()
        {
            List<Snippet> listOfSnippets = new List<Snippet>();
            listOfSnippets.Add(new Snippet("You have chosen to AutoResolve", RLColor.Black, Resolve_FillColor));
            listOfSnippets.Add(new Snippet("Calculating...", RLColor.Blue, Resolve_FillColor));
            listOfSnippets.Add(new Snippet("", RLColor.Black, Resolve_FillColor));
            listOfSnippets.Add(new Snippet(string.Format("Your Strategy is {0}", GetStrategy(Strategy_Player)), RLColor.Black, Resolve_FillColor));
            listOfSnippets.Add(new Snippet(string.Format("Your Opponent's Strategy is {0}", GetStrategy(Strategy_Opponent)), RLColor.Red, Resolve_FillColor));
            return listOfSnippets;
        }

        /// <summary>
        /// returns text version of overall strategy based on int input
        /// </summary>
        /// <param name="strategyCode"></param>
        /// <returns></returns>
        private string GetStrategy(int strategyCode)
        {
            string strategyText = "Balanced";
            if (strategyCode == 2) { strategyText = "Defensive"; }
            else if (strategyCode == 0) { strategyText = "Aggressive"; }
            return strategyText;
        }

        /// <summary>
        /// Set up Card Breakdown
        /// </summary>
        /// <param name="listOfSnippets"></param>
        internal void SetCardBreakdown(List<Snippet> listOfSnippets)
        {
            if (listOfSnippets.Count > 0)
            {
                listCardBreakdown.Clear();
                listCardBreakdown.AddRange(listOfSnippets);
            }
            else
            { Game.SetError(new Error(83, "Invalid Input (List has no records")); }
        }

        /// <summary>
        /// Sets up Strategy array (overwrites existing data)
        /// </summary>
        /// <param name="arrayInput"></param>
        internal void SetStrategy(string[] arrayInput)
        {
            if (arrayInput.Length == 6)
            { arrayInput.CopyTo(arrayStrategy, 0); }
            else
            { Game.SetError(new Error(85, "Invalid Strategy Array input (needs precisely 6 strategies)")); }
        }

        /// <summary>
        /// Sets up Situation array (overwrites existing data)
        /// </summary>
        /// <param name="arraySituation"></param>
        internal void SetSituation(string[,] arrayInput)
        {
            int upper = arrayInput.GetUpperBound(0);
            int lower = arrayInput.GetUpperBound(1);
            int rank = arrayInput.Rank;
            //empty out the array 
            Array.Clear(arraySituation, 0, (upper + 1) * (lower + 1));
            //3 or less factors
            if (upper == 2 && upper > 0 && rank == 2)
            { Array.Copy(arrayInput, arraySituation, (upper + 1) * (lower + 1)); }
            else
            { Game.SetError(new Error(85, "Invalid Situation Array input (needs from 1 to 3 strategies)")); }
        }

        /// <summary>
        /// Sets up Outcome array (overwrites existing data)
        /// </summary>
        /// <param name="arrayInput"></param>
        internal void SetOutcome(string[] arrayInput)
        {
            //empty out the array 
            Array.Clear(arrayOutcome, 0, arrayOutcome.Length);
            //10 factors required
            if (arrayInput.Length == 11)
            { arrayInput.CopyTo(arrayOutcome, 0); }
            else
            { Game.SetError(new Error(90, "Invalid Outcome Array input (needs exactly 10 items)")); }
        }

        /// <summary>
        /// Sets up Card Pool array
        /// </summary>
        /// <param name="tempArray"></param>
        internal void SetCardPool(int[] tempArray)
        {
            //empty out array first
            Array.Clear(arrayCardPool, 0, arrayCardPool.Length);
            //4 factors required
            if (tempArray.Length == 4)
            { tempArray.CopyTo(arrayCardPool, 0); }
            else
            { Game.SetError(new Error(92, "Invalid Card Pool Array Input (needs exactly 3 items")); }
        }

        /// <summary>
        /// passes reference to Card Points Matrix (all set for a 1X card, double for a 2X, etc.)
        /// </summary>
        /// <param name="tempArray"></param>
        internal void SetPoints(int[,,] tempArray)
        { arrayPoints = tempArray; }

        /// <summary>
        /// Sets up Card Hand list & zero out other stuff where needed
        /// </summary>
        /// <param name="tempList"></param>
        internal void SetCardHand(List<Card_Conflict> tempList)
        {
            if (tempList != null)
            {
                //update
                listCardHand.AddRange(tempList);
                cardsRemaining = listCardHand.Count() - 1;
                cardsRemaining = Math.Max(1, cardsRemaining);
            }
            else { Game.SetError(new Error(94, "Invalid tempList input (null)")); }
        }





        /// <summary>
        /// Used by game.cs to check if hand completed (returns false)
        /// </summary>
        /// <returns></returns>
        internal bool CheckHandStatus()
        {
            if (cardCounter >= handSize) { return false; }
            return true;
        }

        /// <summary>
        /// Run each time a card is either played, or ignored
        /// </summary>
        /// <param name="cardPlayed"></param>
        internal void ResolveCard(bool cardPlayed = false)
        {
            bool actionTaken = cardPlayed;
            string textAction;
            string textImmersion;
            //history
            string playerAction = "Ignored";
            int pointsGained = points_ignore;
            //Green for Played cards, black for ignored
            RLColor foreColor = RLColor.Black;
            //last three cards will be autoplayed if ignored by player and enough influence remains
            if (cardPlayed == false)
            {
                if (InfluenceRemaining > 0 && cardsRemaining < 2 &&  cardsRemaining < InfluenceRemaining)
                { actionTaken = true; }
            }
            //update score and influence
            if (actionTaken == true)
            {
                InfluenceRemaining--;
                score += points_play;
                playerAction = "Played";
                pointsGained = points_play;
                foreColor = RLColor.Green;
            }
            else
            { score += points_ignore; }
            //flip next card, provided hand isn't empty
            if (listCardHand.Count > 0)
            { NextCard = true; }
            //add to history
            if (currentCard != null && currentCard.Type != CardType.None)
            {
                cardCounter++;
                //Player Action
                if (currentCard.Conflict_Type == CardConflict.Situation)
                {
                    textAction = string.Format("Card {0}, {1} \"{2}\", ({3}), for {4}{5} points, score {6}{7}", cardCounter, playerAction, currentCard.Title, currentCard.Type,
                    pointsGained > 0 ? "+" : "", pointsGained, score > 0 ? "+" : "", score);
                }
                else
                {
                    textAction = string.Format("Card {0}, {1} {2}, ({3}), for {4}{5} points, score {6}{7}", cardCounter, playerAction, currentCard.Title, currentCard.Type,
                    pointsGained > 0 ? "+" : "", pointsGained, score > 0 ? "+" : "", score);
                }
                Snippet snippetCard = new Snippet(textAction, foreColor, Back_FillColor);
                listHistory.Add(snippetCard);
                //...add to message queue
                messageQueue.Enqueue(snippetCard);
                if (messageQueue.Count > ca_message_max) { messageQueue.Dequeue();}
                //Immersion text
                if (actionTaken == true)
                { textImmersion = currentCard.PlayedText; }
                else { textImmersion = currentCard.IgnoredText; }
                if (textImmersion != null && textImmersion.Length> 0)
                {
                    //deal with any text tags
                    textImmersion = Game.utility.CheckTagsActor(textImmersion, Game.conflict.GetOpponent());
                    //word wrap
                    List<string> tempList = new List<string>();
                    tempList.AddRange(Game.utility.WordWrap(textImmersion, ou_width - 8));
                    for (int i = 0; i < tempList.Count; i++)
                    {
                        Snippet snippetImmersion = new Snippet(tempList[i], RLColor.Brown, Back_FillColor);
                        listHistory.Add(snippetImmersion);
                        //...add to message queue
                        messageQueue.Enqueue(snippetImmersion);
                        if (messageQueue.Count > ca_message_max) { messageQueue.Dequeue(); }
                    }
                }
            }
        }

        //new methods above here
    }
}
