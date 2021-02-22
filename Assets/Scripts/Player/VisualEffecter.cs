using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualEffecter : MonoBehaviour, IBarrierAffected
{
    [SerializeField] private GameObject playerObject;
    [SerializeField] private float blinkTime;

    List<MeshRenderer> playerRenderers;
    List<SkinnedMeshRenderer> skinnedMeshRenderers;
    private void Start()
    {
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
}
