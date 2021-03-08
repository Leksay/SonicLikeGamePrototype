using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;
[RequireComponent(typeof(SplineFollower))]
[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(IBoostable))]
[RequireComponent(typeof(OpponentMovement))]
public class OpponentMover : MonoBehaviour, IMover, IBarrierAffected, IPausable, IOpponentMover, IDefendable
{
    #region Debug
    [SerializeField] private bool changeRight;
    [SerializeField] private bool changeLeft;
    #endregion

    private OpponentMovement opponent;

    [Header("Move")]
    public float defaultSpeed;
    public float changeRoadTime;
    public float changeRoadTreshold;
    public float accelerationSpeed;
    public float maxSpeed;

    [Header("Jump")]
    [SerializeField] private float jumpHeigh;
    [SerializeField] private float upJumpTime;
    [SerializeField] private float downJumpTime;
    [SerializeField] private float inAirTime;
    private float jumpY;
    private bool isJump;

    [Header("Slide")]
    [SerializeField] private float slideTime;
    private float SlideOffset = 0.5f;
    private int currenOffsetId;
    private bool isSliding;
    private float slideOffset = 0.5f;

    [Header("Level")]
    [SerializeField] private LevelHolder levelHolder;

    private SplineFollower follower;
    private bool isPaused;
    private PlayerAnimator animator;
    private bool isAccelerating;
    private bool isDamping;
    private List<IBoostable> boosters;

    private float desiredSpeed;
    private float actualSpeed;

    private CapsuleCollider collider;
    private float defaultColliderHeight;
    private float startOffset;
    public float currentOffset { get; private set; }
    public int CurrentRoadID { get => currenOffsetId; }

    private float nextRoadOffset;
    private float changeRoadTimer;
    private Coroutine changeRoadCorutine;
    private bool initialized;
    private bool defended;

    void Start()
    {
        if(levelHolder == null)
        {
            levelHolder = FindObjectOfType<LevelHolder>();
        }
        follower = GetComponent<SplineFollower>();
        animator = GetComponent<PlayerAnimator>();
        collider = GetComponent<CapsuleCollider>();
        opponent = GetComponent<OpponentMovement>();
        follower.computer = levelHolder.GetComputer();
        defaultColliderHeight = collider.height;
        currentOffset = levelHolder.GetOffsetById(currenOffsetId);
        boosters = new List<IBoostable>();
        boosters.AddRange(GetComponents<IBoostable>());
        Initialize(DataHolder.GetOpponentData(), DataHolder.GetLevelData());
        RegisterPausable();
    }
    void Update()
    {
        if (isPaused) return;
        if (isAccelerating) Accelerate();
        if (isDamping) Damping();
        if(changeLeft)
        {
            ChangePath(SwipeInput.SwipeType.Left);
            changeLeft = false;
        }
        if (changeRight)
        {
            ChangePath(SwipeInput.SwipeType.Right);
            changeRight = false;
        }

    }

    private void Accelerate()
    {
        if (isPaused) return;
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
        if (isPaused) return;
        actualSpeed -= accelerationSpeed * Time.deltaTime;
        if (actualSpeed <= desiredSpeed)
        {
            actualSpeed = desiredSpeed;
            isDamping = false;
        }
        ChangeSpeed();
    }

    public void Initialize(OpponentsData opponentData, LevelData levelData)
    {
        if (initialized) return;

        levelHolder = levelData.levelHolder;
        follower = GetComponent<SplineFollower>();
        animator = GetComponent<PlayerAnimator>();

        defaultSpeed = opponentData.defaultSpeed + Random.Range(-1,2);
        changeRoadTime = opponentData.changeRoadTime;
        changeRoadTreshold = opponentData.changeRoadTreshold;
        accelerationSpeed = opponentData.accelerationSpeed;
        maxSpeed = opponentData.maxSpeed;

        jumpHeigh = opponentData.jumpHeigh;
        upJumpTime = opponentData.upJumpTime;
        downJumpTime = opponentData.downJumpTime;
        inAirTime = opponentData.inAirTime;

        slideTime = opponentData.slideTime;

        actualSpeed= 2;
        desiredSpeed = defaultSpeed;
        currentOffset = levelHolder.GetOffsetById(currenOffsetId);

        follower.autoFollow = true;
        SetupOffset();
        ChangeSpeed();
        initialized = true;
    }

    public void AddSpeed(float speed)
    {
        desiredSpeed = Mathf.Clamp(desiredSpeed + speed, defaultSpeed, maxSpeed);
        ChangeSpeed();
    }

    public void BarrierHited()
    {
        if (defended)
        {
            boosters.ForEach(b => b.StopShild());
            return;
        }
        desiredSpeed = defaultSpeed;
        actualSpeed = 0;
        boosters.ForEach(b => b.StopAllBoosters());
        ChangeSpeed();
    }

    public void Pause()
    {
        follower.followSpeed = 0;
        isPaused = true;
    }

    public void ReduceSpeed(float speed)
    {
        desiredSpeed = Mathf.Clamp(desiredSpeed - speed, defaultSpeed, maxSpeed);
        ChangeSpeed();
    }

    public void Resume()
    {
        follower.followSpeed = actualSpeed;
        isPaused = false;
    }

    private void SetupOffset()
    {
        if (isPaused) return;
        follower.motion.offset = new Vector2(currentOffset, jumpY);
    }

    private void ChangeSpeed()
    {
        if (isPaused) return;
        if (actualSpeed < desiredSpeed)
        {
            isAccelerating = true;
        }
        else if (actualSpeed > desiredSpeed)
        {
            isDamping = true;
        }
        follower.followSpeed = actualSpeed;
        animator.SetAnimatorSpeed(actualSpeed);
    }

    private void StartSlide()
    {
        if (isPaused) return;
        if (!isJump && !isSliding)
        {
            animator.SetSlideAnimation(true);
            isSliding = true;
            collider.direction = 2;
            SetMovementType(MovementType.Slide);
            StartCoroutine(Slide());
        }
    }
    private IEnumerator Slide()
    {
        if (isPaused) yield return null;
        float timer = 0 ;
        while (timer < slideTime)
        {
            jumpY = Mathf.Lerp(0, -slideOffset, (timer) / slideTime);
            timer += Time.deltaTime;
            SetupOffset();
            yield return null;
        }
        timer = 0;
        while (timer < slideTime / 3)
        {
            jumpY = Mathf.Lerp(-slideOffset, 0, (timer * 3) / slideTime);
            timer += Time.deltaTime;
            SetupOffset();
            yield return null;
        }
        StopSlide();
        yield return new WaitForSeconds(Time.deltaTime);
    }
    private void StopSlide()
    {
        isSliding = false;
        jumpY = 0;
        collider.direction = 1;
        SetupOffset();
        SetMovementType(MovementType.Run);
        animator.SetSlideAnimation(false);
    }

    private void StartJump()
    {
        if (isPaused) return;
        if (!isJump && !isSliding)
        {
            isJump = true;
            jumpY = 0;
            collider.height = defaultColliderHeight / 2;
            animator.SetJumpAnimation(true);
            SetMovementType(MovementType.Jump);
            StartCoroutine(HandleJump());
        }
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

    private void StopJump()
    {
        jumpY = 0;
        collider.height = defaultColliderHeight;
        SetupOffset();
        SetMovementType(MovementType.Run);
        isJump = false;
    }


    private bool ChangePath(SwipeInput.SwipeType swipeType)
    {
        if (isPaused) return false;
        if (levelHolder.TryChangePathId(ref currenOffsetId, swipeType))
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

    private IEnumerator MoveNextRoad()
    {
        if (isPaused) yield return null;
        while (Mathf.Abs(nextRoadOffset - currentOffset) > changeRoadTreshold)
        {
            currentOffset = Mathf.Lerp(startOffset, nextRoadOffset, changeRoadTimer / changeRoadTime);
            changeRoadTimer += Time.deltaTime;
            SetupOffset();
            yield return null;
        }
        currentOffset = nextRoadOffset;
        SetupOffset();
        if(changeRoadCorutine != null)
            StopCoroutine(changeRoadCorutine);
        changeRoadCorutine = null;
        yield return new WaitForSeconds(Time.deltaTime);
    }

    public void SetMovementType(MovementType newMovementType) => opponent.SetMovementType(newMovementType);

    void IOpponentMover.ChangePath(SwipeInput.SwipeType swipeType)
    {
        ChangePath(swipeType);
    }

    public void DoJump()
    {
        StartJump();
    }

    public void DoSlide()
    {
        StartSlide();
    }

    public void SetRoad(int roadId)
    {
        Initialize(DataHolder.GetOpponentData(), DataHolder.GetLevelData());
        currenOffsetId = roadId;
        currentOffset = levelHolder.GetOffsetById(currenOffsetId);
        SetupOffset();
    }

    public void SetStartRoad(int roadId)
    {
        SetRoad(roadId);
    }

    public float GetPecent() => (float)follower.clampedPercent;

    public void RegisterPausable()
    {
        PauseController.RegisterPausable(this);
    }
    public void SetDefend(bool isDefended) => defended = isDefended;
}
