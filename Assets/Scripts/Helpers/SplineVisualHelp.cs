using System;
using System.Linq;
using Dreamteck.Splines;
using UnityEditor;
using UnityEngine;
namespace Helpers
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(SplineComputer))]
	public class SplineVisualHelp : MonoBehaviour
	{
		private       SplineComputer _spline;
		public        float          _length     = 1f;
		public static bool           showUp      = true;
		public static bool           showForward = true;
		public static bool           showRight   = true;
		public static bool           showXz      = true;

#if UNITY_EDITOR
		[MenuItem("GameObject/Custom/Spline visual helper", false, -1)]
		public static void AddHelper()
		{
			if (Selection.gameObjects.Length > 0)
				foreach (var go in Selection.gameObjects)
				{
					if (go.GetComponent<SplineVisualHelp>() == null)
						go.AddComponent<SplineVisualHelp>();
				}
			else
			{
				if (Selection.activeGameObject.GetComponent<SplineVisualHelp>() == null)
					Selection.activeGameObject.AddComponent<SplineVisualHelp>();
			}
		}
#endif

		private void OnDrawGizmos() => OnDrawGizmosSelected();
		private void OnDrawGizmosSelected()
		{
			if (_spline == null) _spline = GetComponent<SplineComputer>();
			var points                   = _spline.GetPoints();
			for (var i = 0; i < points.Length; i++)
			{
				var point = _spline.Evaluate(i);
				Gizmos.color = Color.black;
				Gizmos.DrawSphere(point.position, 0.1f);
				if (showUp)
				{
					Gizmos.color = Color.green;
					Gizmos.DrawLine(point.position, point.position + point.normal * _length);
				}
				if (showForward)
				{
					Gizmos.color = Color.blue;
					Gizmos.DrawLine(point.position, point.position + point.direction * _length);
				}
				if (showRight)
				{
					Gizmos.color = Color.red;
					Gizmos.DrawLine(point.position, point.position + point.right * _length);
					if (showXz)
					{
						var r = point.right;
						r.y = 0f;
						Gizmos.DrawLine(point.position, point.position + r * _length);
					}
				}
				if (i > 0)
				{
					var point2 = _spline.Evaluate(i - 1);
					if (showUp)
					{
						Gizmos.color = Color.green;
						Gizmos.DrawLine(point2.position + point2.normal * _length, point.position + point.normal * _length);
					}
					if (showRight)
					{
						Gizmos.color = Color.red;
						Gizmos.DrawLine(point2.position + point2.right * _length, point.position + point.right * _length);
					}
				}
			}
		}
	}
}
