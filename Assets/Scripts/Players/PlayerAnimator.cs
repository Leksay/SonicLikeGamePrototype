using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour, IPausable
{
	[SerializeField] private Animator animator;
	[SerializeField] private float    rotationTime;

	[Tooltip("Максимальное значение turnSpeed в blend tree")]
	[SerializeField] private float animatorRotationK;
	private static readonly int WalkType  = Animator.StringToHash("walkType");
	private static readonly int TurnSpeed = Animator.StringToHash("turnSpeed");
	private static readonly int Speed     = Animator.StringToHash("speed");
	private static readonly int Jump      = Animator.StringToHash("jump");
	private static readonly int Slide     = Animator.StringToHash("slide");

	private void Start()
	{
		if (animator == null)
			Debug.LogError("Animator in PlayerAnimator == null");
		RegisterPausable();
	}

	public void SetRotationTime(float          time)  => this.rotationTime = time;
	public void SetAnimatorSpeed(float         speed) => animator.SetFloat(Speed,     speed);
	public void SetAnimatorRotationSpeed(float speed) => animator.SetFloat(TurnSpeed, speed);
	public void SetJumpAnimation(bool          value) => animator.SetBool(Jump, value);
	public void SetSurfaceAnimation(int        type)  => animator.SetFloat(WalkType, type);
	public void SetSlideAnimation(bool         value) => animator.SetBool(Slide, value);

	/*
	public void SetRotation(SwipeInput.SwipeType swipeType)
	{
	    if (_isPaused) return;
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
	    if (_isPaused) yield return null;
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
	        SetAnimatorRotationSpeed(Mathf.Lerp(animatorRotationK *k, 0, rotationTimer /rotationTime));
	        rotationTimer += Time.deltaTime;
	        yield return null;
	    }
	    SetAnimatorRotationSpeed(0);
	    yield return new WaitForSeconds(Time.deltaTime);
	} /**/

	public void Pause() => animator.speed = 0;
	public void Resume() => animator.speed = 1;

	public void RegisterPausable() => PauseController.RegisterPausable(this);
}
