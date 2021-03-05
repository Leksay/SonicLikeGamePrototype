using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Finish : MonoBehaviour
{
    public static event Action OnCrossFinishLine;
    public static event Action<OpponentBarin> OnCrossFinishLineEnemy;
    public static int playerPlace { get; private set; }
    private static bool playerFinished;
    [SerializeField] private float finishAppearsTime;
    [SerializeField] private GameObject finishModel;

    private void Start()
    {
        finishModel.SetActive(false);
        StartCoroutine(WaitAndAppear(finishAppearsTime));
        playerPlace = 1;
        playerFinished = false;
    }
    
    private IEnumerator WaitAndAppear(float time)
    {
        yield return new WaitForSeconds(time);
        finishModel.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (player != null)
        {
            playerFinished = true;
            OnCrossFinishLine?.Invoke();
        }
        else
        {
            var enemy = other.GetComponent<OpponentBarin>();
            if(enemy != null)
            {
                if(playerFinished == false)
                {
                    playerPlace++;
                    print(playerPlace);
                }
                OnCrossFinishLineEnemy?.Invoke(enemy);
            }
        }
    }
}
