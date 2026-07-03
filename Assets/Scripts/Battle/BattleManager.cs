using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum CompanionSkillUseResult
{
    Success,
    BattleNotRunning,
    Recovering,
    NoEnemy,
    InvalidSlot,
    NoCompanion,
    Cooldown,
    NotEnoughPower
}

public class BattleManager : MonoBehaviour
{
    public event Action OnBattleStateChanged;
    public event Action<int> OnEnemyDefeated;
    public event Action<int> OnEnemyDefeatedVisual;
    public event Action<int> OnStageCleared;
    public event Action<int, CharacterData, int> OnCompanionBasicAttackPerformed;
    public event Action<int> OnEnemyAttackPerformed;
    public event Action OnPlayerDefeated;
    public event Action<int, CharacterData, int> OnCompanionSkillUsed;
    public event Action<BossPatternDefinition, int> OnBossPatternUsed;
    public event Action OnBossChallengeFailed;
    public event Action OnPowerChargePerformed;
    public event Action<float, float> OnPowerCharged;

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
    public bool LastDefeatedEnemyWasBoss { get; private set; }
    public float PowerCharge { get; private set; }
    public float PowerChargeMax => PowerChargeLimit;
    public float PowerChargeRatio =>
        PowerChargeLimit <= 0f ? 0f : PowerCharge / PowerChargeLimit;
    public float BossTimeRemaining { get; private set; }
    public IList<float> SkillCooldowns => skillCooldowns;
    public static float CompanionSkillPowerCost => SkillPowerCost;
    public static float PowerChargePerTapAmount => PowerChargePerTap;
    public static float FullChargeCooldownBoost =>
        SkillCooldownBoostOnFullCharge;

    public string EnemyName
    {
        get
        {
            PlayerData data = Data;
            return data == null
                ? ""
                : GameBalance.GetEnemyName(data.currentStage, IsBoss);
        }
    }

    private PlayerData Data => PlayerDataManager.Instance?.playerData;

    private float enemyAttackTimer;
    private float recoveryTimer;
    private bool isRecovering;
    private float bossPatternTimer;
    private int bossPatternIndex;
    private int enemySpawnSequence;
    private List<BossPatternDefinition> bossPatterns;
    private readonly float[] skillCooldowns =
        new float[CompanionManager.PartySize];
    private readonly float[] companionAttackTimers =
        new float[CompanionManager.PartySize];

    private const float PowerChargeLimit = 100f;
    private const float PowerChargePerTap = 12f;
    private const float SkillPowerCost = 35f;
    private const float SkillCooldownBoostOnFullCharge = 1.25f;

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
        PlayerDataManager.Instance.NotifyPlayerDataChanged(true);
        NotifyChanged();
        return true;
    }

    public void ToggleAutoAdvance()
    {
        PlayerData data = Data;
        if (data == null)
            return;

        data.autoAdvance = !data.autoAdvance;
        PlayerDataManager.Instance.NotifyPlayerDataChanged(true);
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
        if (!IsInitialized)
        {
            PlayerHealth = PlayerMaxHealth;
        }
        else if (isRecovering && PlayerHealth <= 0)
        {
            PlayerHealth = 0;
        }
        else
        {
            PlayerHealth = Mathf.Clamp(
                Mathf.RoundToInt(PlayerMaxHealth * healthRatio),
                1,
                PlayerMaxHealth);
        }

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

            bossPatternTimer -= deltaTime;
            if (bossPatternTimer <= 0f)
                UseBossPattern();

            if (isRecovering)
                return;
        }

        enemyAttackTimer -= deltaTime;
        TickCompanionBasicAttacks(deltaTime);
        TickCompanionSkillCooldowns(deltaTime);

        if (!isRecovering && EnemyHealth > 0 && enemyAttackTimer <= 0f)
        {
            EnemyAttack();
            enemyAttackTimer = IsBoss ? 1.15f : 1.55f;
        }
    }

    private void TickCompanionBasicAttacks(float deltaTime)
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
                companionAttackTimers[slot] = GetCompanionAttackDelay(slot);
                continue;
            }

            companionAttackTimers[slot] -= deltaTime;
            if (companionAttackTimers[slot] > 0f || EnemyHealth <= 0)
                continue;

            int sequenceBeforeAttack = enemySpawnSequence;
            CompanionBasicAttack(slot, character);
            companionAttackTimers[slot] = GetCompanionAttackDelay(slot);
            if (sequenceBeforeAttack != enemySpawnSequence)
                break;
        }
    }

    private void CompanionBasicAttack(int slot, CharacterData character)
    {
        int damage = GetCompanionBasicDamage(character);
        LastPlayerDamage = damage;
        EnemyHealth = Math.Max(0, EnemyHealth - damage);
        SafeEvent.Invoke(
            OnCompanionBasicAttackPerformed,
            slot,
            character,
            damage,
            "Battle",
            nameof(OnCompanionBasicAttackPerformed));

        if (EnemyHealth <= 0)
            DefeatEnemy();

        NotifyChanged();
    }

    private void EnemyAttack()
    {
        LastEnemyDamage =
            GameBalance.GetEnemyAttack(Data.currentStage, IsBoss);
        ApplyDamageToPlayer(LastEnemyDamage);
        SafeEvent.Invoke(
            OnEnemyAttackPerformed,
            LastEnemyDamage,
            "Battle",
            nameof(OnEnemyAttackPerformed));
        NotifyChanged();
    }

    private void UseBossPattern()
    {
        if (bossPatterns == null || bossPatterns.Count == 0)
            return;

        BossPatternDefinition pattern =
            bossPatterns[bossPatternIndex % bossPatterns.Count];
        bossPatternIndex++;
        if (pattern == null)
        {
            bossPatternTimer = 1f;
            return;
        }

        bossPatternTimer = Mathf.Max(1f, pattern.cooldown);

        int hitCount = Mathf.Max(1, pattern.hitCount);
        int damagePerHit = Mathf.Max(
            1,
            Mathf.RoundToInt(
                GameBalance.GetEnemyAttack(
                    Data.currentStage,
                    true) *
                Mathf.Max(0.1f, pattern.damageMultiplier)));
        int totalDamage = damagePerHit * hitCount;
        LastEnemyDamage = totalDamage;
        ApplyDamageToPlayer(totalDamage);

        if (pattern.patternType == BossPatternType.DrainStrike &&
            pattern.healPercent > 0f)
        {
            EnemyHealth = Mathf.Min(
                EnemyMaxHealth,
                EnemyHealth +
                Mathf.RoundToInt(EnemyMaxHealth * pattern.healPercent));
        }

        SafeEvent.Invoke(
            OnBossPatternUsed,
            pattern,
            totalDamage,
            "Battle",
            nameof(OnBossPatternUsed));
        NotifyChanged();
    }

    private void ApplyDamageToPlayer(int damage)
    {
        PlayerHealth = Math.Max(0, PlayerHealth - damage);

        if (PlayerHealth <= 0)
        {
            IsRunning = false;
            isRecovering = true;
            recoveryTimer = 2f;
            SafeEvent.Invoke(
                OnPlayerDefeated,
                "Battle",
                nameof(OnPlayerDefeated));
        }
    }

    private void TickCompanionSkillCooldowns(float deltaTime)
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

            skillCooldowns[slot] = Mathf.Max(
                0f,
                skillCooldowns[slot] - deltaTime);
        }
    }

    public bool TryUseCompanionSkill(int slot)
    {
        return TryUseCompanionSkill(slot, out _) ==
            CompanionSkillUseResult.Success;
    }

    public CompanionSkillUseResult TryUseCompanionSkill(
        int slot,
        out float remainingCooldown)
    {
        remainingCooldown = 0f;

        if (!IsRunning)
            return CompanionSkillUseResult.BattleNotRunning;
        if (isRecovering)
            return CompanionSkillUseResult.Recovering;
        if (EnemyHealth <= 0)
            return CompanionSkillUseResult.NoEnemy;
        if (slot < 0 || slot >= skillCooldowns.Length)
            return CompanionSkillUseResult.InvalidSlot;
        if (skillCooldowns[slot] > 0f)
        {
            remainingCooldown = skillCooldowns[slot];
            return CompanionSkillUseResult.Cooldown;
        }
        if (PowerCharge < SkillPowerCost)
            return CompanionSkillUseResult.NotEnoughPower;

        CharacterData character =
            CompanionManager.Instance?.GetEquippedAtSlot(slot);
        if (character == null)
            return CompanionSkillUseResult.NoCompanion;

        PowerCharge = Mathf.Max(0f, PowerCharge - SkillPowerCost);
        UseCompanionSkill(slot, character);
        SafeEvent.Invoke(
            OnPowerCharged,
            PowerCharge,
            PowerChargeLimit,
            "Battle",
            nameof(OnPowerCharged));
        return CompanionSkillUseResult.Success;
    }

    public bool ChargePower()
    {
        if (!IsInitialized || !IsRunning || isRecovering)
            return false;

        PowerCharge = Mathf.Min(
            PowerChargeLimit,
            PowerCharge + PowerChargePerTap);
        if (PowerCharge >= PowerChargeLimit)
        {
            ReduceSkillCooldowns(SkillCooldownBoostOnFullCharge);
        }

        SafeEvent.Invoke(
            OnPowerChargePerformed,
            "Battle",
            nameof(OnPowerChargePerformed));
        SafeEvent.Invoke(
            OnPowerCharged,
            PowerCharge,
            PowerChargeLimit,
            "Battle",
            nameof(OnPowerCharged));
        NotifyChanged();
        return true;
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
        SafeEvent.Invoke(
            OnCompanionSkillUsed,
            slot,
            character,
            damage,
            "Battle",
            nameof(OnCompanionSkillUsed));

        if (EnemyHealth <= 0)
            DefeatEnemy();

        NotifyChanged();
    }

    private int GetCompanionBasicDamage(CharacterData character)
    {
        float rarityMultiplier = GetBasicAttackMultiplier(character.rarity);
        int stars =
            CompanionManager.Instance?.GetStars(character.characterName) ?? 1;
        float starMultiplier = 1f + Mathf.Max(0, stars - 1) * 0.08f;
        return Math.Max(
            1,
            Mathf.RoundToInt(
                GameBalance.GetPlayerAttack(Data) *
                rarityMultiplier *
                starMultiplier));
    }

    private static float GetBasicAttackMultiplier(string rarity)
    {
        switch (rarity)
        {
            case "SSR":
                return 0.62f;
            case "SR":
                return 0.48f;
            default:
                return 0.36f;
        }
    }

    private float GetCompanionAttackDelay(int slot)
    {
        return Mathf.Max(
            0.65f,
            GameBalance.GetPlayerAttackInterval(Data) +
            0.25f +
            slot * 0.15f);
    }

    private void ReduceSkillCooldowns(float amount)
    {
        for (int slot = 0; slot < skillCooldowns.Length; slot++)
        {
            skillCooldowns[slot] = Mathf.Max(
                0f,
                skillCooldowns[slot] - amount);
        }
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
        LastDefeatedEnemyWasBoss = defeatedBoss;
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

        SafeEvent.Invoke(
            OnEnemyDefeatedVisual,
            reward,
            "Battle",
            nameof(OnEnemyDefeatedVisual));

        SpawnEnemy();
        TryGrantEquipmentDrop(clearedStage, defeatedBoss);

        SafeEvent.Invoke(
            OnEnemyDefeated,
            reward,
            "Battle",
            nameof(OnEnemyDefeated));

        if (defeatedBoss)
        {
            SafeEvent.Invoke(
                OnStageCleared,
                clearedStage,
                "Battle",
                nameof(OnStageCleared));
            _ = SaveProgressAsync();
        }

        PlayerDataManager.Instance.NotifyPlayerDataChanged(!defeatedBoss);
    }

    private static void TryGrantEquipmentDrop(
        int stage,
        bool defeatedBoss)
    {
        try
        {
            EquipmentManager.Instance?.TryGrantDrop(stage, defeatedBoss);
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "[Battle] Equipment drop failed: " +
                exception.Message);
            Debug.LogException(exception);
        }
    }

    private void SpawnEnemy()
    {
        PlayerData data = Data;
        enemySpawnSequence++;
        IsBoss =
            data.stageEnemyIndex >= GameBalance.EnemiesPerStage - 1;
        EnemyMaxHealth =
            GameBalance.GetEnemyMaxHealth(data.currentStage, IsBoss);
        EnemyHealth = EnemyMaxHealth;
        BossTimeRemaining =
            IsBoss ? GameBalance.BossTimeLimit : 0f;
        bossPatterns = IsBoss
            ? BossPatternResolver.GetPatterns(data.currentStage)
            : null;
        bossPatternIndex = 0;
        bossPatternTimer = GetFirstBossPatternCooldown();
        ResetCompanionAttackTimers();
        enemyAttackTimer = IsBoss ? 1.15f : 1.55f;
    }

    private void ResetBossChallenge()
    {
        EnemyHealth = EnemyMaxHealth;
        BossTimeRemaining = GameBalance.BossTimeLimit;
        ResetCompanionAttackTimers();
        enemyAttackTimer = 1.15f;
        bossPatternIndex = 0;
        bossPatternTimer = GetFirstBossPatternCooldown();
        SafeEvent.Invoke(
            OnBossChallengeFailed,
            "Battle",
            nameof(OnBossChallengeFailed));
        NotifyChanged();
    }

    private float GetFirstBossPatternCooldown()
    {
        if (!IsBoss || bossPatterns == null)
            return 0f;

        foreach (BossPatternDefinition pattern in bossPatterns)
        {
            if (pattern != null)
                return Mathf.Max(1f, pattern.cooldown);
        }

        return 0f;
    }

    private void ResetCompanionAttackTimers()
    {
        for (int slot = 0; slot < companionAttackTimers.Length; slot++)
        {
            companionAttackTimers[slot] = 0.25f + slot * 0.22f;
        }
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
            if (PlayerDataSaveScheduler.Instance != null)
            {
                await PlayerDataSaveScheduler.Instance.SaveNowAsync(Data);
            }
            else if (FirestoreManager.Instance != null)
            {
                await FirestoreManager.Instance.SavePlayerDataAsync(Data);
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private void NotifyChanged()
    {
        SafeEvent.Invoke(
            OnBattleStateChanged,
            "Battle",
            nameof(OnBattleStateChanged));
    }
}
