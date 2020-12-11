using System;
using System.Collections.Generic;
using Godot;

namespace JointSolver.core
{
	public static class Utils
	{
		public static IEnumerable<Node> IterChildren(this Node root)
		{
			for (var index = 0; index < root.GetChildCount(); index++)
			{
				yield return root.GetChild(index);
			}
		}
		
		public static IEnumerable<Node> GetDescendants(this Node root)
		{
			foreach (var child in root.IterChildren())
			{
				yield return child;
				
				foreach (var descendant in child.GetDescendants())
				{
					yield return descendant;
				}
			}
		}

		public static Vector2 GetGlobalPos(this Node2D node)
		{
			return node.GlobalTransform.origin;
		}

		public static TV GetOrInit<TK, TV>(this IDictionary<TK, TV> dictionary, TK key, Func<TV> factory)
		{
			if (!dictionary.TryGetValue(key, out var list))
			{
				list = factory();
				dictionary.Add(key, list);
			}

			return list;
		}

		public static string GetDbgName(this object obj)
		{
			return (obj as Node)?.Name ?? obj.ToString();
		}
	}
}