using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField]private float rotationTime;

    [Tooltip("Максимальное значение turnSpeed в blend tree")]
    [SerializeField] private float animatorRotationK;
    private void Awake()
    {

    }
    private void Start()
    {
        if (animator == null)
            Debug.LogError("Animator in PlayerAnimator == null");
    }

    public void SetRotationTime(float time) => this.rotationTime = time;

    public void SetAnimatorSpeed(float speed)
    {
        animator.SetFloat("speed", speed);
    }

    public void SetAnimatorRotationSpeed(float speed)
    {
        animator.SetFloat("turnSpeed", speed);
    }


    public void SetRotation(SwipeInput.SwipeType swipeType)
    {
        if (swipeType == SwipeInput.SwipeType.Left)
        {
            StartCoroutine(SmoothRotation(-1));
        }
        else if(swipeType == SwipeInput.SwipeType.Right)
        {
            StartCoroutine(SmoothRotation(1));
        }
    }

    private IEnumerator SmoothRotation(int k)
    {
        float rotationTimer = 0;
        while(rotationTimer < rotationTime)
        {
            SetAnimatorRotationSpeed(rotationTimer / rotationTime * animatorRotationK * k);
            rotationTimer += Time.deltaTime;
            yield return null;
        }
        rotationTimer = 0;
        while (rotationTimer < rotationTime)
        {
            SetAnimatorRotationSpeed(Mathf.Lerp(animatorRotationK,0,rotationTime/rotationTime));
            rotationTimer += Time.deltaTime;
            yield return null;
        }
        SetAnimatorRotationSpeed(0);
        yield return new WaitForSeconds(Time.deltaTime);
    }
}
