using System.Collections.Generic;
using System.Text;
using Godot;

namespace JointSolver.core
{
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

			public EquationPart(TMemberId member, float angle)
			{
				Member = member;
				Cosine = Mathf.Cos(angle);
				Sine = Mathf.Sign(angle);
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

		public bool Solve()
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
					if (eq.UnsolvedTerms.Count == 1)
					{
						SolvedValues[eq.UnsolvedTerms[0].Member] = -eq.ConstantTerms.x / eq.UnsolvedTerms[0].Cosine;
						return true;
					}

					if (eq.UnsolvedTerms.Count == 2)
					{
						var coefA = eq.UnsolvedTerms[0].Cosine;
						var coefB = eq.UnsolvedTerms[0].Sine;
						var coefC = eq.UnsolvedTerms[1].Cosine;
						var coefD = eq.UnsolvedTerms[1].Sine;

						SolvedValues[eq.UnsolvedTerms[0].Member] = (coefD * eq.ConstantTerms.x - coefB * eq.ConstantTerms.y) 
						                                           / (coefA * coefD - coefC * coefB);
						
						SolvedValues[eq.UnsolvedTerms[1].Member] = (coefC * eq.ConstantTerms.x - coefA * eq.ConstantTerms.y)
						                                           / (coefC * coefB - coefA * coefD);
						
						return true;
					}

					return false;
				});

				// Break if we're unable to proceed with solving.
				if (preBatchCount == RemainingEquations.Count || HasSolvedEverything)
					break;
			}
			
			return HasSolvedEverything;
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