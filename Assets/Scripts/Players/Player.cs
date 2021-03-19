using System;
using System.Collections;
using System.Collections.Generic;
using Players.Camera;
using UnityEngine;

[RequireComponent(typeof(Wallet))]
public class Player : MonoBehaviour, IEnemyAffected, INamedRacer
{
    public static event Action OnEnemyHited;

    [SerializeField] private string racerName;
    [SerializeField] private MovementType movementType;
    [SerializeField] private PlayerCameraData cameraData;
    public Wallet playerWallet { get; private set; }
    private MoneyCounterUI moneyCounter;
    private PlayerFollowCamera playerCameraFollow;
    private RacerStatus status;

    private void Awake()
    {
        racerName = PlayerDataHolder.GetSavedName();
    }

    private void Start()
    {
        playerWallet = GetComponent<Wallet>();
        if(TryGetComponent<RacerStatus>(out status) == false)
        {
            Debug.LogError("Racer status on Player is null");
        }
        moneyCounter = DataHolder.GetMoneyCounterUI();
        playerCameraFollow = FindObjectOfType<PlayerFollowCamera>();
        playerCameraFollow.InitializePlayerCamera(cameraData);
        DataHolder.SetCurrentPlayer(this);
        playerWallet.OnGetMoney += UpdateBalance;
        SetCoinsMultiplier();
    }

    private void SetCoinsMultiplier()
    {
        float multiplier = 1+ PlayerDataHolder.GetXCoin()/100;
        playerWallet.SetCoinsMultiplier(multiplier);
    }

    private void OnDestroy()
    {
        playerWallet.OnGetMoney -= UpdateBalance;
    }

    private void UpdateBalance(int balance)
    {
        moneyCounter.SetText(balance);
    }

    public bool HitedByEnemy(EnemyType enemyType)
    {
        if(movementType == MovementType.Run)
        {
            OnEnemyHited?.Invoke();
            return true;
        }
        return false;
    }

    public void SetMovementType(MovementType movementType) => this.movementType = movementType;

    private void OnTriggerEnter(Collider other)
    {
        var affected = other.GetComponent<IPlayerAffected>();
        if (affected != null)
        {
            affected.HitedByPlayer(movementType, true);
        }
    }

    public string GetName() => racerName;
    public RacerStatus GetRacerStatus() => status;

    public bool isPlayer() => true;
}
