using RLNET;

namespace Next_Game
{
    public static class Color
    {
        //map
        public static RLColor _land = Palette.nude;
        public static RLColor _dot = Palette.khaki;
        public static RLColor _sea = Palette.splash;
        public static RLColor _wave = Palette.robinsegg;
        public static RLColor _house = Palette.latte;
        public static RLColor _bannerlord = Palette.ocean;
        public static RLColor _inn = Palette.salmon;
        public static RLColor _mountainBase = Palette.khaki;
        public static RLColor _mountain1 = Palette.mocha;
        public static RLColor _mountain2 = Palette.latte;
        public static RLColor _mountain3 = Palette.chocolate;
        public static RLColor _forestBase = Palette.honeydew;
        public static RLColor _forest1 = Palette.olive;
        public static RLColor _forest2 = Palette.envy;
        public static RLColor _forest3 = Palette.grass;
        public static RLColor _forest4 = Palette.moss;
        //text
        public static RLColor _active = Palette.peony;
        public static RLColor _player = Palette.salmon;
        //other
        public static RLColor _star = Palette.golden;
        public static RLColor _goodTrait = Palette.jade;
        public static RLColor _badTrait = Palette.salmon;
        public static RLColor _touched = Palette.tulip;
        //box backgrounds
        public static RLColor _background1 = Palette.pinacolada;
        public static RLColor _background2 = Palette.robinsegg;
    }

    public static class Palette
    {
        //browns
        public static RLColor nude = new RLColor(219, 184, 152);
        public static RLColor coffee = new RLColor(154, 141, 107);
        public static RLColor latte = new RLColor(139, 95, 58);
        public static RLColor khaki = new RLColor(223, 210, 201);
        public static RLColor mocha = new RLColor(198, 169, 137);
        public static RLColor chocolate = new RLColor(95, 59, 25);
        //greens
        public static RLColor honeydew = new RLColor(197, 208, 140);
        public static RLColor jade = new RLColor(156, 203, 157);
        public static RLColor eden = new RLColor(164, 203, 110);
        public static RLColor moss = new RLColor(163, 179, 55);
        public static RLColor grass = new RLColor(108, 181, 73);
        public static RLColor envy = new RLColor(27, 139, 67);
        public static RLColor olive = new RLColor(124, 132, 73);
        //blues
        public static RLColor robinsegg = new RLColor(191, 226, 248);
        public static RLColor splash = new RLColor(82, 150, 197);
        public static RLColor ocean = new RLColor(14, 122, 148);
        public static RLColor miami = new RLColor(64, 183, 205);
        //reds
        public static RLColor salmon = new RLColor(234, 106, 103);
        //purples
        public static RLColor tulip = new RLColor(188, 118, 170);
        //pinks
        public static RLColor peony = new RLColor(246, 204, 205);
        //yellows
        public static RLColor vacation = new RLColor(253, 202, 15);
        public static RLColor golden = new RLColor(241, 217, 121);
        public static RLColor pinacolada = new RLColor(251, 245, 213);


    }
}
