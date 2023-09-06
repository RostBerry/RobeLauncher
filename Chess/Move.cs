namespace Chess {
    public struct Move {

        public static readonly uint RegularMove = 0;
        public static readonly uint Capture = 1;
        public static readonly uint QueenPromotion = Piece.Queen;
        public static readonly uint KnightPromotion = Piece.Knight;
        public static readonly uint RookPromotion = Piece.Rook;
        public static readonly uint BishopPromotion = Piece.Bishop;


        public readonly int StartSquare;
        public readonly int TargetSquare;
        public readonly uint Flag;
        
        public Move(int startSquare, int targetSquare, uint flag = 0) {
            StartSquare = startSquare;
            TargetSquare = targetSquare;
            Flag = flag;
        }

        public readonly override bool Equals(object? obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            Move thatMove = (Move)obj;
            if (StartSquare == thatMove.StartSquare 
                && TargetSquare == thatMove.TargetSquare 
                && Flag == thatMove.Flag) {
                    return true;
            }
            return false;
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(StartSquare, TargetSquare, Flag);
        }

        public static bool operator ==(Move thisMove, Move thatMove) {
            return thisMove.Equals(thatMove);
        }

        public static bool operator !=(Move thisMove, Move thatMove) {
            return !(thisMove == thatMove);
        }

        public readonly override string ToString()
        {
            string flagSym = 3 <= Flag && Flag <= 6 ? new string(ChessUtils.BinToPieceSym[Flag], 1): "";
            return $"{ToSquareName(StartSquare)}{ToSquareName(TargetSquare)}{flagSym}";
        }

        public static Move FromUci(string uci) {
            uint flag = 0;
            if (uci.Length == 5) {
                flag = ChessUtils.PieceSymToBin[uci[4]];
                uci = uci.Remove(4);
            }
            string startSquareName = uci[..2];
            string targetSquareName = uci[2..];
            int startSquare = ToSquare(startSquareName);
            int targetSquare = ToSquare(targetSquareName);
            return new Move(startSquare, targetSquare, flag);
        }

        public static string ToUci(Move move) {
            string output = ToSquareName(move.StartSquare);
            output += ToSquareName(move.TargetSquare);
            uint flag = move.Flag;
            if (flag != 0) {
                output += ChessUtils.BinToPieceSym[flag];
            }
            return output;
        }

        public static string ToSquareName(int square) {
            return PrecomputedSquareData.SquareNameData.SquareToSquareName[square];
        }

        public static int ToSquare(string squareName) {
            return PrecomputedSquareData.SquareNameData.SquareNameToSquare[squareName];
        }

        public static int RankFromSquare(int square) {
            return square >> 3;
        }

        public static int FileFromSquare(int square) {
            return square & 0b111;
        }
    }
}