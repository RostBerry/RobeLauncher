using System.Numerics;

namespace Chess {
    public static class BitboardUtils {
        
        public static readonly ulong FirstRankMask   = 0xFF;
        public static readonly ulong SecondRankMask  = 0xFF00;
        public static readonly ulong ThirdRankMask   = 0xFF0000;
        public static readonly ulong FourthRankMask  = 0xFF000000;
        public static readonly ulong FifthRankMask   = 0xFF00000000;
        public static readonly ulong SixthRankMask   = 0xFF0000000000;
        public static readonly ulong SeventhRankMask = 0xFF000000000000;
        public static readonly ulong EighthRankMask  = 0xFF00000000000000;

        public static readonly ulong FullBoardMask   = 0xFFFFFFFFFFFFFFFF;


        public static bool IsSquareOccupied(ulong bitboard, int square) {
            return ((bitboard >> square) & 1) == 1;
        }

        public static int GetLS1BSquare(ulong bitboard) {
            return BitOperations.TrailingZeroCount(bitboard);
        }

        public static ulong GetLS1BBit(ulong bitboard) {
            return 1UL << GetLS1BSquare(bitboard);
        }

        public static int SquareFromBit(ulong bit) {
            return GetLS1BSquare(bit);
        }

        public static ulong BitFromSquare(int square) {
            return 1UL << square;
        }

        public static ulong GetNorthOffset(ulong bitboard, int numSquares) {
            return bitboard << (8 * numSquares);
        }

        public static ulong GetSouthOffset(ulong bitboard, int numSquares) {
            return bitboard >> (8 * numSquares);
        }

        public static ulong GetWestOffest(ulong bitboard, int numSquares) {
            return bitboard << (1 * numSquares);
        }

        public static ulong GetEastOffset(ulong bitboard, int numSquares) {
            return bitboard >> (1 * numSquares);
        }

        public static ulong GetNorthWestOffset(ulong bitboard, int numSquares) {
            return bitboard << (7 * numSquares);
        }

        public static ulong GetNorthEastOffset(ulong bitboard, int numSquares) {
            return bitboard << (9 * numSquares);
        }

        public static ulong GetSouthWestOffset(ulong bitboard, int numSquares) {
            return bitboard >> (9 * numSquares);
        }

        public static ulong GetSouthEastOffset(ulong bitboard, int numSquares) {
            return bitboard >> (7 * numSquares);
        }

        public static ulong GetOffset(ulong bitboard, int numSquares, int direction) {
            return direction switch
            {
                8 => GetNorthOffset(bitboard, numSquares),
                -8 => GetSouthOffset(bitboard, numSquares),
                -1 => GetWestOffest(bitboard, numSquares),
                1 => GetEastOffset(bitboard, numSquares),
                7 => GetNorthWestOffset(bitboard, numSquares),
                9 => GetNorthEastOffset(bitboard, numSquares),
                -9 => GetSouthEastOffset(bitboard, numSquares),
                -7 => GetSouthWestOffset(bitboard, numSquares),
                _ => 1,
            };
        }


    }
}