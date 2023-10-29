using Core;

namespace Chess {
    public class MatchMaker {
        private readonly Player[] players;
        private readonly Board board;
        private AttackGenerator attackGenerator;
        private GameState CurrentGameState = GameState.Running;

        public MatchMaker() {
            board = new();
            board.LoadFromFen(Config.DefaultFen, true);
            attackGenerator = new(board);

            players = new Player[2] {new PlayerHuman(Piece.White), new PlayerHuman(Piece.Black)};

            StartGame();
        }

        private void StartGame() {
            while (true) {
                attackGenerator = new(board);
                board.SwitchColor();
                board.Print();
                Move move = players[board.CurrentColorIndex].ReceiveMove();
                board.MakeMove(move);
            }
        }
    }
}