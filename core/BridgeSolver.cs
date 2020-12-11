using System.Collections.Generic;
using System.Linq;
using Godot;

namespace JointSolver.core
{
	[Tool]
	public class BridgeSolver : Node2D
	{
		private readonly AlgebraSolver<BridgeMember> _solver = new AlgebraSolver<BridgeMember>();

		public override void _Process(float delta)
		{
			CalculateStress();
		}

		private void CalculateStress()
		{
			_solver.Clear();
			
			// Find joint members
			var joints = new Dictionary<BridgeJoint, List<BridgeMember>>();
			var netApplied = Vector2.Zero;
			var locksX = 0;
			var locksY = 0;

			foreach (var descendant in this.GetDescendants())
			{
				if (descendant is BridgeJoint joint)
				{
					if (!joints.ContainsKey(joint))
						joints.Add(joint, new List<BridgeMember>());
					netApplied += joint.AppliedForce;
					if (joint.LockX) locksX++;
					if (joint.LockY) locksY++;
				}
				else if (descendant is BridgeMember member)
				{
					if (member.JointA == null || member.JointB == null) continue;
					joints.GetOrInit(member.JointA, () => new List<BridgeMember>()).Add(member);
					joints.GetOrInit(member.JointB, () => new List<BridgeMember>()).Add(member);
				}
			}
			
			// Generate equations
			foreach (var pair in joints)
			{
				// Setup constant term (solving as a rigid body)
				var constTerm = pair.Key.AppliedForce;
				if (pair.Key.LockX)
					constTerm.x -= netApplied.x / locksX;
				
				if (pair.Key.LockY)
					constTerm.y -= netApplied.y / locksY;
				
				// Setup unsolved terms
				var terms = (from member in pair.Value
					let otherJoint = member.JointA == pair.Key ? member.JointB : member.JointA
					select new AlgebraSolver<BridgeMember>.EquationPart(member, pair.Key.AngleToJoint(otherJoint))).ToList();

				_solver.AddEquation(new AlgebraSolver<BridgeMember>.Equation(constTerm, terms));
			}
			
			// Solve and apply
			_solver.Solve();

			foreach (var pair in _solver.SolvedValues)
			{
				pair.Key.DisplayStress = pair.Value;
			}
		}
	}
}
