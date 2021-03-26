using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour, IPausable
{
    [SerializeField] private Animator animator;
    [SerializeField] private float    rotationTime;
    private                  float    _walkType = 0f;

    private bool isPaused;

    [Tooltip("Максимальное значение turnSpeed в blend tree")]
    [SerializeField] private float animatorRotationK;
    private static readonly int WalkType  = Animator.StringToHash("walkType");
    private static readonly int TurnSpeed = Animator.StringToHash("turnSpeed");
    private static readonly int Speed     = Animator.StringToHash("speed");
    private void Awake()
    {

    }
    private void Start()
    {
        if (animator == null)
            Debug.LogError("Animator in PlayerAnimator == null");
        RegisterPausable();
    }

    public void SetRotationTime(float time) => this.rotationTime = time;

    public void SetAnimatorSpeed(float speed)
    {
        if (isPaused) return;
        animator.SetFloat(Speed, speed);
    }

    public void SetAnimatorRotationSpeed(float speed)
    {
        if (isPaused) return;
        animator.SetFloat(TurnSpeed, speed);
    }

    public void SetJumpAnimation(bool value)
    {
        if (isPaused) return;
        animator.SetBool("jump", value);
    }

    public void SetSurfaceAnimation(int type)
    {
        //this.InvokeDelegate((t) => animator.SetFloat(WalkType, Mathf.Lerp(_walkType, type, t)), 0.3f, () => _walkType = type);
        animator.SetFloat(WalkType,  type);
        _walkType = type;
    }

    public void SetRotation(SwipeInput.SwipeType swipeType)
    {
        if (isPaused) return;
        if (swipeType == SwipeInput.SwipeType.Left)
        {
            StartCoroutine(SmoothRotation(-1));
        }
        else if(swipeType == SwipeInput.SwipeType.Right)
        {
            StartCoroutine(SmoothRotation(1));
        }
    }

    public void SetSlideAnimation(bool value)
    {
        if (isPaused) return;
        animator.SetBool("slide", value);
    }

    private IEnumerator SmoothRotation(int k)
    {
        if (isPaused) yield return null;
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
            SetAnimatorRotationSpeed(Mathf.Lerp(animatorRotationK*k,0,rotationTimer/rotationTime));
            rotationTimer += Time.deltaTime;
            yield return null;
        }
        SetAnimatorRotationSpeed(0);
        yield return new WaitForSeconds(Time.deltaTime);
    }

    public void Pause()
    {
        animator.speed = 0;
        isPaused = true;
    }

    public void Resume()
    {
        animator.speed = 1;
        isPaused = false;
    }

    public void RegisterPausable()
    {
        PauseController.RegisterPausable(this);
    }
}
