namespace Chess {
    public class PlayerHuman : Player {

        public PlayerHuman(int color): base(color) {

        }
        public override Move ReceiveMove(){
            Console.Write("Enter move: ");
            string? uci = Console.ReadLine();
            Move output = Move.FromUCI(uci);
            return output;
        }
    }
}