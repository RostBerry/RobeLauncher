using System.Dynamic;
using System.Numerics;
using System.Reflection.Metadata;

namespace Chess {
    public static class Bitboards {
        
        public const ulong FirstRankMask   = 0x00000000000000FF;
        public const ulong SecondRankMask  = 0x000000000000FF00;
        public const ulong ThirdRankMask   = 0x0000000000FF0000;
        public const ulong FourthRankMask  = 0x00000000FF000000;
        public const ulong FifthRankMask   = 0x000000FF00000000;
        public const ulong SixthRankMask   = 0x0000FF0000000000;
        public const ulong SeventhRankMask = 0x00FF000000000000;
        public const ulong EighthRankMask  = 0xFF00000000000000;

        public const ulong NotFirstFileMask   = 0xFEFEFEFEFEFEFEFE;
        public const ulong NotEighthFileMask   = 0x7F7F7F7F7F7F7F7F;

        public const ulong Board6x6 = 0x7E7E7E7E7E7E00;

        public const ulong WhiteKingSideCastlingMask = 0x60;
        public const ulong BlackKingSideCastlingMask = 0x6000000000000000;
        public const ulong WhiteQueenSideCastlingMask = 0xE;
        public const ulong BlackQueenSideCastlingMask = 0xE00000000000000;

        public const ulong FullBoardMask   = 0xFFFFFFFFFFFFFFFF;


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

        public static ulong Shift(ulong bitboard, int squares) {
            if (squares > 0) {
                bitboard <<= squares;
            } else {
                bitboard >>= -squares;
            }
            return bitboard;
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

        public static int GetBitsCount(ulong bitboard) {
            int count = 0;
            while (bitboard != 0) {
                count++;
                bitboard &= ~GetLS1BBit(bitboard);
            }
            return count;
        }
    }
}