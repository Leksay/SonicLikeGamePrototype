using System;
using Internal;
using Players.Camera;
using UnityEngine;
namespace Players
{
    [RequireComponent(typeof(Wallet))]
    public class Player : MonoBehaviour, IEnemyAffected, INamedRacer
    {
        public static event Action OnEnemyHit;

        [SerializeField] private string             racerName;
        [SerializeField] private MovementType       movementType;
        public                   Wallet             playerWallet { get; private set; }
        private                  MoneyCounterUI     _moneyCounter;
        private                  PlayerFollowCamera _playerCameraFollow;
        private                  RacerStatus        _status;

        private void Awake()
        {
            racerName = PlayerDataHolder.GetSavedName();
        }

        private void Start()
        {
            playerWallet = GetComponent<Wallet>();
            if(TryGetComponent<RacerStatus>(out _status) == false)
            {
                Debug.LogError("Racer status on Player is null");
            }
            _moneyCounter       = Locator.GetObject<MoneyCounterUI>();
            _playerCameraFollow = FindObjectOfType<PlayerFollowCamera>();
            _playerCameraFollow.InitializePlayerCamera(transform);
            DataHolder.SetCurrentPlayer(this);
            playerWallet.OnGetMoney += UpdateBalance;
            SetCoinsMultiplier();
        }

        private void SetCoinsMultiplier()
        {
            float multiplier = 1 + PlayerDataHolder.GetXCoin() /100;
            playerWallet.SetCoinsMultiplier(multiplier);
        }

        private void OnDestroy()
        {
            playerWallet.OnGetMoney -= UpdateBalance;
        }

        private void UpdateBalance(int balance) => _moneyCounter?.SetText(balance);

        public bool HitByEnemy(EnemyType enemyType)
        {
            if(movementType == MovementType.Run)
            {
                OnEnemyHit?.Invoke();
                return true;
            }
            return false;
        }

        public void SetMovementType(MovementType movementType) => this.movementType = movementType;

        private void OnTriggerEnter(Collider other) => other.GetComponent<IPlayerAffected>()?.HitByPlayer(movementType, true);

        public string      GetName()        => racerName;
        public RacerStatus GetRacerStatus() => _status;

        public bool isPlayer() => true;
    }
}
