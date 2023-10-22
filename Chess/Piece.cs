namespace Chess {
    public static class Piece {
        public static readonly int None   = 0;
        public static readonly int King   = 1;
        public static readonly int Pawn   = 2;
        public static readonly int Knight = 3;
        public static readonly int Bishop = 4;
        public static readonly int Rook   = 5;
        public static readonly int Queen  = 6;

        public static readonly int White = 8;
        public static readonly int Black = 16;

        public static readonly int ColorMask = White | Black;
        public static readonly int TypeMask = 7;

        public static int GetColor(int piece) {
            return piece & ColorMask;
        }

        public static int GetOppositeColor(int color) {
            return color ^ ColorMask;
        }

        public static int GetColorIndex(int piece) {
            return IsColor(piece, White) ? 0: 1;
        }

        public static bool IsColor(int piece, int color) {
            return GetColor(piece) == color;
        }

        public static int GetType(int piece) {
            return piece & TypeMask;
        }

        public static bool IsType(int piece, int type) {
            return GetType(piece) == type;
        }

        public static bool IsRayPiece(int piece) {
            return (piece & 0b100) != 0;
        }

    }
}