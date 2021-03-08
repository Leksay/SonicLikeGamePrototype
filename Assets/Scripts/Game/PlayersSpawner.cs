using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class PlayersSpawner : MonoBehaviour
{

    private GameObject playerPrefab;
    private List<GameObject> opponents;
    private int roadCount;

    private void Start()
    {
        playerPrefab = DataHolder.GetPlayerPrefab();
        opponents = DataHolder.GetOpponentsList();
        roadCount = DataHolder.GetRoadCount();
        GeneratePlayerAndOpponents();
        PauseController.SetPause();
    }

    public void GeneratePlayerAndOpponents()
    {
        var roads = new int[roadCount];
        for (int i = 0; i < roads.Length; i++)
            roads[i] = i;
        roads = roads.Shuffle();
        bool isTutorial = PlayerDataHolder.GetTutorial() == 0;
        int playerRoadId = isTutorial ? 2 : roads[UnityEngine.Random.Range(0, roadCount)];
        SpawnPlayer(playerRoadId);
        for (int i = 0; i < roadCount; i++)
        {
            if (i != playerRoadId)
            {
                SpawnEnemy(i);
            }
        }
    }

    private void SpawnEnemy(int road)
    {
        var enemy = GameObject.Instantiate(opponents[0]);
        enemy.GetComponent<IOpponentMover>().SetRoad(road);
    }

    private void SpawnPlayer(int roadId)
    {
        var player = GameObject.Instantiate(playerPrefab).GetComponent<IMover>();
        player.SetStartRoad(roadId);
    }
}
