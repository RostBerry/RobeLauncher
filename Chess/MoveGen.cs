namespace Chess {
    public class MoveGen {

        private readonly Board board;
        private readonly AttackGenerator attackedSquaresGen;
        private readonly bool doSearchLegalMoves;
        private const int MaxMovesPerPos = 218;
        private readonly Move[] AllMoves = new Move[MaxMovesPerPos];
        private int movesFound;

        public MoveGen(Board board, AttackGenerator attackedSquaresGenerator, bool doSearchLegalMoves) {
            this.board = board;
            attackedSquaresGen = attackedSquaresGenerator;
            this.doSearchLegalMoves = doSearchLegalMoves;
            movesFound = 0;

            GenerateMoves();
        }

        private void AddMove(Move move) {
            AllMoves[movesFound] = move;
            movesFound++;
        }

        public void PrintAllMoves() {
            for(int moveIndex = 0; moveIndex < movesFound; moveIndex++) {
                Console.Write($"{AllMoves[moveIndex]} ");
            }
        }

        private void GenerateMoves() {
            GeneratePseudoLegalMoves();
        }


        private void GeneratePseudoLegalMoves() {
            
            GeneratePawnMoves();
            GenerateAllRayMoves();
            GenerateKnightMoves();
            GenerateKingMoves();
        }


        private void GeneratePawnMoves() {

            ulong pawnsBitboard = board.PiecesBitboards[board.CurrentColorIndex][Piece.Pawn];

            // Moves Forward
            ulong allMovesOneSquare = board.EmptySquaresBitboard & 
                (board.IsWhite ? 
                    Bitboards.GetNorthOffset(pawnsBitboard, 1): 
                    Bitboards.GetSouthOffset(pawnsBitboard, 1));
            ulong allPawnsToMoveOneSquare = 
                board.IsWhite ? 
                    Bitboards.GetSouthOffset(allMovesOneSquare, 1): 
                    Bitboards.GetNorthOffset(allMovesOneSquare, 1);

            ulong allPawnsOnSecondRankToMoveOneSquare = allPawnsToMoveOneSquare & 
                (board.IsWhite ? Bitboards.SecondRankMask: Bitboards.SeventhRankMask);

            while (allPawnsToMoveOneSquare != 0) {
                int startSquare = Bitboards.GetLS1BSquare(allPawnsToMoveOneSquare);
                ulong startBit = 1UL << startSquare;
                int targetSquare = Bitboards.GetLS1BSquare(allMovesOneSquare);
                ulong targetBit = 1UL << targetSquare;
                
                AddMove(new Move(startSquare, targetSquare));

                if (allPawnsOnSecondRankToMoveOneSquare != 0) {
                    int twoSquaresTargetSquare = 
                        board.IsWhite ?
                            startSquare + 16:
                            startSquare - 16;
                    ulong twoSquaresTargetBit = 1UL << twoSquaresTargetSquare;

                    if((twoSquaresTargetBit & board.EmptySquaresBitboard) != 0) {
                        AddMove(new Move(startSquare, twoSquaresTargetSquare));
                    } 

                    allPawnsOnSecondRankToMoveOneSquare &= ~startBit;
                }

                allPawnsToMoveOneSquare &= ~startBit;
                allMovesOneSquare &= ~targetBit;
            }
            ////
            
            // Captures
            ////Left
            ulong allCapturesLeft = board.EmptySquaresBitboard & 
                (board.IsWhite ? 
                    Bitboards.GetNorthWestOffset(pawnsBitboard & Bitboards.NotFirstFileMask, 1):
                    Bitboards.GetSouthEastOffset(pawnsBitboard & Bitboards.NotEighthFileMask, 1));

            ulong allPawnsToCaptureLeft = board.EmptySquaresBitboard & 
                (board.IsWhite ?
                    Bitboards.GetSouthEastOffset(allCapturesLeft, 1):
                    Bitboards.GetNorthWestOffset(allCapturesLeft, 1));

            while (allPawnsToCaptureLeft != 0) {
                int startSquare = Bitboards.GetLS1BSquare(allPawnsToCaptureLeft);
                int targetSquare = Bitboards.GetLS1BSquare(allCapturesLeft);

                ulong startBit = 1UL << startSquare;
                ulong targetBit = 1UL << targetSquare;

                AddMove(new Move(startSquare, targetSquare));

                allPawnsToCaptureLeft &= ~startBit;
                allCapturesLeft &= ~targetBit;
            }

            ////Right
            ulong allCapturesRight = board.EmptySquaresBitboard &
                (board.IsWhite ? 
                    Bitboards.GetNorthEastOffset(pawnsBitboard & Bitboards.NotEighthFileMask, 1):
                    Bitboards.GetSouthWestOffset(pawnsBitboard & Bitboards.NotFirstFileMask, 1));

            ulong allPawnsToCaptureRight = board.EmptySquaresBitboard &
                (board.IsWhite ? 
                    Bitboards.GetSouthWestOffset(allCapturesRight, 1):
                    Bitboards.GetNorthEastOffset(allCapturesRight, 1));

            while (allPawnsToCaptureRight != 0) {
                int startSquare = Bitboards.GetLS1BSquare(allPawnsToCaptureRight);
                int targetSquare = Bitboards.GetLS1BSquare(allPawnsToCaptureRight);

                ulong startBit = 1UL << startSquare;
                ulong targetBit = 1UL << targetSquare;

                AddMove(new Move(startSquare, targetSquare));

                allPawnsToCaptureRight &= ~startBit;
                allCapturesRight &= ~targetBit;
            }
            ////
        }

        private void GenerateAllRayMoves() {

            ulong bishopsBitboard = board.PiecesBitboards[board.CurrentColorIndex][Piece.Bishop];
            ulong rooksBitboard = board.PiecesBitboards[board.CurrentColorIndex][Piece.Rook];
            ulong queensBitboard = board.PiecesBitboards[board.CurrentColorIndex][Piece.Queen];

            while (bishopsBitboard != 0) {
                int startSquare = Bitboards.GetLS1BSquare(bishopsBitboard);
                GenerateRayMovesForPiece(4, 8, startSquare);
                bishopsBitboard &= ~Bitboards.BitFromSquare(startSquare);
            }

            while (rooksBitboard != 0) {
                int startSquare = Bitboards.GetLS1BSquare(rooksBitboard);
                GenerateRayMovesForPiece(0, 4, startSquare);
                rooksBitboard &= ~Bitboards.BitFromSquare(startSquare);
            }

            while (queensBitboard != 0) {
                int startSquare = Bitboards.GetLS1BSquare(queensBitboard);
                GenerateRayMovesForPiece(4, 8, startSquare);
                queensBitboard &= ~Bitboards.BitFromSquare(startSquare);
            }
        }

        private void GenerateRayMovesForPiece(int startIndex, int endIndex, int startSquare) {
            for (int dirIndex = startIndex; dirIndex < endIndex; dirIndex++) {
                for (int n = 0; n < PrecomputedSquareData.SquaresToEdge[startSquare][dirIndex]; n++) {
                    int targetSquare = startSquare + PrecomputedSquareData.MovingOffsets[dirIndex] * (n + 1);
                    if (Bitboards.IsSquareOccupied(board.AllPiecesBitboard[board.CurrentColorIndex], targetSquare)) {
                        break;
                    }
                    if (Bitboards.IsSquareOccupied(board.AllPiecesBitboard[board.OppositeColorIndex], targetSquare)) {
                        AddMove(new Move(startSquare, targetSquare));
                        break;
                    }

                    AddMove(new Move(startSquare, targetSquare));
                }
            }
        }

        private void GenerateKnightMoves() {

            ulong knightsBitboard = board.PiecesBitboards[board.CurrentColorIndex][Piece.Knight];

            while (knightsBitboard != 0) {
                int startSquare = Bitboards.GetLS1BSquare(knightsBitboard);
                foreach(int targetSquare in PrecomputedSquareData.SquaresForKnight[startSquare]) {
                    if (Bitboards.IsSquareOccupied(board.AllPiecesBitboard[board.CurrentColorIndex], targetSquare)) {
                        continue;
                    }
                    if (Bitboards.IsSquareOccupied(board.AllPiecesBitboard[board.OppositeColorIndex], targetSquare)) {
                        AddMove(new Move(startSquare, targetSquare));
                        continue;
                    }
                    AddMove(new Move(startSquare, targetSquare));
                }
                knightsBitboard &= ~Bitboards.BitFromSquare(startSquare);
            }
            
        }

        private void GenerateKingMoves() {
            foreach(int targetSquare in PrecomputedSquareData.SquaresForKing[board.KingSquare[board.CurrentColorIndex]]) {
                if (Bitboards.IsSquareOccupied(board.AllPiecesBitboard[board.CurrentColorIndex], targetSquare)) {
                    continue;
                }
                if (Bitboards.IsSquareOccupied(board.AllPiecesBitboard[board.OppositeColorIndex], targetSquare)) {
                    AddMove(new Move(board.KingSquare[board.CurrentColorIndex], targetSquare));
                    continue;
                }

                AddMove(new Move(board.KingSquare[board.CurrentColorIndex], targetSquare));
            }
        }
    }
}