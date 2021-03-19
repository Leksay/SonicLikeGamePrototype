using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;
public class DataHolder : MonoBehaviour
{
    private static DataHolder instance;

    [SerializeField] private SplineComputer spline;
    [SerializeField] private SpawnedObjects spawnedObjects;
    [SerializeField] private OpponentsData opponentsData;
    [SerializeField] private SoundsData soundsData;
    [SerializeField] private LevelData levelData;
    [SerializeField] private GameProcess gameProcess;
    [SerializeField] private Player currentPlayer;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        if (spawnedObjects == null)
            throw new System.Exception("spawned Objects in DataHolder is null");
    }

    public static void             RefreshObjects()  => instance.spawnedObjects.FindObjects();
    public static List<GameObject> GetBarriersList() => instance.spawnedObjects.barriers;
    public static List<GameObject> GetEnemiesList()  => instance.spawnedObjects.enemys;
    public static List<GameObject> GetMoneyList()    => instance.spawnedObjects.money;

    public static OpponentsData GetOpponentData() => instance.opponentsData;

    public static List<GameObject> GetOpponentsList() => instance.spawnedObjects.opponents;

    public static GameObject GetPlayerPrefab() => instance.spawnedObjects.player;

    public static int GetRoadCount() => instance.spawnedObjects.roadCount;
    public static LevelData GetLevelData() => instance.levelData;
    public static MoneyCounterUI GetMoneyCounterUI() => instance.levelData.moneyCounter;
    public static GameProcess GetGameProcess() => instance.gameProcess;
    public static void SetCurrentPlayer(Player player) => instance.currentPlayer = player;
    public static Player GetCurrentPlayer() => instance.currentPlayer;
    public static SoundsData GetSoundsData() => instance.soundsData;
    public static SplineComputer GetSplineComputer() => instance.spline;
}
