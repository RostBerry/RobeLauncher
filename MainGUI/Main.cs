using ChessGUI;
using Core;

namespace GUI {

    public class MainGUI {
        public static void Main(string[] args) {
            using Chess chess = new(Config.ScreenWidth, Config.ScreenHeight, "Chess Session");
            chess.Run();
        }
    }
}