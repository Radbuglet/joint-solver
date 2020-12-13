using Godot;

namespace JointSolver.core
{
	[Tool]
	public class BridgeMember : Node2D
	{
		[Export] public NodePath JointAPath;
		[Export] public NodePath JointBPath;

		public float? DisplayStress;
		public BridgeJoint JointA => JointAPath != null ? GetNodeOrNull<BridgeJoint>(JointAPath) : null;
		public BridgeJoint JointB => JointBPath != null ? GetNodeOrNull<BridgeJoint>(JointBPath) : null;

		public override void _Process(float delta)
		{
			Update();
		}

		public override void _Draw()
		{
			if (JointA == null || JointB == null) return;
			
			DrawSetTransformMatrix(GetGlobalTransform().AffineInverse());
			DrawLine(JointA.GetGlobalPos(), JointB.GetGlobalPos(),
				DisplayStress != null ? DisplayStress.Value < 0 ? Colors.Black : Colors.White : Colors.Magenta, 5);
		}
	}
}
