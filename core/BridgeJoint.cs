using Godot;

namespace JointSolver.core
{
	[Tool]
	public class BridgeJoint : Node2D
	{
		[Export] public bool LockX;
		[Export] public bool LockY;
		[Export] public Vector2 AppliedForce;

		public override void _Process(float delta)
		{
			Update();
		}

		public override void _Draw()
		{
			const float radius = 10;
			const float lockBoxWidth = 5;
			
			// Draw joint
			DrawCircle(Vector2.Zero, radius, Colors.Gray);

			// Draw locks
			if (LockX) DrawLine(radius * Vector2.Left, radius * Vector2.Right, Colors.Red, lockBoxWidth);
			if (LockY) DrawLine(radius * Vector2.Up, radius * Vector2.Down, Colors.Green, lockBoxWidth);

			// Draw applied force arrow
			var arrowColor = Colors.Blue;
			var tailRel = AppliedForce * 10;
			const float openingAngle = 30;
			const float pointLen = 10;
			const float weight = 2;
			
			DrawLine(Vector2.Zero, tailRel, arrowColor, weight);
			DrawLine(tailRel, tailRel - AppliedForce.Normalized().Rotated(Mathf.Deg2Rad(-openingAngle)) * pointLen, arrowColor, weight);
			DrawLine(tailRel, tailRel - AppliedForce.Normalized().Rotated(Mathf.Deg2Rad( openingAngle)) * pointLen, arrowColor, weight);
		}

		public float AngleToJoint(BridgeJoint joint)
		{
			return this.GetGlobalPos().AngleTo(joint.GetGlobalPos());
		}
	}
}
