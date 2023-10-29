namespace Chess {
    public abstract class Player {
        public readonly int color;
        public abstract Move ReceiveMove();
        public Player(int color) {
            this.color = color;
        }
    }
}