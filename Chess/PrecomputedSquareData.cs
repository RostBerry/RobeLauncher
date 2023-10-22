

namespace Chess {
    public static class PrecomputedSquareData {

        private static readonly Dictionary<int, char> NumToBoardLetter = new() 
        {
            { 0, 'a'},
            { 1, 'b'},
            { 2, 'c'},
            { 3, 'd'},
            { 4, 'e'},
            { 5, 'f'},
            { 6, 'g'},
            { 7, 'h'},
        };

        public static readonly (Dictionary<int, string> SquareToSquareName, 
                                Dictionary<string, int> SquareNameToSquare) 
            SquareNameData = LoadSquareNames();

        public static readonly int[] MovingOffsets = new int[8] { 8, -8, -1, 1, 7, 9, -9, -7};

        public static readonly Dictionary<int, int[]> SquaresToEdge = LoadSquaresToEdge();

        public static readonly Dictionary<int, List<int>> SquaresForKing = LoadSquaresForKing();

        public static readonly Dictionary<int, List<int>> SquaresForKnight = LoadSquaresForKnight();


        private static (Dictionary<int, string>, Dictionary<string, int>) LoadSquareNames() {
            Dictionary<string, int> squareNameToSquare = new();
            Dictionary<int, string> squareToSquareName = new();
            for (int y = 7; y >= 0; y--) {
                for (int x = 0; x < 8; x++) {
                    int square = x + y * 8;
                    char squareNameLetter = NumToBoardLetter[x];
                    string squareName = $"{squareNameLetter}{y + 1}";
                    squareNameToSquare.Add(squareName, square);
                    squareToSquareName.Add(square, squareName);
                }
            }
            return (squareToSquareName, squareNameToSquare);
        } 

        private static Dictionary<int, int[]> LoadSquaresToEdge() {
            Dictionary<int, int[]> squareDirData = new();
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
                    squareDirData.Add(square, new int[8] { toNorth, toSouth, toWest, toEast, 
                                                            Math.Min(toNorth, toWest), Math.Min(toNorth, toEast), 
                                                            Math.Min(toSouth, toWest), Math.Min(toSouth, toEast) });
                }
            }

            return squareDirData;
        }

        private static Dictionary<int, List<int>> LoadSquaresForKing() {
            Dictionary<int, List<int>> squaresForKing = new();
            for (int y = 7; y >= 0; y--) {
                for (int x = 0; x < 8; x++) {
                    int square = x + y * 8;

                    List<int> possibleMoves = new();
                    
                    for(int offsetIndex = 0; offsetIndex < MovingOffsets.Length; offsetIndex ++) {
                        if (SquaresToEdge[square][offsetIndex] > 0) {

                            possibleMoves.Add(square + MovingOffsets[offsetIndex]);
                        }
                    }

                    squaresForKing[square] = possibleMoves;
                }
            }
            return squaresForKing;
        }

        private static Dictionary<int, List<int>> LoadSquaresForKnight() {
            int[] knightMovingOffsets = new int[8] {15, 17, 6, 10, -10, -6, -17, -15};
            Dictionary<int, List<int>> squaresForKnight = new();
            for(int y = 7; y >= 0; y--) {
                for(int x = 0; x < 8; x++) {
                    int square = x + y * 8;
                    int squareRank = Move.RankFromSquare(square);
                    int squareFile = Move.FileFromSquare(square);

                    List<int> possibleMoves = new();

                    foreach (int offset in knightMovingOffsets) {
                        int targetSquare = square + offset;
                        int targetSquareRank = Move.RankFromSquare(targetSquare);
                        int targetSquareFile = Move.FileFromSquare(targetSquare);

                        if (targetSquareFile >= 0 && targetSquareFile < 8 && targetSquareRank >= 0 && targetSquareRank < 8 
                            && Math.Abs(squareRank - targetSquareRank) < 3 && Math.Abs(squareFile - targetSquareFile) < 3)
                        {
                            possibleMoves.Add(targetSquare);
                        }
                    }

                    squaresForKnight[square] = possibleMoves;
                }
            }

            return squaresForKnight;
        }
    }
}