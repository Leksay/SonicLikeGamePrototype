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
			var points = _spline.GetPoints();
			foreach (var point in points)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawSphere(point.position, 0.1f);
				Gizmos.color = Color.red;
				Gizmos.DrawLine(point.position, point.position + point.normal * 0.5f);
			}
		}
	}
}
