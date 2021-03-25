using System.Collections;
using UnityEngine;
namespace Bonus
{
	public class GoldRingBonus : MonoBehaviour, IBonus
	{
		public                  int     count { get => _count; set => _count = value; }
		private const           float   MoveTime  = 1f;
		private static readonly Vector3 DownScale = Vector3.one * .5f;

		[SerializeField] private int _count;

		public void GetBonus(IWallet wallet) => wallet.GetMoney(count);

		private void OnTriggerEnter(Collider other)
		{
			if (other.TryGetComponent<IWallet>(out var wallet))
			{
				GetBonus(wallet);
				if (wallet is FieldWallet)
					StartCoroutine(MoveAndScale(wallet.walletTransform));
				else
					Destroy(gameObject);
			}
		}

		private IEnumerator MoveAndScale(Transform targetTransform)
		{
			float timer = 0;
			var   myT   = transform;
			while (timer < MoveTime - .5f)
			{
				var t = Mathf.Sqrt(timer);
				myT.position   =  Vector3.Lerp(myT.position,   targetTransform.position + targetTransform.up, t);
				myT.localScale =  Vector3.Lerp(myT.localScale, DownScale,                                     t);
				timer          += Time.deltaTime / 2;
				yield return null;
			}
			Destroy(gameObject);
		}
	}
}
