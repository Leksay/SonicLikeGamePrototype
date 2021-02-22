using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class BarrierGenerator : MonoBehaviour
{
    [SerializeField] private GameObject splineHolder;
    [SerializeField] private SplineComputer roadSpline;
    [SerializeField] private List<GameObject> barriers;

    [Header("Generation Parametres")]
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    private void Start()
    {
        if (roadSpline == null)
            throw new System.Exception("road spline in Barrier Generator is null");
        if (splineHolder == null)
            throw new System.Exception("spline Holder Object in BarrierGenerator is null");

        List<GameObject> barriersToCopy = DataHolder.GetBarriersList();
        barriers = new List<GameObject>();
        barriers.AddRange(barriersToCopy.ToArray());
        GenerateBarriers();
    }

    private void OnValidate()
    {
        if(splineHolder.TryGetComponent<SplineComputer>(out roadSpline) == false)
        {
            Debug.Log("Added to Barrier Generator spline holder is does't contain any spline");
            splineHolder = null;
        }
        if(barriers == null)
        {

        }
    }

    private void GenerateBarriers()
    {
        float length = roadSpline.CalculateLength();
        double percentLength = length / 100;
        double percent = percentLength / length;
        for(int i = 10; i < 100; i+=10)
        {
            var testCube = GameObject.Instantiate(barriers[Random.Range(0,barriers.Count-1)]);
            var splineOnPercent = roadSpline.Evaluate(percent * i );
            testCube.transform.position = splineOnPercent.position;
            testCube.transform.rotation = Quaternion.LookRotation(splineOnPercent.direction, -splineOnPercent.right);
        }
    }
}


