using UnityEngine;
using Dreamteck;
using Dreamteck.Splines;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SplineFollower))]
[RequireComponent(typeof(PlayerAnimator))]
public class PlayerMover : MonoBehaviour, IPausable
{
    [Header("Move")]
    [SerializeField] private float defaultSpeed;
    [SerializeField] private float changeRoadTime;
    [SerializeField] private float changeRoadTreshold;
    private float startOffset;
    private float currentOffset;
    private float nextRoadOffset;
    private float changeRoadTimer;
    private Coroutine changeRoadCorutine;

    [Header("Jump")]
    [SerializeField] private float jumpHeigh;
    [SerializeField] private float upJumpTime;
    [SerializeField] private float downJumpTime;
    [SerializeField] private float inAirTime;

    [Header("Slide")]
    [SerializeField] private float slideTime;


    [Header("Level")]
    [SerializeField] private LevelHolder levelHolder;
    [SerializeField] private int currenOffsetId;

    [Header("Dreamteck")]
    [SerializeField] private SplineFollower follower;

    private PlayerAnimator animator;
    private Rigidbody rb;
    private float jumpY;
    private bool isJump;
    private bool isPaused;
    private float actualSpeed;
    private bool isChangingPath;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (follower == null)
            follower = GetComponent<SplineFollower>();
        if (levelHolder == null)
            levelHolder = FindObjectOfType<LevelHolder>();
        animator = GetComponent<PlayerAnimator>();
    }

    private void Start()
    {
        SwipeInput.OnPlayerSwiped += OnSwipe;
        ChangeSpeed(defaultSpeed);
        currenOffsetId = Random.Range(0, levelHolder.AviableRoadCount() - 1);
        currentOffset = levelHolder.GetOffsetById(currenOffsetId);
        animator.SetRotationTime(changeRoadTime);
        SetupOffset();
    }

    private void Update()
    {
        if (isPaused) return;
    }

    private void ChangeSpeed(float newSpeed)
    {
        actualSpeed = newSpeed;
        follower.followSpeed = actualSpeed;
        animator.SetAnimatorSpeed(actualSpeed);
    }

    public void Pause()
    {
        isPaused = true;
    }

    public void Resume()
    {
        isPaused = false;
    }
    
    private void OnSwipe(SwipeInput.SwipeType swipeType)
    {
        if (changeRoadCorutine == null && ChangePath(swipeType))
        {
        }
        else
        {
            switch (swipeType)
            {
                case SwipeInput.SwipeType.Up:
                    StartJump();
                    break;
                case SwipeInput.SwipeType.Down:
                    StartSlide();
                    break;
                case SwipeInput.SwipeType.Tap:
                    break;
            }
        }
    }

    private void StartSlide()
    {
        if(!isJump)
        {

        }
    }

    private void StartJump()
    {
        if (!isJump)
        {
            isJump = true;
            jumpY = 0;
            StartCoroutine(HandleJump());
        }
    }

    private bool ChangePath(SwipeInput.SwipeType swipeType)
    {
        if(levelHolder.TryChangePathId(ref currenOffsetId,swipeType))
        {
            nextRoadOffset = levelHolder.GetOffsetById(currenOffsetId);
            changeRoadTimer = 0;
            startOffset = currentOffset;
            changeRoadCorutine = StartCoroutine(MoveNextRoad());
            animator.SetRotation(swipeType);
            return true;
        }
        return false;
    }
    #region Enumarators
    private IEnumerator MoveNextRoad()
    {
        while(Mathf.Abs(nextRoadOffset - currentOffset) > changeRoadTreshold)
        {
            currentOffset = Mathf.Lerp(startOffset, nextRoadOffset, changeRoadTimer/changeRoadTime);
            changeRoadTimer += Time.deltaTime;
            SetupOffset();
            yield return null;
        }
        SetupOffset();
        currentOffset = nextRoadOffset;
        StopCoroutine(changeRoadCorutine);
        changeRoadCorutine = null;
        yield return new WaitForSeconds(Time.deltaTime);
    }

    private IEnumerator HandleJump()
    {
        float upJumpTimer = 0;
        while(upJumpTimer < upJumpTime)
        {
            SetupOffset();
            jumpY = Mathf.Lerp(0, jumpHeigh, upJumpTimer / upJumpTime);
            upJumpTimer += Time.deltaTime;
            yield return null;
        }

        float inAirTimer = 0;
        while(inAirTimer < inAirTime)
        {
            inAirTimer += Time.deltaTime;
            yield return null;
        }

        float downJumpTimer = 0;
        while (downJumpTimer < downJumpTime)
        {
            SetupOffset();
            jumpY = Mathf.Lerp(jumpHeigh, 0, downJumpTimer / downJumpTime);
            downJumpTimer += Time.deltaTime;
            yield return null;
        }
        jumpY = 0;
        SetupOffset();
        isJump = false;
        yield return new WaitForSeconds(Time.deltaTime);
    }
    #endregion
    private void SetupOffset()
    {
        follower.motion.offset = new Vector2(-jumpY, currentOffset);
    }
}
