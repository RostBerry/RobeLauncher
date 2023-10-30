namespace Chess {
    public class MoveGen {

        private readonly Board board;
        private readonly AttackGenerator attackedSquaresGen;
        private readonly bool doAllPromotions;
        private const int MaxMovesPerPos = 218;
        private readonly Move[] AllMoves = new Move[MaxMovesPerPos];
        private int movesFound;

        public MoveGen(Board board, AttackGenerator attackedSquaresGenerator, bool doAllPromotions = false) {
            this.board = board;
            attackedSquaresGen = attackedSquaresGenerator;
            this.doAllPromotions = doAllPromotions;
            movesFound = 0;

            GenerateMoves();
        }

        private void AddMove(Move move) {
            AllMoves[movesFound++] = move;
        }

        public void PrintAllMoves() {
            for(int moveIndex = 0; moveIndex < movesFound; moveIndex++) {
                Console.Write($"{AllMoves[moveIndex]} ");
            }
            Console.WriteLine($"({movesFound} moves)");
        }

        private void GenerateMoves() {
            GenerateKingMoves();
            GeneratePawnMoves();
            GenerateAllRayMoves();
            GenerateKnightMoves();
        }

        private void GenerateKingMoves() {
            int startSquare = board.KingSquare[board.CurrentColorIndex];
            ulong possibleMoves = PrecomputedSquareData.SquaresForKing[startSquare] & ~(board.AllPiecesBitboard[board.CurrentColorIndex]);
            while (possibleMoves != 0) {
                int targetSquare = Bitboards.GetLS1BSquare(possibleMoves);
                possibleMoves &= ~Bitboards.BitFromSquare(targetSquare);

                AddMove(new Move(startSquare, targetSquare));
            }

            ulong castlingBlockers = board.AllOccupiedSquaresBitboard;

            if (board.CastlingStates[board.CurrentColorIndex].CanCastleKingSide) {
                ulong kingSideCastlingMask = board.IsWhite? Bitboards.WhiteKingSideCastlingMask: Bitboards.BlackKingSideCastlingMask;
                if ((kingSideCastlingMask & castlingBlockers) == 0) {
                    AddMove(new Move(startSquare, startSquare + 2, Move.Flag.Castling));
                }
            }

            if (board.CastlingStates[board.CurrentColorIndex].CanCastleQueenSide) {
                ulong queenSideCastlingMask = board.IsWhite? Bitboards.WhiteQueenSideCastlingMask: Bitboards.BlackQueenSideCastlingMask;
                if ((queenSideCastlingMask & castlingBlockers) == 0) {
                    AddMove (new Move(startSquare, startSquare - 2, Move.Flag.Castling));
                }
            }
        }


        private void GeneratePawnMoves() {

            ulong pawnsBitboard = board.PiecesBitboards[board.CurrentColorIndex, Piece.Pawn];

            if (pawnsBitboard == 0) {
                return; // if there is no pawns, we can avoid calculating that garbage
            }
            int perspective = board.IsWhite? 1: -1;

            ulong promotionRank = board.IsWhite? Bitboards.EighthRankMask: Bitboards.FirstRankMask;

            // Moves Forward
            int squaresToMove = perspective * 8;
            ulong pushOneSquare = Bitboards.Shift(pawnsBitboard, squaresToMove) & board.EmptySquaresBitboard;

            ulong pushPromotions = pushOneSquare & promotionRank;

            ulong pushNoPromotions = pushOneSquare & ~promotionRank;

            while (pushNoPromotions != 0) {
                int targetSquare = Bitboards.GetLS1BSquare(pushNoPromotions);
                pushNoPromotions &= ~Bitboards.BitFromSquare(targetSquare);

                int startSquare = targetSquare - squaresToMove;

                AddMove(new Move(startSquare, targetSquare));
            }

            ulong pushTwoSquaresTargetRank = board.IsWhite? Bitboards.FourthRankMask: Bitboards.FifthRankMask;

            ulong pushTwoSquares = Bitboards.Shift(pushOneSquare, squaresToMove) & board.EmptySquaresBitboard & pushTwoSquaresTargetRank;

            while (pushTwoSquares != 0) {
                int targetSquare = Bitboards.GetLS1BSquare(pushTwoSquares);
                pushTwoSquares &= ~Bitboards.BitFromSquare(targetSquare);

                int startSquare = targetSquare - squaresToMove * 2;

                AddMove(new Move(startSquare, targetSquare));
            }


            // Captures
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

            ulong capture1 = Bitboards.Shift(pawnsBitboard & capture1ClampMask, squaresToCapture1) & board.AllPiecesBitboard[board.OppositeColorIndex];
            ulong capture2 = Bitboards.Shift(pawnsBitboard & capture2ClampMask, squaresToCapture2) & board.AllPiecesBitboard[board.OppositeColorIndex];

            ulong capture1Promotions = capture1 & promotionRank;
            ulong capture2Promotions = capture2 & promotionRank;

            capture1 &= ~promotionRank;
            capture2 &= ~promotionRank;

            while (capture1 != 0) {
                int targetSquare = Bitboards.GetLS1BSquare(capture1);
                capture1 &= ~Bitboards.BitFromSquare(targetSquare);

                int startSquare = targetSquare - squaresToCapture1;

                AddMove(new Move(startSquare, targetSquare));
            }

            while (capture2 != 0) {
                int targetSquare = Bitboards.GetLS1BSquare(capture2);
                capture1 &= ~Bitboards.BitFromSquare(targetSquare);

                int startSquare = targetSquare - squaresToCapture2;

                AddMove(new Move(startSquare, targetSquare));
            }

            // Promotions

            while (pushPromotions != 0) {
                int targetSquare = Bitboards.GetLS1BSquare(pushPromotions);
                pushPromotions &= ~Bitboards.BitFromSquare(targetSquare);
                int startSquare = targetSquare - squaresToMove;

                GeneratePromotions(startSquare, targetSquare);
            }

            while (capture1Promotions != 0) {
                int targetSquare = Bitboards.GetLS1BSquare(capture1Promotions);
                capture1Promotions &= ~Bitboards.BitFromSquare(targetSquare);

                int startSquare = targetSquare - squaresToCapture1;

                GeneratePromotions(startSquare, targetSquare);
            }

            while (capture2Promotions != 0) {
                int targetSquare = Bitboards.GetLS1BSquare(capture2Promotions);
                capture2Promotions &= ~Bitboards.BitFromSquare(targetSquare);

                int startSquare = targetSquare - squaresToCapture2;

                GeneratePromotions(startSquare, targetSquare);
            }
        }

        private void GeneratePromotions(int startSquare, int targetSquare) {
            AddMove(new Move(startSquare, targetSquare, Move.Flag.QueenPromotion));
            AddMove(new Move(startSquare, targetSquare, Move.Flag.KnightPromotion));

            if (doAllPromotions) {
                AddMove(new Move(startSquare, targetSquare, Move.Flag.KnightPromotion));
                AddMove(new Move(startSquare, targetSquare, Move.Flag.RookPromotion));
                AddMove(new Move( startSquare, targetSquare, Move.Flag.BishopPromotion));
            }
        }

        private void GenerateAllRayMoves() {

            ulong orthogonalSliders = board.OrthogonalSliders[board.CurrentColorIndex];
            ulong diagonalSliders = board.DiagonalSliders[board.CurrentColorIndex];

            while (orthogonalSliders != 0) {
                int startSquare = Bitboards.GetLS1BSquare(orthogonalSliders);
                ulong startBit = Bitboards.BitFromSquare(startSquare);
                orthogonalSliders &= ~startBit;

                ulong legalMovesBB = MagicBitboardsData.GetRookLegalMoves(startSquare, board.AllOccupiedSquaresBitboard) & ~board.AllPiecesBitboard[board.CurrentColorIndex];

                while (legalMovesBB != 0) {
                    int targetSquare = Bitboards.GetLS1BSquare(legalMovesBB);
                    ulong targetBit = Bitboards.BitFromSquare(targetSquare);
                    legalMovesBB &= ~targetBit;

                    AddMove(new Move(startSquare, targetSquare));
                }
            }

            while (diagonalSliders != 0) {
                int startSquare = Bitboards.GetLS1BSquare(diagonalSliders);
                ulong startBit = Bitboards.BitFromSquare(startSquare);
                diagonalSliders &= ~startBit;

                ulong legalMovesBB = MagicBitboardsData.GetBishopLegalMoves(startSquare, board.AllOccupiedSquaresBitboard) & ~board.AllPiecesBitboard[board.CurrentColorIndex];

                while (legalMovesBB != 0) {
                    int targetSquare = Bitboards.GetLS1BSquare(legalMovesBB);
                    ulong targetBit = Bitboards.BitFromSquare(targetSquare);
                    legalMovesBB &= ~targetBit;

                    AddMove(new Move(startSquare, targetSquare));
                }
            }
        }

        private void GenerateKnightMoves() {

            ulong knightsBitboard = board.PiecesBitboards[board.CurrentColorIndex, Piece.Knight];

            while (knightsBitboard != 0) {
                int startSquare = Bitboards.GetLS1BSquare(knightsBitboard);
                ulong possibleMoves = PrecomputedSquareData.SquaresForKnight[startSquare];
                while(possibleMoves != 0) {
                    int targetSquare = Bitboards.GetLS1BSquare(possibleMoves);
                    possibleMoves &= ~Bitboards.BitFromSquare(targetSquare);
                    if (Bitboards.IsSquareOccupied(board.AllPiecesBitboard[board.CurrentColorIndex], targetSquare)) {
                        continue;
                    }
                    AddMove(new Move(startSquare, targetSquare));
                }
                knightsBitboard &= ~Bitboards.BitFromSquare(startSquare);
            }
            
        }

        public bool ContainsMove(Move move) {
            return AllMoves.Contains(move);
        }
    }
}