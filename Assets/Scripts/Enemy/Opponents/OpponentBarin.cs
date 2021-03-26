using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data.DataScripts;
using Dreamteck.Splines;
using Level;
using UnityEngine;
namespace Enemy.Opponents
{
	[RequireComponent(typeof(IOpponentMover))]
	public class OpponentBarin : MonoBehaviour, IPausable, INamedRacer
	{

		[SerializeField] private string         racerName;
		[SerializeField] private LevelHolder    levelHolder;
		[SerializeField] private SplineFollower follower;

		[Header("Move params")]
		[SerializeField] private float avoidDistance;
		[SerializeField] private float                followDinstace;
		[SerializeField] private float                jumpDistance;
		[SerializeField] private float                slideDistance;
		[SerializeField] private bool                 inDeathLoop;
		private                  IOpponentMover       _mover;
		private                  RoadEntityData       _nextEntity;
		private                  bool                 _isPaused;
		private                  Action               _currentBehaviour;
		private                  BehaviourType        _behaviour;
		private                  int                  _currentFollowRoad;
		[SerializeField] private List<RoadEntityData> entitiesData;

		public void Pause() => _isPaused = true;
		public void Resume() => _isPaused = false;

		private void Awake() => _mover = GetComponent<IOpponentMover>();

		private void Start()
		{
			if (levelHolder == null) levelHolder = FindObjectOfType<LevelHolder>();
			if (follower    == null) Debug.LogError("Opponent's brain follower is null");
			RoadEntityData[] roadEntityArray = new RoadEntityData[levelHolder.allEntities.Count];
			entitiesData = new List<RoadEntityData>();
			levelHolder.allEntities.CopyTo(roadEntityArray);
			entitiesData.AddRange(roadEntityArray);
			entitiesData                  =  entitiesData.OrderBy(d => d.percentAtRoad).ToList();
			_nextEntity                   =  GetNextEntityData();
			racerName                     += UnityEngine.Random.Range(0, 100);
			Finish.OnCrossFinishLineEnemy += DisableEnemy;
			RegisterPausable();
		}

		private void DoNothing() {}

		private void Update()
		{
			if (_isPaused) return;
			OpponentLogic();
			_currentBehaviour?.Invoke();
		}
		private void OnDestroy() => Finish.OnCrossFinishLineEnemy -= DisableEnemy;
		private void OpponentLogic()
		{
			if (PercentToNextEntity() < 0 || _currentBehaviour == null)
			{
				_nextEntity = GetNextEntityData();
				MakeDecision();
				ChooseCurrentBehaviour();
			}

		}

		private void ChooseCurrentBehaviour()
		{
			switch (_behaviour)
			{
				case BehaviourType.Avoid:
					_currentBehaviour = Avoid;
					break;
				case BehaviourType.Follow:
					if (_nextEntity.desiredRoad == -1)
					{
						_nextEntity.desiredRoad = UnityEngine.Random.Range(0, _nextEntity.roadCount - 1);
					}
					_currentBehaviour = Follow;
					break;
				case BehaviourType.Attack:
					_currentBehaviour = Attack;
					break;
				case BehaviourType.Nothing:
					_currentBehaviour = DoNothing;
					break;
				default:
					break;
			}
		}

		private void Attack()
		{
			if (PercentToNextEntity() <= jumpDistance)
			{
				if (_nextEntity.enemyType == EnemyType.Fly)
				{
					_mover.DoJump();
				}
				else if (_nextEntity.enemyType == EnemyType.Ground)
				{
					_mover.DoSlide();
				}
				entitiesData.Remove(_nextEntity);
			}
		}

		private void Follow()
		{
			if (PercentToNextEntity() < followDinstace)
			{
				GoToRoad(_nextEntity.desiredRoad);
				entitiesData.Remove(_nextEntity);
				_currentBehaviour = DoNothing;
			}
		}

		private void Avoid()
		{
			if (_nextEntity == null) return;
			if (PercentToNextEntity() <= avoidDistance)
			{
				Avoid(_nextEntity);
				entitiesData.Remove(_nextEntity);
			}
		}

		private float PercentToNextEntity() => _nextEntity != null ? (_nextEntity.percentAtRoad - GetCurrentPercent()) : 1.0f;

		private void Avoid(RoadEntityData nextEntity)
		{
			switch (nextEntity.barrierType)
			{
				case BarrierType.Ground_FullRoad:
					if (PercentToNextEntity() < jumpDistance)
					{
						_mover.DoJump();
						entitiesData.Remove(nextEntity);
					}
					break;
				case BarrierType.Ground_SingePath:
					if (nextEntity.roadCount + 1 < DataHolder.GetRoadCount())
					{
						GoToRoad(nextEntity.roadCount + 1);
						entitiesData.Remove(nextEntity);
					}
					break;
				case BarrierType.Flying_FullRoad:
					if (nextEntity.desiredRoad == -1)
					{
						nextEntity.desiredRoad = UnityEngine.Random.Range(1, 3);
						GoToRoad(nextEntity.desiredRoad);
					}
					if (PercentToNextEntity() < slideDistance)
					{
						_mover.DoSlide();
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
			_behaviour = BehaviourType.Nothing;
			if (_nextEntity != null)
				switch (_nextEntity.entityType)
				{
					case EntityType.Barrier:
						_behaviour = BehaviourType.Avoid;
						break;
					case EntityType.Enemy:
						_behaviour = BehaviourType.Attack;
						break;
					case EntityType.Coins:
						_behaviour = BehaviourType.Follow;
						break;
					default:
						break;
				}
		}

		private RoadEntityData GetNextEntityData()
		{
			for (int i = 0; i < entitiesData.Count; i++)
			{
				if (entitiesData[i].percentAtRoad > GetCurrentPercent())
				{
					return entitiesData[i];
				}
			}
			return null;
		}

		private void GoToRoad(int toRoadId)
		{
			if (toRoadId == _mover.CurrentRoadID || inDeathLoop) return;
			if (toRoadId > _mover.CurrentRoadID)
			{
				_mover.ChangePath(SwipeInput.SwipeType.Right);
			}
			else if (toRoadId < _mover.CurrentRoadID)
			{
				_mover.ChangePath(SwipeInput.SwipeType.Left);
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
			if (brain == this)
			{
				gameObject.SetActive(false);
			}
		}

		public void DeathLoopSetup(bool inDeathLoop) => this.inDeathLoop = inDeathLoop;
	}
}
