using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Data.DataScripts;

public class PlayersSpawner : MonoBehaviour
{

    private GameObject _playerPrefab;
    private List<GameObject> _opponents;
    private int _roadCount;

    private IEnumerator Start()
    {
        _playerPrefab = SkinDataHolder.GetPlayerSkinData().players[SkinnController.currentSkin].PlayerHolder;
        _opponents = new List<GameObject>();
        //for (var i = 0; i < SkinDataHolder.GetPlayerSkinData().opponents.Count; i++) _opponents.Add(SkinDataHolder.GetPlayerSkinData().opponents[i].PlayerHolder);
        _opponents.AddRange( SkinDataHolder.GetPlayerSkinData().opponents.Select(t=>t.PlayerHolder) );
        yield return null;
        _roadCount = DataHolder.GetRoadCount();
        GeneratePlayerAndOpponents();
        PauseController.SetPause();
    }

    public void GeneratePlayerAndOpponents()
    {
        var roads = new int[_roadCount];
        for (var i = 0; i < roads.Length; i++)
            roads[i] = i;
        roads = roads.Shuffle();
        var isTutorial = PlayerDataHolder.GetTutorial() == 0;
        var playerRoadId = isTutorial ? 2 : roads[UnityEngine.Random.Range(0, _roadCount)];
        SpawnPlayer(playerRoadId);
        var opponentsSkinsId = new List<int>();
        for (var i = 0; i < SkinDataHolder.GetPlayerSkinData().opponents.Count; i++)
        {
            if (i != SkinnController.currentSkin)
                opponentsSkinsId.Add(i);
        }
        for (var i = 0; i < _roadCount; i++)
        {
            if (i != playerRoadId)
            {
                SpawnEnemy(i, opponentsSkinsId);
            }
        }
    }

    private void SpawnEnemy(int road, List<int> ids)
    {
        var enemy = Instantiate(_opponents[ids[0]]);
        ids.RemoveAt(0);
        enemy.GetComponent<IOpponentMover>().SetRoad(road);
    }

    private void SpawnPlayer(int roadId)
    {
        var player = Instantiate(_playerPrefab).GetComponent<IMover>();
        player.SetStartRoad(roadId);
    }
}
