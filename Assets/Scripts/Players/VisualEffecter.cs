using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualEffecter : MonoBehaviour, IBarrierAffected, IBoostable
{
    [Header("Effects")]
    [Space]
    [Header("Boost")]
    [SerializeField] private GameObject boostEffectPrefab;
    [SerializeField] private float boostEffectTime;

    [SerializeField] private GameObject playerObject;
    [SerializeField] private float blinkTime;

    private Transform playerT;
    List<MeshRenderer> playerRenderers;
    List<SkinnedMeshRenderer> skinnedMeshRenderers;
    private void Start()
    {
        playerT = transform;
        skinnedMeshRenderers = new List<SkinnedMeshRenderer>();
        playerRenderers = new List<MeshRenderer>();
        playerRenderers.AddRange(playerObject?.GetComponentsInChildren<MeshRenderer>());
        skinnedMeshRenderers.AddRange(playerObject?.GetComponentsInChildren<SkinnedMeshRenderer>());
    }

    public void BarrierHited()
    {
        StartCoroutine(PlayerHitEffect());
    }

    private IEnumerator PlayerHitEffect()
    {
        float timer = 0;
        for(int i = 0; i < 5; i++)
        {
            timer = 0;
            playerRenderers.ForEach(e => e.enabled = false);
            skinnedMeshRenderers.ForEach(e => e.enabled = false);
            while (timer < blinkTime)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            playerRenderers.ForEach(e => e.enabled = true);
            skinnedMeshRenderers.ForEach(e => e.enabled = true);
            timer = 0;  
            while (timer < blinkTime)
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }
        yield return new WaitForSeconds(Time.deltaTime);
    }

    public void BoostSpeed(float time, float value)
    {
        GameObject.Destroy(GameObject.Instantiate(boostEffectPrefab,playerT.position + playerT.up * 0.25f,Quaternion.LookRotation(playerT.forward,playerT.up),playerT), boostEffectTime);
    }

    public void StopAllBoosters()
    {
    }
}
