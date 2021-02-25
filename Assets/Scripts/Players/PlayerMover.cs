using UnityEngine;
using Dreamteck;
using Dreamteck.Splines;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SplineFollower))]
[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Player))]
[RequireComponent(typeof(IBoostable))]
public class PlayerMover : MonoBehaviour, IPausable, IBarrierAffected, IMover
{
    public delegate void UAction();
    public static event UAction OnSlide;
    public static event UAction OnSpeedBoost;
    public static event UAction OnSpeedBoostStopped;

    [SerializeField] private Player player;

    [Header("Move")]
    [SerializeField] private float defaultSpeed;
    [SerializeField] private float changeRoadTime;
    [SerializeField] private float changeRoadTreshold;
    [SerializeField] private float accelerationSpeed;
    [SerializeField] private float maxSpeed;
    private float desiredSpeed;// Speed to accelerate to 
    private bool isDamping;
    private IBoostable booster;

    private CapsuleCollider collider;
    private float defaultColliderHeight;
    private float startOffset;
    public float currentOffset { get; private set; }
    private float nextRoadOffset;
    private float changeRoadTimer;
    private Coroutine changeRoadCorutine;
    [SerializeField]private bool isAccelerating;

    [Header("Jump")]
    [SerializeField] private float jumpHeigh;
    [SerializeField] private float upJumpTime;
    [SerializeField] private float downJumpTime;
    [SerializeField] private float inAirTime;

    [Header("Slide")]
    [SerializeField] private float slideTime;
    private float playerSlideOffset = 0.5f;
    private bool isSliding;


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
        collider = GetComponent<CapsuleCollider>();
        defaultColliderHeight = collider.height;
        if (player == null)
            player = GetComponent<Player>();
        if (follower == null)
            follower = GetComponent<SplineFollower>();
        if (levelHolder == null)
            levelHolder = FindObjectOfType<LevelHolder>();
        animator = GetComponent<PlayerAnimator>();
        collider = GetComponent<CapsuleCollider>();
        booster = GetComponent<IBoostable>();
    }

    private void Start()
    {
        SwipeInput.OnPlayerSwiped += OnSwipe;
        desiredSpeed = defaultSpeed;
        currenOffsetId = Random.Range(0, levelHolder.AviableRoadCount() - 1);
        currentOffset = levelHolder.GetOffsetById(currenOffsetId);
        animator.SetRotationTime(changeRoadTime);
        SetPlayerMovementType(PlayerMovementType.Run);
        SetupOffset();
        ChangeSpeed();
    }

    private void Update()
    {
        if (isPaused) return;
        if (isAccelerating) Accelerate();
        if (isDamping) Damping();
    }

    private void Accelerate()
    {
        actualSpeed += accelerationSpeed * Time.deltaTime;
        if (actualSpeed >= desiredSpeed)
        {
            actualSpeed = desiredSpeed;
            isAccelerating = false;
            isDamping = false;
        }
        ChangeSpeed();
    }

    private void Damping()
    {
        actualSpeed -= accelerationSpeed * Time.deltaTime;
        if (actualSpeed <= desiredSpeed)
        {
            actualSpeed = desiredSpeed;
            isDamping = false;
        }
        ChangeSpeed();
    }

    private void ChangeSpeed()
    {
        //print($"Actual speed is {actualSpeed} DesiredSpeedIs {desiredSpeed}");
        if(actualSpeed < desiredSpeed)
        {
            isAccelerating = true;
        }
        else if(actualSpeed > desiredSpeed)
        {
            isDamping = true;
        }
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
        if(!isJump && !isSliding)
        {
            animator.SetSlideAnimation(true);
            isSliding = true;
            collider.direction = 2;
            SetPlayerMovementType(PlayerMovementType.Slide);
            OnSlide?.Invoke();
            StartCoroutine(Slide());
        }
    }

    private void StopSlide()
    {
        isSliding = false;
        jumpY = 0;
        collider.direction = 1;
        SetupOffset();
        SetPlayerMovementType(PlayerMovementType.Run);
        animator.SetSlideAnimation(false);
    }

    private void StartJump()
    {
        if (!isJump && !isSliding)
        {
            isJump = true;
            jumpY = 0;
            collider.height = defaultColliderHeight / 2;
            animator.SetJumpAnimation(true);
            SetPlayerMovementType(PlayerMovementType.Jump);
            StartCoroutine(HandleJump());
        }
    }

    private void StopJump()
    {
        jumpY = 0;
        collider.height = defaultColliderHeight;
        SetupOffset();
        SetPlayerMovementType(PlayerMovementType.Run);
        isJump = false;
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
    private IEnumerator Slide()
    {
        float timer = 0;
        while(timer < slideTime)
        {
            jumpY = Mathf.Lerp(0, -playerSlideOffset, (timer) / slideTime);
            timer += Time.deltaTime;
            SetupOffset();
            yield return null;
        }
        timer = 0;
        while (timer < slideTime / 3)
        {
            jumpY = Mathf.Lerp(-playerSlideOffset, 0, (timer * 3) / slideTime);
            timer += Time.deltaTime;
            SetupOffset();
            yield return null;
        }
        StopSlide();
        yield return new WaitForSeconds(Time.deltaTime);
    }

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
        while (upJumpTimer < upJumpTime)
        {
            SetupOffset();
            jumpY = Mathf.Lerp(0, jumpHeigh, upJumpTimer / upJumpTime);
            upJumpTimer += Time.deltaTime;
            yield return null;
        }

        float inAirTimer = 0;
        while (inAirTimer < inAirTime)
        {
            inAirTimer += Time.deltaTime;
            yield return null;
        }
        animator.SetJumpAnimation(false);

        float downJumpTimer = 0;
        while (downJumpTimer < downJumpTime)
        {
            SetupOffset();
            jumpY = Mathf.Lerp(jumpHeigh, 0, downJumpTimer / downJumpTime);
            downJumpTimer += Time.deltaTime;
            yield return null;
        }
        StopJump();
        yield return new WaitForSeconds(Time.deltaTime);
    }
    #endregion
    private void SetupOffset()
    {
        follower.motion.offset = new Vector2(currentOffset, jumpY);
    }

    public void BarrierHited()
    {
        desiredSpeed = defaultSpeed;
        actualSpeed = 0;
        booster.StopAllBoosters();
        ChangeSpeed();
    }

    public void SetPlayerMovementType(PlayerMovementType newMovementType) => player.SetMovementType(newMovementType);

    // From IMover interface
    public void AddSpeed(float speed)
    {
        print("Speed changing");
        OnSpeedBoost?.Invoke();
        desiredSpeed = Mathf.Clamp(desiredSpeed + speed,defaultSpeed, maxSpeed );
        ChangeSpeed();
    }

    // Frome IMover interface
    public void ReduceSpeed(float speed)
    {
        OnSpeedBoostStopped?.Invoke();
        desiredSpeed = Mathf.Clamp(desiredSpeed - speed, defaultSpeed, maxSpeed);
        ChangeSpeed();
    }
}
