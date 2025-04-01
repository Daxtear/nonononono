using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Nonononono {
	public class Nonogram {

		public class Analysis {
			public AnalysisState[] States;
		}

		public enum AnalysisState {
			None,
			Mixed,
			Filled,
			Blocked
		}

		public enum MaskState {
			None,
			Filled,
			Blocked
		}

		public struct Move {
			public int X;
			public int Y;
			public MaskState State;
		}

		public int Width;
		public int Height;
		public bool[] Template;

		public int[][] VHints;
		public int[][] HHints;

		public MaskState[] Mask;

		public Nonogram(int width, int height, bool[] values) {
			Width = width;
			Height = height;
			Template = values;

			VHints = new int[Width][];
			for(int a = 0; a < Width; ++a) {
				List<int> hints = new List<int>();
				int currentHint = 0;

				for(int b = 0; b < Height; ++b) {
					if(TemplateAt(a,b)) {
						currentHint++;
					}
					else {
						if(currentHint > 0) {
							hints.Add(currentHint);
							currentHint = 0;
						}
					}
				}

				if(currentHint > 0 || hints.Count == 0)
					hints.Add(currentHint);

				VHints[a] = hints.ToArray();
			}

			HHints = new int[Height][];
			for (int b = 0; b < Height; ++b) {
				List<int> hints = new List<int>();
				int currentHint = 0;

				for (int a = 0; a < Width; ++a) {
					if (TemplateAt(a, b)) {
						currentHint++;
					}
					else {
						if (currentHint > 0) {
							hints.Add(currentHint);
							currentHint = 0;
						}
					}
				}

				if (currentHint > 0 || hints.Count == 0)
					hints.Add(currentHint);

				HHints[b] = hints.ToArray();
			}

			Mask = new MaskState[values.Length];
		}

		public static Nonogram FromPng(string path) {

			Bitmap image = Bitmap.FromFile(path) as Bitmap;
			bool[] values = new bool[image.Width * image.Height];
			for(int x = 0; x < image.Width; ++x) {
				for (int y = 0; y < image.Height; ++y) {
					Color clr = image.GetPixel(x, y);
					values[x + y * image.Width] = clr.ToArgb() == Color.Black.ToArgb();
				}
			}

			return new Nonogram(image.Width, image.Height, values);
		}

		public bool TemplateAt(int x, int y) {
			if (x < 0 || x >= Width || y < 0 || y >= Height)
				throw new Exception("TemplateAt out of range: " + x + "|" + y);
			return Template[x + y * Width];
		}

		public MaskState MaskAt(int x, int y) {
			if (x < 0 || x >= Width || y < 0 || y >= Height)
				throw new Exception("MaskAt out of range: " + x + "|" + y);
			return Mask[x + y * Width];
		}

		public void MaskSet(int x, int y, MaskState state) {
			if (x < 0 || x >= Width || y < 0 || y >= Height)
				throw new Exception("MaskSet out of range: " + x + "|" + y);
			Mask[x + y * Width] = state;
		}

		public List<Move> GetAvailableMoves() {
			List<Move> moves = new List<Move>();

			for(int row = 0; row < Height; ++row) {
				if (HHints[row][0] == 0)
					continue;

				Analysis analysis = new Analysis();
				analysis.States = new AnalysisState[Width];

				for (int a = 0; a < Width - (HHints[row].Sum() + HHints[row].Length - 2); ++a) {
					FindRowMoves(row, HHints[row].Length, 0, a, new int[HHints[row].Length], analysis);
				}

				for(int a = 0; a < Width; ++a) {
					switch (analysis.States[a]) {
						case AnalysisState.None:
							throw new Exception("Wha hapun?");
							break;
						case AnalysisState.Mixed:
							break;
						case AnalysisState.Filled:
							if(MaskAt(a, row) == MaskState.None)
								moves.Add(new Move() { X = a, Y = row, State = MaskState.Filled });
							else if (MaskAt(a, row) == MaskState.Blocked)
								throw new Exception("???");
							break;
						case AnalysisState.Blocked:
							if (MaskAt(a, row) == MaskState.None)
								moves.Add(new Move() { X = a, Y = row, State = MaskState.Blocked });
							else if (MaskAt(a, row) == MaskState.Filled)
								throw new Exception("???");
							break;
					}
				}
			}

			for (int col = 0; col < Width; ++col) {
				if (VHints[col][0] == 0)
					continue;

				Analysis analysis = new Analysis();
				analysis.States = new AnalysisState[Height];

				for (int a = 0; a < Height - (VHints[col].Sum() + VHints[col].Length - 2); ++a) {
					FindColumnMoves(col, VHints[col].Length, 0, a, new int[VHints[col].Length], analysis);
				}

				for (int a = 0; a < Height; ++a) {
					switch (analysis.States[a]) {
						case AnalysisState.None:
							throw new Exception("Wha hapun?");
							break;
						case AnalysisState.Mixed:
							break;
						case AnalysisState.Filled:
							if (MaskAt(col, a) == MaskState.None)
								moves.Add(new Move() { X = col, Y = a, State = MaskState.Filled });
							else if (MaskAt(col, a) == MaskState.Blocked)
								throw new Exception("???");
							break;
						case AnalysisState.Blocked:
							if (MaskAt(col, a) == MaskState.None)
								moves.Add(new Move() { X = col, Y = a, State = MaskState.Blocked });
							else if (MaskAt(col, a) == MaskState.Filled)
								throw new Exception("???");
							break;
					}
				}
			}

			return moves;
		}

		public void FindRowMoves(int row, int rowhints, int hint, int pos, int[] positions, Analysis analysis) {
			int hintvalue = HHints[row][hint];

			if (pos + hintvalue > Width)
				return;

			if (hint == 0) {
				for (int a = 0; a < pos; ++a) {
					if (MaskAt(a, row) == MaskState.Filled)
						return;
				}
			}

			for (int a = 0; a < hintvalue; ++a) {
				if (MaskAt(pos + a, row) == MaskState.Blocked)
					return;
			}

			positions[hint] = pos;

			if(hint == rowhints - 1) {

				for (int a = pos + hintvalue; a < Height; ++a) {
					if (MaskAt(a, row) == MaskState.Filled)
						return;
				}

				int nexthint = 0;
				for(int a = 0; a < Width; ++a) {
					if (nexthint < rowhints && positions[nexthint] == a) {
						for(int b = a + HHints[row][nexthint]; a < b; ++a) {
							switch (analysis.States[a]) {
								case AnalysisState.None:
									analysis.States[a] = AnalysisState.Filled;
									break;
								case AnalysisState.Blocked:
									analysis.States[a] = AnalysisState.Mixed;
									break;
								default:
									break;
							}
						}
						a--;
						nexthint++;
					}
					else {
						switch(analysis.States[a]) {
							case AnalysisState.None:
								analysis.States[a] = AnalysisState.Blocked;
								break;
							case AnalysisState.Filled:
								analysis.States[a] = AnalysisState.Mixed;
								break;
							default:
								break;
						}
					}
				}
			}
			else {

				int nextstartpos = pos + hintvalue + 1;
				
				for(int a = nextstartpos; a < Width - (HHints[row].Skip(hint + 1).Sum() + HHints[row].Skip(hint + 1).Count() - 2); ++a) {

					if (MaskAt(a - 1, row) == MaskState.Filled)
						break;

					FindRowMoves(row, rowhints, hint + 1, a, positions, analysis);
				}
			}
		}

		public void FindColumnMoves(int col, int colhints, int hint, int pos, int[] positions, Analysis analysis) {
			int hintvalue = VHints[col][hint];

			if (pos + hintvalue > Height)
				return;

			if(hint == 0) {
				for(int a = 0; a < pos; ++a) {
					if (MaskAt(col, a) == MaskState.Filled)
						return;
				}
			}

			for (int a = 0; a < hintvalue; ++a) {
				if (MaskAt(col, pos + a) == MaskState.Blocked)
					return;
			}

			positions[hint] = pos;

			if (hint == colhints - 1) {

				for (int a = pos + hintvalue; a < Width; ++a) {
					if (MaskAt(col, a) == MaskState.Filled)
						return;
				}

				int nexthint = 0;
				for (int a = 0; a < Height; ++a) {
					if (nexthint < colhints && positions[nexthint] == a) {
						for (int b = a + VHints[col][nexthint]; a < b; ++a) {
							switch (analysis.States[a]) {
								case AnalysisState.None:
									analysis.States[a] = AnalysisState.Filled;
									break;
								case AnalysisState.Blocked:
									analysis.States[a] = AnalysisState.Mixed;
									break;
								default:
									break;
							}
						}
						a--;
						nexthint++;
					}
					else {
						switch (analysis.States[a]) {
							case AnalysisState.None:
								analysis.States[a] = AnalysisState.Blocked;
								break;
							case AnalysisState.Filled:
								analysis.States[a] = AnalysisState.Mixed;
								break;
							default:
								break;
						}
					}
				}
			}
			else {

				int nextstartpos = pos + hintvalue + 1;

				for (int a = nextstartpos; a < Height - (VHints[col].Skip(hint + 1).Sum() + VHints[col].Skip(hint + 1).Count() - 2); ++a) {

					if (MaskAt(col, a - 1) == MaskState.Filled)
						return;

					FindColumnMoves(col, colhints, hint + 1, a, positions, analysis);
				}
			}
		}

		public string MaskToString() {
			string str = string.Empty;
			for(int a = 0; a < Height; ++a) {
				for (int b = 0; b < Width; ++b) {
					switch (MaskAt(b, a)) {
						case MaskState.None:
							str += " ";
							break;
						case MaskState.Filled:
							str += "#";
							break;
						case MaskState.Blocked:
							str += ".";
							break;
					}
				}
				str += Environment.NewLine;
			}
			return str;
		}

		public bool Check() {
			for(int a = 0; a < Template.Length; ++a) {
				if (Template[a] && Mask[a] != MaskState.Filled)
					return false;
				if (!Template[a] && Mask[a] == MaskState.Filled)
					return false;
			}
			return true;
		}
	}
}
