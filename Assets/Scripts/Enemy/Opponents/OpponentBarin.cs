using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;
using System.Linq;
using System;

[RequireComponent(typeof(IOpponentMover))]
public class OpponentBarin : MonoBehaviour, IPausable, INamedRacer
{
    /// Должен определять что впереди врага 
    /// Должен прыгать, меня путь или слайдить в зависимости от препятсвий

    [SerializeField] private string racerName;
    [SerializeField] private RoadEntityGenerator levelHolder;
    [SerializeField] private SplineFollower follower;

    [Header("Move params")]
    [SerializeField] private float avoidDistance;
    [SerializeField] private float followDinstace;
    [SerializeField] private float jumpDistance;
    [SerializeField] private float slideDistance;
    private IOpponentMover mover;
    RoadEntityData nextEntity;
    private OpponentsData data;
    private bool isPaused;
    private Action currentBehaviour;
    private BehaviourType behaviour;
    private int currentFollowRoad;
    [SerializeField] private List<RoadEntityData> entitiesData;

    public void Pause()
    {
        isPaused = true;
    }

    public void Resume()
    {
        isPaused = false;
    }

    private void Awake()
    {
        mover = GetComponent<IOpponentMover>();
    }

    private void Start()
    {
        if (levelHolder == null) levelHolder = FindObjectOfType<LevelHolder>().GetComponent<RoadEntityGenerator>();
        if (follower == null) Debug.LogError("Opponent's brain follower is null");
        data = DataHolder.GetOpponentData();
        RoadEntityData[] roadEntityArray = new RoadEntityData[levelHolder.GetRoadEntities().Count];
        entitiesData = new List<RoadEntityData>();
        levelHolder.GetRoadEntities().CopyTo(roadEntityArray);
        entitiesData.AddRange(roadEntityArray);
        entitiesData = entitiesData.OrderBy(d => d.percentAtRoad).ToList();
        nextEntity = GetNextEntityData();
        racerName += UnityEngine.Random.Range(0, 100);
        Finish.OnCrossFinishLineEnemy += DisableEnemy;
        RegisterPausable();
    }

    private void DoNothing()
    {
    }

    private void Update()
    {
        if (isPaused) return;
        OpponentLogic();
        currentBehaviour();
    }
    private void OnDestroy()
    {
        Finish.OnCrossFinishLineEnemy -= DisableEnemy;
    }
    private void OpponentLogic()
    {
        if(PercentToNextEntity() < 0 || currentBehaviour == null)
        {
            nextEntity = GetNextEntityData();
            MakeDecision();
            ChooseCurrentBahaviour();
        }
        
    }

    private void ChooseCurrentBahaviour()
    {
        switch (behaviour)
        {
            case BehaviourType.Avoid:
                currentBehaviour = Avoid;
                break;
            case BehaviourType.Follow:
                if(nextEntity.desiredRoad == -1)
                {
                    nextEntity.desiredRoad = UnityEngine.Random.Range(0, nextEntity.roadCount - 1);
                }
                currentBehaviour = Follow;
                break;
            case BehaviourType.Attack:
                currentBehaviour = Attack;
                break;
            case BehaviourType.Nothing:
                currentBehaviour = DoNothing;
                break;
            default:
                break;
        }
    }

    private void Attack()
    {
        if(PercentToNextEntity() <= jumpDistance)
        {
            if(nextEntity.enemyType == EnemyType.Fly)
            {
                mover.DoJump();
            }
            else if(nextEntity.enemyType == EnemyType.Ground)
            {
                mover.DoSlide();
            }
            entitiesData.Remove(nextEntity);
        }
    }

    private void Follow()
    {
        if (PercentToNextEntity() < followDinstace)
        {
            GoToRoad(nextEntity.desiredRoad);
            entitiesData.Remove(nextEntity);
            currentBehaviour = DoNothing;
        }
    }

    private void Avoid()
    {
        if (nextEntity == null) return;
        if (PercentToNextEntity() <= avoidDistance)
        {
            Avoid(nextEntity);
            entitiesData.Remove(nextEntity);
        }
    }

    private float PercentToNextEntity() => nextEntity != null? (nextEntity.percentAtRoad - GetCurrentPercent()): 1.0f;

    private void Avoid(RoadEntityData nextEntity)
    {
        switch (nextEntity.barrierType)
        {
            case BarrierType.Ground_FullRoad:
                if(PercentToNextEntity() < jumpDistance)
                {
                    mover.DoJump();
                    entitiesData.Remove(nextEntity);
                }
                break;
            case BarrierType.Ground_SingePath:
                if(nextEntity.roadCount + 1 < DataHolder.GetRoadCount())
                {
                    GoToRoad(nextEntity.roadCount+1);
                    entitiesData.Remove(nextEntity);
                }
                break;
            case BarrierType.Flying_FullRoad:
                if(nextEntity.desiredRoad == -1)
                {
                    nextEntity.desiredRoad = UnityEngine.Random.Range(1, 3);
                    GoToRoad(nextEntity.desiredRoad);
                }
                if(PercentToNextEntity() < slideDistance)
                {
                    mover.DoSlide();
                    entitiesData.Remove(nextEntity);
                    GetNextEntityData();
                }
                break;
            case BarrierType.Flying_SinglePath:
                // уклониться 
                break;
            default:
                break;
        }
    }

    private void MakeDecision()
    {
        behaviour = BehaviourType.Nothing;
        if(nextEntity != null)
            switch (nextEntity.entityType)
            {
                case EntityType.Barrier:
                    behaviour = BehaviourType.Avoid;
                    break;
                case EntityType.Enemy:
                    behaviour = BehaviourType.Attack;
                    break;
                case EntityType.Coins:
                    behaviour = BehaviourType.Follow;
                    break;
                default:
                    break;
            }
    }

    private RoadEntityData GetNextEntityData()
    {
        for(int i = 0; i < entitiesData.Count; i++)
        {
            if(entitiesData[i].percentAtRoad > GetCurrentPercent())
            {
                return entitiesData[i];
            }
        }
        return null;
    }

    private void GoToRoad(int toRoadId)
    {
        if (toRoadId == mover.CurrentRoadID) return;
        if(toRoadId > mover.CurrentRoadID)
        {
            mover.ChangePath(SwipeInput.SwipeType.Right);
            //StartCoroutine(WaitAndDoAction(data.changeRoadTime + 0.3f, ()=> { GoToRoad(toRoadId); }));
        }
        else if (toRoadId < mover.CurrentRoadID)
        {
            mover.ChangePath(SwipeInput.SwipeType.Left);
            //StartCoroutine(WaitAndDoAction(data.changeRoadTime + 0.3f, () => { GoToRoad(toRoadId); }));
        }
    }

    private IEnumerator WaitAndDoAction(float waitTime, Action action)
    {
        print(waitTime);
        yield return new WaitForSeconds(waitTime);
        action();
    }

    private float GetCurrentPercent() => (float)follower.clampedPercent;

    public string GetName() => racerName;

    public void RegisterPausable()
    {
        PauseController.RegisterPausable(this);
    }

    private void DisableEnemy(OpponentBarin brain)
    {
        if(brain == this)
        {
            gameObject.SetActive(false);
        }
    }
}
