using System.Collections;
using System.Collections.Generic;
using Data.DataScripts;
using UnityEngine;

public class FollowArrowManager : MonoBehaviour
{
    [SerializeField] private GameObject arrowPrefab;
    List<FollowArrow> arrows;
    private void Start()
    {
        arrows = new List<FollowArrow>();
        arrows.AddRange(GetComponentsInChildren<FollowArrow>());
        StartCoroutine(WaitAndInitializeArrows(.15f));
    }

    private IEnumerator WaitAndInitializeArrows(float time)
    {
        yield return new WaitForSeconds(time);
        for (int i = 0; i < DataHolder.GetGameProcess().GetRacers().Count; i++)
        {
            var racer = DataHolder.GetGameProcess().GetRacers()[i];
            if (racer != DataHolder.GetCurrentPlayer().GetRacerStatus())
            {
                var arr = GameObject.Instantiate(arrowPrefab, transform);
                arr.GetComponent<FollowArrow>().Initialize(racer.transform);
            }
        }
    }
}
