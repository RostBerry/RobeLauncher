using System.Diagnostics;
using Core;

namespace Chess {
    public static class MoveGenSpeedTest {
        private const int singlePosTestRepeatCount = 100000;

        public static void TestLegalMovesInSinglePos(string fenPos = Config.PerftFen) { // Runs movegen for the same position for 100000 times or smth like that and averages the time
        Board board = new();
        board.LoadFromFen(fenPos, true);
        board.Print();
        AttackGenerator attackGen = new(board);
        board.SwitchColor();
        MoveGen moveGen;

        attackGen.Print();

        Stopwatch stopwatch;
        double[] elapsedNanoseconds = new double[singlePosTestRepeatCount];

        for (int i = 0; i < singlePosTestRepeatCount; i++) {
            stopwatch = new();
            stopwatch.Start();
            moveGen = new(board, attackGen);
            stopwatch.Stop();
            elapsedNanoseconds[i] = (double)stopwatch.ElapsedTicks * 1000000000 / Stopwatch.Frequency;
        }
        Console.WriteLine($"Average time: {elapsedNanoseconds.Average()}\t|\tTime in first iteration: {elapsedNanoseconds[0]}\t|\tTime in last iteration: {elapsedNanoseconds[singlePosTestRepeatCount - 1]}");

        }
    }
}