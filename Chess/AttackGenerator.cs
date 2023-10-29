

namespace Chess {
    public class AttackGenerator {
        
        private readonly Board board;

        public ulong AttackedSquares = 0;

        public ulong SquaresToBlockCheck = 0;

        private ulong SquaresInPinX = 0;
        private ulong SquaresInPinY = 0;

        public bool isInCheck = false;
        public bool isInDoubleCheck = false;

        public AttackGenerator(Board board) {
            this.board = board;

            GenerateAllAttacks();
        }

        private void AddAttackedSquare(int square) {
            AttackedSquares |= Bitboards.BitFromSquare(square);
        }

        private void AddAttackedSquare(ulong bit) {
            AttackedSquares |= bit;
        }

        private void AddSquareToBlockCheck(int square) {
            SquaresToBlockCheck |= Bitboards.BitFromSquare(square);
        }

        private void AddSquareToBlockCheck(ulong bit) {
            SquaresToBlockCheck |= bit;
        }

        private void GenerateAllAttacks() {
            
            GenerateAllKingsAttacks();
            GenerateAllPawnAttacks();
            GenerateAllRayAttacks();
            GenerateAllKnightsAttacks();
        }

        private void GenerateAllKingsAttacks() {
            AddAttackedSquare(PrecomputedSquareData.SquaresForKing[board.KingSquare[board.CurrentColorIndex]]);
        }

        private void GenerateAllPawnAttacks() {
            
            ulong pawnsBitboard = board.PiecesBitboards[board.CurrentColorIndex, Piece.Pawn];

            if (pawnsBitboard == 0) {
                return; // if there is no pawns, we can avoid calculating that garbage
            }
            int perspective = board.IsWhite? 1: -1;

            ulong capture1ClampMask;
            ulong capture2ClampMask;

            if (board.IsWhite) {
                capture1ClampMask = Bitboards.NotFirstFileMask;
                capture2ClampMask = Bitboards.NotEighthFileMask;
            } else {
                capture1ClampMask = Bitboards.NotEighthFileMask;
                capture2ClampMask = Bitboards.NotFirstFileMask;
            }

            int squaresToCapture1 = 7 * perspective;
            int squaresToCapture2 = 9 * perspective;

            ulong capture1 = Bitboards.Shift(pawnsBitboard & capture1ClampMask, squaresToCapture1);
            ulong capture2 = Bitboards.Shift(pawnsBitboard & capture2ClampMask, squaresToCapture2);

            AddAttackedSquare(capture1);
            AddAttackedSquare(capture2);

            ulong pawnChecks1 = Bitboards.Shift(capture1 & board.PiecesBitboards[board.OppositeColorIndex, Piece.King], -squaresToCapture1);
            ulong allPawnChecks = Bitboards.Shift(capture2 & board.PiecesBitboards[board.OppositeColorIndex, Piece.King], -squaresToCapture2) | pawnChecks1;

            if (allPawnChecks != 0) {
                isInCheck = true;
                AddSquareToBlockCheck(allPawnChecks);
            }
        }

        private void GenerateAllRayAttacks() {
            GenerateRayPieceAttacks(board.OrthogonalSliders[board.CurrentColorIndex], false);
            GenerateRayPieceAttacks(board.DiagonalSliders[board.CurrentColorIndex], true);
        }

        private void GenerateRayPieceAttacks(ulong pieceBitboard, bool isBishop) {
            while (pieceBitboard != 0) {
                int startSquare = Bitboards.GetLS1BSquare(pieceBitboard);
                pieceBitboard &= ~Bitboards.BitFromSquare(startSquare);

                ulong legalMovesBB = MagicBitboardsData.GetRayPieceLegalMoves(startSquare, board.AllOccupiedSquaresBitboard, isBishop);

                AddAttackedSquare(legalMovesBB);
                if (isInDoubleCheck) {
                    continue;
                }

                if ((legalMovesBB & board.PiecesBitboards[board.OppositeColorIndex, Piece.King]) != 0) {
                    AddSquareToBlockCheck(PrecomputedSquareData.LinesFromSquareToSquareBB[startSquare, 
                                            board.KingSquare[board.OppositeColorIndex]] & 
                                            ~board.PiecesBitboards[board.OppositeColorIndex, Piece.King]);
                    isInDoubleCheck = isInCheck;
                    isInCheck = true;
                }
            }
        }

        private void GenerateAllKnightsAttacks() {

            ulong knightsBitboard = board.PiecesBitboards[board.CurrentColorIndex, Piece.Knight];

            while (knightsBitboard != 0) {
                int startSquare = Bitboards.GetLS1BSquare(knightsBitboard);
                ulong startBit = Bitboards.BitFromSquare(startSquare);
                knightsBitboard &= ~startBit;

                ulong possibleAttacks = PrecomputedSquareData.SquaresForKnight[startSquare];
                
                AddAttackedSquare(possibleAttacks);

                if(isInDoubleCheck) {
                    continue;
                }

                if ((possibleAttacks & board.PiecesBitboards[board.OppositeColorIndex, Piece.King]) != 0) {
                    AddSquareToBlockCheck(startBit);
                    isInDoubleCheck = isInCheck;
                    isInCheck = true;
                }
            }
        }

        public void Print() {
            Console.WriteLine("All attacked squares: ");
            board.Print(true, AttackedSquares);
            Console.WriteLine($"({Bitboards.GetBitsCount(AttackedSquares)} squares)");

            Console.WriteLine("All squares in check: ");
            board.Print(true, SquaresToBlockCheck);
            Console.WriteLine($"({Bitboards.GetBitsCount(SquaresToBlockCheck)} squares)");

            Console.WriteLine("All squares in X pin: ");
            board.Print(true, SquaresInPinX);
            Console.WriteLine($"({Bitboards.GetBitsCount(SquaresInPinX)} squares)");

            Console.WriteLine("All squares in Y pin: ");
            board.Print(true, SquaresInPinY);
            Console.WriteLine($"({Bitboards.GetBitsCount(SquaresInPinY)} squares)");
        }
    }
}