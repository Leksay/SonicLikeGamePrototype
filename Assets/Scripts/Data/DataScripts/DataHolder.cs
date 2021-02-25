using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataHolder : MonoBehaviour
{
    private static DataHolder instance ;

    [SerializeField] private SpawnedObjects spawnedObjects;

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


    public static List<GameObject> GetBarriersList() => instance.spawnedObjects.barriers;
    public static List<GameObject> GetEnemiesList() => instance.spawnedObjects.enemys;
    public static List<GameObject> GetMoneyList() => instance.spawnedObjects.money;
}
