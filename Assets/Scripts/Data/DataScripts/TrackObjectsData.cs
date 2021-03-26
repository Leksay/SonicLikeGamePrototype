using System;
using UnityEngine;
namespace Data.DataScripts
{
	[CreateAssetMenu(fileName = "TrackObjectsData", menuName = "Objects/Track objects data", order = 0)]
	public class TrackObjectsData : ScriptableObject
	{
		public enum ObjectType
		{
			Accelerator,
			Obstacle,
			ObstacleHard,
			EnemyGround,
			EnemyFly
		}
		[Serializable] public class SpeedMod
		{
			public ObjectType type;
			public float       time  = 1f;
			public float       value = -10f;
		}
		public SpeedMod[] data;
	}
}
