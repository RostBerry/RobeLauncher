using Chess;
using System.Diagnostics;

Board board = new();
board.LoadFromFen(ChessUtils.PerftFen, true);
board.Print();
board.SwitchColor();
AttackedSquaresGenerator attackedSquaresGen = new(board);
MoveGen moveGen;
// moveGen.GenerateMoves();

// Stopwatch stopwatch;
// double[] stopwatches = new double[100000];
// for (int i = 0; i < 100000; i++) {
//     stopwatch = new();
//     stopwatch.Start();
//     moveGen = new(board, attackedSquaresGen);
//     stopwatch.Stop();
//     stopwatches[i] = stopwatch.Elapsed.TotalNanoseconds;
// }

// moveGen = new(board, attackedSquaresGen);

// foreach(Move move in moveGen.LegalMoves) {
//     Console.Write($"{move} ");
// }
// Console.WriteLine($"{stopwatches.Average()}");