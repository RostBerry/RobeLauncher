
namespace Chess {
    public class AttackGenerator {
        
        private readonly Board board;

        private const int MaxSquaresInPinPerPos = 20;
        private const int MaxAttackedSquaresPerPos = 332;
        private const int MaxSquaresInCheckPerPos = 14;
        private const int MaxAttackedSquaresPerPiece = 28;
        private const int MaxPinLength = 8;
        private const int MaxCheckRayLength = 7;

        private readonly int[] AttackedSquares = new int[MaxAttackedSquaresPerPos];
        private int AttackedSquaresFound = 0;
        private readonly int[] SquaresInCheck = new int[MaxSquaresInCheckPerPos];
        private int SquaresInCheckFound = 0;
        private readonly int[] SquaresInPinX = new int[MaxSquaresInPinPerPos];
        private int SquaresInPinXFound = 0;
        private readonly int[] SquaresInPinY = new int[MaxSquaresInPinPerPos];
        private int SquaresInPinYFound = 0;

        public AttackGenerator(Board board) {
            this.board = board;

            GenerateAllAttacks();
        }

        private void AddAttackedSquare(int square) {
            AttackedSquares[AttackedSquaresFound] = square;
            AttackedSquaresFound++;
        }

        private void AddSquareInCheck(int square) {
            SquaresInCheck[SquaresInCheckFound] = square;
            SquaresInCheckFound++;
        }

        private void AddSquareInPinX(int square) {
            SquaresInPinX[SquaresInPinXFound] = square;
            SquaresInPinXFound++;
        }

        private void AddSquareInPinY(int square) {
            SquaresInPinY[SquaresInPinYFound] = square;
            SquaresInPinYFound++;
        }

        private void GenerateAllAttacks() {

            GenerateAllPawnAttacks();
            GenerateAllRayAttacks();
            GenerateAllKnightsAttacks();
            GenerateAllKingsAttacks();
        }

        private void GenerateAllPawnAttacks() {
            
            ulong pawnsBitboard = board.PiecesBitboards[board.CurrentColorIndex][Piece.Pawn];

            ulong clampedBitboardWithFirstFile = pawnsBitboard & Bitboards.NotFirstFileMask;
            ulong clampedBitboardWithEighthFile = pawnsBitboard & Bitboards.NotEighthFileMask;

            ulong allCapturesLeft = board.IsWhite ? 
                    Bitboards.GetNorthWestOffset(clampedBitboardWithFirstFile, 1):
                    Bitboards.GetSouthEastOffset(clampedBitboardWithEighthFile, 1);

            ulong allCapturesRight = board.IsWhite ? 
                    Bitboards.GetNorthEastOffset(clampedBitboardWithEighthFile, 1):
                    Bitboards.GetSouthWestOffset(clampedBitboardWithFirstFile, 1);

            ulong allPawnsAttacksBitboard = allCapturesLeft | allCapturesRight;

            while(allPawnsAttacksBitboard != 0) {
                int attackedSquare = Bitboards.GetLS1BSquare(allPawnsAttacksBitboard);
                ulong attackedBit = 1UL << attackedSquare;
                AddAttackedSquare(attackedSquare);
                allPawnsAttacksBitboard &= ~attackedBit;
            }
        }

        private void GenerateAllRayAttacks() {
            ulong bishopsBitboard = board.PiecesBitboards[board.CurrentColorIndex][Piece.Bishop];
            ulong rooksBitboard = board.PiecesBitboards[board.CurrentColorIndex][Piece.Rook];
            ulong queensBitboard = board.PiecesBitboards[board.CurrentColorIndex][Piece.Queen];

            while (bishopsBitboard != 0) {
                int startSquare = Bitboards.GetLS1BSquare(bishopsBitboard);
                GenerateRayAttacksForPiece(4, 8, startSquare);
                bishopsBitboard &= ~Bitboards.BitFromSquare(startSquare);
            }

            while (rooksBitboard != 0) {
                int startSquare = Bitboards.GetLS1BSquare(rooksBitboard);
                GenerateRayAttacksForPiece(0, 4, startSquare);
                rooksBitboard &= ~Bitboards.BitFromSquare(startSquare);
            }

            while (queensBitboard != 0) {
                int startSquare = Bitboards.GetLS1BSquare(queensBitboard);
                GenerateRayAttacksForPiece(0, 8, startSquare);
                queensBitboard &= ~Bitboards.BitFromSquare(startSquare);
            }
        }

        private void GenerateRayAttacksForPiece(int startIndex, int endIndex, int startSquare) {
            for (int dirIndex = startIndex; dirIndex < endIndex; dirIndex++) {

                bool isPin = false;
                int[] squaresInPin = new int[MaxPinLength];
                squaresInPin[0] = startSquare;
                int squaresInPinFound = 1;

                int[] squaresInCheck = new int[MaxCheckRayLength];
                int squaresInCheckFound = 0;

                for (int n = 0; n < PrecomputedSquareData.SquaresToEdge[startSquare][dirIndex]; n++) {
                    int targetSquare = startSquare + PrecomputedSquareData.MovingOffsets[dirIndex] * (n + 1);
                    squaresInPin[squaresInPinFound] = targetSquare;
                    squaresInPinFound++;
                    if (isPin) {
                        if (targetSquare == board.KingSquare[board.OppositeColorIndex]) {
                            if (dirIndex <2 || dirIndex >= 4) {
                                for (int index = 0; index < squaresInPinFound; index++) {
                                    SquaresInPinX[SquaresInPinXFound] = squaresInPin[index];
                                    SquaresInPinXFound++;
                                }
                            }
                            if (dirIndex >=2) {
                                for (int index = 0; index < squaresInPinFound; index++) {
                                    SquaresInPinY[SquaresInPinYFound] = squaresInPin[index];
                                    SquaresInPinYFound++;
                                }
                            }
                            break;
                        }
                    } else {
                        AddAttackedSquare(targetSquare);
                        squaresInCheck[squaresInCheckFound] = targetSquare;
                        squaresInCheckFound++;
                        if (Bitboards.IsSquareOccupied(board.AllPiecesBitboard[board.CurrentColorIndex], targetSquare)) {
                            break;
                        }
                        if (Bitboards.IsSquareOccupied(board.AllPiecesBitboard[board.OppositeColorIndex], targetSquare)) {
                            if (targetSquare == board.KingSquare[board.OppositeColorIndex]) {
                                for (int index = 0; index < squaresInCheckFound; index++) {
                                    SquaresInCheck[SquaresInCheckFound] = squaresInCheck[index];
                                    SquaresInCheckFound++;
                                 }
                                break;
                            } else {
                                isPin = true;
                            }
                        }
                    }

                }
            }
        }

        private void GenerateAllKnightsAttacks() {

            ulong knightsBitboard = board.PiecesBitboards[board.CurrentColorIndex][Piece.Knight];

            while (knightsBitboard != 0) {
                int startSquare = Bitboards.GetLS1BSquare(knightsBitboard);
                foreach (int targetSquare in PrecomputedSquareData.SquaresForKnight[startSquare]) {
                    if (Bitboards.IsSquareOccupied(board.PiecesBitboards[board.OppositeColorIndex][Piece.King], targetSquare)) {
                        AddSquareInCheck(targetSquare);
                    }
                    AddAttackedSquare(targetSquare);
                }
                knightsBitboard &= ~Bitboards.BitFromSquare(startSquare);
            }
        }

        private void GenerateAllKingsAttacks() {
            foreach(int targetSquare in PrecomputedSquareData.SquaresForKing[board.KingSquare[board.CurrentColorIndex]]) {
                AddAttackedSquare(targetSquare);
            }
        }

        public void Print() {
            Console.WriteLine("All attacked squares: ");
            for (int squareIndex = 0; squareIndex < AttackedSquaresFound; squareIndex++) {
                Console.Write($"{Board.SquareToSquareName(AttackedSquares[squareIndex])} ");
            }
            Console.WriteLine();
            Console.WriteLine("All squares in check: ");
            for (int squareIndex = 0; squareIndex < SquaresInCheckFound; squareIndex++) {
                Console.Write($"{Board.SquareToSquareName(SquaresInCheck[squareIndex])} ");
            }
            Console.WriteLine();
            Console.WriteLine("All squares in X pin: ");
            for (int squareIndex = 0; squareIndex < SquaresInPinXFound; squareIndex++) {
                Console.Write($"{Board.SquareToSquareName(SquaresInPinX[squareIndex])} ");
            }
            Console.WriteLine();
            Console.WriteLine("All squares in Y pin: ");
            for (int squareIndex = 0; squareIndex < SquaresInPinYFound; squareIndex++) {
                Console.Write($"{Board.SquareToSquareName(SquaresInPinY[squareIndex])} ");
            }
            Console.WriteLine();
        }
    }
}