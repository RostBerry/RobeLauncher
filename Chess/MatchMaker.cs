using Core;

namespace Chess {
    public class MatchMaker {
        private readonly Player[] players;
        private readonly Board board;
        private AttackGenerator attackGenerator;
        private MoveGen moveGen;
        private GameState CurrentGameState = GameState.Running;

        public MatchMaker() {
            board = new();
            board.LoadFromFen(Config.MateIn2Fen, true);
            attackGenerator = new(board);
            moveGen = new(board, attackGenerator, true);

            players = new Player[2] {new PlayerHuman(Piece.White), new PlayerHuman(Piece.Black)};

            StartGame();
        }

        private void StartGame() {
            while (true) {
                attackGenerator = new(board);
                board.SwitchColor();
                moveGen = new(board, attackGenerator);

                while (true) {
                    board.Print();
                    Move move = players[board.CurrentColorIndex].ReceiveMove();
                    if (IsMoveRightColor(move) && IsMoveAvailable(move)) {
                        board.MakeMove(move);
                        break;
                    }
                }
                
            }
        }

        private bool IsMoveRightColor(Move move) {
            return Piece.GetColor(board.GetPieceOnSquare(move.StartSquare)) == board.CurrentColor;
        }

        private bool IsMoveAvailable(Move move) {
            return moveGen.ContainsMove(move);
        }
    }
}