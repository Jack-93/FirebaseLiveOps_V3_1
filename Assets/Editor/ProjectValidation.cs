using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ProjectValidation
{
    [MenuItem("Tools/Validate Core Data")]
    public static void Run()
    {
        ValidatePlayerDataRoundTrip();
        ValidateLegacyMailCompatibility();
        ValidateCoreProgression();
        ValidateBalanceConfiguration();
        ValidateGachaEconomy();
        ValidatePrototypeScene();
        ValidateCharacterPlaceholders();
        ValidateRuntimeComposition();

        Debug.Log("[Validation] Core data checks passed.");
    }

    private static void ValidateBalanceConfiguration()
    {
        Require(
            GameBalanceConfig.DailyRewardItemNames.Length == 7 &&
            GameBalanceConfig.DailyRewardAmounts.Length == 7,
            "Daily reward configuration must contain seven days.");
        Require(
            GameBalanceConfig.GachaSingleGemCost > 0 &&
            GameBalanceConfig.GachaTenGemCost > 0,
            "Gacha costs must be positive.");
        Require(
            GameBalanceConfig.GachaTenGemCost <
            GameBalanceConfig.GachaSingleGemCost * 10,
            "Ten-pull cost must include a discount.");
        Require(
            GameBalanceConfig.GachaPityLimit > 0,
            "Gacha pity limit must be positive.");
        Require(
            GameBalanceConfig.PlayerAbsoluteMinAttackInterval > 0f &&
            GameBalanceConfig.PlayerAbsoluteMinAttackInterval <=
            GameBalanceConfig.PlayerMinAttackInterval,
            "Player attack interval limits are invalid.");
        Require(
            GameBalanceConfig.EventKillPoints +
            GameBalanceConfig.EventGachaPoints >=
            GameBalanceConfig.EventRewardPointTarget,
            "Event missions cannot reach the reward target.");
    }

    private static void ValidatePlayerDataRoundTrip()
    {
        PlayerData source = new PlayerData
        {
            uid = "test-user",
            nickname = "Tester",
            level = 7,
            gold = 1234,
            tutorialCompleted = true,
            pityCount = 42,
            lastLoginDate = "2026-06-06",
            lastRewardDate = "2026-06-05",
            loginDay = 3,
            currentStage = 12,
            highestStage = 15,
            stageEnemyIndex = 3,
            attackLevel = 8,
            healthLevel = 7,
            attackSpeedLevel = 6,
            tutorialStep = 2,
            totalMonstersDefeated = 123,
            lastOnlineUnixTime = 1780786800L,
            autoAdvance = false,
            equippedWeapon = "Iron Blade",
            equippedArmor = "Iron Guard",
            weaponUpgradeLevel = 3,
            armorUpgradeLevel = 2,
            dailyQuestDate = "2026-06-07",
            dailyQuestKills = 7,
            dailyQuestClaimed = false,
            equippedCompanion = "Astra",
            equippedCompanionRarity = "SSR",
            equippedCompanions = new List<string>
            {
                "Astra",
                "Rook"
            },
            equippedCompanionRarities = new List<string>
            {
                "SSR",
                "SR"
            },
            companionStars = new Dictionary<string, int>
            {
                { "Astra", 3 }
            }
        };

        source.inventory.items.Clear();
        source.inventory.items["Gem"] = 777;
        source.claimedMailIds.Add("claimed-global-mail");
        source.mailbox.Add(new MailData
        {
            mailId = "active-global-mail",
            isGlobalMail = true,
            title = "Validation Mail",
            itemName = "Gem",
            amount = 50,
            isClaimed = false
        });

        Dictionary<string, object> encoded =
            PlayerDataConverter.ToDictionary(source);
        PlayerData decoded =
            PlayerDataConverter.FromDictionary(encoded);

        Require(decoded.uid == source.uid, "UID round trip failed.");
        Require(decoded.nickname == source.nickname,
            "Nickname round trip failed.");
        Require(decoded.inventory.items["Gem"] == 777,
            "Inventory round trip failed.");
        Require(decoded.claimedMailIds.Contains("claimed-global-mail"),
            "Claimed mail IDs round trip failed.");
        Require(decoded.mailbox.Count == 1,
            "Mailbox count round trip failed.");
        Require(decoded.mailbox[0].isGlobalMail,
            "Global mail marker round trip failed.");
        Require(decoded.currentStage == 12,
            "Stage round trip failed.");
        Require(decoded.highestStage == 15,
            "Highest stage round trip failed.");
        Require(decoded.attackLevel == 8,
            "Growth round trip failed.");
        Require(decoded.lastOnlineUnixTime == 1780786800L,
            "Offline timestamp round trip failed.");
        Require(!decoded.autoAdvance,
            "Auto advance round trip failed.");
        Require(decoded.weaponUpgradeLevel == 3 &&
            decoded.armorUpgradeLevel == 2,
            "Equipment round trip failed.");
        Require(decoded.dailyQuestKills == 7,
            "Quest round trip failed.");
        Require(decoded.equippedCompanion == "Astra",
            "Equipped companion round trip failed.");
        Require(decoded.equippedCompanionRarity == "SSR",
            "Companion rarity round trip failed.");
        Require(decoded.equippedCompanions.Count == 3,
            "Companion party round trip failed.");
        Require(decoded.companionStars["Astra"] == 3,
            "Companion stars round trip failed.");
    }

    private static void ValidateLegacyMailCompatibility()
    {
        Dictionary<string, object> legacy = new Dictionary<string, object>
        {
            { "gold", 900L },
            {
                "mailbox",
                new List<object>
                {
                    new Dictionary<string, object>
                    {
                        { "mailId", "legacy-mail" },
                        { "title", "Legacy" },
                        { "itemName", "Gem" },
                        { "amount", 10L },
                        { "isClaimed", false }
                    }
                }
            }
        };

        PlayerData decoded =
            PlayerDataConverter.FromDictionary(legacy);

        Require(decoded.gold == 900,
            "Firestore Int64 conversion failed.");
        Require(decoded.mailbox.Count == 1,
            "Legacy mailbox conversion failed.");
        Require(!decoded.mailbox[0].isGlobalMail,
            "Legacy mail should use the safe local-mail fallback.");
        Require(decoded.currentStage == 1,
            "Legacy stage fallback failed.");
        Require(decoded.attackLevel == 1,
            "Legacy growth fallback failed.");
    }

    private static void ValidateCoreProgression()
    {
        PlayerData data = new PlayerData();
        int baseAttack = GameBalance.GetPlayerAttack(data);
        int baseHealth = GameBalance.GetPlayerMaxHealth(data);
        float baseInterval =
            GameBalance.GetPlayerAttackInterval(data);

        data.attackLevel++;
        data.healthLevel++;
        data.attackSpeedLevel++;

        Require(GameBalance.GetPlayerAttack(data) > baseAttack,
            "Attack growth formula failed.");
        Require(GameBalance.GetPlayerMaxHealth(data) > baseHealth,
            "Health growth formula failed.");
        Require(
            GameBalance.GetPlayerAttackInterval(data) < baseInterval,
            "Attack speed growth formula failed.");
        Require(
            GameBalance.GetEnemyMaxHealth(2, false) >
            GameBalance.GetEnemyMaxHealth(1, false),
            "Stage difficulty formula failed.");
        Require(
            GameBalance.GetEnemyMaxHealth(1, true) >
            GameBalance.GetEnemyMaxHealth(1, false),
            "Boss difficulty formula failed.");
    }

    private static void ValidatePrototypeScene()
    {
        const string mainScenePath =
            "Assets/Scenes/MainGameScene.unity";
        const string gachaScenePath =
            "Assets/Scenes/VerticalGachaScene.unity";

        SceneAsset scene =
            AssetDatabase.LoadAssetAtPath<SceneAsset>(mainScenePath);
        Require(scene != null, "MainGameScene is missing.");
        SceneAsset gachaScene =
            AssetDatabase.LoadAssetAtPath<SceneAsset>(gachaScenePath);
        Require(gachaScene != null, "GachaScene is missing.");

        EditorBuildSettingsScene[] scenes =
            EditorBuildSettings.scenes;
        Require(scenes.Length >= 2,
            "Prototype build scenes are incomplete.");
        Require(scenes[0].enabled &&
            scenes[0].path == mainScenePath,
            "MainGameScene must be the first build scene.");
        Require(scenes[1].enabled &&
            scenes[1].path == gachaScenePath,
            "GachaScene must be the second build scene.");
    }

    private static void ValidateGachaEconomy()
    {
        PlayerData data = new PlayerData();
        data.inventory.items.Clear();
        data.inventory.items["Gem"] = 1000;
        data.inventory.items["GachaTicket"] = 1;

        Require(
            GachaEconomy.TrySpend(
                data,
                1,
                out GachaPayment ticketPayment),
            "Single gacha payment failed.");
        Require(ticketPayment.UsedTickets,
            "Gacha ticket should be spent before Gems.");
        Require(
            GachaEconomy.GetItemCount(data, "GachaTicket") == 0,
            "Gacha ticket was not deducted.");

        Require(
            GachaEconomy.TrySpend(
                data,
                10,
                out GachaPayment gemPayment),
            "Ten gacha Gem payment failed.");
        Require(!gemPayment.UsedTickets &&
            gemPayment.Amount == GachaEconomy.TenGemCost,
            "Ten gacha used the wrong payment.");
        Require(
            GachaEconomy.GetItemCount(data, "Gem") == 100,
            "Ten gacha Gem cost was not deducted.");

        GachaEconomy.Refund(data, gemPayment);
        Require(
            GachaEconomy.GetItemCount(data, "Gem") == 1000,
            "Gacha payment refund failed.");
    }

    private static void ValidateRuntimeComposition()
    {
        GameObject playerObject =
            new GameObject("ValidationPlayerData");
        PlayerDataManager playerManager =
            playerObject.AddComponent<PlayerDataManager>();
        PlayerDataManager.Instance = playerManager;
        playerManager.playerData = new PlayerData();
        playerManager.playerData.inventory.items["Pip"] = 1;
        playerManager.playerData.inventory.items["Astra"] = 1;
        playerManager.playerData.currentStage = 100;
        playerManager.playerData.highestStage = 100;
        playerManager.playerData.stageEnemyIndex =
            GameBalance.EnemiesPerStage - 1;

        GameObject runtimeObject =
            new GameObject("ValidationRuntime");
        BattleManager battle =
            runtimeObject.AddComponent<BattleManager>();
        GrowthManager growth =
            runtimeObject.AddComponent<GrowthManager>();
        TutorialManager tutorial =
            runtimeObject.AddComponent<TutorialManager>();
        CompanionManager companion =
            runtimeObject.AddComponent<CompanionManager>();
        MainGameUI ui =
            runtimeObject.AddComponent<MainGameUI>();

        try
        {
            int attackWithoutCompanion =
                GameBalance.GetPlayerAttack(playerManager.playerData);
            Require(companion.Initialize(),
                "Best owned companion was not equipped.");
            Require(
                playerManager.playerData.equippedCompanion == "Astra",
                "SSR companion should be equipped before R companion.");
            Require(
                GameBalance.GetPlayerAttack(playerManager.playerData) >
                attackWithoutCompanion,
                "Equipped companion did not increase attack.");

            growth.Initialize(battle);
            battle.Initialize();
            tutorial.Initialize(battle, growth);
            ui.Configure(
                null,
                battle,
                growth,
                tutorial,
                companion);

            Require(
                battle.PlayerHealth == battle.PlayerMaxHealth,
                "Battle must start at full health.");
            Require(battle.PlayerHealth > 1,
                "Battle started with invalid health.");

            GameObject canvas = GameObject.Find("MainGameCanvas");
            Require(canvas != null,
                "Main game canvas was not created.");
            Transform safeArea =
                canvas.transform.Find("SafeAreaRoot");
            Require(safeArea != null,
                "Safe area root was not created.");
            Require(safeArea.Find("BattlePanel") != null,
                "Battle panel was not created.");
            Require(safeArea.Find("GrowthPanel") != null,
                "Growth panel was not created.");
            Require(safeArea.Find("BottomNavigation") != null,
                "Bottom navigation was not created.");

            tutorial.BeginTutorial();
            Require(playerManager.playerData.tutorialStep == 1,
                "Tutorial start transition failed.");

            bool upgraded = growth
                .TryUpgradeAsync(UpgradeType.Attack)
                .GetAwaiter()
                .GetResult();

            Require(upgraded, "Tutorial growth action failed.");
            Require(playerManager.playerData.tutorialStep == 2,
                "Tutorial growth transition failed.");
            Require(battle.IsRunning,
                "Battle did not start after tutorial growth.");

            battle.Tick(2f);
            Require(battle.IsRecovering,
                "Player defeat did not start recovery.");
            Require(battle.PlayerHealth == 0,
                "Defeated player health should be zero.");

            battle.Tick(2.1f);
            Require(!battle.IsRecovering,
                "Player recovery did not finish.");
            Require(battle.IsRunning,
                "Auto battle did not resume after recovery.");
            Require(
                battle.PlayerHealth == battle.PlayerMaxHealth,
                "Player did not recover to full health.");
        }
        finally
        {
            GameObject canvas = GameObject.Find("MainGameCanvas");
            if (canvas != null)
                UnityEngine.Object.DestroyImmediate(canvas);

            GameObject eventSystem = GameObject.Find("EventSystem");
            if (eventSystem != null)
                UnityEngine.Object.DestroyImmediate(eventSystem);

            UnityEngine.Object.DestroyImmediate(runtimeObject);
            UnityEngine.Object.DestroyImmediate(playerObject);
            PlayerDataManager.Instance = null;
            CompanionManager.Instance = null;
        }
    }

    private static void ValidateCharacterPlaceholders()
    {
        CharacterDatabase database =
            AssetDatabase.LoadAssetAtPath<CharacterDatabase>(
                "Assets/Resources/CharacterDatabase.asset");

        Require(database != null,
            "Character database is missing.");
        Require(database.characters != null &&
            database.characters.Count >= 3,
            "Character database needs R, SR, and SSR placeholders.");

        bool hasR = false;
        bool hasSR = false;
        bool hasSSR = false;

        foreach (CharacterData character in database.characters)
        {
            Require(character != null,
                "Character database contains a missing asset.");
            Require(!string.IsNullOrWhiteSpace(character.characterName),
                "A placeholder character has no name.");

            hasR |= character.rarity == "R";
            hasSR |= character.rarity == "SR";
            hasSSR |= character.rarity == "SSR";
        }

        Require(hasR && hasSR && hasSSR,
            "Gacha placeholder rarity pools are incomplete.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
