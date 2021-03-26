using System;
using System.Linq;
using Data.DataScripts;
using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
	[SerializeField] private TrackObjectsData            _data;
	[SerializeField] private TrackObjectsData.ObjectType type;
	private                  float                       BoostTime  => _data.data.FirstOrDefault(t => t.type == type).time;
	private                  float                       SpeedValue => _data.data.FirstOrDefault(t => t.type == type).value;

	private void OnTriggerEnter(Collider other)
	{
		Array.ForEach(other.GetComponents<IBoostable>(), t => t.BoostSpeed(BoostTime, SpeedValue));
	}
}
