namespace Chess {
    public static class MagicBitboardsGenerator {

        public static ulong GenerateMovesMask(int square, bool isBishop) {
            ulong movesMask = 0;
            int startIndex = isBishop? 4: 0;
            int endIndex = isBishop? 8: 4;
            for (int dirIndex = startIndex; dirIndex < endIndex; dirIndex++) {
                for (int squareToEdge = 0; squareToEdge < PrecomputedSquareData.SquaresToEdgeArr[square, dirIndex]; squareToEdge++) {
                    if (squareToEdge < PrecomputedSquareData.SquaresToEdgeArr[square, dirIndex] - 1) {
                        movesMask |= Bitboards.BitFromSquare(square + PrecomputedSquareData.MovingOffsets[dirIndex] * (squareToEdge + 1));
                    } else {break;}
                }
            }
            return movesMask;
        }

        public static ulong[] CreateBlockersBB(ulong movesMask) {
            int[] allImportantSquares = new int[64];
            int squaresFound = 0;
            for (int i = 0; i < 64; i++) {
                if (((movesMask >> i) & 1) == 1) {
                    allImportantSquares[squaresFound] = i;
                    squaresFound++;
                }
            }

            int blockerPatternsCount = 1 << squaresFound;
            ulong[] allBlockersBB = new ulong[blockerPatternsCount];

            for (int patternIndex = 0; patternIndex < blockerPatternsCount; patternIndex++) {
                for (int bitIndex = 0; bitIndex < squaresFound; bitIndex++) {
                    int bit = (patternIndex >> bitIndex) & 1;
                    allBlockersBB[patternIndex] |= (ulong)bit << allImportantSquares[bitIndex];
                }
            }

            return allBlockersBB;
        }

        public static ulong GenerateLegalMoves(int square, ulong blockersBB, bool isBishop) {
            ulong legalMovesBB = 0;
            int startIndex = isBishop? 4: 0;
            int endIndex = isBishop? 8: 4;

            for (int dirIndex = startIndex; dirIndex < endIndex; dirIndex++) {
                for (int squareToEdge = 0; squareToEdge < PrecomputedSquareData.SquaresToEdgeArr[square, dirIndex]; squareToEdge++) {
                    ulong targetBit = Bitboards.BitFromSquare(square + PrecomputedSquareData.MovingOffsets[dirIndex] * (squareToEdge + 1));
                    legalMovesBB |= targetBit;
                    if ((blockersBB & targetBit) != 0) {
                        break;
                    }
                }
            }

            return legalMovesBB;
        }

    }
}