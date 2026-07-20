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
    public event Action<EnemyCombatProfile> OnEnemyAttackStarted;
    public event Action<int> OnEnemyAttackPerformed;
    public event Action OnHeroDefeated;
    public event Action OnHeroRecovered;
    public event Action<int> OnHeroDamaged;
    public event Action<int, CharacterData, int> OnCompanionSkillUsed;
    public event Action<BossPatternRuntime> OnBossPatternWarning;
    public event Action<BossPatternRuntime> OnBossPatternCast;
    public event Action<BossPatternRuntime, int> OnBossPatternUsed;
    public event Action OnBossChallengeFailed;
    public event Action OnPowerChargePerformed;
    public event Action<float, float> OnPowerCharged;

    public bool IsInitialized { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsRecovering => isRecovering;
    public bool IsHeroDefeatPlaying => isHeroDefeatPlaying;
    public bool IsBoss { get; private set; }
    public int HeroHealth { get; private set; }
    public int HeroMaxHealth { get; private set; }
    public bool IsHeroDefeated =>
        (isHeroDefeatPlaying || isRecovering) && HeroHealth <= 0;
    public int EnemyHealth { get; private set; }
    public int EnemyMaxHealth { get; private set; }
    public int LastPlayerDamage { get; private set; }
    public int LastEnemyDamage { get; private set; }
    public bool LastDefeatedEnemyWasBoss { get; private set; }
    public EnemyCombatProfile CurrentEnemyCombatProfile { get; private set; }
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
    private float enemyMeleeAttackTimeout;
    private float heroDefeatTimer;
    private float recoveryTimer;
    private bool isHeroDefeatPlaying;
    private bool isRecovering;
    private bool isEnemyMeleeAttackPending;
    private bool isBossPatternWarning;
    private bool isBossPatternCasting;
    private float bossPatternTimer;
    private float bossPatternWarningTimer;
    private float bossPatternImpactTimer;
    private int bossPatternIndex;
    private BossPatternRuntime pendingBossPattern;
    private Vector2 heroBattlePosition = new Vector2(0.25f, 0.5f);
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
    private const float HeroDefeatAnimationSeconds = 1.55f;
    private const float HeroRecoverySeconds = 2f;
    private const float EnemyMeleeAttackTimeoutSeconds = 2.5f;

    public void Initialize()
    {
        PlayerData data = Data;
        if (data == null)
            throw new InvalidOperationException(
                "[Battle] PlayerData is not ready.");

        data.EnsureInitialized();
        HeroMaxHealth = GameBalance.GetHeroMaxHealth(data);
        HeroHealth = HeroMaxHealth;
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

        int previousMax = Math.Max(1, HeroMaxHealth);
        float healthRatio = HeroHealth / (float)previousMax;

        HeroMaxHealth = GameBalance.GetHeroMaxHealth(data);
        if (!IsInitialized)
        {
            HeroHealth = HeroMaxHealth;
        }
        else if (IsHeroDefeated)
        {
            HeroHealth = 0;
        }
        else
        {
            HeroHealth = Mathf.Clamp(
                Mathf.RoundToInt(HeroMaxHealth * healthRatio),
                1,
                HeroMaxHealth);
        }

        NotifyChanged();
    }

    private void Update()
    {
        Tick(BattleTempo.ScaleDeltaTime(Time.deltaTime));
    }

    public void Tick(float deltaTime)
    {
        if (!IsInitialized)
            return;

        if (isHeroDefeatPlaying)
        {
            heroDefeatTimer -= deltaTime;
            if (heroDefeatTimer <= 0f)
                BeginHeroRecovery();

            return;
        }

        if (isRecovering)
        {
            recoveryTimer -= deltaTime;
            if (recoveryTimer <= 0f)
                RestartCurrentStage();

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

            if (isBossPatternWarning)
            {
                bossPatternWarningTimer -= deltaTime;
                if (bossPatternWarningTimer <= 0f)
                    CastPendingBossPattern();
            }
            else if (isBossPatternCasting)
            {
                bossPatternImpactTimer -= deltaTime;
                if (bossPatternImpactTimer <= 0f)
                    ResolvePendingBossPattern();
            }
            else
            {
                bossPatternTimer -= deltaTime;
                if (bossPatternTimer <= 0f)
                    WarnBossPattern();
            }

            if (!IsRunning || isRecovering || isHeroDefeatPlaying)
                return;
        }

        TickPartyDamageBuff(deltaTime);
        TickCompanionBasicAttacks(deltaTime);
        TickCompanionSkillCooldowns(deltaTime);

        if (IsBoss)
            return;

        enemyAttackTimer -= deltaTime;

        if (isEnemyMeleeAttackPending)
        {
            enemyMeleeAttackTimeout -= deltaTime;
            if (enemyMeleeAttackTimeout <= 0f)
                ResolveEnemyAttack();
        }
        else if (!isRecovering && EnemyHealth > 0 && enemyAttackTimer <= 0f)
        {
            BeginEnemyAttack();
            enemyAttackTimer =
                CurrentEnemyCombatProfile.AttackInterval;
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

    public void SetHeroBattlePosition(Vector2 normalizedPosition)
    {
        heroBattlePosition = new Vector2(
            Mathf.Clamp01(normalizedPosition.x),
            Mathf.Clamp01(normalizedPosition.y));
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

    public void ResolveEnemyAttack()
    {
        if (!isEnemyMeleeAttackPending)
            return;

        isEnemyMeleeAttackPending = false;
        enemyMeleeAttackTimeout = 0f;
        if (isRecovering || isHeroDefeatPlaying || EnemyHealth <= 0)
            return;

        EnemyAttack();
    }

    private void BeginEnemyAttack()
    {
        if (isEnemyMeleeAttackPending)
            return;

        isEnemyMeleeAttackPending = true;
        enemyMeleeAttackTimeout = EnemyMeleeAttackTimeoutSeconds;
        SafeEvent.Invoke(
            OnEnemyAttackStarted,
            CurrentEnemyCombatProfile,
            "Battle",
            nameof(OnEnemyAttackStarted));
    }

    private void EnemyAttack()
    {
        int incomingDamage =
            Mathf.RoundToInt(
                GameBalance.GetEnemyAttack(Data.currentStage, IsBoss) *
                CurrentEnemyCombatProfile.DamageMultiplier);
        LastEnemyDamage = ApplyDamageToHero(incomingDamage);
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

        BossPatternDefinition pattern =
            bossPatterns[bossPatternIndex % bossPatterns.Count];
        int sequence = bossPatternIndex;
        bossPatternIndex++;
        if (pattern == null)
        {
            bossPatternTimer = 1f;
            return;
        }

        pendingBossPattern = CreateBossPatternRuntime(pattern, sequence);
        isBossPatternWarning = true;
        bossPatternWarningTimer = Mathf.Max(0.1f, pattern.warningSeconds);
        SafeEvent.Invoke(
            OnBossPatternWarning,
            pendingBossPattern,
            "Battle",
            nameof(OnBossPatternWarning));
        NotifyChanged();
    }

    private void CastPendingBossPattern()
    {
        isBossPatternWarning = false;
        bossPatternWarningTimer = 0f;
        if (pendingBossPattern?.Pattern == null)
        {
            pendingBossPattern = null;
            bossPatternTimer = 1f;
            return;
        }

        isBossPatternCasting = true;
        bossPatternImpactTimer = Mathf.Max(
            0.05f,
            pendingBossPattern.Pattern.impactDelay);
        SafeEvent.Invoke(
            OnBossPatternCast,
            pendingBossPattern,
            "Battle",
            nameof(OnBossPatternCast));
        NotifyChanged();
    }

    private void ResolvePendingBossPattern()
    {
        BossPatternRuntime runtime = pendingBossPattern;
        pendingBossPattern = null;
        isBossPatternCasting = false;
        bossPatternImpactTimer = 0f;
        BossPatternDefinition pattern = runtime?.Pattern;
        if (pattern == null)
        {
            bossPatternTimer = 1f;
            return;
        }

        bossPatternTimer = Mathf.Max(1f, pattern.cooldown);

        int hitCount = GetBossPatternHitCount(runtime);
        int damagePerHit = Mathf.Max(
            1,
            Mathf.RoundToInt(
                GameBalance.GetEnemyAttack(
                    Data.currentStage,
                    true) *
                Mathf.Max(0.1f, pattern.damageMultiplier)));
        int totalDamage = damagePerHit * hitCount;
        LastEnemyDamage = totalDamage > 0
            ? ApplyDamageToHero(totalDamage)
            : 0;

        SafeEvent.Invoke(
            OnBossPatternUsed,
            runtime,
            LastEnemyDamage,
            "Battle",
            nameof(OnBossPatternUsed));
        NotifyChanged();
    }

    private BossPatternRuntime CreateBossPatternRuntime(
        BossPatternDefinition pattern,
        int sequence)
    {
        Vector2 target = heroBattlePosition;
        Vector2[] targets;
        int safeLane = -1;
        switch (pattern.patternType)
        {
            case BossPatternType.TargetedThunder:
                targets = new[] { target };
                break;
            case BossPatternType.TripleFireBreath:
                safeLane = sequence % 3;
                targets = new Vector2[0];
                break;
            case BossPatternType.SpiritVolley:
                targets = new[]
                {
                    ClampBattlePosition(target + new Vector2(-0.16f, 0.08f)),
                    ClampBattlePosition(target + new Vector2(0.16f, 0.08f)),
                    ClampBattlePosition(target + new Vector2(0f, -0.12f))
                };
                break;
            default:
                targets = new[] { target };
                break;
        }

        return new BossPatternRuntime(
            pattern,
            targets,
            safeLane,
            sequence);
    }

    private int GetBossPatternHitCount(BossPatternRuntime runtime)
    {
        BossPatternDefinition pattern = runtime.Pattern;
        switch (pattern.patternType)
        {
            case BossPatternType.TripleFireBreath:
                int heroLane = Mathf.Clamp(
                    Mathf.FloorToInt(heroBattlePosition.y * 3f),
                    0,
                    2);
                return heroLane == runtime.SafeLaneIndex
                    ? 0
                    : Mathf.Max(1, pattern.hitCount);
            case BossPatternType.SpiritVolley:
                int spiritHits = 0;
                foreach (Vector2 target in runtime.TargetPositions)
                {
                    if (IsInsidePatternTarget(heroBattlePosition, target, pattern))
                        spiritHits++;
                }
                return spiritHits;
            default:
                return runtime.TargetPositions.Length > 0 &&
                    IsInsidePatternTarget(
                        heroBattlePosition,
                        runtime.TargetPositions[0],
                        pattern)
                    ? 1
                    : 0;
        }
    }

    private static bool IsInsidePatternTarget(
        Vector2 position,
        Vector2 target,
        BossPatternDefinition pattern)
    {
        float radiusX = Mathf.Max(0.01f, pattern.targetRadiusX);
        float radiusY = Mathf.Max(0.01f, pattern.targetRadiusY);
        float x = (position.x - target.x) / radiusX;
        float y = (position.y - target.y) / radiusY;
        return x * x + y * y <= 1f;
    }

    private static Vector2 ClampBattlePosition(Vector2 position)
    {
        return new Vector2(
            Mathf.Clamp(position.x, 0.08f, 0.92f),
            Mathf.Clamp(position.y, 0.08f, 0.92f));
    }

    private int ApplyDamageToHero(int incomingDamage)
    {
        int damage = GameBalance.GetHeroDamageAfterArmor(
            Data,
            incomingDamage);
        int previousHealth = HeroHealth;
        HeroHealth = Math.Max(0, HeroHealth - damage);
        int actualDamage = previousHealth - HeroHealth;
        if (actualDamage > 0)
        {
            SafeEvent.Invoke(
                OnHeroDamaged,
                actualDamage,
                "Battle",
                nameof(OnHeroDamaged));
        }

        if (HeroHealth <= 0)
            BeginHeroDefeatSequence();

        return actualDamage;
    }

    private void BeginHeroDefeatSequence()
    {
        if (isHeroDefeatPlaying || isRecovering)
            return;

        IsRunning = false;
        isHeroDefeatPlaying = true;
        isRecovering = false;
        heroDefeatTimer = HeroDefeatAnimationSeconds;
        SafeEvent.Invoke(
            OnHeroDefeated,
            "Battle",
            nameof(OnHeroDefeated));
    }

    private void BeginHeroRecovery()
    {
        if (isRecovering)
            return;

        isHeroDefeatPlaying = false;
        isRecovering = true;
        recoveryTimer = GetHeroRecoverySeconds();
        NotifyChanged();
    }

    private void RestartCurrentStage()
    {
        isHeroDefeatPlaying = false;
        isRecovering = false;
        HeroHealth = HeroMaxHealth;
        partyDamageBuffTimer = 0f;
        partyDamageBuffMultiplier = 1f;
        PlayerData data = Data;
        if (data != null)
            data.stageEnemyIndex = 0;
        SpawnEnemy();
        IsRunning = true;
        SafeEvent.Invoke(
            OnHeroRecovered,
            "Battle",
            nameof(OnHeroRecovered));
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

        if (isHeroDefeatPlaying || isRecovering)
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
            case CompanionSkillEffect.HealHero:
                effectValue = ApplyCompanionHeroHeal(character);
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

    private int ApplyCompanionHeroHeal(CharacterData character)
    {
        float healPercent =
            Mathf.Clamp01(character.heroHealPercent);
        int healAmount = Math.Max(
            1,
            Mathf.RoundToInt(
                HeroMaxHealth *
                healPercent *
                GetEquipmentHeroHealingMultiplier()));
        int previousHealth = HeroHealth;
        HeroHealth = Math.Min(
            HeroMaxHealth,
            HeroHealth + healAmount);
        return Math.Max(0, HeroHealth - previousHealth);
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

    private float GetEquipmentHeroHealingMultiplier()
    {
        return 1f +
            EquipmentManager.GetHeroHealingPercent(Data) / 100f;
    }

    private float GetHeroRecoverySeconds()
    {
        float speedPercent =
            EquipmentManager.GetHeroRecoverySpeedPercent(Data);
        return HeroRecoverySeconds /
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
        bool firstClear = clearedStage >= data.highestStage;
        if (firstClear)
        {
            data.highestStage = clearedStage + 1;
            if (defeatedBoss)
                data.level++;
        }
        data.currentStage = data.autoAdvance
            ? Math.Min(clearedStage + 1, data.highestStage)
            : clearedStage;
        data.stageEnemyIndex = 0;

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

        if (firstClear)
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
        IsBoss = GameBalance.IsBossStage(data.currentStage);
        CurrentEnemyCombatProfile =
            EnemyCombatProfileResolver.Resolve(data.currentStage, IsBoss);
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
        isBossPatternCasting = false;
        bossPatternWarningTimer = 0f;
        bossPatternImpactTimer = 0f;
        ResetCompanionAttackTimers();
        isEnemyMeleeAttackPending = false;
        enemyMeleeAttackTimeout = 0f;
        enemyAttackTimer = CurrentEnemyCombatProfile.AttackInterval;
    }

    private void ResetBossChallenge()
    {
        EnemyHealth = EnemyMaxHealth;
        BossTimeRemaining = GameBalance.BossTimeLimit;
        ResetCompanionAttackTimers();
        isEnemyMeleeAttackPending = false;
        enemyMeleeAttackTimeout = 0f;
        enemyAttackTimer = CurrentEnemyCombatProfile.AttackInterval;
        bossPatternIndex = 0;
        bossPatternTimer = GetFirstBossPatternCooldown();
        pendingBossPattern = null;
        isBossPatternWarning = false;
        isBossPatternCasting = false;
        bossPatternWarningTimer = 0f;
        bossPatternImpactTimer = 0f;
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
