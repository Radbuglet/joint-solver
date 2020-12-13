using System.Collections.Generic;
using System.Text;
using Godot;

namespace JointSolver.core
{
	// TODO: This solver is trash! Replace this once I learn about matrices!
	public class AlgebraSolver<TMemberId>
	{
		public class Equation
		{
			public Vector2 ConstantTerms;
			public readonly List<EquationPart> UnsolvedTerms;

			public Equation(Vector2 constantTerms, List<EquationPart> unsolvedTerms)
			{
				ConstantTerms = constantTerms;
				UnsolvedTerms = unsolvedTerms;
			}
		}

		public class EquationPart
		{
			public readonly float Cosine;
			public readonly float Sine;
			public readonly TMemberId Member;

			public EquationPart(TMemberId member, float cosine, float sine)
			{
				Member = member;
				Cosine = cosine;
				Sine = sine;
			}

			public Vector2 GetValueGiven(float stress)
			{
				return new Vector2(Cosine, Sine) * stress;
			}
		}

		public IDictionary<TMemberId, float> SolvedValues { get; } = new Dictionary<TMemberId, float>();
		public List<Equation> RemainingEquations { get; } = new List<Equation>();
		public bool HasSolvedEverything => RemainingEquations.Count == 0;

		public void Clear()
		{
			SolvedValues.Clear();
			RemainingEquations.Clear();
		}
		
		public void AddEquation(Equation equation)
		{
			RemainingEquations.Add(equation);
		}

		public void Solve()
		{
			while (true)
			{
				var preBatchCount = RemainingEquations.Count;
				
				// Try to solve at least one of the remaining equations
				RemainingEquations.RemoveAll(eq =>
				{
					// Substitute already solved terms
					eq.UnsolvedTerms.RemoveAll(part =>
					{
						if (!SolvedValues.TryGetValue(part.Member, out var value)) return false;
						eq.ConstantTerms += part.GetValueGiven(value);
						return true;
					});

					if (eq.UnsolvedTerms.Count == 0) return true;

					// Try to solve new unknowns
					if (eq.UnsolvedTerms.Count <= 2)
					{
						var aCos = eq.UnsolvedTerms[0].Cosine;
						var aSin = eq.UnsolvedTerms[0].Sine;
						var bCos = eq.UnsolvedTerms.Count == 1 ? 0 : eq.UnsolvedTerms[1].Cosine;
						var bSin = eq.UnsolvedTerms.Count == 1 ? 0 : eq.UnsolvedTerms[1].Sine;

						// Solve for special case
						if (aCos != 0 && aSin != 0 && bCos != 0 && bSin != 0)
						{
							SolvedValues[eq.UnsolvedTerms[0].Member] = (bSin * eq.ConstantTerms.x - aSin * eq.ConstantTerms.y) 
							                                           / (aCos * bSin - bCos * aSin);
						
							SolvedValues[eq.UnsolvedTerms[1].Member] = (bCos * eq.ConstantTerms.x - aCos * eq.ConstantTerms.y)
							                                           / (bCos * aSin - aCos * bSin);

							return true;
						}

						// TODO: Ew, copy-paste. It's been a long time since I had to do something like this.
						// Try to simple-solve, starting with F_a
						if (aCos != 0 && bCos == 0)
						{
							// Solve F_a (horizontally)
							var aSln = SolvedValues[eq.UnsolvedTerms[0].Member] = -eq.ConstantTerms.x / aCos;

							// Try to solve F_b (vertically)
							if (bSin == 0)
								return true; // We can't solve b, only a.

							SolvedValues[eq.UnsolvedTerms[1].Member] = -(eq.ConstantTerms.y + aSln * aSin) / bSin;
							return true;
						}

						if (aSin != 0 && bSin == 0)
						{
							// Solve F_a (vertically)
							var aSln = SolvedValues[eq.UnsolvedTerms[0].Member] = -eq.ConstantTerms.y / aSin;

							// Try to solve F_b (horizontally)
							if (bCos == 0)
								return true; // We can't solve b, only a.

							SolvedValues[eq.UnsolvedTerms[1].Member] = -(eq.ConstantTerms.x + aSln * aCos) / bCos;
							return true;
						}

						// Try to simple-solve, starting with F_b
						if (bCos != 0 && aCos == 0)
						{
							// Solve F_b (horizontally)
							var bSln = SolvedValues[eq.UnsolvedTerms[1].Member] = -eq.ConstantTerms.x / bCos;

							// Try to solve F_a (vertically)
							if (aSin == 0)
								return true; // We can't solve a, only b.

							SolvedValues[eq.UnsolvedTerms[0].Member] = -(eq.ConstantTerms.y + bSln * bSin) / aSin;
							return true;
						}

						if (bSin != 0 && aSin == 0)
						{
							// Solve F_b (vertically)
							var bSln = SolvedValues[eq.UnsolvedTerms[1].Member] = -eq.ConstantTerms.y / bSin;

							// Try to solve F_a (horizontally)
							if (aCos == 0)
								return true; // We can't solve a, only b.

							SolvedValues[eq.UnsolvedTerms[0].Member] = -(eq.ConstantTerms.x + bSln * bCos) / aCos;
							return true;
						}

						// Give up on solving this for now
						return false;
					}

					return false;
				});

				// Break if we're unable to proceed with solving.
				if (preBatchCount == RemainingEquations.Count || HasSolvedEverything)
					break;
			}
		}

		public void ShowDebug()
		{
			GD.Print("Unsolved equations:");
			foreach (var eq in RemainingEquations)
			{
				{
					var builder = new StringBuilder();
					builder.Append("\t0 = ").Append(eq.ConstantTerms.x);
					foreach (var term in eq.UnsolvedTerms)
					{
						builder.Append(" + ").Append(term.Cosine).Append(" * ").Append(term.Member.GetDbgName());
					}
					GD.Print(builder.ToString());
				}

				{
					var builder = new StringBuilder();
					builder.Append("\t0 = ").Append(eq.ConstantTerms.y);
					foreach (var term in eq.UnsolvedTerms)
					{
						builder.Append(" + ").Append(term.Sine).Append(" * ").Append(term.Member.GetDbgName());
					}
					GD.Print(builder.ToString());
				}
			}
			
			GD.Print("Solved members:");
			foreach (var pair in SolvedValues)
			{
				GD.Print("\t", pair.Key.GetDbgName() ?? "Unnamed", " = ", pair.Value);
			}
		}
	}
}