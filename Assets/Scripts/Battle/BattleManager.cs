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
    public event Action<int> OnPoleDamaged;
    public event Action OnPoleDestroyed;
    public event Action<int, CharacterData, int> OnCompanionSkillUsed;
    public event Action<BossPatternDefinition, float> OnBossPatternWarning;
    public event Action<BossPatternDefinition, int> OnBossPatternUsed;
    public event Action OnBossChallengeFailed;
    public event Action OnPowerChargePerformed;
    public event Action<float, float> OnPowerCharged;

    public bool IsInitialized { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsRecovering => isRecovering;
    public bool IsPoleDeathPlaying => isPoleDeathPlaying;
    public bool IsBoss { get; private set; }
    public int PlayerHealth { get; private set; }
    public int PlayerMaxHealth { get; private set; }
    public int PoleDurability => PlayerHealth;
    public int PoleMaxDurability => PlayerMaxHealth;
    public bool IsPoleDestroyed =>
        (isPoleDeathPlaying || isRecovering) && PlayerHealth <= 0;
    public int EnemyHealth { get; private set; }
    public int EnemyMaxHealth { get; private set; }
    public int LastPlayerDamage { get; private set; }
    public int LastEnemyDamage { get; private set; }
    public bool LastDefeatedEnemyWasBoss { get; private set; }
    public float PowerCharge { get; private set; }
    public float PowerChargeMax => PowerChargeLimit;
    public float PowerChargeRatio =>
        PowerChargeLimit <= 0f ? 0f : PowerCharge / PowerChargeLimit;
    public float CurrentPowerChargePerTap =>
        GetPowerChargePerTap(Data);
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
    private float poleDeathTimer;
    private float recoveryTimer;
    private bool isPoleDeathPlaying;
    private bool isRecovering;
    private bool isBossPatternWarning;
    private float bossPatternTimer;
    private float bossPatternWarningTimer;
    private int bossPatternIndex;
    private BossPatternDefinition pendingBossPattern;
    private int enemySpawnSequence;
    private List<BossPatternDefinition> bossPatterns;
    private readonly float[] skillCooldowns =
        new float[CompanionManager.PartySize];
    private readonly float[] companionAttackTimers =
        new float[CompanionManager.PartySize];
    private float partyDamageBuffTimer;
    private float partyDamageBuffMultiplier = 1f;

    private const float PowerChargeLimit = 100f;
    private const float PowerChargePerTap = 5f;
    private const float SkillPowerCost = 35f;
    private const float SkillCooldownBoostOnFullCharge = 1.25f;
    private const float PoleDeathAnimationSeconds = 1.55f;
    private const float PoleRecoverySeconds = 2f;

    public void Initialize()
    {
        PlayerData data = Data;
        if (data == null)
            throw new InvalidOperationException(
                "[Battle] PlayerData is not ready.");

        data.EnsureInitialized();
        PlayerMaxHealth = GameBalance.GetPoleMaxDurability(data);
        PlayerHealth = PlayerMaxHealth;
        partyDamageBuffTimer = 0f;
        partyDamageBuffMultiplier = 1f;
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

        PlayerMaxHealth = GameBalance.GetPoleMaxDurability(data);
        if (!IsInitialized)
        {
            PlayerHealth = PlayerMaxHealth;
        }
        else if (IsPoleDestroyed)
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

        if (isPoleDeathPlaying)
        {
            poleDeathTimer -= deltaTime;
            if (poleDeathTimer <= 0f)
                BeginPoleRecovery();

            return;
        }

        if (isRecovering)
        {
            recoveryTimer -= deltaTime;
            if (recoveryTimer <= 0f)
                RecoverPole();

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
            if (isBossPatternWarning)
            {
                bossPatternWarningTimer -= deltaTime;
                if (bossPatternWarningTimer <= 0f)
                    UsePendingBossPattern();
            }
            else if (bossPatternTimer <= 0f)
            {
                WarnBossPattern();
            }

            if (isRecovering)
                return;
        }

        enemyAttackTimer -= deltaTime;
        TickPartyDamageBuff(deltaTime);
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
        int incomingDamage =
            GameBalance.GetEnemyAttack(Data.currentStage, IsBoss);
        LastEnemyDamage = ApplyDamageToPole(incomingDamage);
        SafeEvent.Invoke(
            OnEnemyAttackPerformed,
            LastEnemyDamage,
            "Battle",
            nameof(OnEnemyAttackPerformed));
        NotifyChanged();
    }

    private void WarnBossPattern()
    {
        if (bossPatterns == null || bossPatterns.Count == 0)
            return;

        pendingBossPattern =
            bossPatterns[bossPatternIndex % bossPatterns.Count];
        bossPatternIndex++;
        if (pendingBossPattern == null)
        {
            bossPatternTimer = 1f;
            return;
        }

        isBossPatternWarning = true;
        bossPatternWarningTimer = GameBalance.BossPatternWarningSeconds;
        SafeEvent.Invoke(
            OnBossPatternWarning,
            pendingBossPattern,
            bossPatternWarningTimer,
            "Battle",
            nameof(OnBossPatternWarning));
        NotifyChanged();
    }

    private void UsePendingBossPattern()
    {
        BossPatternDefinition pattern = pendingBossPattern;
        pendingBossPattern = null;
        isBossPatternWarning = false;
        bossPatternWarningTimer = 0f;
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
        LastEnemyDamage = ApplyDamageToPole(totalDamage);

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
            LastEnemyDamage,
            "Battle",
            nameof(OnBossPatternUsed));
        NotifyChanged();
    }

    private int ApplyDamageToPole(int incomingDamage)
    {
        int damage = GameBalance.GetPoleDamageAfterArmor(
            Data,
            incomingDamage);
        int previousHealth = PlayerHealth;
        PlayerHealth = Math.Max(0, PlayerHealth - damage);
        int actualDamage = previousHealth - PlayerHealth;
        if (actualDamage > 0)
        {
            SafeEvent.Invoke(
                OnPoleDamaged,
                actualDamage,
                "Battle",
                nameof(OnPoleDamaged));
        }

        if (PlayerHealth <= 0)
            BeginPoleDeathSequence();

        return actualDamage;
    }

    private void BeginPoleDeathSequence()
    {
        if (isPoleDeathPlaying || isRecovering)
            return;

        IsRunning = false;
        isPoleDeathPlaying = true;
        isRecovering = false;
        poleDeathTimer = PoleDeathAnimationSeconds;
        SafeEvent.Invoke(
            OnPlayerDefeated,
            "Battle",
            nameof(OnPlayerDefeated));
        SafeEvent.Invoke(
            OnPoleDestroyed,
            "Battle",
            nameof(OnPoleDestroyed));
    }

    private void BeginPoleRecovery()
    {
        if (isRecovering)
            return;

        isPoleDeathPlaying = false;
        isRecovering = true;
        recoveryTimer = GetPoleRecoverySeconds();
        NotifyChanged();
    }

    private void RecoverPole()
    {
        isPoleDeathPlaying = false;
        isRecovering = false;
        PlayerHealth = PlayerMaxHealth;
        partyDamageBuffTimer = 0f;
        partyDamageBuffMultiplier = 1f;
        SpawnEnemy();
        IsRunning = true;
        NotifyChanged();
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

        if (isPoleDeathPlaying || isRecovering)
            return CompanionSkillUseResult.Recovering;
        if (!IsRunning)
            return CompanionSkillUseResult.BattleNotRunning;
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
            PowerCharge + CurrentPowerChargePerTap);
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
        bool damagesEnemy =
            character.skillEffect == CompanionSkillEffect.DamageEnemy;
        int effectValue;
        switch (character.skillEffect)
        {
            case CompanionSkillEffect.RepairPole:
                effectValue = ApplyCompanionPoleRepair(character);
                break;
            case CompanionSkillEffect.PartyDamageBuff:
                effectValue = ApplyCompanionPartyDamageBuff(character);
                break;
            default:
                effectValue = ApplyCompanionSkillDamage(character);
                break;
        }

        skillCooldowns[slot] =
            Mathf.Max(1f, character.skillCooldown);
        SafeEvent.Invoke(
            OnCompanionSkillUsed,
            slot,
            character,
            effectValue,
            "Battle",
            nameof(OnCompanionSkillUsed));

        if (damagesEnemy && EnemyHealth <= 0)
            DefeatEnemy();

        NotifyChanged();
    }

    private int ApplyCompanionSkillDamage(CharacterData character)
    {
        int hitCount = Mathf.Max(1, character.skillHitCount);
        int damage = Math.Max(
            1,
            Mathf.RoundToInt(
                GameBalance.GetPlayerAttack(Data) *
                Mathf.Max(1f, character.skillDamageMultiplier) *
                GetEquipmentSkillDamageMultiplier() *
                GetActivePartyDamageMultiplier())) *
            hitCount;
        EnemyHealth = Math.Max(0, EnemyHealth - damage);
        return damage;
    }

    private int ApplyCompanionPartyDamageBuff(CharacterData character)
    {
        float percent =
            Mathf.Max(0f, character.skillDamageBuffPercent);
        float duration =
            Mathf.Max(0.1f, character.skillDamageBuffDuration);
        partyDamageBuffMultiplier = Mathf.Max(
            partyDamageBuffMultiplier,
            1f + percent / 100f);
        partyDamageBuffTimer = Mathf.Max(
            partyDamageBuffTimer,
            duration);
        return Mathf.RoundToInt(percent);
    }

    private int ApplyCompanionPoleRepair(CharacterData character)
    {
        float repairPercent =
            Mathf.Clamp01(character.poleRepairPercent);
        int repairAmount = Math.Max(
            1,
            Mathf.RoundToInt(
                PlayerMaxHealth *
                repairPercent *
                GetEquipmentPoleRepairMultiplier()));
        int previousHealth = PlayerHealth;
        PlayerHealth = Math.Min(
            PlayerMaxHealth,
            PlayerHealth + repairAmount);
        return Math.Max(0, PlayerHealth - previousHealth);
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
                Mathf.Max(0.1f, character.basicAttackMultiplier) *
                starMultiplier *
                GetActivePartyDamageMultiplier()));
    }

    private void TickPartyDamageBuff(float deltaTime)
    {
        if (partyDamageBuffTimer <= 0f)
            return;

        partyDamageBuffTimer = Mathf.Max(
            0f,
            partyDamageBuffTimer - deltaTime);
        if (partyDamageBuffTimer <= 0f)
            partyDamageBuffMultiplier = 1f;
    }

    private float GetActivePartyDamageMultiplier()
    {
        return partyDamageBuffTimer > 0f
            ? Mathf.Max(1f, partyDamageBuffMultiplier)
            : 1f;
    }

    private float GetEquipmentSkillDamageMultiplier()
    {
        float percent = EquipmentManager.GetSkillDamagePercent(Data);
        if (IsBoss)
            percent += EquipmentManager.GetBossDamagePercent(Data);

        return 1f + Mathf.Max(0f, percent) / 100f;
    }

    private float GetEquipmentPoleRepairMultiplier()
    {
        return 1f +
            EquipmentManager.GetPoleRepairPercent(Data) / 100f;
    }

    private float GetPoleRecoverySeconds()
    {
        float speedPercent =
            EquipmentManager.GetPoleRecoverySpeedPercent(Data);
        return PoleRecoverySeconds /
            (1f + Mathf.Max(0f, speedPercent) / 100f);
    }

    private static float GetPowerChargePerTap(PlayerData data)
    {
        return Mathf.Max(
            1f,
            PowerChargePerTap +
            EquipmentManager.GetPowerChargePerTapBonus(data));
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
        pendingBossPattern = null;
        isBossPatternWarning = false;
        bossPatternWarningTimer = 0f;
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
        pendingBossPattern = null;
        isBossPatternWarning = false;
        bossPatternWarningTimer = 0f;
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
