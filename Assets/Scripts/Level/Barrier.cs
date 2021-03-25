using UnityEngine;
namespace Level
{
    public class Barrier : MonoBehaviour
    {
        public  BarrierType barrierType;
        public  float       time      = 1f;
        public  float       speedSlow = -20f;
        private Material    myMaterial;

        private void Start()
        {
            if (myMaterial == null)
                myMaterial = GetComponent<MeshRenderer>().material;
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
