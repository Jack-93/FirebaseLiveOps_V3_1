using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public event Action OnBattleStateChanged;
    public event Action<int> OnEnemyDefeated;
    public event Action<int> OnStageCleared;
    public event Action<int> OnPlayerAttackPerformed;
    public event Action<int> OnEnemyAttackPerformed;
    public event Action OnPlayerDefeated;
    public event Action<int, CharacterData, int> OnCompanionSkillUsed;
    public event Action OnBossChallengeFailed;

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
    public float BossTimeRemaining { get; private set; }
    public IReadOnlyList<float> SkillCooldowns => skillCooldowns;

    public string EnemyName =>
        GameBalance.GetEnemyName(Data.currentStage, IsBoss);

    private PlayerData Data => PlayerDataManager.Instance?.playerData;

    private float playerAttackTimer;
    private float enemyAttackTimer;
    private float recoveryTimer;
    private bool isRecovering;
    private readonly float[] skillCooldowns =
        new float[CompanionManager.PartySize];

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

    public bool SelectStage(int stage)
    {
        PlayerData data = Data;
        if (data == null)
            return false;

        int selected = Mathf.Clamp(stage, 1, data.highestStage);
        if (selected == data.currentStage)
            return false;

        data.currentStage = selected;
        data.stageEnemyIndex = 0;
        SpawnEnemy();
        PlayerDataManager.Instance.NotifyPlayerDataChanged();
        NotifyChanged();
        return true;
    }

    public void ToggleAutoAdvance()
    {
        PlayerData data = Data;
        if (data == null)
            return;

        data.autoAdvance = !data.autoAdvance;
        PlayerDataManager.Instance.NotifyPlayerDataChanged();
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

        if (IsBoss)
        {
            BossTimeRemaining -= deltaTime;
            if (BossTimeRemaining <= 0f)
            {
                ResetBossChallenge();
                return;
            }
        }

        playerAttackTimer -= deltaTime;
        enemyAttackTimer -= deltaTime;
        TickCompanionSkills(deltaTime);

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
        OnPlayerAttackPerformed?.Invoke(LastPlayerDamage);

        if (EnemyHealth <= 0)
            DefeatEnemy();

        NotifyChanged();
    }

    private void EnemyAttack()
    {
        LastEnemyDamage =
            GameBalance.GetEnemyAttack(Data.currentStage, IsBoss);
        PlayerHealth = Math.Max(0, PlayerHealth - LastEnemyDamage);
        OnEnemyAttackPerformed?.Invoke(LastEnemyDamage);

        if (PlayerHealth <= 0)
        {
            IsRunning = false;
            isRecovering = true;
            recoveryTimer = 2f;
            OnPlayerDefeated?.Invoke();
        }

        NotifyChanged();
    }

    private void TickCompanionSkills(float deltaTime)
    {
        CompanionManager companions = CompanionManager.Instance;
        if (companions == null)
            return;

        for (int slot = 0;
             slot < CompanionManager.PartySize;
             slot++)
        {
            CharacterData character =
                companions.GetEquippedAtSlot(slot);
            if (character == null)
            {
                skillCooldowns[slot] = 0f;
                continue;
            }

            skillCooldowns[slot] -= deltaTime;
            if (skillCooldowns[slot] <= 0f)
                UseCompanionSkill(slot, character);
        }
    }

    private void UseCompanionSkill(int slot, CharacterData character)
    {
        int damage = Math.Max(
            1,
            Mathf.RoundToInt(
                GameBalance.GetPlayerAttack(Data) *
                Mathf.Max(1f, character.skillDamageMultiplier)));

        EnemyHealth = Math.Max(0, EnemyHealth - damage);
        skillCooldowns[slot] =
            Mathf.Max(1f, character.skillCooldown);
        OnCompanionSkillUsed?.Invoke(slot, character, damage);

        if (EnemyHealth <= 0)
            DefeatEnemy();

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
        EquipmentManager.Instance?.TryGrantDrop(
            clearedStage,
            defeatedBoss);
        if (defeatedBoss)
        {
            bool firstClear = clearedStage >= data.highestStage;
            if (firstClear)
            {
                data.highestStage = clearedStage + 1;
                data.level++;
            }

            data.currentStage = data.autoAdvance
                ? Math.Min(clearedStage + 1, data.highestStage)
                : clearedStage;
            data.stageEnemyIndex = 0;
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
        BossTimeRemaining =
            IsBoss ? GameBalance.BossTimeLimit : 0f;
        playerAttackTimer = 0.25f;
        enemyAttackTimer = IsBoss ? 1.15f : 1.55f;
    }

    private void ResetBossChallenge()
    {
        EnemyHealth = EnemyMaxHealth;
        BossTimeRemaining = GameBalance.BossTimeLimit;
        playerAttackTimer = 0.25f;
        enemyAttackTimer = 1.15f;
        OnBossChallengeFailed?.Invoke();
        NotifyChanged();
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
