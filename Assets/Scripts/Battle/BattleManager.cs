using System;
using System.Threading.Tasks;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public event Action OnBattleStateChanged;
    public event Action<int> OnEnemyDefeated;
    public event Action<int> OnStageCleared;

    public bool IsInitialized { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsRecovering => isRecovering;
    public bool IsBoss { get; private set; }
    public int PlayerHealth { get; private set; }
    public int PlayerMaxHealth { get; private set; }
    public int EnemyHealth { get; private set; }
    public int EnemyMaxHealth { get; private set; }
    public int LastPlayerDamage { get; private set; }
    public int LastEnemyDamage { get; private set; }

    public string EnemyName =>
        IsBoss ? $"Stage {Data.currentStage} Boss" : "Dream Creature";

    private PlayerData Data => PlayerDataManager.Instance?.playerData;

    private float playerAttackTimer;
    private float enemyAttackTimer;
    private float recoveryTimer;
    private bool isRecovering;

    public void Initialize()
    {
        PlayerData data = Data;
        if (data == null)
            throw new InvalidOperationException(
                "[Battle] PlayerData is not ready.");

        data.EnsureInitialized();
        PlayerMaxHealth = GameBalance.GetPlayerMaxHealth(data);
        PlayerHealth = PlayerMaxHealth;
        IsInitialized = true;
        SpawnEnemy();
        NotifyChanged();
    }

    public void SetRunning(bool running)
    {
        IsRunning = running;
        NotifyChanged();
    }

    public void RefreshPlayerStats()
    {
        PlayerData data = Data;
        if (data == null)
            return;

        int previousMax = Math.Max(1, PlayerMaxHealth);
        float healthRatio = PlayerHealth / (float)previousMax;

        PlayerMaxHealth = GameBalance.GetPlayerMaxHealth(data);
        PlayerHealth = IsInitialized
            ? Mathf.Clamp(
                Mathf.RoundToInt(PlayerMaxHealth * healthRatio),
                1,
                PlayerMaxHealth)
            : PlayerMaxHealth;

        NotifyChanged();
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    public void Tick(float deltaTime)
    {
        if (!IsInitialized)
            return;

        if (isRecovering)
        {
            recoveryTimer -= deltaTime;
            if (recoveryTimer <= 0f)
                RecoverPlayer();

            return;
        }

        if (!IsRunning)
            return;

        playerAttackTimer -= deltaTime;
        enemyAttackTimer -= deltaTime;

        if (playerAttackTimer <= 0f)
        {
            PlayerAttack();
            playerAttackTimer =
                GameBalance.GetPlayerAttackInterval(Data);
        }

        if (!isRecovering && EnemyHealth > 0 && enemyAttackTimer <= 0f)
        {
            EnemyAttack();
            enemyAttackTimer = IsBoss ? 1.15f : 1.55f;
        }
    }

    private void PlayerAttack()
    {
        LastPlayerDamage = GameBalance.GetPlayerAttack(Data);
        EnemyHealth = Math.Max(0, EnemyHealth - LastPlayerDamage);

        if (EnemyHealth <= 0)
            DefeatEnemy();

        NotifyChanged();
    }

    private void EnemyAttack()
    {
        LastEnemyDamage =
            GameBalance.GetEnemyAttack(Data.currentStage, IsBoss);
        PlayerHealth = Math.Max(0, PlayerHealth - LastEnemyDamage);

        if (PlayerHealth <= 0)
        {
            IsRunning = false;
            isRecovering = true;
            recoveryTimer = 2f;
        }

        NotifyChanged();
    }

    private void DefeatEnemy()
    {
        PlayerData data = Data;
        int clearedStage = data.currentStage;
        int reward =
            GameBalance.GetEnemyGold(data.currentStage, IsBoss);

        data.gold += reward;
        data.totalMonstersDefeated++;

        bool defeatedBoss = IsBoss;
        if (defeatedBoss)
        {
            data.currentStage++;
            data.highestStage =
                Math.Max(data.highestStage, data.currentStage);
            data.stageEnemyIndex = 0;
            data.level++;
        }
        else
        {
            data.stageEnemyIndex++;
        }

        PlayerDataManager.Instance.NotifyPlayerDataChanged();
        OnEnemyDefeated?.Invoke(reward);

        if (defeatedBoss)
        {
            OnStageCleared?.Invoke(clearedStage);
            _ = SaveProgressAsync();
        }

        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        PlayerData data = Data;
        IsBoss =
            data.stageEnemyIndex >= GameBalance.EnemiesPerStage - 1;
        EnemyMaxHealth =
            GameBalance.GetEnemyMaxHealth(data.currentStage, IsBoss);
        EnemyHealth = EnemyMaxHealth;
        playerAttackTimer = 0.25f;
        enemyAttackTimer = IsBoss ? 1.15f : 1.55f;
    }

    private void RecoverPlayer()
    {
        isRecovering = false;
        PlayerHealth = PlayerMaxHealth;
        SpawnEnemy();
        IsRunning = true;
        NotifyChanged();
    }

    private async Task SaveProgressAsync()
    {
        try
        {
            if (FirestoreManager.Instance != null)
                await FirestoreManager.Instance.SavePlayerDataAsync(Data);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private void NotifyChanged()
    {
        OnBattleStateChanged?.Invoke();
    }
}
