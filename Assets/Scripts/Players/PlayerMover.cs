using UnityEngine;
using Dreamteck.Splines;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SplineFollower))]
[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Player))]
[RequireComponent(typeof(IBoostable))]
public class PlayerMover : MonoBehaviour, IPausable, IBarrierAffected, IMover, IPlayerControllable, IDefendable
{
    public delegate void UAction();
    public delegate void UActionFkloat(float desiredSpeed, bool justStop);
    public static event UAction OnSlide;
    public static event UAction OnSpeedBoost;
    public static event UActionFkloat OnSpeedBoostStopped;
    public static event Action OnSwipeAction;
    public static event Action OnJumpAction;

    [SerializeField] private Player player;

    [Header("Move")]
    [SerializeField] private float defaultSpeed;
    [SerializeField] private float changeRoadTime;
    [SerializeField] private float changeRoadTreshold;
    [SerializeField] private float accelerationSpeed;
    [SerializeField] private float maxSpeed;
    private float desiredSpeed;// Speed to accelerate to 
    private bool isDamping;
    private List<IBoostable> boosters;

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
    [SerializeField] private int currentRoadId;

    [Header("Dreamteck")]
    [SerializeField] private SplineFollower follower;

    [Header("Skills")]
    [SerializeField] private float speedSkill;
    [SerializeField] private float accelerationSkill;

    [Header("Effects")]
    [SerializeField] private ParticleSystem slideEffect;
    [SerializeField] private ParticleSystem jumpEffect;

    private PlayerAnimator animator;
    private float jumpY;
    private bool isJump;
    private bool isPaused;
    private bool isUnderControll;
    private float actualSpeed;
    private bool defended;

    private void Awake()
    {
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
        boosters = new List<IBoostable>();
        boosters.AddRange(GetComponents<IBoostable>());
    }

    private void Start()
    {
        SwipeInput.OnPlayerSwiped += OnSwipe;
        desiredSpeed = defaultSpeed;
        animator.SetRotationTime(changeRoadTime);
        follower.computer = levelHolder.GetComputer();
        SetPlayerMovementType(MovementType.Run);
        ChangeSpeed();
        SetupSkills();
        RegisterPausable();
        RegisterControllable();
        if(GameProcess.isTutorial)
        {
            TutorialController.OnTutorialFakeInput += OnSwipe;
        }
    }

    private void SetupSkills()
    {
        speedSkill = PlayerDataHolder.GetSpeed() / 10;
        accelerationSkill = PlayerDataHolder.GetAcceleration() / 10;

        defaultSpeed += speedSkill;
        accelerationSpeed += accelerationSkill;
    }

    private void Update()
    {
        if (isPaused) return;
        if (isAccelerating) Accelerate();
        if (isDamping) Damping();
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

    private void ChangeSpeed()
    {
        if (isPaused)
        {
            follower.followSpeed = 0;
            return;
        }
        if (actualSpeed < desiredSpeed)
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
        follower.followSpeed = 0;
        isPaused = true;
    }

    public void Resume()
    {
        follower.followSpeed = actualSpeed;
        isPaused = false;
    }
    
    private void OnSwipe(SwipeInput.SwipeType swipeType)
    {
        if (isUnderControll == false) return;
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
            slideEffect.Play();
            isSliding = true;
            collider.direction = 2;
            SetPlayerMovementType(MovementType.Slide);
            OnSlide?.Invoke();
            StartCoroutine(Slide());
        }
    }

    private void StopSlide()
    {
        isSliding = false;
        slideEffect.Stop();
        jumpY = 0;
        collider.direction = 1;
        SetupOffset();
        SetPlayerMovementType(MovementType.Run);
        animator.SetSlideAnimation(false);
    }

    private void StartJump()
    {
        if (!isJump && !isSliding)
        {
            isJump = true;
            jumpEffect.Play();
            OnJumpAction?.Invoke();
            jumpY = 0;
            collider.height = defaultColliderHeight / 2;
            animator.SetJumpAnimation(true);
            SetPlayerMovementType(MovementType.Jump);
            StartCoroutine(HandleJump());
        }
    }

    private void StopJump()
    {
        jumpY = 0;
        collider.height = defaultColliderHeight;
        SetupOffset();
        SetPlayerMovementType(MovementType.Run);
        isJump = false;
    }

    private bool ChangePath(SwipeInput.SwipeType swipeType)
    {
        if (levelHolder.TryChangePathId(ref currentRoadId,swipeType))
        {
            nextRoadOffset = levelHolder.GetOffsetById(currentRoadId);
            changeRoadTimer = 0;
            OnSwipeAction?.Invoke();
            startOffset = currentOffset;
            changeRoadCorutine = StartCoroutine(MoveNextRoad());
            animator.SetRotation(swipeType);
            return true;
        }
        return false;
    }
    #region Getters
    public float GetDefaultSpeed() => defaultSpeed;
    #endregion
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
        jumpEffect.Stop();
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
        if (isPaused) return;
        follower.motion.offset = new Vector2(currentOffset, jumpY);
    }

    public void BarrierHited()
    {
        if(defended)
        {
            boosters.ForEach(b=>b.StopShild());
            defended = false;
            return;
        }
        desiredSpeed = defaultSpeed;
        actualSpeed = 0;
        OnSpeedBoostStopped?.Invoke(desiredSpeed, desiredSpeed == defaultSpeed);
        boosters.ForEach(b => b.StopAllBoosters());
        ChangeSpeed();
    }

    public void SetPlayerMovementType(MovementType newMovementType) => player.SetMovementType(newMovementType);

    // From IMover interface
    public void AddSpeed(float speed)
    {
        OnSpeedBoost?.Invoke();
        desiredSpeed = Mathf.Clamp(desiredSpeed + speed,defaultSpeed, maxSpeed );
        ChangeSpeed();
    }

    // Frome IMover interface
    public void ReduceSpeed(float speed)
    {
        OnSpeedBoostStopped?.Invoke(desiredSpeed, desiredSpeed == defaultSpeed);
        desiredSpeed = Mathf.Clamp(desiredSpeed - speed, defaultSpeed, maxSpeed);
        ChangeSpeed();
    }

    public void SetStartRoad(int roadId)
    {
        currentRoadId = roadId;
        currentOffset = levelHolder.GetOffsetById(roadId);
        SetupOffset();
    }

    public float GetPecent() => (float)follower.clampedPercent;

    public void RegisterPausable()
    {
        PauseController.RegisterPausable(this);
    }

    public void StartPlayerControll() => isUnderControll = true;

    public void StopPlayerControll() => isUnderControll = false;

    public void RegisterControllable() => ControllManager.RegisterControllable(this);

    public void SetDefend(bool isDefended)
    {
        defended = isDefended;
    }
}
