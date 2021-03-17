using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skin : MonoBehaviour
{

    [SerializeField] private List<RendererMaterials> rendererMaterials;
    [SerializeField] private List<Renderer> renderers;
    private void Awake()
    {
        renderers = new List<Renderer>();
        renderers.AddRange(GetComponentsInChildren<Renderer>());

        rendererMaterials = new List<RendererMaterials>();
        foreach (var r in renderers)
        {
            RendererMaterials rm = new RendererMaterials();
            rm.renderer = r;
            rm.materials = new Material[r.materials.Length];
            for (int i = 0; i < r.materials.Length; i++)
            {
                rm.materials[i] = new Material(r.materials[i]);
            }
            rendererMaterials.Add(rm);
        }
    }

    public void SetLock(bool isLocked)
    {
        if (isLocked)
        {
            renderers.ForEach(r => 
            {
                for (int i = 0; i < r.materials.Length; i++)
                {
                    r.materials[i].SetColor("_Color", Color.white * 0.15f);
                    r.materials[i].SetFloat("Shininess", 0);
                }
            });
        }
        else
        {
            rendererMaterials.ForEach(rm =>
            {
                rm.renderer.materials = rm.materials;
            });
        }
    }

    [System.Serializable]
    public class RendererMaterials
    {
        public Renderer renderer;
        public Material[] materials;
    }
}
