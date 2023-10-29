namespace Chess {
    public readonly struct CastlingState{
        private readonly bool kingSide;
        private readonly bool queenSide;

        public CastlingState() {
            kingSide = true;
            queenSide = true;
        }

        public CastlingState(bool kingSide, bool queenSide) {
            this.kingSide = kingSide;
            this.queenSide = queenSide;
        }

        public readonly bool CanCastleKingSide {
            get {return kingSide;}
        }

        public readonly bool CanCastleQueenSide {
            get {return queenSide;}
        }
    }
}