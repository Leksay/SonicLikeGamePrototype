using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private Tutorial holder;

    private void Awake()
    {
        holder = transform.GetComponentInParent<Tutorial>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Player>())
        {
            holder.Trigger();
        }
    }
}
