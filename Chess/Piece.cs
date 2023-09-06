namespace Chess {
    public static class Piece {
        public static readonly uint None   = 0;
        public static readonly uint King   = 1;
        public static readonly uint Pawn   = 2;
        public static readonly uint Knight = 3;
        public static readonly uint Bishop = 4;
        public static readonly uint Rook   = 5;
        public static readonly uint Queen  = 6;

        public static readonly uint White = 8;
        public static readonly uint Black = 16;

        public static readonly uint ColorMask = White | Black;
        public static readonly uint TypeMask = 7;

        public static uint GetColor(uint piece) {
            return piece & ColorMask;
        }

        public static uint GetOppositeColor(uint color) {
            return color ^ ColorMask;
        }

        public static int GetColorIndex(uint piece) {
            return IsColor(piece, White) ? 0: 1;
        }

        public static bool IsColor(uint piece, uint color) {
            return GetColor(piece) == color;
        }

        public static uint GetType(uint piece) {
            return piece & TypeMask;
        }

        public static bool IsType(uint piece, uint type) {
            return GetType(piece) == type;
        }

        public static bool IsRayPiece(uint piece) {
            return (piece & 0b100) != 0;
        }

    }
}