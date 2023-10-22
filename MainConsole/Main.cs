using Chess;
using Core;
using System.Diagnostics;
using System.Threading;

Board board = new();
board.LoadFromFen(Config.PerftFen, true);
board.Print();
AttackGenerator attackGen = new(board);
board.SwitchColor();
MoveGen moveGen;

attackGen.Print();

Stopwatch stopwatch;
int repeatCount = 100000;
double[] elapsedNanoseconds = new double[repeatCount];

for (int i = 0; i < repeatCount; i++) {
    stopwatch = new();
    stopwatch.Start();
    moveGen = new(board, attackGen, true);
    stopwatch.Stop();
    elapsedNanoseconds[i] = (double)stopwatch.ElapsedTicks * 1000000000 / Stopwatch.Frequency;
}

moveGen = new(board, attackGen, true);

foreach(Move move in moveGen.LegalMoves) {
    Console.Write($"{move} ");
}


Console.WriteLine();
Console.WriteLine($"Average time: {elapsedNanoseconds.Average()}\t|\tTime in first iteration: {elapsedNanoseconds[0]}\t|\tTime in last iteration: {elapsedNanoseconds[repeatCount - 1]}");