namespace Core {

    public static class Config {

        private static int _chessSquareSize = 72;
        private static int _ticTacToeSquareSize = 192;
        private static int _shogiSquareSize = 64;

        private static bool _doTerminalOutput = true;

        private static GameMode _gameModeChess = GameMode.PvP;
        private static GameMode _gameModeTicTacToe  = GameMode.PvP;
        private static GameMode _gameModeShogi = GameMode.PvP;

        public const string DecorativeRow =        "+-----+-----+-----+-----+-----+-----+-----+-----+";
        public const string DecorativeLettersRow = "   a     b     c     d     e     f     g     h   ";

        public const string DefaultFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        public const string PerftFen = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8";
        public const string MateIn2Fen = "kbK5/pp6/1P6/8/8/8/8/R7 w - -";

        public static int ChessSquareSize {
            get {return _chessSquareSize;}
            set { _chessSquareSize = value;}
        }
        public static int TicTacToeSquare {
            get {return _ticTacToeSquareSize;}
            set { _ticTacToeSquareSize = value;}
        }
        public static int ShogiSquareSize {
            get {return _shogiSquareSize;}
            set { _shogiSquareSize = value;}
        }
        public static bool DoTerminalOutput {
            get {return _doTerminalOutput;}
            set { _doTerminalOutput = value;}
        }
        public static GameMode GameModeChess {
            get {return _gameModeChess;}
            set { _gameModeChess = value;}
        }
        public static GameMode GameModeTicTacToe {
            get {return _gameModeTicTacToe;}
            set { _gameModeTicTacToe = value;}
        }
        public static GameMode GameModeShogi {
            get {return _gameModeShogi;}
            set { _gameModeShogi = value;}
        }
        
    }
    
}