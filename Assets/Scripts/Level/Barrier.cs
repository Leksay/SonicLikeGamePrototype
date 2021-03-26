using System.Linq;
using Data.DataScripts;
using UnityEngine;
namespace Level
{
	public class Barrier : MonoBehaviour
	{
		[SerializeField] private TrackObjectsData            _data;
		[SerializeField] private TrackObjectsData.ObjectType type;
		[Space]
		public                   BarrierType                 barrierType;
		public                   float                       time => _data.data.FirstOrDefault(t => t.type == type).time;
		public                   float                       speedSlow => _data.data.FirstOrDefault(t => t.type == type).value;
		private                  Material                    myMaterial;

		private void Start()
		{
			if (myMaterial == null) myMaterial = GetComponent<MeshRenderer>().material;
			if (LayerMask.NameToLayer("Barrier") == 0)
			{
				Debug.LogError("Wrong name of barrier layer in Barrier scrips");
			}
			else
			{
				gameObject.layer = LayerMask.NameToLayer("Barrier");
			}
		}

		public void SetTransparent(float value)
		{
			myMaterial.SetFloat("_Metallic", value);
			var color = myMaterial.color;
			color.a          = value;
			myMaterial.color = color;
		}
	}
}
