namespace Core {

    public static class Konfig {

        private static int _chessSquareSize = 72;
        private static int _ticTacToeSquareSize = 192;
        private static int _shogiSquareSize = 64;

        private static int _fps = 90;

        private static bool _doSquareSubscript = true;
        private static bool _doAvailableSquareDrawing = true;
        private static bool _doTerminalOutput = true;

        private static GameMode _gameModeChess = GameMode.PvP;
        private static GameMode _gameModeTicTacToe  = GameMode.PvP;
        private static GameMode _gameModeShogi = GameMode.PvP;



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
        public static int FPS {
            get {return _fps;}
            set { _fps = value;}
        }
        public static bool DoSquareSubscript {
            get {return _doSquareSubscript;}
            set { _doSquareSubscript = value;}
        }
        public static bool DoAvailableSquareDrawing {
            get {return _doAvailableSquareDrawing;}
            set { _doAvailableSquareDrawing = value;}
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