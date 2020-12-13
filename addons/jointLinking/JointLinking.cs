using Godot;
using Godot.Collections;
using JointSolver.core;

namespace JointSolver.addons.jointLinking
{
	[Tool]
	public class JointLinking : EditorPlugin
	{
		public override void _Input(InputEvent e)
		{
			// TODO: Use the official way to make key binds
			if (e is InputEventKey key && key.Pressed && !key.Echo && OS.GetScancodeString(key.Scancode).Equals("L"))
			{
				var selected = new Array<BridgeJoint>();

				foreach (var selection in GetEditorInterface().GetSelection().GetSelectedNodes())
				{
					if (selection is BridgeJoint joint)
					{
						selected.Add(joint);
					}
				}

				if (selected.Count == 2)
				{
					var parent = selected[0].GetParent();
					NodePath pathToMember;

					// Create unscripted node
					{
						var member = new Node2D {Name = "Member"};
						parent.AddChild(member);
						pathToMember = parent.GetPathTo(member);
						member.Owner = GetEditorInterface().GetEditedSceneRoot();
						member.SetScript(GD.Load<Script>("res://core/BridgeMember.cs"));
						// (member is disposed here)
					}
					
					// Configure script
					{
						var member = parent.GetNode<BridgeMember>(pathToMember);
						member.JointAPath = member.GetPathTo(selected[0]);
						member.JointBPath = member.GetPathTo(selected[1]);
					}
				}
			}
		}
	}
}
