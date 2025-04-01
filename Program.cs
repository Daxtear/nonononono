using System;

namespace Nonononono {
	internal class Program {
		static void Main(string[] args) {

			try {
				Nonogram n = Nonogram.FromPng(args[0]);

				var moves = n.GetAvailableMoves();

				while (moves.Count > 0) {

					Console.WriteLine(n.MaskToString());

					foreach (var move in moves) {
						n.MaskSet(move.X, move.Y, move.State);
					}

					moves = n.GetAvailableMoves();
				}

				Console.WriteLine(n.MaskToString());
				Console.WriteLine(n.Check());
			}
			catch {
				Console.WriteLine(
					"Nonononono simple nonogram solver by Daxtear.\r\n" +
					"Input a black & white image and see if it is solveable as a nonogram!\r\n" +
					"It is recommended to keep resolutions below about 50px*50px\r\n"
					);
				Console.WriteLine("Usage: Nonononono.exe [image-path]");
			}
		}
	}
}
