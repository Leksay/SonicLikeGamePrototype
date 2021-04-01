using System;
using System.Collections.Generic;
using System.Linq;
using Data.DataScripts;
using Dreamteck.Splines;
#if UNITY_EDITOR
using EasyEditorGUI;
using UnityEditor;
#endif
using Level;
using UnityEngine;
using UnityEngine.Rendering;
namespace Helpers
{
	[CreateAssetMenu(fileName = "TrackGenerator", menuName = "Generators/Track generator", order = 0)]
	public class TrackGenerator : ScriptableObject
	{
		private enum TrackItem
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

		private enum TrackDir
		{
			None = 0,

			Left  = 1, // L
			Right = 2, // R
			Up    = 3, // U
			Down  = 4, // D

			ShiftLeft  = 5, // l
			ShiftRight = 6, // r
			ShiftUp    = 7, // u
			ShiftDown  = 8, // d

			RotateLeft  = 11, // -
			RotateRight = 12, // +

			DeathloopStart = 9,  // S
			DeathloopEnd   = 10, // E

			Fence = 13, // F
		}

		private class TrackSlot
		{
			public TrackItem[]  values;
			public TrackDir[]   dirs;
			public TrackSurface surface;
			public TrackSlot(TrackItem[] values, TrackDir[] dirs)
			{
				this.values = values;
				this.dirs   = dirs;
			}
		}

		private class TrackStep
		{
			public TrackSlot[] Lines;
			public TrackDir[]  dir;
			public TrackStep(int lines) => Lines = new TrackSlot[lines];
		}

		[Serializable]
		public class TrackInterval
		{
			public          double       start;
			public          double       end;
			public          TrackSurface type;
			public          int          segments = 1;
			public override string       ToString() => $"{start:F3}-{end:F3} ({segments}):{type}";
		}
		public enum FileDivider
		{
			Comma     = 0,
			Semicolon = 1
		}

		[Tooltip("Таблица с треком")]
		public TextAsset _inputFile;
		[Tooltip("Разделитель в файле\ncolon = ','\nsemicolon = sp';'")]
		public FileDivider _divider = FileDivider.Semicolon;
		public char _inputFileDivider => _divider == FileDivider.Comma ? ',' : ';';

		[Tooltip("Настройки генерации")]
		public TrackPrefabs _prefabs;
		[Space]
		[Tooltip("Длина сегмента")]
		public float _stepLength = 3f;
		[Tooltip("Шаг поворота горизонтально")]
		public float _stepHorizontalRotate = 5f;
		[Tooltip("Шаг поворота вертикально")]
		public float _stepVerticalRotate = 5f;
		[Tooltip("Шаг смещения горизонтально")]
		public float _stepHorizontalShift = 0.2f;
		[Tooltip("Шаг смещения вертикально")]
		public float _stepVerticalShift = 0.2f;
		[Tooltip("Угол вращения")]
		public float _stepRotate = 5f;
		[Space]
		[Tooltip("Количество линий на треке")]
		public int _lines = 4;
		[Tooltip("Расстояние между линиями")]
		public float _linesInterval = 1f;
		[Tooltip("Упрощать сегменты на сплайне (рекомендуется)")]
		public bool _simplifyTrackMeshes = true;
		[Tooltip("Сегмент старта игроков")]
		public int _playerStartIndex = 0;

		[Serializable] public class TrackPrefabs
		{
			[Serializable] public class MeshData
			{
				[Tooltip("Меш сегмента")]
				public Mesh mesh;
				[Tooltip("Масштаб меша")]
				public Vector3 scale = Vector3.one;
				[Tooltip("Смещение меша")]
				public Vector2 offset = Vector2.zero;
			}
			[Serializable] public class TrackObject
			{
				[Tooltip("Префаб объекта")]
				public GameObject prefab;
				[Tooltip("Смещение от линии")]
				public Vector3 offset;
			}
			[Header("Line")]
			[Tooltip("Обычная дорога")]
			public MeshData Normal;
			[Tooltip("Рельсы")]
			public MeshData Rails;
			[Tooltip("Материал дороги (один для обоих видов)")]
			public Material RoadMaterial;
			[Tooltip("Префаб начала петли")]
			public GameObject DeathLoopStart;
			[Tooltip("Префаб конца петли")]
			public GameObject DeathLoopEnd;
			[Header("On track objects")]
			[Tooltip("Префаб начала трека (опционально)")]
			public TrackObject TrackHeader;
			[Tooltip("Префаб конца трека (опционально)")]
			public TrackObject TrackFooter;
			[Space]
			[Tooltip("Префаб стратовой линии")]
			public TrackObject StartLine;
			[Tooltip("Количество сегментов перед стартом (прямая)")]
			public int startPointOffset = 10;
			[Tooltip("Индекс стартовой линии от начала трека (с учётом пред пункта)")]
			public int startPointIndex = 3;
			[Space]
			[Tooltip("Префаб финиша")]
			public TrackObject FinishLine;
			[Tooltip("Количество сегментов после финиша (прямая)")]
			public int finishAdd = 10;
			[Space]
			[Tooltip("Объект \"забор\"")]
			public TrackObject Fence;
			[Space]
			[Tooltip("Объект \"магнит\"")]
			public TrackObject Magnet;
			[Tooltip("Объект \"блок\"")]
			public TrackObject Obstacle;
			[Tooltip("Объект \"большой блок\"")]
			public TrackObject ObstacleBlock;
			[Tooltip("Объект \"враг на земле\"")]
			public TrackObject EnemyGround;
			[Tooltip("Объект \"враг в воздухе\"")]
			public TrackObject EnemyAir;
			[Tooltip("Объект \"бонус щит\"")]
			public TrackObject Shield;
			[Tooltip("Объект \"ускоритель\"")]
			public TrackObject Accelerator;
			[Space]
			[Tooltip("Объект \"монета внизу\"")]
			public TrackObject CoinLow;
			[Tooltip("Объект \"монета в середине\"")]
			public TrackObject CoinMiddle;
			[Tooltip("Объект \"монета высоко\"")]
			public TrackObject CoinHigh;
		}

		private TrackStep[] _track;
		private Vector3[]   _directions2;

#if UNITY_EDITOR
		private void CreateRotations()
		{
			_directions2 = new[] {
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
		private void PatchRotation(Transform t)
		{
			var r = t.right;
			var a = Mathf.Asin(r.y) * Mathf.Rad2Deg;
			t.Rotate(t.forward, -a, Space.World);
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
			steps  += _prefabs.finishAdd + _prefabs.startPointOffset;
			_track =  new TrackStep[steps];
			for (var i = 0; i < steps; i++)
			{
				_track[i] = new TrackStep(_lines);
				var line = (i > _prefabs.startPointOffset && i < steps - 1 - _prefabs.finishAdd) ?
					input1[i - _prefabs.startPointOffset].Split(_inputFileDivider) :
					new[] { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", };
				_track[i].dir = ParseSlotDir(line[_lines]);
				for (var j = 0; j <= 3; j++)
				{
					_track[i].Lines[j]         = ParseSlotItem(line[j]);
					_track[i].Lines[j].surface = ParseSlotSurface(line[j]);
				}
			}
		}

		public void CreateTrack()
		{
			ParseTrack();
			CreateRotations();


			var holder      = new GameObject("LevelHolder");
			var levelHolder = holder.AddComponent<LevelHolder>();
			levelHolder._startPercent = (_playerStartIndex + _prefabs.startPointOffset) / (double)(_track.Length);

			var go = new GameObject("Track", typeof(SplineComputer));
			go.transform.parent = holder.transform;
			var spline = go.GetComponent<SplineComputer>();
			levelHolder.Init(spline);

			var points = new List<SplinePoint>(_track.Length);
			var point  = new GameObject().transform;
			for (var i = 0; i < _track.Length; i++)
			{
				var t     = _track[i];
				var p     = new SplinePoint(point.position);
				var angle = 0f;
				foreach (var dir in t.dir)
					switch (dir)
					{
						case TrackDir.None:
						case TrackDir.Fence:
							break;
						case TrackDir.Left:
						case TrackDir.Right:
						case TrackDir.Up:
						case TrackDir.Down:
							point.Rotate(_directions2[(int)dir]);
							break;
						case TrackDir.ShiftLeft:
							point.position += point.right * (-1f * _stepHorizontalShift);
							break;
						case TrackDir.ShiftRight:
							point.position += point.right * _stepHorizontalShift;
							break;
						case TrackDir.ShiftUp:
							point.position += point.up * _stepVerticalShift;
							break;
						case TrackDir.ShiftDown:
							point.position += point.up * (-1f * _stepVerticalShift);
							break;
						case TrackDir.DeathloopStart:
						{
							var rot = Quaternion.LookRotation(p.position - points[points.Count - 1].position, p.normal);
							var dl  = (GameObject)PrefabUtility.InstantiatePrefab(_prefabs.DeathLoopStart);
							var dlt = dl.transform;
							dlt.position   = p.position;
							dlt.rotation   = rot;
							dlt.parent     = spline.transform.parent;
							dlt.localScale = new Vector3(_lines * _linesInterval, dlt.localScale.y, dlt.localScale.z);
							break;
						}
						case TrackDir.DeathloopEnd:
						{
							var rot = Quaternion.LookRotation(p.position - points[points.Count - 1].position, p.normal);
							var dl  = (GameObject)PrefabUtility.InstantiatePrefab(_prefabs.DeathLoopEnd);
							var dlt = dl.transform;
							dlt.position   = p.position;
							dlt.rotation   = rot;
							dlt.parent     = spline.transform.parent;
							dlt.localScale = new Vector3(_lines * _linesInterval, dlt.localScale.y, dlt.localScale.z);
							break;
						}
						case TrackDir.RotateLeft:
							angle += _stepRotate;
							break;
						case TrackDir.RotateRight:
							angle -= _stepRotate;
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				var r = point.rotation;
				point.Rotate(0f, 0f, angle * _stepRotate);
				p.normal = point.up;
				PatchRotation(point);
				point.position += point.forward * _stepLength;
				points.Add(p);
			}
			spline.SetPoints(points.ToArray());
			spline.type = Spline.Type.BSpline;

			DestroyImmediate(point.gameObject);
			{
				var start = (GameObject)PrefabUtility.InstantiatePrefab(_prefabs.StartLine.prefab);
				var pos   = spline.Evaluate(spline.Project(points[_prefabs.startPointIndex + _prefabs.startPointOffset].position));
				start.transform.rotation = pos.rotation;
				start.transform.position = pos.position + pos.rotation * _prefabs.StartLine.offset;
				start.transform.parent   = spline.transform.parent;
			}
			{
				var finish = (GameObject)PrefabUtility.InstantiatePrefab(_prefabs.FinishLine.prefab);
				var pos    = spline.Evaluate(spline.Project(points[points.Count - 1 - _prefabs.finishAdd].position));
				finish.transform.rotation = pos.rotation;
				finish.transform.position = pos.position + pos.rotation * _prefabs.FinishLine.offset;
				finish.transform.parent   = spline.transform.parent;
			}
			if (_prefabs.TrackHeader.prefab != null)
			{
				var trackHeader = (GameObject)PrefabUtility.InstantiatePrefab(_prefabs.TrackHeader.prefab);
				var pos         = spline.Evaluate(spline.Project(points[0].position));
				trackHeader.transform.rotation = pos.rotation;
				trackHeader.transform.position = pos.position + pos.rotation * _prefabs.TrackHeader.offset;
				trackHeader.transform.parent   = spline.transform.parent;
			}
			if (_prefabs.TrackFooter.prefab != null)
			{
				var trackFooter = (GameObject)PrefabUtility.InstantiatePrefab(_prefabs.TrackFooter.prefab);
				var pos         = spline.Evaluate(spline.Project(points[points.Count - 1].position));
				trackFooter.transform.rotation = pos.rotation;
				trackFooter.transform.position = pos.position + pos.rotation * _prefabs.TrackFooter.offset;
				trackFooter.transform.parent   = spline.transform.parent;
			}
			var dataHolder = FindObjectOfType<DataHolder>();
			dataHolder.SetInternals(spline, levelHolder);
			eGUI.SetDirty(dataHolder);
			CreateLines(levelHolder);
		}

		private void CreateLines(LevelHolder levelHolder)
		{
			var spline   = levelHolder.GetComputer();
			var deviance = new float[_lines];
			var points   = spline.GetPoints();
			var tracks   = new SplinePoint[_lines][];

			// calc line deviance from center
			for (var i = 0; i < _lines; i++)
			{
				deviance[i] = _linesInterval * (i - (_lines - 1) / 2f);
				tracks[i]   = new SplinePoint[points.Length];
			}

			var trackSurfaces                                 = new List<TrackInterval>[_lines];
			for (var i = 0; i < _lines; i++) trackSurfaces[i] = new List<TrackInterval>();

			var offset = new Vector3[_lines];

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

					foreach (var dir in _track[i].Lines[line].dirs)
						switch (dir)
						{
							case TrackDir.ShiftLeft:
								offset[line].x += -_stepHorizontalShift;
								break;
							case TrackDir.ShiftRight:
								offset[line].x += _stepHorizontalShift;
								break;
							case TrackDir.ShiftUp:
								offset[line].y += _stepVerticalShift;
								break;
							case TrackDir.ShiftDown:
								offset[line].y += -_stepVerticalShift;
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
					var p2 = points[i].position + p.right * deviance[line] + (Quaternion.LookRotation(p.direction, p.normal) * offset[line]);
					tracks[line][i].position = p2;
					tracks[line][i].size     = _linesInterval;
					tracks[line][i].normal   = points[i].normal;
				}
				{
					var p    = spline.Evaluate(spline.Project(points[i].position)).position;
					var pos0 = tracks[0][i].position          - p;
					var pos1 = tracks[_lines - 1][i].position - p;
					if (_track[i].dir.Any(t => t == TrackDir.RotateLeft || t == TrackDir.RotateRight))
					{
						var dy = Mathf.Max(pos0.y, pos1.y);
						foreach (var track in tracks)
							track[i].position.y += dy;
					}
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

				s.space = SplineComputer.Space.Local;
				s.SetPoints(tracks[i]);
				s.type = Spline.Type.BSpline;
				s.RebuildImmediate();

				var sm = go.AddComponent<SplineMesh>();
				sm.computer = s;
				sm.RemoveChannel(0);

				var divider = int.MaxValue;
				foreach (var interval in trackSurfaces[i])
					if (divider > interval.segments)
						divider = interval.segments;
				divider = _simplifyTrackMeshes ? divider : 1;

				for (var index = 0; index < trackSurfaces[i].Count; index++)
				{
					EditorUtility.DisplayProgressBar("Create lines & road bake", $"Line #{i + 1} of {_lines} :: Interval: {index} of {trackSurfaces[i].Count - 1}", index / (float)(trackSurfaces[i].Count - 1));
					var interval = trackSurfaces[i][index];
					var md       = interval.type == TrackSurface.Railroad ? _prefabs.Rails : _prefabs.Normal;

					var channel = sm.AddChannel($"{interval.type}");
					channel.minScale  = md.scale;
					channel.maxScale  = md.scale;
					channel.minOffset = md.offset;
					channel.maxOffset = md.offset;
					channel.AddMesh(md.mesh);
					channel.count    = interval.segments / divider;
					channel.clipFrom = interval.start;
					channel.clipTo   = interval.end;
				}
				EditorUtility.DisplayProgressBar("Create lines & road bake", $"Line #{i + 1} of {_lines} :: Build mesh", 0);
				sm.RebuildImmediate(true);
				sm.Rebuild(true);
				var mr = sm.gameObject.GetComponent<MeshRenderer>();
				mr.sharedMaterial       = _prefabs.RoadMaterial;
				mr.shadowCastingMode    = ShadowCastingMode.Off;
				mr.receiveShadows       = false;
				mr.lightProbeUsage      = LightProbeUsage.Off;
				mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
			}
			levelHolder.Init(spline);
			levelHolder.Init(splines.ToArray(), _linesInterval);

			{
				for (var i = 0; i < points.Length; i++)
					if (_track[i].dir.Any(t => t == TrackDir.Fence))
					{
						var p  = splines[0].Evaluate(i);
						var go = Instantiate(_prefabs.Fence.prefab).transform;
						go.position   = p.position - p.right * _linesInterval * 0.5f;
						go.rotation   = p.rotation;
						go.localScale = Vector3.one * _stepLength;
						go.parent     = splines[0].transform;

						p             = splines[_lines - 1].Evaluate(i);
						go            = Instantiate(_prefabs.Fence.prefab).transform;
						go.position   = p.position + p.right * _linesInterval * 0.5f;
						go.rotation   = p.rotation;
						go.localScale = Vector3.one * _stepLength;
						go.parent     = splines[_lines - 1].transform;
					}
			}

			levelHolder.barriers  = new List<GameObject>();
			levelHolder.boosters  = new List<GameObject>();
			levelHolder.enemies   = new List<GameObject>();
			levelHolder.money     = new List<GameObject>();
			levelHolder.intervals = new LevelHolder.TrackSurface[trackSurfaces.Length];
			for (var i = 0; i < trackSurfaces.Length; i++)
				levelHolder.intervals[i] = new LevelHolder.TrackSurface(trackSurfaces[i]);
			CreateObjects(levelHolder);
		}

		private void CreateObjects(LevelHolder levelHolder)
		{
			var allEntities = new List<RoadEntityData>();
			var splines     = levelHolder._lines;
			var points      = levelHolder.GetComputer().GetPoints();

			for (var i = 0; i < points.Length; i++)
			{
				for (var j = 0; j < _track[i].Lines.Length; j++)
				{
					foreach (var item in _track[i].Lines[j].values)
					{
						var point = splines[j].Evaluate(splines[j].Project(splines[j].GetPoint(i).position));

						TrackPrefabs.TrackObject prefab = null;
						IList<GameObject>        list   = null;
						switch (item)
						{
							case TrackItem.Magnet:
							{
								prefab = _prefabs.Magnet;
								break;
							}
							case TrackItem.Obstacle:
							{
								prefab = _prefabs.Obstacle;
								list   = levelHolder.barriers;
								allEntities.Add(RoadEntityData.CreateBarrier(BarrierType.Ground_SingePath, i / (float)(points.Length - 1), 0.5f, j));
								break;
							}
							case TrackItem.ObstacleHard:
							{
								prefab = _prefabs.ObstacleBlock;
								list   = levelHolder.barriers;
								allEntities.Add(RoadEntityData.CreateBarrier(BarrierType.Ground_SingePath, i / (float)(points.Length - 1), 1.5f, j));
								break;
							}
							case TrackItem.EnemyGround:
							{
								prefab = _prefabs.EnemyGround;
								list   = levelHolder.enemies;
								allEntities.Add(RoadEntityData.CreateEnemy(EnemyType.Ground, i / (float)(points.Length - 1), 0.5f, j));
								break;
							}
							case TrackItem.EnemyAir:
							{
								prefab = _prefabs.EnemyAir;
								list   = levelHolder.enemies;
								allEntities.Add(RoadEntityData.CreateEnemy(EnemyType.Fly, i / (float)(points.Length - 1), 0.5f, j));
								break;
							}
							case TrackItem.Shield:
							{
								prefab = _prefabs.Shield;
								break;
							}
							case TrackItem.Accelerator:
							{
								prefab = _prefabs.Accelerator;
								list   = levelHolder.boosters;
								break;
							}
						}
						if (prefab != null)
						{
							var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab.prefab);
							obj.transform.rotation = Quaternion.LookRotation(point.direction, point.normal);
							obj.transform.position = point.position + point.rotation * prefab.offset;
							obj.transform.parent   = splines[j].transform;
							list?.Add(obj);
						}
						CreateCoins(_track[i].Lines[j].values.Where(t => t == TrackItem.CoinLow || t == TrackItem.CoinMiddle || t == TrackItem.CoinHigh).ToArray(), point, splines[j].transform, levelHolder.money, allEntities, j);
					}
					EditorUtility.DisplayProgressBar("Create track objects", $"Line #{j + 1} of {_track[i].Lines.Length} :: Point: {i + 1} of {points.Length}", i / (float)(points.Length - 1));
				}
			}
			levelHolder.allEntities = allEntities;
			EditorUtility.ClearProgressBar();
			eGUI.SetDirty(levelHolder.gameObject);
		}

		private void CreateCoins(TrackItem[] coins, SplineResult point, Transform parent, IList<GameObject> list, List<RoadEntityData> a, int road)
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
						a.Add(RoadEntityData.CreateCoin((float)point.percent, 0, road));
						break;
					case TrackItem.CoinMiddle:
						prefab = _prefabs.CoinMiddle;
						a.Add(RoadEntityData.CreateCoin((float)point.percent, 0, road));
						break;
					case TrackItem.CoinHigh:
						prefab = _prefabs.CoinHigh;
						a.Add(RoadEntityData.CreateCoin((float)point.percent, 0, road));
						break;
				}
				if (prefab != null)
				{

					var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab.prefab);
					go.transform.rotation = Quaternion.LookRotation(point.direction, point.normal);
					go.transform.position = point.position + point.rotation * prefab.offset + point.direction * (distance * i);
					go.transform.parent   = parent;
					list?.Add(go);
				}
			}
		}


		private TrackSlot ParseSlotItem(string data)
		{
			var items = new List<TrackItem>();
			var dirs  = new List<TrackDir>();
			foreach (var d in data)
			{
				switch (d)
				{
					case 'M':
						items.Add(TrackItem.Magnet);
						break;
					case 'O':
						items.Add(TrackItem.Obstacle);
						break;
					case 'P':
						items.Add(TrackItem.ObstacleHard);
						break;
					case 'X':
						items.Add(TrackItem.EnemyGround);
						break;
					case '+':
						items.Add(TrackItem.EnemyAir);
						break;
					case 'S':
						items.Add(TrackItem.Shield);
						break;
					case 'A':
						items.Add(TrackItem.Accelerator);
						break;
					case '1':
						items.Add(TrackItem.CoinLow);
						break;
					case '2':
						items.Add(TrackItem.CoinMiddle);
						break;
					case '3':
						items.Add(TrackItem.CoinHigh);
						break;
					case 'l':
						dirs.Add(TrackDir.ShiftLeft);
						break;
					case 'r':
						dirs.Add(TrackDir.ShiftRight);
						break;
					case 'u':
						dirs.Add(TrackDir.ShiftUp);
						break;
					case 'd':
						dirs.Add(TrackDir.ShiftDown);
						break;
				}
			}
			return new TrackSlot(items.ToArray(), dirs.ToArray());
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
		private TrackDir[] ParseSlotDir(string data)
		{
			var res = new TrackDir[data.Length];
			for (var i = 0; i < data.Length; i++)
			{
				switch (data[i])
				{
					case 'L':
						res[i] = TrackDir.Left;
						break;
					case 'R':
						res[i] = TrackDir.Right;
						break;
					case 'U':
						res[i] = TrackDir.Up;
						break;
					case 'D':
						res[i] = TrackDir.Down;
						break;
					case 'l':
						res[i] = TrackDir.ShiftLeft;
						break;
					case 'r':
						res[i] = TrackDir.ShiftRight;
						break;
					case 'u':
						res[i] = TrackDir.ShiftUp;
						break;
					case 'd':
						res[i] = TrackDir.ShiftDown;
						break;
					case 'S':
						res[i] = TrackDir.DeathloopStart;
						break;
					case 'E':
						res[i] = TrackDir.DeathloopEnd;
						break;
					case '-':
						res[i] = TrackDir.RotateLeft;
						break;
					case '+':
						res[i] = TrackDir.RotateRight;
						break;
					case 'F':
						res[i] = TrackDir.Fence;
						break;
					default:
						res[i] = TrackDir.None;
						break;
				}
			}
			return res;
		}
#endif
	}
}
