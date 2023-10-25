
namespace Chess {
    public class AttackGenerator {
        
        private readonly Board board;

        private const int MaxSquaresInPinPerPos = 20;
        private const int MaxAttackedSquaresPerPos = 332;
        private const int MaxSquaresInCheckPerPos = 14;
        private const int MaxAttackedSquaresPerPiece = 28;

        private readonly int[] AttackedSquares = new int[MaxAttackedSquaresPerPos];
        private int attackedSquaresFound = 0;
        private readonly int[] SquaresInCheck = new int[MaxSquaresInCheckPerPos];
        private int squaresInCheckFound = 0;
        private readonly int[] SquaresInPinX = new int[MaxSquaresInPinPerPos];
        private int squaresInPinXFound = 0;
        private readonly int[] SquaresInPinY = new int[MaxSquaresInPinPerPos];
        private int squaresInPinYFound = 0;

        public AttackGenerator(Board board) {
            this.board = board;

            GenerateAllAttacks();
        }

        private void AddAttackedSquare(int square) {
            AttackedSquares[attackedSquaresFound] = square;
            attackedSquaresFound++;
        }

        private void AddSquareInCheck(int square) {
            SquaresInCheck[squaresInCheckFound] = square;
            squaresInCheckFound++;
        }

        private void AddSquareInPinX(int square) {
            SquaresInPinX[squaresInPinXFound] = square;
            squaresInPinXFound++;
        }

        private void AddSquareInPinY(int square) {
            SquaresInPinY[squaresInPinYFound] = square;
            squaresInPinYFound++;
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
                GenerateRayAttacksForPiece(4, 8, startSquare);
                queensBitboard &= ~Bitboards.BitFromSquare(startSquare);
            }
        }

        private void GenerateRayAttacksForPiece(int startIndex, int endIndex, int startSquare) {
            for (int dirIndex = startIndex; dirIndex < endIndex; dirIndex++) {
                bool isPin = false;
                List<int> squaresInPin = new() {startSquare};
                List<int> squaresInCheck = new() {startSquare};
                for (int n = 0; n < PrecomputedSquareData.SquaresToEdge[startSquare][dirIndex]; n++) {
                    int targetSquare = startSquare + PrecomputedSquareData.MovingOffsets[dirIndex] * (n + 1);
                    if (isPin) {
                        squaresInPin.Add(targetSquare);
                        if (Bitboards.IsSquareOccupied(board.PiecesBitboards[board.OppositeColorIndex][Piece.King], targetSquare)) {
                            if ((dirIndex >= 0 && dirIndex <2) || (dirIndex >= 4 && dirIndex <8)) {
                                SquaresInPinX.AddRange(squaresInPin);
                            }
                            if ((dirIndex >=2 && dirIndex <4) || (dirIndex >=4 && dirIndex <8)) {
                                SquaresInPinY.AddRange(squaresInPin);
                            }
                            break;
                        }
                    } else {
                        AddAttackedSquare(targetSquare);
                        squaresInCheck.Add(targetSquare);
                        if (Bitboards.IsSquareOccupied(board.AllPiecesBitboard[board.CurrentColorIndex], targetSquare)) {
                            break;
                        }
                        if (Bitboards.IsSquareOccupied(board.AllPiecesBitboard[board.OppositeColorIndex], targetSquare)) {
                            if (board.GetPieceOnSquare(targetSquare) == (board.OppositeColor | Piece.King)) {
                                SquaresInCheck.AddRange(squaresInCheck);
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
            foreach(int square in AttackedSquares) {
                Console.Write($"{Board.SquareToSquareName(square)} ");
            }
            Console.WriteLine();
            Console.WriteLine("All squares in check: ");
            foreach(int square in SquaresInCheck) {
                Console.Write($"{Board.SquareToSquareName(square)} ");
            }
            Console.WriteLine();
            Console.WriteLine("All squares in X pin: ");
            foreach(int square in SquaresInPinX) {
                Console.WriteLine($"{Board.SquareToSquareName(square)} ");
            }
            Console.WriteLine();
            Console.WriteLine("All squares in Y pin: ");
            foreach(int square in SquaresInPinY) {
                Console.WriteLine($"{Board.SquareToSquareName(square)} ");
            }
            Console.WriteLine();
        }
    }
}