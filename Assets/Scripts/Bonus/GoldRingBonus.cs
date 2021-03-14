using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldRingBonus : MonoBehaviour, IBonus
{
    public int count { get => _count; set => _count = value; }
    private static float moveTime = 1f;
    private static Vector3 downScale = Vector3.one * .5f;

    [SerializeField] private int _count;

    public void GetBonus(IWallet wallet)
    {
        wallet.GetMoney(count);
    }

    private void OnTriggerEnter(Collider other)
    {
        var wallet = other.GetComponent<IWallet>();
        if (wallet != null)
        {
            GetBonus(wallet);
            if(wallet.GetType().Equals(typeof(FieldWallet)))
            {
                StartCoroutine(MoveAndScale(wallet.walletTransform));
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator MoveAndScale(Transform targetTransform)
    {
        float timer = 0;
        Transform myT = transform;
        while(timer < moveTime-.5f)
        {
            myT.position = Vector3.Lerp(myT.position, targetTransform.position + targetTransform.up, timer);
            myT.localScale = Vector3.Lerp(myT.localScale, downScale, timer);
            timer += Time.deltaTime/2;
            yield return null;
        }
        Destroy(gameObject);
    }
}
