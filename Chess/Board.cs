﻿using Core;

namespace Chess {

    public class Board {
        
        public Dictionary<int, uint> AllSquaresWithPieces = new(); //{square, piece}
        public int[] KingSquare = new int[2];
        public List<int>[] SquaresWithPawns = new List<int>[2];
        public List<int>[] SquaresWithKnights = new List<int>[2];
        public List<int>[] SquaresWithBishops = new List<int>[2];
        public List<int>[] SquaresWithRooks = new List<int>[2];
        public List<int>[] SquaresWithQueens = new List<int>[2];
        public ulong[] AllPiecesBitboard = new ulong[2]; //{all white pieces bitboard, all black pieces bitboard} 
        public Dictionary<uint, ulong>[] PiecesBitboards = new Dictionary<uint, ulong>[2]; //{{piece, bitboard}, {piece, bitboard}}
        public ulong EmptySquaresBitboard; // 0000000000000000111111111111111111111111111111110000000000000000 or smth like that
        public ulong AllOccupiedSquaresBitboard; //1111111111111111000000000000000000000000000000001111111111111111 or smth like that


        public uint CurrentColor;
        public uint OppositeColor;
        public int CurrentColorIndex;
        public int OppositeColorIndex;
        public bool IsWhite;

        public GameStatus GameState;

        public CastlingStatus[] CastlingStates;

        public Board()
        {
            ClearSquares();
            CurrentColor = Piece.White;  
            UpdateColors();   
            GameState = GameStatus.Running;       

            CastlingStates = new CastlingStatus[2];
        }


        private void ClearSquares() {
            AllSquaresWithPieces.Clear();

            for (int index = 0; index < 2; index ++) {
                
                AllPiecesBitboard[index] = 0;

                PiecesBitboards[index] = new Dictionary<uint, ulong>
                {
                    { Piece.King, 0 },
                    { Piece.Pawn, 0 },
                    { Piece.Knight, 0 },
                    { Piece.Bishop, 0 },
                    { Piece.Rook, 0 },
                    { Piece.Queen, 0 }
                };

                KingSquare[index] = -1;
                SquaresWithPawns[index] = new List<int>();
                SquaresWithKnights[index] = new List<int>();
                SquaresWithBishops[index] = new List<int>();
                SquaresWithRooks[index] = new List<int>();
                SquaresWithQueens[index] = new List<int>();
            }

            EmptySquaresBitboard = GetEmptySquaresBitboard();
            AllOccupiedSquaresBitboard = ~EmptySquaresBitboard;
        }

        public void UpdateColors() {
            OppositeColor = Piece.GetOppositeColor(CurrentColor);
            IsWhite = CurrentColor == Piece.White;
            if (IsWhite) {
                CurrentColorIndex = 0;
                OppositeColorIndex = 1;
            } else {
                CurrentColorIndex = 1;
                OppositeColorIndex = 0;
            }
        }

        public void SwitchColor() {
            CurrentColor = IsWhite ? Piece.Black : Piece.White;
            UpdateColors();
        }


        private void CreatePiece(uint piece, int square) {
            uint pieceColor = Piece.GetColor(piece);
            uint pieceType = Piece.GetType(piece);
            int colorIndex = pieceColor == Piece.White ? 0: 1;

            AllPiecesBitboard[colorIndex] |= (ulong)1 << square;
            PiecesBitboards[colorIndex][pieceType] |= 1UL << square;

            AllSquaresWithPieces.Add(square, piece);

            switch (pieceType) {
                case 1:
                    KingSquare[colorIndex] = square;
                    break;
                case 2:
                    SquaresWithPawns[colorIndex].Add(square);
                    break;
                case 3:
                    SquaresWithKnights[colorIndex].Add(square);
                    break;
                case 4:
                    SquaresWithBishops[colorIndex].Add(square);
                    break;
                case 5:
                    SquaresWithRooks[colorIndex].Add(square);
                    break;
                case 6:
                    SquaresWithQueens[colorIndex].Add(square);
                    break;
                default:
                    Console.WriteLine($"Not good1");
                    return;
            }
        }

        private void DelPiece(uint piece, int square) {
            uint pieceColor = Piece.GetColor(piece);
            uint pieceType = Piece.GetType(piece);
            int colorIndex = pieceColor == Piece.White ? 0: 1;

            AllPiecesBitboard[colorIndex] &= ~(1UL << square);
            PiecesBitboards[colorIndex][pieceType] &= ~(1UL << square);

            AllSquaresWithPieces.Remove(square);

            switch (pieceType) {
                case 1:
                    KingSquare[colorIndex] = -1;
                    break;
                case 2:
                    SquaresWithPawns[colorIndex].Remove(square);
                    break;
                case 3:
                    SquaresWithKnights[colorIndex].Remove(square);
                    break;
                case 4:
                    SquaresWithBishops[colorIndex].Remove(square);
                    break;
                case 5:
                    SquaresWithRooks[colorIndex].Remove(square);
                    break;
                case 6:
                    SquaresWithQueens[colorIndex].Remove(square);
                    break;
                default:
                    Console.WriteLine($"Not good2");
                    return;
            }
        }

        public ulong GetPieceBitboard(uint piece) {
            return PiecesBitboards[Piece.GetColorIndex(piece)][Piece.GetType(piece)];
        }

        public uint GetPieceOnSquare(int square) {
            return AllSquaresWithPieces[square];
        }

        public char GetPieceSymOnSquare(int square) {
            return ChessUtils.BinToPieceSym[GetPieceOnSquare(square)];
        }

        public ulong GetEmptySquaresBitboard() {
            return ~(AllPiecesBitboard[0] | AllPiecesBitboard[1]);
        }


        public void LoadFromFen(string fen_str, bool isFenFull) {
            string[] fen_arr = fen_str.Split(' ');
            List<string> fen_list = new(fen_arr);
            if (!isFenFull) {
                if (fen_list.Count < 5) {
                    fen_list.Add("0");
                }
                if (fen_list.Count < 6) {
                    fen_list.Add("1");
                }
            }
            LoadFenPos(fen_list[0]);
            CurrentColor = fen_list[1] == "w"? Piece.Black: Piece.White; 
            UpdateColors();

            if (fen_list[2] == "-") {
                CastlingStates[0] = CastlingStatus.None;
                CastlingStates[1] = CastlingStatus.None;
                return;
            }

            
            for (int i = 0; i < 2; i++) {
                bool shortSide = fen_list[2].Contains(i == 0 ? 'K': 'k');
                bool longSide = fen_list[2].Contains(i == 0 ? 'Q': 'q');
                if (shortSide && longSide) {
                    CastlingStates[i] = CastlingStatus.BothSides;
                } else if (shortSide) {
                    CastlingStates[i] = CastlingStatus.ShortSide;
                } else {
                    CastlingStates[i] = CastlingStatus.LongSide;
                }
            }

        }

        private void LoadFenPos(string fen) {
            int symIndex = -1;
            bool toBreak = false;
            int y = 7;
            while(y >= 0) {
                int x = 0;
                while(x < 8) {
                    int square = x + y * 8;
                    if (symIndex < fen.Length) {
                        symIndex++;
                        char sym = fen[symIndex];
                        if (sym != '/') {
                            bool isNumeric = char.IsDigit(sym);
                            if (isNumeric) {
                                int symInt = (int) char.GetNumericValue(sym);
                                for (int i = 0; i < symInt; i++) {
                                    x ++;
                                }
                            } else {
                                uint piece = ChessUtils.PieceSymToBin[sym];
                                CreatePiece(piece, square);
                                x++;
                            }
                        }
                    } else {
                        toBreak = true;
                        break;
                    }
                if (toBreak) {
                    break;
                }
                }
            y--;
            }
        }

        public void Print(bool isBitboard = false, ulong bitboard = 0) {
            if (Konfig.DoTerminalOutput) {
                Console.WriteLine(ChessUtils.DecorativeRow);
                for (int y = 7; y >= 0; y--) {
                    Console.Write("|");
                    for (int x = 0; x < 8; x++) {
                        int square = x + y * 8;
                        char pieceSym;
                        if (!isBitboard) {
                            pieceSym = AllSquaresWithPieces.ContainsKey(square)? GetPieceSymOnSquare(square): ' ';
                        } else {
                            pieceSym = BitboardUtils.IsSquareOccupied(bitboard, square)? '1': '0';
                        }
                        Console.Write($"  {pieceSym}  |");
                    }
                    Console.WriteLine($" {y + 1}");
                    Console.WriteLine(ChessUtils.DecorativeRow);
                }
                Console.WriteLine(ChessUtils.DecorativeLettersRow);
            }

        }
    }

}