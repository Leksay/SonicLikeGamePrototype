using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="GenerateObjectsData",menuName ="Objects/GenerateObjectsData")]
public class SpawnedObjects : ScriptableObject
{
    public List<GameObject> barriers;
    public List<GameObject> enemys;

    private void OnValidate()
    {
        if(barriers != null && barriers.Count > 0)
        {
            barriers.ForEach(b =>
            {
                if(b!=null)
                    if (b.GetComponent<Barrier>() == null && b.GetComponentInChildren<Barrier>() == null)
                    {
                        barriers.Remove(b);
                    }
            });
        }
        if(enemys != null && enemys.Count > 0)
        {
            enemys.ForEach(e =>
            {

                if(e!=null)
                    if(e.GetComponent<Enemy>() == null && e.GetComponentInChildren<Enemy>() == null)
                    {
                        enemys.Remove(e);
                    }
            });
        }
    }
}
