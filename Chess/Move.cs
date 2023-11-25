namespace Chess {
    public struct Move {

        public readonly struct Flag {
            public const int RegularMove = 0;
            public const int EnPassant = 1;
            public const int Castling = 2;
            public const int QueenPromotion = Piece.Queen;
            public const int KnightPromotion = Piece.Knight;
            public const int RookPromotion = Piece.Rook;
            public const int BishopPromotion = Piece.Bishop;
        }

        private readonly ushort value;
        /* 
        Move value looks like this:
          flag  targetSquare  startSquare       
        0b1111__111111________111111
          0-16   0-63          0-63


          This concept is mostly borrowed from Sebastian Lague because it's much better than any of my alternatives
        */

        private const ushort startSquareMask = 0b0000000000111111;
		private const ushort targetSquareMask = 0b0000111111000000;
		private const ushort flagMask = 0b1111000000000000;

        public Move(ushort value) {
            this.value = value;
        }

        public Move(int startSquare, int targetSquare) {
            value = (ushort) (startSquare | targetSquare << 6); // targetSquare is moved to 0b0000111111000000 register
        }

        public Move(int startSquare, int targetSquare, int flag) {
            value = (ushort) (startSquare | targetSquare << 6 | flag << 12); // flag is moved to 0b1111000000000000 register
        }

        public readonly override bool Equals(object? obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            Move other = (Move)obj;
            if (value == other.value) {
                return true;
            }
            return false;
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(value);
        }

        public static bool operator ==(Move thisMove, Move other) {
            return thisMove.Equals(other);
        }

        public static bool operator !=(Move thisMove, Move other) {
            return !(thisMove == other);
        }

        public readonly int StartSquare {
			get {
				return value & startSquareMask;
			}
		}

		public readonly int TargetSquare {
			get {
				return (value & targetSquareMask) >> 6;
			}
		}

        public readonly int MoveFlag {
			get {
				return value >> 12;
			}
		}

        public readonly bool IsPromotion {
			get {
				return Flag.QueenPromotion >= MoveFlag && MoveFlag >= Flag.KnightPromotion;
			}
		}

        public readonly int PromotionPieceType {
			get {
				return IsPromotion? MoveFlag: Flag.RegularMove;
			}
		}

        public static Move InvalidMove{
            get {
                return new Move(0);
            }
        }

        public readonly bool IsInvalid{
            get {
                return value == 0;
            }
        }

        public readonly ushort Value{
            get{
                return value;
            }
        }

        public readonly override string ToString(){
            string flagSym = 3 <= MoveFlag && MoveFlag <= 6 ? new string(Board.BinToPieceSym[MoveFlag | Piece.Black], 1) : "";
            return $"{Board.SquareToSquareName(StartSquare)}{Board.SquareToSquareName(TargetSquare)}{flagSym}";
        }

        public static Move FromUCI(string? uci) {
            if (uci == null) {
                return InvalidMove;
            }
            if (uci.Length >= 4) {
                int startSquare = Board.SquareNameToSquare(uci[..2]);
                int targetSquare = Board.SquareNameToSquare(uci[2..4]);
                int flag = Flag.RegularMove;
                if (uci.Length == 5) {
                    flag = Board.PieceSymToBin[uci[4]];
                }

                return new Move(startSquare, targetSquare, flag);
            }
            return InvalidMove;
        }
    }
}