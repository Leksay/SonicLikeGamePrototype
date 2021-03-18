using System;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Splines;
using UnityEditor;
using UnityEngine;
namespace Helpers
{
	[CreateAssetMenu(fileName = "TrackGenerator", menuName = "Generators/Track generator", order = 0)]
	public class TrackGenerator : ScriptableObject
	{
		/*
		 Track details										
M		R			O			P				X				+			S		A			1			2			3
magnet	railroad	obstacle	obstacle block	enemy ground	enemy air	shield	accelerator	coin down	coin middle	coin up
										
Road directions										
L			R			U		D							
left turn	right turn	up turn	down turn							

		 */
		public enum TrackItem
		{
			None,
			Magnet,       //M
			Obstacle,     //O
			ObstacleHard, //P
			EnemyGround,  //X
			EnemyAir,     //+
			Shield,       //S
			Accelerator,  //A
			CoinLow,      //1
			CoinMiddle,   //2
			CoinHigh,     //3
		}

		public enum TrackSurface
		{
			Normal,
			Railroad, //R
		}

		public enum TrackDir
		{
			None      = 0,
			Left      = 1, //L
			Right     = 2, //R
			Up        = 3, //U
			Down      = 4, //D
			LeftUp    = 5, //LU
			LeftDown  = 6, //LD
			RightUp   = 7, //RU
			RightDown = 8, //RD
		}

		[Serializable] public class TrackSlot
		{
			public TrackItem[]  values;
			public TrackSurface surface;
		}

		[Serializable] public class TrackStep
		{
#if UNITY_EDITOR
			public bool fold = false;
#endif
			public TrackSlot[] Lines;
			public TrackDir    dir;
			public TrackStep(int lines) => Lines = new TrackSlot[lines];
		}

		[Serializable] public class TrackInterval
		{
			public          double       start = 0d;
			public          double       end   = 0d;
			public          TrackSurface type;
			public          int          segments = 0;
			public override string       ToString() => $"{start:F3}-{end:F3} ({segments}):{type}";
		}

		public TextAsset    _inputFile;
		public TrackPrefabs _prefabs;
		public float        _stepLength           = 3f;
		public float        _stepHorizontalRotate = 5f;
		public float        _stepVerticalRotate   = 5f;
		public int          _lines                = 4;
		public float        _linesInterval        = 1f;

		[Serializable] public class TrackPrefabs
		{
			[Serializable] public class MeshData
			{
				public Mesh     mesh;
				public Material material;
				public Vector3  scale = Vector3.one;
			}
			[Serializable] public class TrackObject
			{
				public GameObject prefab;
				public float      offset;
			}
			[Header("Line")]
			public MeshData Normal;
			public MeshData Rails;
			[Header("On track objects")]
			public TrackObject StartLine;
			public TrackObject FinishLine;
			public TrackObject Magnet;
			public TrackObject Obstacle;
			public TrackObject ObstacleBlock;
			public TrackObject EnemyGround;
			public TrackObject EnemyAir;
			public TrackObject Shield;
			public TrackObject Accelerator;
			public TrackObject CoinLow;
			public TrackObject CoinMiddle;
			public TrackObject CoinHigh;
		}

		public TrackStep[] _track;

		private Vector3[] _directions2;

		private void CreateRotations()
		{
			_directions2 = new Vector3[] {
				Vector3.zero,
				new Vector3(0f,                   -_stepHorizontalRotate, 0f), // L
				new Vector3(0f,                   _stepHorizontalRotate,  0f), // R
				new Vector3(-_stepVerticalRotate, 0f,                     0f), // U
				new Vector3(_stepVerticalRotate,  0f,                     0f), // D
				new Vector3(-_stepVerticalRotate, -_stepHorizontalRotate, 0f), //LU
				new Vector3(_stepVerticalRotate,  -_stepHorizontalRotate, 0f), //LD
				new Vector3(-_stepVerticalRotate, _stepHorizontalRotate,  0f), //RU
				new Vector3(_stepVerticalRotate,  _stepHorizontalRotate,  0f), //RD
			};
		}

		private void ParseTrack()
		{
			if (_inputFile == null) return;
			var input1 = _inputFile.text.Split('\n');
			var steps  = 0;
			for (var i = 0; i < input1.Length; i++)
				if (input1[i].StartsWith("-"))
				{
					steps = i;
					break;
				}
			_track = new TrackStep[steps];
			for (var i = 0; i < steps; i++)
			{
				_track[i] = new TrackStep(_lines);
				var line = input1[i].Split(';');
				for (var j = 0; j <= 3; j++)
				{
					_track[i].Lines[j]         = ParseSlotItem(line[j]);
					_track[i].Lines[j].surface = ParseSlotSurface(line[j]);
				}
				_track[i].dir = ParseSlotDir(line[4]);
			}
		}

		public void CreateTrack()
		{
			ParseTrack();
			CreateRotations();
			var holder      = new GameObject("LevelHolder");
			var levelHolder = holder.AddComponent<LevelHolder>();
			var go          = new GameObject("Track", typeof(SplineComputer));
			go.transform.parent = holder.transform;
			var spline = go.GetComponent<SplineComputer>();
			levelHolder.Init(spline, null);

			var points = new List<SplinePoint>(_track.Length);
			var point  = new GameObject().transform;
			for (var i = 0; i < _track.Length; i++)
			{
				var p = new SplinePoint(point.position);
				point.Rotate(_directions2[(int)_track[i].dir]);
				point.position += point.forward * _stepLength;
				points.Add(p);
			}
			spline.SetPoints(points.ToArray());
			spline.type = Spline.Type.BSpline;

			//spline.Rebuild();
			DestroyImmediate(point.gameObject);
			CreateLines(levelHolder);
		}

		private void CreateLines(LevelHolder levelHolder)
		{
			var spline     = levelHolder.GetComputer();
			var totalWidth = _lines * _linesInterval;
			var deviance   = new float[_lines];
			var points     = spline.GetPoints();
			var tracks     = new SplinePoint[_lines][];

			// calc line deviance from center
			for (var i = 0; i < _lines; i++)
			{
				deviance[i] = Mathf.Lerp(-totalWidth / 2f, totalWidth / 2f, i / (float)(_lines - 1));
				tracks[i]   = new SplinePoint[points.Length];
			}
			levelHolder.Init(spline, deviance);

			var trackSurfaces                                 = new List<TrackInterval>[_lines];
			for (var i = 0; i < _lines; i++) trackSurfaces[i] = new List<TrackInterval>();

			// create points for lines & calc intervals for mesh
			for (var i = 0; i < points.Length; i++)
			{
				EditorUtility.DisplayProgressBar("Create points for lines & calc intervals for mesh", $" Point {i + 1} of {points.Length}", i / (float)(points.Length - 1));
				for (var line = 0; line < _lines; line++)
				{
					var currSurface = _track[i].Lines[line].surface;
					if (i == 0)
					{
						var ti = new TrackInterval {
							type     = currSurface,
							start    = 0,
							end      = 0,
							segments = 0
						};
						trackSurfaces[line].Add(ti);
					}
					else if (i == points.Length - 1)
					{
						trackSurfaces[line][trackSurfaces[line].Count - 1].end      = 1d;
						trackSurfaces[line][trackSurfaces[line].Count - 1].segments = i - trackSurfaces[line][trackSurfaces[line].Count - 1].segments;
					}
					else
					{
						var prevSurface = _track[i - 1].Lines[line].surface;
						if (currSurface != prevSurface)
						{
							trackSurfaces[line][trackSurfaces[line].Count - 1].end      = (i) / (double)(points.Length - 1);
							trackSurfaces[line][trackSurfaces[line].Count - 1].segments = i - trackSurfaces[line][trackSurfaces[line].Count - 1].segments;
							var ti2 = new TrackInterval {
								start    = i / (double)(points.Length - 1),
								type     = currSurface,
								segments = i
							};
							trackSurfaces[line].Add(ti2);
						}
					}

					var pos = spline.Project(points[i].position);
					var p   = spline.Evaluate(pos);
					tracks[line][i].position = points[i].position + p.right * deviance[line];
					tracks[line][i].size     = _linesInterval * 0.9f;
					tracks[line][i].normal   = points[i].normal;
				}
			}

			var splines = new List<SplineComputer>(_lines);

			// create splines from lines & set SplineMesh for road bake
			for (var i = 0; i < _lines; i++)
			{
				var go = new GameObject($"Line_{i}");
				go.transform.parent = spline.gameObject.transform;
				var s = go.AddComponent<SplineComputer>();
				splines.Add(s);
				s.SetPoints(tracks[i]);
				s.type = Spline.Type.BSpline;
				s.RebuildImmediate();
				var sm = go.AddComponent<SplineMesh>();
				sm.computer = s;
				sm.RemoveChannel(0);
				var divider = int.MaxValue;
				foreach (var interval in trackSurfaces[i])
				{
					if (divider > interval.segments) divider = interval.segments;
				}
				for (var index = 0; index < trackSurfaces[i].Count; index++)
				{
					EditorUtility.DisplayProgressBar("Create splines from lines & set SplineMesh for road bake", $"Line #{i + 1} of {_lines} :: Interval: {index} of {trackSurfaces[i].Count - 1}", index / (float)(trackSurfaces[i].Count - 1));
					var interval = trackSurfaces[i][index];
					var md       = interval.type == TrackSurface.Railroad ? _prefabs.Rails : _prefabs.Normal;

					var channel = sm.AddChannel($"{interval.type}");
					channel.minScale = md.scale;
					channel.maxScale = md.scale;
					channel.AddMesh(md.mesh);

					channel.count = interval.segments / divider;

					//channel.count    = (int)((interval.end - interval.start) / 0.005d);
					channel.clipFrom = interval.start;
					channel.clipTo   = interval.end;
				}
				EditorUtility.DisplayProgressBar("Create splines from lines & set SplineMesh for road bake", $"Line #{i + 1} of {_lines} :: Build mesh", 0);
				sm.RebuildImmediate(true);
				sm.Rebuild(true);
			}

			// create track objects
			for (var i = 0; i < points.Length; i++)
			{
				for (var j = 0; j < _track[i].Lines.Length; j++)
				{
					foreach (var item in _track[i].Lines[j].values)
					{
						var point = splines[j].Evaluate(splines[j].Project(splines[j].GetPoint(i).position));

						TrackPrefabs.TrackObject prefab = null;
						switch (item)
						{
							case TrackItem.Magnet:
								prefab = _prefabs.Magnet;
								break;
							case TrackItem.Obstacle:
								prefab = _prefabs.Obstacle;
								break;
							case TrackItem.ObstacleHard:
								prefab = _prefabs.ObstacleBlock;
								break;
							case TrackItem.EnemyGround:
								prefab = _prefabs.EnemyGround;
								break;
							case TrackItem.EnemyAir:
								prefab = _prefabs.EnemyAir;
								break;
							case TrackItem.Shield:
								prefab = _prefabs.Shield;
								break;
							case TrackItem.Accelerator:
								prefab = _prefabs.Accelerator;
								break;
						}
						if (prefab != null)
						{
							var obj = Instantiate(prefab.prefab);
							obj.transform.position = point.position + point.normal * prefab.offset;
							obj.transform.rotation = Quaternion.LookRotation(point.direction, Vector3.up);
							obj.transform.parent   = splines[j].transform;
						}
						CreateCoins(_track[i].Lines[j].values.Where(t => t == TrackItem.CoinLow || t == TrackItem.CoinMiddle || t == TrackItem.CoinHigh).ToArray(), point, splines[j].transform);
					}
					EditorUtility.DisplayProgressBar("Create track objects", $"Line #{j + 1} of {_track[i].Lines.Length} :: Point: {i + 1} of {points.Length}", i / (float)(points.Length - 1));
				}
			}
			EditorUtility.ClearProgressBar();
		}

		private void CreateCoins(TrackItem[] coins, SplineResult point, Transform parent)
		{
			if (coins == null || coins.Length == 0) return;
			var distance = _linesInterval / coins.Length;
			for (var i = 0; i < coins.Length; i++)
			{
				TrackPrefabs.TrackObject prefab = null;
				switch (coins[i])
				{
					case TrackItem.CoinLow:
						prefab = _prefabs.CoinLow;
						break;
					case TrackItem.CoinMiddle:
						prefab = _prefabs.CoinMiddle;
						break;
					case TrackItem.CoinHigh:
						prefab = _prefabs.CoinHigh;
						break;
				}
				if (prefab != null)
				{
					var go = Instantiate(prefab.prefab);
					go.transform.position = point.position + point.normal * prefab.offset + point.direction * (distance * i);
					go.transform.rotation = Quaternion.LookRotation(point.direction, Vector3.up);
					go.transform.parent    = parent;
				}
			}
		}

		private TrackSlot ParseSlotItem(string data)
		{
			var result = new TrackSlot {
				values = new TrackItem[data.Length]
			};
			var i = 0;
			foreach (var d in data)
			{
				switch (d)
				{
					case 'M':
						result.values[i++] = TrackItem.Magnet;
						break;
					case 'O':
						result.values[i++] = TrackItem.Obstacle;
						break;
					case 'P':
						result.values[i++] = TrackItem.ObstacleHard;
						break;
					case 'X':
						result.values[i++] = TrackItem.EnemyGround;
						break;
					case '+':
						result.values[i++] = TrackItem.EnemyAir;
						break;
					case 'S':
						result.values[i++] = TrackItem.Shield;
						break;
					case 'A':
						result.values[i++] = TrackItem.Accelerator;
						break;
					case '1':
						result.values[i++] = TrackItem.CoinLow;
						break;
					case '2':
						result.values[i++] = TrackItem.CoinMiddle;
						break;
					case '3':
						result.values[i++] = TrackItem.CoinHigh;
						break;
				}
			}
			return result;
		}
		private TrackSurface ParseSlotSurface(string data)
		{
			foreach (var d in data)
				switch (d)
				{
					case 'R':
						return TrackSurface.Railroad;
				}
			return TrackSurface.Normal;
		}
		private TrackDir ParseSlotDir(string data)
		{
			switch (data)
			{
				case "L": return TrackDir.Left;
				case "R": return TrackDir.Right;
				case "U": return TrackDir.Up;
				case "D": return TrackDir.Down;
				case "LU":
				case "UL": return TrackDir.LeftUp;
				case "LD":
				case "DL": return TrackDir.LeftDown;
				case "RU":
				case "UR": return TrackDir.RightUp;
				case "RD":
				case "DR": return TrackDir.RightDown;
			}
			return TrackDir.None;
		}
	}
}
