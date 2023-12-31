﻿using System.IO.Pipes;
using Core;

namespace Chess {

    public class Board {

        public static readonly Dictionary<char, int> PieceSymToBin = new()
        {
            {' ', Piece.None},
            {'K', Piece.White | Piece.King},
            {'k', Piece.Black | Piece.King},
            {'P', Piece.White | Piece.Pawn},
            {'p', Piece.Black | Piece.Pawn},
            {'N', Piece.White | Piece.Knight},
            {'n', Piece.Black | Piece.Knight},
            {'B', Piece.White | Piece.Bishop},
            {'b', Piece.Black | Piece.Bishop},
            {'R', Piece.White | Piece.Rook},
            {'r', Piece.Black | Piece.Rook},
            {'Q', Piece.White | Piece.Queen},
            {'q', Piece.Black | Piece.Queen}
        };

        public static readonly Dictionary<int, char> BinToPieceSym = 
            PieceSymToBin.ToDictionary(pair => pair.Value, pair => pair.Key);

        public CastlingState[] CastlingStates = new CastlingState[2]; // white, black

        public readonly int[] KingSquare = new int[2];
        public readonly ulong[] AllPiecesBitboard = new ulong[2]; //{all white pieces bitboard, all black pieces bitboard} 
        public readonly ulong[,] PiecesBitboards = CreatePiecesBitboardsArray(); //{{piece, bitboard}, {piece, bitboard}}
        public readonly ulong[] OrthogonalSliders = new ulong[2];
        public readonly ulong[] DiagonalSliders = new ulong[2];
        public ulong EmptySquaresBitboard; // 0000000000000000111111111111111111111111111111110000000000000000 or smth like that
        public ulong AllOccupiedSquaresBitboard; //1111111111111111000000000000000000000000000000001111111111111111 or smth like that
        public readonly int[] Squares = new int[64];


        public int CurrentColor = Piece.White;
        public int OppositeColor = Piece.Black;
        public int CurrentColorIndex = 0;
        public int OppositeColorIndex = 1;
        public bool IsWhite = true;
        private static ulong[,] CreatePiecesBitboardsArray() {
            ulong[,] bitboards = new ulong[2,7]; // elem at index 0 is empty because of indexation issues
            for (int i = 0; i < 2; i++) {
                for(int j = 0; j < 7; j++) {
                    bitboards[i, j] = 0ul;
                }
            }
            return bitboards;
        }

        public Board()
        {
            Init();       
        }


        private void Init() {

            for (int index = 0; index < 2; index ++) {
                
                AllPiecesBitboard[index] = 0;

                OrthogonalSliders[index] = 0;
                DiagonalSliders[index] = 0;

                for(int j = 0; j < 7; j++) {
                    PiecesBitboards[index, j] = 0ul;
                }

                KingSquare[index] = -1;

                CastlingStates[index] = new CastlingState();
            }
            
            WriteEmptyAndOccupiedSquares();
        }

        public void UpdateColors() {
            OppositeColor = Piece.GetOppositeColor(CurrentColor);
            IsWhite = CurrentColor == Piece.White;
            CurrentColorIndex = IsWhite? 0: 1;
            OppositeColorIndex = 1 - CurrentColorIndex;
        }

        public void SwitchColor() {
            CurrentColor = IsWhite ? Piece.Black : Piece.White;
            UpdateColors();
        }


        private void CreatePiece(int piece, int square) {
            int pieceColor = Piece.GetColor(piece);
            int pieceType = Piece.GetType(piece);
            int colorIndex = pieceColor == Piece.White ? 0: 1;

            ulong bit = 1ul << square;

            AllPiecesBitboard[colorIndex] |= bit;
            PiecesBitboards[colorIndex, pieceType] |= bit;
            AllOccupiedSquaresBitboard |= bit;
            EmptySquaresBitboard = ~AllOccupiedSquaresBitboard;
            Squares[square] = piece;

            switch(pieceType) {
                case Piece.King:
                    KingSquare[colorIndex] = square;
                    break;
                case Piece.Rook:
                    OrthogonalSliders[colorIndex] |= bit;
                    break;
                case Piece.Bishop:
                    DiagonalSliders[colorIndex] |= bit;
                    break;
                case Piece.Queen:
                    OrthogonalSliders[colorIndex] |= bit;
                    DiagonalSliders[colorIndex] |= bit;
                    break;
            }
        }

        private void DelPiece(int piece, int square) {
            int pieceColor = Piece.GetColor(piece);
            int pieceType = Piece.GetType(piece);
            int colorIndex = pieceColor == Piece.White ? 0: 1;

            ulong bit = 1ul << square;

            AllPiecesBitboard[colorIndex] &= ~bit;
            PiecesBitboards[colorIndex, pieceType] &= ~bit;
            AllOccupiedSquaresBitboard &= ~bit;
            EmptySquaresBitboard = ~AllOccupiedSquaresBitboard;
            Squares[square] = Piece.None;

            OrthogonalSliders[colorIndex] &= ~bit;
            DiagonalSliders[colorIndex] &= ~bit;

            if (pieceType == Piece.King) {
                KingSquare[colorIndex] = -1;
            }
        }

        public void MakeMove(Move move) {
            int startSquare = move.StartSquare;
            int targetSquare = move.TargetSquare;
            int flag = move.MoveFlag;
            
            int piece = GetPieceOnSquare(startSquare);
            int pieceColor = Piece.GetColor(piece);
            int pieceType = Piece.GetType(piece);

            DelPiece(piece, startSquare);
            DelPiece(GetPieceOnSquare(targetSquare), targetSquare);

            if (move.IsPromotion) {
                piece = pieceColor | flag;
            }
            CreatePiece(piece, targetSquare);
        }

        public ulong GetPieceBitboard(int piece) {
            return PiecesBitboards[Piece.GetColorIndex(piece), Piece.GetType(piece)];
        }

        public int GetPieceOnSquare(int square) {
            return Squares[square];
        }

        private char GetPieceSymOnSquare(int square) {
            return BinToPieceSym[GetPieceOnSquare(square)];
        }

        private ulong GetEmptySquaresBitboard() {
            return ~(AllPiecesBitboard[0] | AllPiecesBitboard[1]);
        }

        public static string SquareToSquareName(int square) {
            return PrecomputedSquareData.SquareNameData.SquareToSquareName[square];
        }

        public static int SquareNameToSquare(string squareName) {
            return PrecomputedSquareData.SquareNameData.SquareNameToSquare[squareName];
        }

        public static int RankFromSquare(int square) {
            return square >> 3;
        }

        public static int FileFromSquare(int square) {
            return square & 0b111;
        }

        private void WriteEmptyAndOccupiedSquares() {
            EmptySquaresBitboard = GetEmptySquaresBitboard();
            AllOccupiedSquaresBitboard = ~EmptySquaresBitboard;
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
                CastlingStates[0] = new CastlingState(false, false);
                CastlingStates[1] = new CastlingState(false, false);
                return;
            }

            
            for (int i = 0; i < 2; i++) {
                bool shortSide = fen_list[2].Contains(i == 0 ? 'K': 'k');
                bool longSide = fen_list[2].Contains(i == 0 ? 'Q': 'q');
                if (shortSide && longSide) {
                    CastlingStates[i] = new CastlingState();
                } else if (shortSide) {
                    CastlingStates[i] = new CastlingState(true, false);
                } else {
                    CastlingStates[i] = new CastlingState(false, true);
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
                                int piece = PieceSymToBin[sym];
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
            WriteEmptyAndOccupiedSquares();
        }

        public void Print(bool isBitboard = false, ulong bitboard = 0) {
            Console.WriteLine($"   {Config.DecorativeLettersRow}");
            if (Config.DoTerminalOutput) {
                Console.WriteLine($"   {Config.DecorativeRow}");
                for (int y = 7; y >= 0; y--) {
                    Console.Write($" {y + 1} ");
                    Console.Write("|");
                    for (int x = 0; x < 8; x++) {
                        int square = x + y * 8;
                        char pieceSym;
                        if (!isBitboard) {
                            pieceSym = Bitboards.IsSquareOccupied(AllOccupiedSquaresBitboard, square) ? GetPieceSymOnSquare(square): ' ';
                        } else {
                            pieceSym = Bitboards.IsSquareOccupied(bitboard, square)? '1': '.';
                        }
                        Console.Write($"  {pieceSym}  |");
                    }
                    Console.WriteLine($" {y + 1} ");
                    Console.WriteLine($"   {Config.DecorativeRow}");
                }
                Console.WriteLine($"   {Config.DecorativeLettersRow}");
            }

        }
    }

}