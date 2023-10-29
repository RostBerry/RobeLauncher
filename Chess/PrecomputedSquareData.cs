

namespace Chess {
    public static class PrecomputedSquareData {

        private static readonly char[] NumToBoardLetter = {'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'};

        public static readonly (string[] SquareToSquareName, 
                                Dictionary<string, int> SquareNameToSquare) 
            SquareNameData = LoadSquareNames();

        public static readonly int[] MovingOffsets = new int[8] { 8, -8, -1, 1, 7, 9, -9, -7}; // up, down, left, right, upleft, upright, downleft, downright

        public static readonly int[,] SquaresToEdgeArr = LoadSquaresToEdgeArr();
        public static readonly ulong[,] SquaresToEdgeBB = LoadSquaresToEdgeBB();

        public static readonly ulong[] SquaresForKing = LoadSquaresForKing();

        public static readonly ulong[] SquaresForKnight = LoadSquaresForKnight();

        public static readonly ulong[,] LinesFromSquareToSquareBB = LoadLinesFromSquareToSquare();


        private static (string[], Dictionary<string, int>) LoadSquareNames() {
            Dictionary<string, int> squareNameToSquare = new();
            string[] squareToSquareName = new string[64];
            for (int y = 7; y >= 0; y--) {
                for (int x = 0; x < 8; x++) {
                    int square = x + y * 8;
                    char squareNameLetter = NumToBoardLetter[x];
                    string squareName = $"{squareNameLetter}{y + 1}";
                    squareNameToSquare.Add(squareName, square);
                    squareToSquareName[square] = squareName;
                }
            }
            return (squareToSquareName, squareNameToSquare);
        } 

        private static int[,] LoadSquaresToEdgeArr() {
            int[,] squareDirData = new int[64, 8];
            int toNorth;
            int toSouth;
            int toWest;
            int toEast;
            int square;

            for (int y = 7; y >= 0; y--) {
                for (int x = 0; x < 8; x++) {
                    toNorth = 7 - y;
                    toSouth = y;
                    toWest = x;
                    toEast = 7 - x;

                    square = x + y * 8;
                    squareDirData[square, 0] = toNorth;
                    squareDirData[square, 1] = toSouth;
                    squareDirData[square, 2] = toWest;
                    squareDirData[square, 3] = toEast;
                    squareDirData[square, 4] = Math.Min(toNorth, toWest);
                    squareDirData[square, 5] = Math.Min(toNorth, toEast);
                    squareDirData[square, 6] = Math.Min(toSouth, toWest);
                    squareDirData[square, 7] = Math.Min(toSouth, toEast);
                }
            }

            return squareDirData;
        }

        private static ulong[,] LoadSquaresToEdgeBB() {
            ulong[,] dataBB = new ulong[64, 8];
            int square;

            for (int y = 7; y >= 0; y--) {
                for (int x = 0; x < 8; x++) {
                    square = x + y * 8;
                    for (int offset = 0; offset < MovingOffsets.Length; offset++) {
                        ulong squaresToEdge = 0ul;
                        for (int n = 0; n < SquaresToEdgeArr[square, offset]; n++) {
                            squaresToEdge |= Bitboards.BitFromSquare(square + MovingOffsets[offset] * n);
                        }
                        dataBB[square, offset] = squaresToEdge;
                    }
                }
            }
            return dataBB;
        }

        private static ulong[] LoadSquaresForKing() {
            ulong[] squaresForKing = new ulong[64];
            for (int y = 7; y >= 0; y--) {
                for (int x = 0; x < 8; x++) {
                    int square = x + y * 8;
                    ulong possibleMoves = 0ul;
                    
                    for(int offsetIndex = 0; offsetIndex < MovingOffsets.Length; offsetIndex ++) {
                        if (SquaresToEdgeArr[square,offsetIndex] > 0) {
                            possibleMoves |= Bitboards.BitFromSquare(square + MovingOffsets[offsetIndex]);
                        }
                    }

                    squaresForKing[square] = possibleMoves;
                }
            }
            return squaresForKing;
        }

        private static ulong[] LoadSquaresForKnight() {
            int[] knightMovingOffsets = new int[8] {15, 17, 6, 10, -10, -6, -17, -15};
            ulong[] squaresForKnight = new ulong[64];
            for(int y = 7; y >= 0; y--) {
                for(int x = 0; x < 8; x++) {
                    int square = x + y * 8;
                    int squareRank = Board.RankFromSquare(square);
                    int squareFile = Board.FileFromSquare(square);
                    ulong possibleMoves = 0;

                    foreach (int offset in knightMovingOffsets) {
                        int targetSquare = square + offset;
                        int targetSquareRank = Board.RankFromSquare(targetSquare);
                        int targetSquareFile = Board.FileFromSquare(targetSquare);

                        if (targetSquareFile >= 0 && targetSquareFile < 8 && targetSquareRank >= 0 && targetSquareRank < 8 
                            && Math.Abs(squareRank - targetSquareRank) < 3 && Math.Abs(squareFile - targetSquareFile) < 3)
                        {
                            possibleMoves |= Bitboards.BitFromSquare(targetSquare);
                        }
                    }

                    squaresForKnight[square] = possibleMoves;
                }
            }

            return squaresForKnight;
        }

        private static ulong[,] LoadLinesFromSquareToSquare() {
            ulong[,] lines = new ulong[64, 64];
            for (int square1 = 0; square1 < 64; square1++) {
                for (int square2 = 0; square2 < 64; square2++) {
                    lines[square1, square2] = Bitboards.BitFromSquare(square1);
                    if (square1 == square2) {
                        continue;
                    }
                    bool isLineFound = false;
                    for (int dirIndex = 0; dirIndex < 8; dirIndex++) {
                        ulong line = 0;
                        for (int squaresToEdge = 0; squaresToEdge < SquaresToEdgeArr[square1, dirIndex]; squaresToEdge++) {
                            int targetSquare = square1 + MovingOffsets[dirIndex] * (squaresToEdge + 1);
                            line |= Bitboards.BitFromSquare(targetSquare);
                            if (targetSquare == square2) {
                                lines[square1, square2] |= line;
                                isLineFound = true;
                                break;
                            }
                        }
                        if (isLineFound) {
                            break;
                        }
                    }
                    if (!isLineFound) {
                        lines[square1, square2] = 0;
                    }
                }
            }

            return lines;
        }
    }
}