using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

[RequireComponent(typeof(LevelHolder))]
public class RoadEntityGenerator : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private List<RoadEntityData> data;

    [Header("Holders")]
    [SerializeField] private Transform barrierHolder;
    [SerializeField] private Transform enemysHolder;
    [SerializeField] private Transform moneyHolder;
    private LevelHolder levelHolder;

    [SerializeField] private GameObject splineHolder;
    [SerializeField] private SplineComputer roadSpline;
    private List<GameObject> barriers;
    private List<GameObject> enemies;
    private List<GameObject> moneys;

    [Header("Generation Parametres")]
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    [SerializeField] private int countOfRoads = 4;
    [SerializeField] private int startBarriersPercent;

    [SerializeField] private float[] roadOffsets;

    [SerializeField] private RingsGenerationParams ringsGenerationParams;

    [Header("Tutorial")]
    [SerializeField] private int tutorialEndPercent;

    private void Awake()
    {
        levelHolder = GetComponent<LevelHolder>();
    }

    private void Start()
    {
        if (roadSpline == null)
            throw new System.Exception("road spline in Barrier Generator is null");
        if (splineHolder == null)
            throw new System.Exception("spline Holder Object in BarrierGenerator is null");
        
        roadOffsets = levelHolder.GetPathsOffsets();
        barriers = new List<GameObject>();
        barriers.AddRange(DataHolder.GetBarriersList().ToArray());
        enemies = new List<GameObject>();
        enemies.AddRange(DataHolder.GetEnemiesList().ToArray());
        moneys = new List<GameObject>();
        moneys.AddRange(DataHolder.GetMoneyList().ToArray());
        Generate();
    }

    private void OnValidate()
    {
        if(splineHolder.TryGetComponent<SplineComputer>(out roadSpline) == false)
        {
            Debug.Log("Added to Barrier Generator spline holder is does't contain any spline");
            splineHolder = null;
        }
    }

    private void AddData(EntityType entityType, EnemyType enemyType, BarrierType barrierType, float percent)
    {
        var newData = new RoadEntityData();
        newData.entityType = entityType;
        newData.enemyType = enemyType;
        newData.barrierType = barrierType;
        newData.percentAtRoad = percent;
        data.Add(newData);
    }
    private void AddData(RoadEntityData newData)
    {
        data.Add(newData);
    }

    private void Generate()
    {
        data = new List<RoadEntityData>();
        bool isTutorial = PlayerDataHolder.GetTutorial() == 0;
        float length = roadSpline.CalculateLength();
        double percentLength = length / 100;
        double percent = percentLength / length;
        float randomDist = DistanceToPercent(Random.Range(minDistance,maxDistance));
        int randomDistPercent = (int)(randomDist * 100);
        int i = isTutorial ? tutorialEndPercent : startBarriersPercent;
        for( ; i < 98; i+= randomDistPercent)
        {
            if(randomDist > DistanceToPercent(maxDistance - 25))
            {
                // 70% chanse to spawn coins on long road sector
                int randomInt = Random.Range(0, 100);
                if(randomInt > 30)
                {
                    var moneyPercent = 0.0;
                    for (int j = 0; j < countOfRoads; j++)
                    {
                        moneyPercent = percent * (i + randomDistPercent / 2);
                        GenerateRingsOnRoad((float)moneyPercent, 0, 10, j);
                    }
                    RoadEntityData entityData = new RoadEntityData() { entityType = EntityType.Coins, percentAtRoad = (float)moneyPercent, height = 0, roadCount = countOfRoads };
                    AddData(entityData);
                }
                
            }
            var splineOnPercent = roadSpline.Evaluate(percent * i);
            if (levelHolder.InDeathLoops((float)(percent * i)))
            {
                continue;
            }
            var position = splineOnPercent.position;
            var rotation = splineOnPercent.rotation;
            GenerateEnemyOrBarrier(percent * i);
            randomDist = DistanceToPercent(Random.Range(minDistance, maxDistance));
            randomDistPercent = (int)(randomDist * 100);
        }
    }

    private void GenerateRingsOnRoad(float centerPercent, float centerHeight, int count, int roadId)
    {
        if(roadId < roadOffsets.Length)
        {
            float offset = roadOffsets[roadId];
            float distance = count * ringsGenerationParams.DistanceBtwRings;
            float moenySegment = DistanceToPercent(distance);
            float dintanceInteratorPercent = DistanceToPercent(ringsGenerationParams.DistanceBtwRings);
            float heightIterator = centerHeight / count;
            float halfPercent = centerPercent - (dintanceInteratorPercent * count / 2);
            float currentPercent = halfPercent;
            float currentHeight = 0;
            for(int i = 0; i <= count; i++)
            {
                GenerateRing(currentPercent, currentHeight, offset);
                currentPercent += dintanceInteratorPercent;
                if(i<= count/2)
                {
                    currentHeight += heightIterator;
                }
                else
                {
                    currentHeight -= heightIterator;
                }
            }
        }
    }

    private void GenerateRing(float roadPositionPercent, float height, float roadOffset)
    {
        var spineResult = roadSpline.Evaluate(roadPositionPercent);
        var moneyT = GameObject.Instantiate(moneys[0], spineResult.position, spineResult.rotation, moneyHolder).transform;
        moneyT.position += moneyT.up * height;
        moneyT.position += moneyT.right * roadOffset;
    }

    private void GenerateEnemyOrBarrier(double percent)
    {
        // 0 - barrier 
        // 1 - enemy
        var splineResult = roadSpline.Evaluate(percent);
        var position = splineResult.position;
        var rotation = splineResult.rotation;
        var randomObject = new System.Random(System.DateTime.Now.Millisecond);
        int random = (randomObject.Next() + (int)(position.x * position.z))%2; // 2 is count of aviable entities to spawn

        RoadEntityData entityData = new RoadEntityData();
        entityData.percentAtRoad = (float)percent;
        // if this is barrier
        if(random == 0)
        {
            int randomTypeId = Random.Range(0, barriers.Count);
            entityData.percentAtRoad = (float)percent;
            var barrier = GenerateBarrier(position, rotation, randomTypeId, entityData);
            var barrierType = barrier.barrierType;
            if (barrierType == BarrierType.Ground_FullRoad)
            {
                for (int i = 0; i < countOfRoads; i++)
                {
                    GenerateRingsOnRoad((float)percent, 3, 10, i);
                }
            }
            else if (barrierType == BarrierType.Flying_FullRoad)
            {
                for (int i = 1; i < 3; i++)
                {
                    GenerateRingsOnRoad((float)percent, 0, 10, i);
                }
            }
        }
        else if (random == 1)
        {
            GenerateEnemy(position, rotation, entityData);
        }
        
    }

    private Barrier GenerateBarrier(Vector3 position, Quaternion rotation, int randomTypeId, RoadEntityData entityData)
    {
        var barrier = barriers[randomTypeId].GetComponent<Barrier>();
        if(barrier == null)
        {
            barrier = barriers[randomTypeId].GetComponentInChildren<Barrier>();
            if (barrier == null)
            {
                Debug.LogError("Can't generate barries => Barrier component is null");
                return null;
            }
        }
        entityData.entityType = EntityType.Barrier;
        entityData.barrierType = barrier.barrierType;
        switch (barrier.barrierType)
        {
            case BarrierType.Ground_FullRoad:
                CreateBarrierAtPosition(randomTypeId, position, rotation, entityData);
                entityData.roadCount = 1;
                break;
            case BarrierType.Ground_SingePath:
                System.Random random = new System.Random(System.DateTime.Now.Millisecond);
                int randomBarriersCount = random.Next(0, countOfRoads-1);
                CreateMultipleBarriersAtPosition(randomTypeId, randomBarriersCount, position, rotation);
                entityData.roadCount = randomBarriersCount;
                break;
            case BarrierType.Flying_FullRoad:
                CreateBarrierAtPosition(randomTypeId, position, rotation, entityData);
                entityData.roadCount = 1;
                break;
            case BarrierType.Flying_SinglePath:
                System.Random randomF = new System.Random(System.DateTime.Now.Millisecond);
                int randomBarriersCountF = randomF.Next(0, countOfRoads);
                CreateMultipleBarriersAtPosition(randomTypeId, randomBarriersCountF, position, rotation);
                entityData.roadCount = randomBarriersCountF;
                break;
            default:
                break;
        }
        AddData(entityData);
        return barrier;
    }

    private void CreateBarrierAtPosition(int barrierId,Vector3 position, Quaternion rotation, RoadEntityData entityData)
    {
        var barrierT = GameObject.Instantiate(barriers[barrierId]).transform;
        barrierT.position = position;
        barrierT.rotation = rotation;
        barrierT.parent = barrierHolder;
    }

    private void CreateMultipleBarriersAtPosition(int barrierId, int count, Vector3 position, Quaternion rotation)
    {
        for(int i = 0; i <= count; i++)
        {
            var barrierT = GameObject.Instantiate(barriers[barrierId], position, rotation).transform;
            barrierT.position += barrierT.right * roadOffsets[i];
            barrierT.rotation = rotation;
            barrierT.parent = barrierHolder;
        }
    }

    private void GenerateEnemy(Vector3 position, Quaternion rotation, RoadEntityData entityData)
    {
        entityData.entityType = EntityType.Enemy;
        System.Random random = new System.Random(System.DateTime.Now.Millisecond);
        int randomEnemyCount = random.Next(0, countOfRoads);
        random = new System.Random(System.DateTime.Now.Millisecond);
        int randomEnemyType = random.Next(0, enemies.Count);
        Transform enemy;
        var enemyType = enemies[randomEnemyType].GetComponentInChildren<Enemy>().enemyType;
        entityData.enemyType = enemyType;
        entityData.roadCount = randomEnemyCount;
        for(int i = 0; i <= randomEnemyCount; i++)
        {
            enemy = GameObject.Instantiate(enemies[randomEnemyType], position, rotation, enemysHolder).transform;
            enemy.position += enemy.right * roadOffsets[i];
            float yOffset = enemy.GetComponentInChildren<Enemy>()?.enemyType == EnemyType.Fly? .5f:0;
            entityData.height = yOffset;
            enemy.position += enemy.up * yOffset;
            // NEW
            enemy.forward = -enemy.forward;
        }
        AddData(entityData);
    }

    private float DistanceToPercent(float distance)
    {
        float length = roadSpline.CalculateLength();
        return (distance / length);
    }

    public List<RoadEntityData> GetRoadEntities() => data;
}


