using System;
using Dreamteck.Splines;
using UnityEngine;
namespace Helpers
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(SplineComputer))]
	public class SplineVisualHelp : MonoBehaviour
	{
		private SplineComputer _spline;

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
				Gizmos.color = Color.red;
				Gizmos.DrawLine(point.position, point.position + point.normal * 0.5f);
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(point.position, point.position + point.direction * 0.5f);
			}
		}
	}
}
