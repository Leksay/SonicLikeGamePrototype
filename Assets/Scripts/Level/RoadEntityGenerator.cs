using System;
using System.Collections.Generic;
using Dreamteck.Splines;
using Level;
using UnityEngine;
using Random = UnityEngine.Random;
[RequireComponent(typeof(LevelHolder))]
public class RoadEntityGenerator : MonoBehaviour
{
	[Header("Data")]
	[SerializeField] private List<RoadEntityData> data = new List<RoadEntityData>();

	[Header("Holders")]
	[SerializeField] private Transform barrierHolder;
	[SerializeField] private Transform enemysHolder;
	[SerializeField] private Transform moneyHolder;

	[SerializeField] private GameObject     splineHolder;
	[SerializeField] private SplineComputer roadSpline;

	[Header("Generation Parametres")]
	[SerializeField] private float minDistance;
	[SerializeField] private float maxDistance;
	[SerializeField] private int   countOfRoads = 4;
	[SerializeField] private int   startBarriersPercent;

	[SerializeField] private float[] roadOffsets;

	[SerializeField] private RingsGenerationParams ringsGenerationParams;

	[Header("Tutorial")]
	[SerializeField] private int tutorialEndPercent;
	private List<GameObject> barriers;
	private List<GameObject> enemies;
	private LevelHolder      levelHolder;
	private List<GameObject> moneys;

	private void Awake()
	{
		levelHolder = GetComponent<LevelHolder>();
	}

	private void Start()
	{
		if (roadSpline == null)
			throw new Exception("road spline in Barrier Generator is null");
		if (splineHolder == null)
			throw new Exception("spline Holder Object in BarrierGenerator is null");

		//roadOffsets = levelHolder.GetPathsOffsets();
		barriers    = new List<GameObject>();
		barriers.AddRange(DataHolder.GetBarriersList().ToArray());
		enemies = new List<GameObject>();
		enemies.AddRange(DataHolder.GetEnemiesList().ToArray());
		moneys = new List<GameObject>();
		moneys.AddRange(DataHolder.GetMoneyList().ToArray());
		Generate();
	}

	private void OnValidate()
	{
		if (splineHolder && splineHolder.TryGetComponent(out roadSpline) == false)
		{
			Debug.Log("Added to Barrier Generator spline holder is does't contain any spline");
			splineHolder = null;
		}
	}

	private void AddData(EntityType entityType, EnemyType enemyType, BarrierType barrierType, float percent)
	{
		var newData = new RoadEntityData();
		newData.entityType    = entityType;
		newData.enemyType     = enemyType;
		newData.barrierType   = barrierType;
		newData.percentAtRoad = percent;
		data.Add(newData);
	}
	private void AddData(RoadEntityData newData) => data.Add(newData);

	private void Generate()
	{
		data = new List<RoadEntityData>();
		var    isTutorial        = PlayerDataHolder.GetTutorial() == 0;
		var    length            = roadSpline.CalculateLength();
		double percentLength     = length        / 100;
		var    percent           = percentLength / length;
		var    randomDist        = DistanceToPercent(Random.Range(minDistance, maxDistance));
		var    randomDistPercent = (int)(randomDist * 100);
		var    i                 = isTutorial ? tutorialEndPercent : startBarriersPercent;
		for (; i < 98; i += randomDistPercent)
		{
			if (randomDist > DistanceToPercent(maxDistance - 25))
			{
				// 70% chanse to spawn coins on long road sector
				var randomInt = Random.Range(0, 100);
				if (randomInt > 30)
				{
					var moneyPercent = 0.0;
					for (var j = 0; j < countOfRoads; j++)
					{
						moneyPercent = percent * (i + randomDistPercent / 2);
						GenerateRingsOnRoad((float)moneyPercent, 0, 10, j);
					}
					var entityData = new RoadEntityData { entityType = EntityType.Coins, percentAtRoad = (float)moneyPercent, height = 0, roadCount = countOfRoads };
					AddData(entityData);
				}

			}
			var splineOnPercent = roadSpline.Evaluate(percent * i);
			//if (levelHolder.InDeathLoops((float)(percent * i))) continue;
			var position = splineOnPercent.position;
			var rotation = splineOnPercent.rotation;
			GenerateEnemyOrBarrier(percent * i);
			randomDist        = DistanceToPercent(Random.Range(minDistance, maxDistance));
			randomDistPercent = (int)(randomDist * 100);
		}
	}

	private void GenerateRingsOnRoad(float centerPercent, float centerHeight, int count, int roadId)
	{
		if (roadId < roadOffsets.Length)
		{
			var   offset                   = roadOffsets[roadId];
			var   distance                 = count * ringsGenerationParams.DistanceBtwRings;
			var   moenySegment             = DistanceToPercent(distance);
			var   dintanceInteratorPercent = DistanceToPercent(ringsGenerationParams.DistanceBtwRings);
			var   heightIterator           = centerHeight / count;
			var   halfPercent              = centerPercent - dintanceInteratorPercent * count / 2;
			var   currentPercent           = halfPercent;
			float currentHeight            = 0;
			for (var i = 0; i <= count; i++)
			{
				GenerateRing(currentPercent, currentHeight, offset);
				currentPercent += dintanceInteratorPercent;
				if (i <= count / 2)
					currentHeight += heightIterator;
				else
					currentHeight -= heightIterator;
			}
		}
	}

	private void GenerateRing(float roadPositionPercent, float height, float roadOffset)
	{
		var spineResult = roadSpline.Evaluate(roadPositionPercent);
		var moneyT      = Instantiate(moneys[0], spineResult.position, spineResult.rotation, moneyHolder).transform;
		moneyT.position += moneyT.up    * height;
		moneyT.position += moneyT.right * roadOffset;
	}

	private void GenerateEnemyOrBarrier(double percent)
	{
		// 0 - barrier 
		// 1 - enemy
		var splineResult = roadSpline.Evaluate(percent);
		var position     = splineResult.position;
		var rotation     = splineResult.rotation;
		var randomObject = new System.Random(DateTime.Now.Millisecond);
		var random       = (randomObject.Next() + (int)(position.x * position.z)) % 2; // 2 is count of aviable entities to spawn

		var entityData = new RoadEntityData();
		entityData.percentAtRoad = (float)percent;

		// if this is barrier
		if (random == 0)
		{
			var randomTypeId = Random.Range(0, barriers.Count);
			entityData.percentAtRoad = (float)percent;
			var barrier     = GenerateBarrier(position, rotation, randomTypeId, entityData);
			var barrierType = barrier.barrierType;
			if (barrierType == BarrierType.Ground_FullRoad)
				for (var i = 0; i < countOfRoads; i++)
					GenerateRingsOnRoad((float)percent, 3, 10, i);
			else if (barrierType == BarrierType.Flying_FullRoad)
				for (var i = 1; i < 3; i++)
					GenerateRingsOnRoad((float)percent, 0, 10, i);
		}
		else if (random == 1)
		{
			GenerateEnemy(position, rotation, entityData);
		}

	}

	private Barrier GenerateBarrier(Vector3 position, Quaternion rotation, int randomTypeId, RoadEntityData entityData)
	{
		var barrier = barriers[randomTypeId].GetComponent<Barrier>();
		if (barrier == null)
		{
			barrier = barriers[randomTypeId].GetComponentInChildren<Barrier>();
			if (barrier == null)
			{
				Debug.LogError("Can't generate barries => Barrier component is null");
				return null;
			}
		}
		entityData.entityType  = EntityType.Barrier;
		entityData.barrierType = barrier.barrierType;
		switch (barrier.barrierType)
		{
			case BarrierType.Ground_FullRoad:
				CreateBarrierAtPosition(randomTypeId, position, rotation, entityData);
				entityData.roadCount = 1;
				break;
			case BarrierType.Ground_SingePath:
				var random              = new System.Random(DateTime.Now.Millisecond);
				var randomBarriersCount = random.Next(0, countOfRoads - 1);
				CreateMultipleBarriersAtPosition(randomTypeId, randomBarriersCount, position, rotation);
				entityData.roadCount = randomBarriersCount;
				break;
			case BarrierType.Flying_FullRoad:
				CreateBarrierAtPosition(randomTypeId, position, rotation, entityData);
				entityData.roadCount = 1;
				break;
			case BarrierType.Flying_SinglePath:
				var randomF              = new System.Random(DateTime.Now.Millisecond);
				var randomBarriersCountF = randomF.Next(0, countOfRoads);
				CreateMultipleBarriersAtPosition(randomTypeId, randomBarriersCountF, position, rotation);
				entityData.roadCount = randomBarriersCountF;
				break;
		}
		AddData(entityData);
		return barrier;
	}

	private void CreateBarrierAtPosition(int barrierId, Vector3 position, Quaternion rotation, RoadEntityData entityData)
	{
		var barrierT = Instantiate(barriers[barrierId]).transform;
		barrierT.position = position;
		barrierT.rotation = rotation;
		barrierT.parent   = barrierHolder;
	}

	private void CreateMultipleBarriersAtPosition(int barrierId, int count, Vector3 position, Quaternion rotation)
	{
		for (var i = 0; i <= count; i++)
		{
			var barrierT = Instantiate(barriers[barrierId], position, rotation).transform;
			barrierT.position += barrierT.right * roadOffsets[i];
			barrierT.rotation =  rotation;
			barrierT.parent   =  barrierHolder;
		}
	}

	private void GenerateEnemy(Vector3 position, Quaternion rotation, RoadEntityData entityData)
	{
		entityData.entityType = EntityType.Enemy;
		var random           = new System.Random(DateTime.Now.Millisecond);
		var randomEnemyCount = random.Next(0, countOfRoads);
		random = new System.Random(DateTime.Now.Millisecond);
		var       randomEnemyType = random.Next(0, enemies.Count);
		Transform enemy;
		var       enemyType = enemies[randomEnemyType].GetComponentInChildren<Enemy.Opponents.Enemy>().enemyType;
		entityData.enemyType = enemyType;
		entityData.roadCount = randomEnemyCount;
		for (var i = 0; i <= randomEnemyCount; i++)
		{
			enemy          =  Instantiate(enemies[randomEnemyType], position, rotation, enemysHolder).transform;
			enemy.position += enemy.right * roadOffsets[i];
			var yOffset = enemy.GetComponentInChildren<Enemy.Opponents.Enemy>()?.enemyType == EnemyType.Fly ? .5f : 0;
			entityData.height =  yOffset;
			enemy.position    += enemy.up * yOffset;

			// NEW
			enemy.forward = -enemy.forward;
		}
		AddData(entityData);
	}

	private float DistanceToPercent(float distance)
	{
		var length = roadSpline.CalculateLength();
		return distance / length;
	}

	public List<RoadEntityData> GetRoadEntities() => data;
}
