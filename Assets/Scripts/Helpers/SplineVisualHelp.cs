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
		public  float          _length = 1f;

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
				Gizmos.color = Color.green;
				Gizmos.DrawLine(point.position, point.position + point.normal * _length);
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(point.position, point.position + point.direction * _length);
				Gizmos.color = Color.red;
				Gizmos.DrawLine(point.position, point.position + point.right * _length);
				var r = point.right;
				r.y = 0f;
				Gizmos.DrawLine(point.position, point.position + r * _length);
			}
		}
	}
}
