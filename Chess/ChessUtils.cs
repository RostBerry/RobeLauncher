using System.Linq;

namespace Chess {
    public static class ChessUtils {
        public static readonly Dictionary<char, uint> PieceSymToBin = new()
        {
            {' ', Piece.None},
            {'K', Piece.White | Piece.King},
            {'k', Piece.Black | Piece.King},
            {'P', Piece.White | Piece.Pawn},
            {'p', Piece.Black | Piece.Pawn},
            {'N', Piece.White | Piece.Knight},
            {'n', Piece.Black | Piece.Knight},
            {'B', Piece.White | Piece.Bishop},
            {'b', Piece.Black | Piece.Bishop},
            {'R', Piece.White | Piece.Rook},
            {'r', Piece.Black | Piece.Rook},
            {'Q', Piece.White | Piece.Queen},
            {'q', Piece.Black | Piece.Queen}
        };

        public static readonly Dictionary<uint, char> BinToPieceSym = 
            PieceSymToBin.ToDictionary(pair => pair.Value, pair => pair.Key);


        public static readonly string DecorativeRow =        "+-----+-----+-----+-----+-----+-----+-----+-----+";
        public static readonly string DecorativeLettersRow = "   a     b     c     d     e     f     g     h   ";

        public static readonly string DefaultFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public static readonly string PerftFen = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8";

    }

    public enum CastlingStatus {
            BothSides,
            ShortSide,
            LongSide,
            None
        }
}