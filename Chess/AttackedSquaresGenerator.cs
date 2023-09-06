namespace Chess {
    public class AttackedSquaresGenerator {
        
        private readonly Board board;

        public List<int> AttackedSquares = new();
        public List<int> SquaresInCheck = new();
        public List<int> SquaresInPinX = new();
        public List<int> SquaresInPinY = new();

        public AttackedSquaresGenerator(Board board) {
            this.board = board;

            GenerateAllAttacks();
        }

        private void GenerateAllAttacks() {

            GenerateAllPawnAttacks();
            GenerateAllRayAttacks();
            GenerateAllKnightsAttacks();
            GenerateAllKingsAttacks();

            foreach (int square in SquaresInCheck) {
                Console.Write($"{square} ");
            }
            Console.WriteLine();

        }

        private void GenerateAllPawnAttacks() {
            
            ulong pawnsBitboard = board.PiecesBitboards[board.CurrentColorIndex][Piece.Pawn];

            ulong allCapturesLeft = board.AllPiecesBitboard[board.OppositeColorIndex] & 
                (board.IsWhite ? 
                    BitboardUtils.GetNorthWestOffset(pawnsBitboard, 1):
                    BitboardUtils.GetSouthEastOffset(pawnsBitboard, 1));

            ulong allCapturesRight = board.AllPiecesBitboard[board.OppositeColorIndex] & 
                (board.IsWhite ? 
                    BitboardUtils.GetNorthEastOffset(pawnsBitboard, 1):
                    BitboardUtils.GetSouthWestOffset(pawnsBitboard, 1));

            ulong allPawnsAttacksBitboard = allCapturesLeft | allCapturesRight;

            while(allPawnsAttacksBitboard != 0) {
                int attackedSquare = BitboardUtils.GetLS1BSquare(allPawnsAttacksBitboard);
                ulong attackedBit = 1UL << attackedSquare;
                AttackedSquares.Add(attackedSquare);
                allPawnsAttacksBitboard &= ~attackedBit;
            }
        }

        private void GenerateAllRayAttacks() {
            foreach(int startSquare in board.SquaresWithQueens[board.CurrentColorIndex]) {
                GenerateRayAttacksForPiece(0, 8, startSquare);
            }

            foreach(int startSquare in board.SquaresWithRooks[board.CurrentColorIndex]) {
                GenerateRayAttacksForPiece(0, 4, startSquare);
            }

            foreach(int startSquare in board.SquaresWithBishops[board.CurrentColorIndex]) {
                GenerateRayAttacksForPiece(4, 8, startSquare);
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
                        if (BitboardUtils.IsSquareOccupied(board.PiecesBitboards[board.OppositeColorIndex][Piece.King], targetSquare)) {
                            if ((dirIndex >= 0 && dirIndex <2) || (dirIndex >= 4 && dirIndex <8)) {
                                SquaresInPinX.AddRange(squaresInPin);
                            }
                            if ((dirIndex >=2 && dirIndex <4) || (dirIndex >=4 && dirIndex <8)) {
                                SquaresInPinY.AddRange(squaresInPin);
                            }
                            break;
                        }
                    } else {
                        AttackedSquares.Add(targetSquare);
                        squaresInCheck.Add(targetSquare);
                        if (BitboardUtils.IsSquareOccupied(board.AllPiecesBitboard[board.CurrentColorIndex], targetSquare)) {
                            break;
                        }
                        if (BitboardUtils.IsSquareOccupied(board.AllPiecesBitboard[board.OppositeColorIndex], targetSquare)) {
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
            foreach(int startSquare in board.SquaresWithKnights[board.CurrentColorIndex]) {
                foreach (int targetSquare in PrecomputedSquareData.SquaresForKnight[startSquare]) {
                    if (BitboardUtils.IsSquareOccupied(board.PiecesBitboards[board.OppositeColorIndex][Piece.King], targetSquare)) {
                        SquaresInCheck.Add(targetSquare);
                    }
                    AttackedSquares.Add(targetSquare);
                }
            }
        }

        private void GenerateAllKingsAttacks() {
            foreach(int targetSquare in PrecomputedSquareData.SquaresForKing[board.KingSquare[board.CurrentColorIndex]]) {
                AttackedSquares.Add(targetSquare);
            }
        }
    }
}