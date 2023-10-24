namespace Chess {
    public static class Piece {
        public const int None   = 0;
        public const int King   = 1;
        public const int Pawn   = 2;
        public const int Knight = 3;
        public const int Bishop = 4;
        public const int Rook   = 5;
        public const int Queen  = 6;

        public const int White = 8;
        public const int Black = 16;

        public const int ColorMask = White | Black;
        public const int TypeMask = 7;

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