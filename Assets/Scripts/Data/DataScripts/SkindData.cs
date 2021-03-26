using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Data.DataScripts
{
    [CreateAssetMenu(fileName ="SkinData",menuName ="Objects/SkinData")]
    public class SkindData : ScriptableObject
    {
        public Material                 blackMaterial;
        public List<SkillHolderAdapted> players;
        public List<SkillHolderAdapted> opponents;
    }

    [System.Serializable]
    public class SkillHolderAdapted
    {
        public GameObject Skin;
        public GameObject PlayerHolder;
        public bool       aviable;
    }
}
