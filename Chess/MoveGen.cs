using System.ComponentModel;

namespace Chess {
    public class MoveGen {

        private readonly Board board;
        private readonly AttackedSquaresGenerator attackedSquaresGen;
        public List<Move> LegalMoves = new();
        public readonly List<Move> PseudoLegalMoves = new();

        public MoveGen(Board board, AttackedSquaresGenerator attackedSquaresGenerator) {
            this.board = board;
            attackedSquaresGen = attackedSquaresGenerator;

            GenerateMoves();
        }

        public void ClearMoves() {
            LegalMoves.Clear();
            PseudoLegalMoves.Clear();
        }
        private void GenerateMoves() {
            GeneratePseudoLegalMoves();

            FilterLegalMoves();

            LegalMoves = PseudoLegalMoves;
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
                    BitboardUtils.GetNorthOffset(pawnsBitboard, 1): 
                    BitboardUtils.GetSouthOffset(pawnsBitboard, 1));
            ulong allPawnsToMoveOneSquare = 
                board.IsWhite ? 
                    BitboardUtils.GetSouthOffset(allMovesOneSquare, 1): 
                    BitboardUtils.GetNorthOffset(allMovesOneSquare, 1);

            ulong allPawnsOnSecondRankToMoveOneSquare = allPawnsToMoveOneSquare & 
                (board.IsWhite ? BitboardUtils.SecondRankMask: BitboardUtils.SeventhRankMask);

            while (allPawnsToMoveOneSquare != 0) {
                int startSquare = BitboardUtils.GetLS1BSquare(allPawnsToMoveOneSquare);
                ulong startBit = 1UL << startSquare;
                int targetSquare = BitboardUtils.GetLS1BSquare(allMovesOneSquare);
                ulong targetBit = 1UL << targetSquare;
                
                PseudoLegalMoves.Add(new Move(startSquare, targetSquare));

                if (allPawnsOnSecondRankToMoveOneSquare != 0) {
                    int twoSquaresTargetSquare = 
                        board.IsWhite ?
                            startSquare + 16:
                            startSquare - 16;
                    ulong twoSquaresTargetBit = 1UL << twoSquaresTargetSquare;

                    if((twoSquaresTargetBit & board.EmptySquaresBitboard) != 0) {
                        PseudoLegalMoves.Add(new Move(startSquare, twoSquaresTargetSquare));
                    } 

                    allPawnsOnSecondRankToMoveOneSquare &= ~startBit;
                }

                allPawnsToMoveOneSquare &= ~startBit;
                allMovesOneSquare &= ~targetBit;
            }
            ////
            
            // Captures
            ////Left
            ulong allCapturesLeft = board.AllPiecesBitboard[board.OppositeColorIndex] & 
                (board.IsWhite ? 
                    BitboardUtils.GetNorthWestOffset(pawnsBitboard, 1):
                    BitboardUtils.GetSouthEastOffset(pawnsBitboard, 1));

            ulong allPawnsToCaptureLeft = 
                board.IsWhite ?
                    BitboardUtils.GetSouthEastOffset(allCapturesLeft, 1):
                    BitboardUtils.GetNorthWestOffset(allCapturesLeft, 1);

            while (allPawnsToCaptureLeft != 0) {
                int startSquare = BitboardUtils.GetLS1BSquare(allPawnsToCaptureLeft);
                int targetSquare = BitboardUtils.GetLS1BSquare(allCapturesLeft);

                ulong startBit = 1UL << startSquare;
                ulong targetBit = 1UL << targetSquare;

                PseudoLegalMoves.Add(new Move(startSquare, targetSquare, Move.Capture));

                allPawnsToCaptureLeft &= ~startBit;
                allCapturesLeft &= ~targetBit;
            }

            ////Right
            ulong allCapturesRight = board.AllPiecesBitboard[board.OppositeColorIndex] & 
                (board.IsWhite ? 
                    BitboardUtils.GetNorthEastOffset(pawnsBitboard, 1):
                    BitboardUtils.GetSouthWestOffset(pawnsBitboard, 1));

            ulong allPawnsToCaptureRight = 
                board.IsWhite ? 
                    BitboardUtils.GetSouthWestOffset(allCapturesRight, 1):
                    BitboardUtils.GetNorthEastOffset(allCapturesRight, 1);

            while (allPawnsToCaptureRight != 0) {
                int startSquare = BitboardUtils.GetLS1BSquare(allPawnsToCaptureRight);
                int targetSquare = BitboardUtils.GetLS1BSquare(allPawnsToCaptureRight);

                ulong startBit = 1UL << startSquare;
                ulong targetBit = 1UL << targetSquare;

                PseudoLegalMoves.Add(new Move(startSquare, targetSquare, Move.Capture));

                allPawnsToCaptureRight &= ~startBit;
                allCapturesRight &= ~targetBit;
            }
            ////
        }

        private void GenerateAllRayMoves() {

            foreach(int startSquare in board.SquaresWithBishops[board.CurrentColorIndex]) {
                GenerateRayMovesForPiece(4, 8, startSquare);
            }

            foreach(int startSquare in board.SquaresWithRooks[board.CurrentColorIndex]) {
                GenerateRayMovesForPiece(0, 4, startSquare);
            }

            foreach(int startSquare in board.SquaresWithQueens[board.CurrentColorIndex]) {
                GenerateRayMovesForPiece(0, 8, startSquare);
            }
        }

        private void GenerateRayMovesForPiece(int startIndex, int endIndex, int startSquare) {
            for (int dirIndex = startIndex; dirIndex < endIndex; dirIndex++) {
                for (int n = 0; n < PrecomputedSquareData.SquaresToEdge[startSquare][dirIndex]; n++) {
                    int targetSquare = startSquare + PrecomputedSquareData.MovingOffsets[dirIndex] * (n + 1);
                    if (BitboardUtils.IsSquareOccupied(board.AllPiecesBitboard[board.CurrentColorIndex], targetSquare)) {
                        break;
                    }
                    if (BitboardUtils.IsSquareOccupied(board.AllPiecesBitboard[board.OppositeColorIndex], targetSquare)) {
                        PseudoLegalMoves.Add(new Move(startSquare, targetSquare, Move.Capture));
                        break;
                    }

                    PseudoLegalMoves.Add(new Move(startSquare, targetSquare));
                }
            }
        }

        private void GenerateKnightMoves() {

            foreach(int startSquare in board.SquaresWithKnights[board.CurrentColorIndex]) {
                foreach(int targetSquare in PrecomputedSquareData.SquaresForKnight[startSquare]) {
                    if (BitboardUtils.IsSquareOccupied(board.AllPiecesBitboard[board.CurrentColorIndex], targetSquare)) {
                        continue;
                    }
                    if (BitboardUtils.IsSquareOccupied(board.AllPiecesBitboard[board.OppositeColorIndex], targetSquare)) {
                        PseudoLegalMoves.Add(new Move(startSquare, targetSquare, Move.Capture));
                        continue;
                    }
                    PseudoLegalMoves.Add(new Move(startSquare, targetSquare));
                }
            }
            
        }

        private void GenerateKingMoves() {
            foreach(int targetSquare in PrecomputedSquareData.SquaresForKing[board.KingSquare[board.CurrentColorIndex]]) {
                if (BitboardUtils.IsSquareOccupied(board.AllPiecesBitboard[board.CurrentColorIndex], targetSquare)) {
                    continue;
                }
                if (BitboardUtils.IsSquareOccupied(board.AllPiecesBitboard[board.OppositeColorIndex], targetSquare)) {
                    PseudoLegalMoves.Add(new Move(board.KingSquare[board.CurrentColorIndex], targetSquare, Move.Capture));
                    continue;
                }

                PseudoLegalMoves.Add(new Move(board.KingSquare[board.CurrentColorIndex], targetSquare));
            }
        }

        private void FilterLegalMoves() {

        }
    }
}