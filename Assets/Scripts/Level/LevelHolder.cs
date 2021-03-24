using System;
using System.Collections.Generic;
using Dreamteck.Splines;
using Internal;
using Players;
using UnityEngine;
namespace Level
{
	public class LevelHolder : MonoBehaviour, IRegistrable
	{
		[Header("Computer")]
		[SerializeField] private SplineComputer computer;

		[Header("On Road Generated Entities")]
		public List<GameObject> barriers;
		public List<GameObject>     enemies;
		public List<GameObject>     boosters;
		public List<GameObject>     money;
		public List<RoadEntityData> allEntities = new List<RoadEntityData>();

		[Header("Debug")]
		[SerializeField] private bool enableOffsetDebug;
		[SerializeField] private SplineFollower follower;

		//[SerializeField] private float           currentOffset;
		//[SerializeField] private List<float>     pathOffsets = new List<float>();
		//[SerializeField] private List<DeathLoop> deathLoops;

		public SplineComputer[] _lines;
		public float            _lineWidth;

		public enum ObjectType
		{
			Barrier = 0,
			Enemy = 1,
			Coin = 2,
			Booster = 3,
			Unknown = 255
		}

		public ObjectType GetType(GameObject go)
		{
			if (barriers.Contains(go)) return ObjectType.Barrier;
			if (enemies.Contains(go)) return ObjectType.Enemy;
			if (money.Contains(go)) return ObjectType.Coin;
			if (boosters.Contains(go)) return ObjectType.Booster;
			return ObjectType.Unknown;
		}
		
		public SplineComputer GetComputer() => computer;

		public void Init(SplineComputer spline) => computer = spline;

		public void Init(SplineComputer[] splines, float lineWidth)
		{
			_lines     = splines;
			_lineWidth = lineWidth;
		}

		/*
		public int GetOffsetId(float yOffset)
		{
			for (int i = 0; i < pathOffsets.Count; i++)
			{
				if (yOffset == pathOffsets[i])
					return i;
			}
			Debug.LogError($"{yOffset} is not existings");
			return -1;
		}
		public bool TryChangePathId(ref int currentId, SwipeInput.SwipeType swipeType)
		{
			if (swipeType == SwipeInput.SwipeType.Up || swipeType == SwipeInput.SwipeType.Down || swipeType == SwipeInput.SwipeType.Tap)
				return false;

			int nextId = currentId;
			nextId = swipeType == SwipeInput.SwipeType.Left ? nextId  - 1 : nextId;
			nextId = swipeType == SwipeInput.SwipeType.Right ? nextId + 1 : nextId;
			if (nextId >= 0 && nextId < pathOffsets.Count)
			{
				currentId = nextId;
				return true;
			}
			return false;
		} 
		public float GetOffsetById(int offsetId) => offsetId < pathOffsets.Count ? pathOffsets[offsetId] : -1;
		public int AviableRoadCount() => pathOffsets.Count;
		
		public void Init(SplineComputer spline, float[] offsets)
		{
			computer = spline;
			if (offsets != null)
			{
				pathOffsets.Clear();
				pathOffsets.AddRange(offsets);
			}
		}
		public float[] GetPathsOffsets() => pathOffsets.ToArray();
		public bool InDeathLoops(float pecent)
		{
			foreach (var dl in deathLoops)
			{
				if (pecent >= dl.GetStartPercent() && pecent <= dl.GetEndPercent())
				{
					return true;
				}
			}
			return false;
		}
		private void Awake()
		{
			deathLoops = new List<DeathLoop>();
			deathLoops.AddRange(FindObjectsOfType<DeathLoop>());
		}

		private void Start()
		{
			if (pathOffsets.Count == 0)
			{
				throw new System.Exception("pathOffsets.Count == 0");
			}
		}

		private void OnValidate()
		{
			if (enableOffsetDebug)
				follower.motion.offset = new Vector2(0, currentOffset);
		}
		/**/

		private void OnDrawGizmosSelected()
		{
			for (var i = 1; i < _lines.Length; i += 2)
			{
				var points = _lines[i].GetPoints();
				for (var j = 0; j < points.Length; j++)
				{
					var p    = _lines[i].Evaluate(j);
					var pL   = _lines[i - 1].EvaluatePosition(_lines[i - 1].Project(p.position));
					Gizmos.color = PlayerMover.CheckLineSwap(_lines[i - 1], p, _lineWidth) ? Color.green : Color.red;
					Gizmos.DrawLine(p.position, pL);
					if (i + 1 < _lines.Length)
					{
						pL           = _lines[i + 1].EvaluatePosition(_lines[i + 1].Project(p.position));
						Gizmos.color = PlayerMover.CheckLineSwap(_lines[i + 1], p, _lineWidth) ? Color.green : Color.red;
						Gizmos.DrawLine(p.position, pL);
					}
				}
			}
		}

		private void Awake()      => Register();
		public  void Register()   => Locator.Register(typeof(LevelHolder), this);
		public  void Unregister() => Locator.Unregister(typeof(LevelHolder));
		private void OnDestroy()  => Locator.Unregister(typeof(LevelHolder));
	}
}
