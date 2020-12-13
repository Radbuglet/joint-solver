using Godot;

namespace JointSolver.core
{
	[Tool]
	public class BridgeMember : Node2D
	{
		[Export] public NodePath JointAPath;
		[Export] public NodePath JointBPath;
		[Export] public float MaxTension = 20;
		[Export] public float MaxCompression = 20;

		public float? DisplayStress;
		public BridgeJoint JointA => JointAPath != null ? GetNodeOrNull<BridgeJoint>(JointAPath) : null;
		public BridgeJoint JointB => JointBPath != null ? GetNodeOrNull<BridgeJoint>(JointBPath) : null;

		public override void _Process(float delta)
		{
			if (JointA != null && JointB != null)
			{
				this.SetGlobalPos((JointA.GetGlobalPos() + JointB.GetGlobalPos()) / 2f);
			}

			Update();
		}

		public override void _Draw()
		{
			if (JointA == null || JointB == null) return;
			
			DrawSetTransformMatrix(GetGlobalTransform().AffineInverse());
			DrawLine(JointA.GetGlobalPos(), JointB.GetGlobalPos(),
				DisplayStress != null ? GetColor(DisplayStress.Value) : Colors.Magenta, 5);

			if (DisplayStress != null && DisplayStress > MaxTension || DisplayStress < -MaxCompression)
			{
				DrawLine(JointA.GetGlobalPos(), JointB.GetGlobalPos(), Colors.Black, 3);
			}
		}
		
		private Color GetColor(float stress)
		{
			return stress > 0
				? Colors.White.LinearInterpolate(Colors.Red, Mathf.Min(1, stress / MaxTension))
				: Colors.White.LinearInterpolate(Colors.Purple, Mathf.Min(1, -stress / MaxCompression));
		}
	}
}
