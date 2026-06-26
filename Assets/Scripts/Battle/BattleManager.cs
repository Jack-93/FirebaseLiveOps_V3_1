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
    public float PowerCharge { get; private set; }
    public float PowerChargeMax => PowerChargeLimit;
    public float PowerChargeRatio =>
        PowerChargeLimit <= 0f ? 0f : PowerCharge / PowerChargeLimit;
    public float BossTimeRemaining { get; private set; }
    public IReadOnlyList<float> SkillCooldowns => skillCooldowns;
    public static float CompanionSkillPowerCost => SkillPowerCost;

    public string EnemyName =>
        GameBalance.GetEnemyName(Data.currentStage, IsBoss);

    private PlayerData Data => PlayerDataManager.Instance?.playerData;

    private float supportFallbackAttackTimer;
    private float enemyAttackTimer;
    private float recoveryTimer;
    private bool isRecovering;
    private float bossPatternTimer;
    private int bossPatternIndex;
    private List<BossPatternDefinition> bossPatterns;
    private readonly float[] skillCooldowns =
        new float[CompanionManager.PartySize];
    private readonly float[] companionAttackTimers =
        new float[CompanionManager.PartySize];

    private const float PowerChargeLimit = 100f;
    private const float PowerChargePerTap = 12f;
    private const float SkillPowerCost = 35f;
    private const float SkillCooldownBoostOnFullCharge = 1.25f;
    private const float EmptyPartyFallbackInterval = 2.4f;

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

            bossPatternTimer -= deltaTime;
            if (bossPatternTimer <= 0f)
                UseBossPattern();

            if (isRecovering)
                return;
        }

        supportFallbackAttackTimer -= deltaTime;
        enemyAttackTimer -= deltaTime;
        TickCompanionBasicAttacks(deltaTime);
        TickCompanionSkillCooldowns(deltaTime);

        if (!isRecovering && EnemyHealth > 0 && enemyAttackTimer <= 0f)
        {
            EnemyAttack();
            enemyAttackTimer = IsBoss ? 1.15f : 1.55f;
        }
    }

    private void PlayerAttack()
    {
        LastPlayerDamage = Mathf.Max(
            1,
            Mathf.RoundToInt(GameBalance.GetPlayerAttack(Data) * 0.25f));
        EnemyHealth = Math.Max(0, EnemyHealth - LastPlayerDamage);
        OnPlayerAttackPerformed?.Invoke(LastPlayerDamage);

        if (EnemyHealth <= 0)
            DefeatEnemy();

        NotifyChanged();
    }

    private void TickCompanionBasicAttacks(float deltaTime)
    {
        CompanionManager companions = CompanionManager.Instance;
        if (companions == null)
            return;

        bool hasCompanion = false;
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

            hasCompanion = true;
            companionAttackTimers[slot] -= deltaTime;
            if (companionAttackTimers[slot] > 0f || EnemyHealth <= 0)
                continue;

            CompanionBasicAttack(slot, character);
            companionAttackTimers[slot] = GetCompanionAttackDelay(slot);
        }

        if (!hasCompanion &&
            supportFallbackAttackTimer <= 0f &&
            EnemyHealth > 0)
        {
            PlayerAttack();
            supportFallbackAttackTimer = EmptyPartyFallbackInterval;
        }
    }

    private void CompanionBasicAttack(int slot, CharacterData character)
    {
        int damage = GetCompanionBasicDamage(character);
        LastPlayerDamage = damage;
        EnemyHealth = Math.Max(0, EnemyHealth - damage);
        OnCompanionBasicAttackPerformed?.Invoke(slot, character, damage);

        if (EnemyHealth <= 0)
            DefeatEnemy();

        NotifyChanged();
    }

    private void EnemyAttack()
    {
        LastEnemyDamage =
            GameBalance.GetEnemyAttack(Data.currentStage, IsBoss);
        ApplyDamageToPlayer(LastEnemyDamage);
        OnEnemyAttackPerformed?.Invoke(LastEnemyDamage);
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

        OnBossPatternUsed?.Invoke(pattern, totalDamage);
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
            OnPlayerDefeated?.Invoke();
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
        if (!IsRunning || isRecovering || EnemyHealth <= 0 ||
            slot < 0 || slot >= skillCooldowns.Length ||
            skillCooldowns[slot] > 0f ||
            PowerCharge < SkillPowerCost)
        {
            return false;
        }

        CharacterData character =
            CompanionManager.Instance?.GetEquippedAtSlot(slot);
        if (character == null)
            return false;

        PowerCharge = Mathf.Max(0f, PowerCharge - SkillPowerCost);
        UseCompanionSkill(slot, character);
        OnPowerCharged?.Invoke(PowerCharge, PowerChargeLimit);
        return true;
    }

    public bool ChargePower()
    {
        if (!IsInitialized || !IsRunning || isRecovering)
            return false;

        float previous = PowerCharge;
        PowerCharge = Mathf.Min(
            PowerChargeLimit,
            PowerCharge + PowerChargePerTap);
        if (PowerCharge >= PowerChargeLimit &&
            previous < PowerChargeLimit)
        {
            ReduceSkillCooldowns(SkillCooldownBoostOnFullCharge);
        }

        OnPowerChargePerformed?.Invoke();
        OnPowerCharged?.Invoke(PowerCharge, PowerChargeLimit);
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
        OnCompanionSkillUsed?.Invoke(slot, character, damage);

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
        bossPatterns = IsBoss
            ? BossPatternResolver.GetPatterns(data.currentStage)
            : null;
        bossPatternIndex = 0;
        bossPatternTimer = GetFirstBossPatternCooldown();
        ResetCompanionAttackTimers();
        supportFallbackAttackTimer = 0.8f;
        enemyAttackTimer = IsBoss ? 1.15f : 1.55f;
    }

    private void ResetBossChallenge()
    {
        EnemyHealth = EnemyMaxHealth;
        BossTimeRemaining = GameBalance.BossTimeLimit;
        ResetCompanionAttackTimers();
        supportFallbackAttackTimer = 0.8f;
        enemyAttackTimer = 1.15f;
        bossPatternIndex = 0;
        bossPatternTimer = GetFirstBossPatternCooldown();
        OnBossChallengeFailed?.Invoke();
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
