using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    [SerializeField] private float speedValue;
    [SerializeField] private float boostTime;
    private void OnTriggerEnter(Collider other)
    {
        var boostable = other.GetComponents<IBoostable>();
        if(boostable != null)
        {
            for(int i = 0; i < boostable.Length; i++)
            {
                boostable[i].BoostSpeed(speedValue, boostTime);
            }
        }
    }
}
