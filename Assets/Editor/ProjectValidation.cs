using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class ProjectValidation
{
    [MenuItem("Tools/Validate Core Data")]
    public static void Run()
    {
        ValidatePlayerDataRoundTrip();
        ValidateSaveSafety();
        ValidateSavePathOwnership();
        ValidateLegacyMailCompatibility();
        ValidateCoreProgression();
        ValidateBalanceConfiguration();
        ValidateBattleLayout();
        ValidateGachaEconomy();
        ValidateStoryIntro();
        ValidatePrototypeScene();
        ValidatePrototypeUiArt();
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

    private static void ValidateBattleLayout()
    {
        string[] stageBackgrounds =
        {
            "StageSunset",
            "StageForest",
            "StageRooftop",
            "StageRain"
        };
        foreach (string background in stageBackgrounds)
        {
            ValidatePrototypeSpriteAsset(
                "Assets/Resources/PrototypeArt/Backgrounds/" +
                background + ".png",
                background + " battle background is missing.");
        }

        string[] enemies =
        {
            "CatScout",
            "CatForest",
            "CatRooftop",
            "CatRain",
            "CatScoutBoss",
            "CatForestBoss",
            "CatRooftopBoss",
            "CatRainBoss"
        };
        foreach (string enemy in enemies)
        {
            ValidatePrototypeSpriteAsset(
                "Assets/Resources/PrototypeArt/Enemies/" +
                enemy + ".png",
                enemy + " battle enemy art is missing.");
        }

        string[] enemyAnimationFolders =
        {
            "CatScout/Idle",
            "CatScout/Attack",
            "CatForest/Idle",
            "CatRooftop/Attack",
            "CatRainBoss/Skill"
        };
        foreach (string folder in enemyAnimationFolders)
        {
            ValidatePrototypeSpriteFolder(
                "Assets/Resources/PrototypeArt/Enemies/Animations/" +
                folder,
                folder + " enemy animation frames are missing.");
        }

        Require(
            BattleLayoutConfig.CompanionAnchors.Length ==
            CompanionManager.PartySize,
            "Battle layout must define one anchor per party slot.");
        Require(
            BattleLayoutConfig.SupportSparrowAnchor.x <
            BattleLayoutConfig.CompanionAnchors[0].x,
            "Support sparrow must remain behind companions.");
        Require(
            BattleLayoutConfig.CompanionAnchors[0].y >
            BattleLayoutConfig.CompanionAnchors[1].y &&
            BattleLayoutConfig.CompanionAnchors[1].y >
            BattleLayoutConfig.CompanionAnchors[2].y,
            "Companion slots must keep the fixed top-to-bottom order.");
        Require(
            BattleLayoutConfig.EnemyAnchor.x >
            BattleLayoutConfig.CompanionAnchors[2].x,
            "Enemy must remain on the right side of the party.");
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
            tutorialGachaClaimed = true,
            tutorialGachaTicketsGranted = true,
            storyIntroCompleted = true,
            storyIntroCutIndex = 6,
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
        Require(decoded.tutorialGachaClaimed,
            "Tutorial free gacha flag round trip failed.");
        Require(decoded.tutorialGachaTicketsGranted,
            "Tutorial gacha ticket gift flag round trip failed.");
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
        Require(decoded.storyIntroCompleted &&
            decoded.storyIntroCutIndex == 6,
            "Story intro round trip failed.");
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

    private static void ValidateSaveSafety()
    {
        PlayerData olderServer = new PlayerData
        {
            uid = "validation-user",
            lastOnlineUnixTime = 100
        };
        PlayerData newerLocal = new PlayerData
        {
            uid = "validation-user",
            lastOnlineUnixTime = 101
        };
        PlayerData sameAgeLocal = new PlayerData
        {
            uid = "validation-user",
            lastOnlineUnixTime = 100
        };

        Require(
            PlayerDataLocalCache.IsNewerThan(newerLocal, olderServer),
            "Newer local cache should be preferred over server data.");
        Require(
            !PlayerDataLocalCache.IsNewerThan(sameAgeLocal, olderServer),
            "Equal local cache age should not override server data.");
        Require(
            !PlayerDataLocalCache.IsNewerThan(null, olderServer),
            "Missing local cache should not override server data.");
        Require(
            !PlayerDataLocalCache.IsNewerThan(newerLocal, null),
            "Missing server data must be handled explicitly.");
    }

    private static void ValidateSavePathOwnership()
    {
        HashSet<string> allowed = new HashSet<string>
        {
            "Assets/Scripts/Battle/BattleManager.cs",
            "Assets/Scripts/Core/MainGameBootstrap.cs",
            "Assets/Scripts/Data/PlayerDataSaveScheduler.cs",
            "Assets/Scripts/Firebase/FirestoreManager.cs",
            "Assets/Scripts/LiveOps/MonetizationManager.cs",
            "Assets/Scripts/Tutorial/TutorialManager.cs"
        };

        foreach (string file in Directory.GetFiles(
            "Assets/Scripts",
            "*.cs",
            SearchOption.AllDirectories))
        {
            string normalized = file.Replace('\\', '/');
            if (allowed.Contains(normalized))
                continue;

            string text = File.ReadAllText(file);
            Require(
                !text.Contains("SavePlayerDataAsync("),
                "Gameplay save must go through PlayerDataSaveScheduler: " +
                normalized);
        }
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

        SceneAsset scene =
            AssetDatabase.LoadAssetAtPath<SceneAsset>(mainScenePath);
        Require(scene != null, "MainGameScene is missing.");

        EditorBuildSettingsScene[] scenes =
            EditorBuildSettings.scenes;
        Require(scenes.Length >= 1,
            "Prototype build scenes are incomplete.");
        Require(scenes[0].enabled &&
            scenes[0].path == mainScenePath,
            "MainGameScene must be the first build scene.");
    }

    private static void ValidatePrototypeUiArt()
    {
        const string panelFramePath =
            "Assets/Resources/PrototypeArt/UI/PanelFrame.png";
        Texture2D panelFrame =
            AssetDatabase.LoadAssetAtPath<Texture2D>(panelFramePath);
        Require(panelFrame != null,
            "PanelFrame UI art is missing.");

        TextureImporter importer =
            AssetImporter.GetAtPath(panelFramePath) as TextureImporter;
        Require(importer != null,
            "PanelFrame importer is missing.");
        Require(importer.textureType == TextureImporterType.Sprite,
            "PanelFrame must be imported as a Sprite.");
        Require(importer.spriteBorder == new Vector4(32f, 32f, 32f, 32f),
            "PanelFrame must use the stable 32px 9-slice border.");
        Require(!importer.mipmapEnabled,
            "PanelFrame mipmaps must stay disabled for pixel UI.");

        ValidateUiSpriteBorder(
            "Assets/Resources/PrototypeArt/UI/ButtonNormal.png",
            new Vector4(32f, 32f, 32f, 32f));
        ValidateUiSpriteBorder(
            "Assets/Resources/PrototypeArt/UI/ButtonSelected.png",
            new Vector4(32f, 32f, 32f, 32f));

        ValidatePrototypeSpriteAsset(
            "Assets/Resources/PrototypeArt/Banners/" +
            "StandardRecruitment.png",
            "StandardRecruitment gacha banner art is missing.");
        ValidatePrototypeSpriteAsset(
            "Assets/Resources/PrototypeArt/UI/KenneyIcons/Game/" +
            "Game_gear.png",
            "Kenney game icon subset is missing.");
        ValidatePrototypeSpriteAsset(
            "Assets/Resources/PrototypeArt/UI/KenneyIcons/Board/" +
            "Board_cards_stack.png",
            "Kenney board icon subset is missing.");
        ValidatePrototypeSpriteAsset(
            "Assets/Resources/PrototypeArt/UI/ThemeIcons/" +
            "ThemeElectric_01.png",
            "Electric theme icon prototype is missing.");
        ValidatePrototypeSpriteAsset(
            "Assets/Resources/PrototypeArt/Enemies/" +
            "CatScout.png",
            "Last Tick cat enemy art is missing.");
    }

    private static void ValidateUiSpriteBorder(
        string assetPath,
        Vector4 expectedBorder)
    {
        Texture2D texture =
            AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        Require(texture != null,
            assetPath + " is missing.");

        TextureImporter importer =
            AssetImporter.GetAtPath(assetPath) as TextureImporter;
        Require(importer != null,
            assetPath + " importer is missing.");
        Require(importer.textureType == TextureImporterType.Sprite,
            assetPath + " must be imported as a Sprite.");
        Require(importer.spriteBorder == expectedBorder,
            assetPath + " has an invalid 9-slice border.");
        Require(!importer.mipmapEnabled,
            assetPath + " mipmaps must stay disabled for pixel UI.");
    }

    private static void ValidatePrototypeSpriteAsset(
        string assetPath,
        string missingMessage)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        Require(sprite != null, missingMessage);
    }

    private static void ValidatePrototypeSpriteFolder(
        string assetFolderPath,
        string missingMessage)
    {
        string[] guids =
            AssetDatabase.FindAssets("t:Sprite", new[] { assetFolderPath });
        Require(guids != null && guids.Length > 0, missingMessage);
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

    private static void ValidateStoryIntro()
    {
        List<StoryIntroCut> cuts =
            StoryIntroDatabase.GetCuts();

        Require(cuts.Count >= 5 && cuts.Count <= 7,
            "Story intro tutorial should contain five to seven cuts.");
        Require(
            StoryIntroDatabase.PlayerRole ==
            "\uCC38\uC0C8 \uC774\uB4F1\uBCD1",
            "Player role story setting is invalid.");
        Require(
            StoryIntroDatabase.EnemyFaction == "\uACE0\uC591\uC774",
            "Enemy faction story setting is invalid.");
        Require(
            StoryIntroDatabase.WarObjective == "\uC804\uBD07\uB300",
            "War objective story setting is invalid.");

        for (int i = 0; i < cuts.Count; i++)
        {
            StoryIntroCut cut = cuts[i];
            Require(cut != null,
                "Story intro contains an empty cut.");
            Require(cut.cutIndex == i + 1,
                "Story intro cut index is not sequential.");
            Require(!string.IsNullOrWhiteSpace(cut.title),
                "Story intro cut title is missing.");
            Require(!string.IsNullOrWhiteSpace(cut.body),
                "Story intro cut body is missing.");
            Require(
                !cut.body.Contains(
                    "\uB300\uC0AC\uB294 \uCD94\uD6C4 \uD655\uC815"),
                "Story intro cut body must contain draft dialogue.");
            Require(!string.IsNullOrWhiteSpace(cut.artDirection),
                "Story intro art direction is missing.");
            Require(
                cut.artDirection.Contains(
                    "\uC544\uD2B8 \uD544\uC694"),
                "Story intro cut must mark pending art clearly.");
            Require(!string.IsNullOrWhiteSpace(cut.artResourcePath),
                "Story intro cut art resource path is missing.");
            ValidatePrototypeSpriteAsset(
                "Assets/Resources/" + cut.artResourcePath + ".png",
                "Story intro cut art is missing: " +
                cut.artResourcePath);
        }
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

        CompanionManager.Instance = null;
        GachaManager.Instance = null;
        InventoryManager.Instance = null;

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
        GachaManager gacha =
            runtimeObject.AddComponent<GachaManager>();
        InventoryManager inventory =
            runtimeObject.AddComponent<InventoryManager>();
        InventoryManager.Instance = inventory;
        MainGameUI ui =
            runtimeObject.AddComponent<MainGameUI>();

        try
        {
            CharacterDatabase characterDatabase =
                AssetDatabase.LoadAssetAtPath<CharacterDatabase>(
                    "Assets/Resources/CharacterDatabase.asset");
            Require(gacha.Initialize(characterDatabase),
                "Gacha database was not initialized.");

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
            Transform storyIntro =
                FindDescendant(safeArea, "StoryIntroOverlay");
            Require(storyIntro != null,
                "Story intro overlay was not created.");
            Transform previousButtonTransform =
                FindDescendant(storyIntro, "StoryIntroPreviousButton");
            Require(
                previousButtonTransform != null,
                "Story intro previous button was not created.");
            Button previousButton =
                previousButtonTransform.GetComponent<Button>();
            Require(previousButton != null,
                "Story intro previous button has no Button component.");
            Require(
                FindDescendant(storyIntro, "StoryIntroSkipButton") == null,
                "Story intro skip button should not exist.");
            Transform storyCounter =
                FindDescendant(storyIntro, "StoryIntroCounter");
            Require(
                storyCounter != null &&
                storyCounter.GetComponent<TMP_Text>()?.text == "1/7",
                "Story intro counter should show current and total cuts.");

            playerManager.playerData.storyIntroCutIndex = 2;
            ui.RefreshAll();
            Require(previousButton.interactable,
                "Story intro previous button should be interactable after first cut.");
            previousButton.onClick.Invoke();
            Require(playerManager.playerData.storyIntroCutIndex == 1,
                "Story intro previous button click failed.");
            previousButton.onClick.Invoke();
            previousButton.onClick.Invoke();
            Require(playerManager.playerData.storyIntroCutIndex == 0,
                "Story intro previous transition should stop at first cut.");
            ui.RefreshAll();
            Require(!previousButton.interactable,
                "Story intro previous button should be disabled on first cut.");

            int tutorialTicketsBefore =
                GachaEconomy.GetItemCount(
                    playerManager.playerData,
                    "GachaTicket");

            tutorial.BeginTutorial();
            Require(playerManager.playerData.storyIntroCompleted,
                "Tutorial start should complete the story intro.");
            Require(playerManager.playerData.tutorialStep == 0,
                "Tutorial should wait for ticket gift confirmation.");
            Require(!playerManager.playerData.tutorialGachaClaimed,
                "Tutorial gacha should not auto-claim after story.");
            Require(!playerManager.playerData.tutorialGachaTicketsGranted,
                "Tutorial tickets should not be granted before the gift button.");
            Require(
                playerManager.playerData.pendingTutorialGachaResults.Count == 0,
                "Tutorial gacha results should not exist before click.");

            ui.RefreshAll();
            Transform tutorialAction =
                FindDescendant(safeArea, "TutorialAction");
            Require(tutorialAction != null,
                "Tutorial action button was not created.");
            Require(tutorialAction.gameObject.activeSelf,
                "Tutorial ticket gift action should be visible.");
            Button tutorialActionButton =
                tutorialAction.GetComponent<Button>();
            Require(tutorialActionButton != null,
                "Tutorial action button has no Button component.");
            tutorialActionButton.onClick.Invoke();

            Require(playerManager.playerData.tutorialGachaTicketsGranted,
                "Tutorial tickets were not granted by gift button.");
            Require(playerManager.playerData.tutorialStep == 0,
                "Tutorial ticket gift should keep recruitment step active.");
            Require(!playerManager.playerData.tutorialGachaClaimed,
                "Tutorial gacha should wait for manual 10x recruitment.");
            Require(
                GachaEconomy.GetItemCount(
                    playerManager.playerData,
                    "GachaTicket") ==
                tutorialTicketsBefore +
                TutorialManager.TutorialGachaTicketCount,
                "Tutorial ticket gift amount is wrong.");

            Transform recruitTenTransform =
                FindDescendant(safeArea, "RecruitTenButton");
            Require(recruitTenTransform != null,
                "Recruit ten button was not created.");
            Button recruitTenButton =
                recruitTenTransform.GetComponent<Button>();
            Require(recruitTenButton != null,
                "Recruit ten button has no Button component.");
            recruitTenButton.onClick.Invoke();

            Require(playerManager.playerData.tutorialStep == 1,
                "Tutorial 10x recruitment did not advance to power charge.");
            Require(playerManager.playerData.tutorialGachaClaimed,
                "Tutorial gacha was not completed by 10x recruitment.");
            Require(
                GachaEconomy.GetItemCount(
                    playerManager.playerData,
                    "GachaTicket") == tutorialTicketsBefore,
                "Tutorial 10x recruitment did not spend granted tickets.");
            Require(
                playerManager.playerData.pendingTutorialGachaResults.Count == 0,
                "Tutorial gacha should not store pending free results.");
            Require(
                playerManager.playerData.equippedCompanions.Exists(
                    companionName => !string.IsNullOrEmpty(companionName)),
                "Tutorial gacha did not keep a companion equipped.");

            ui.RefreshAll();
            Require(
                tutorialAction != null &&
                !tutorialAction.gameObject.activeSelf,
                "Power charge tutorial should not show a battle button.");

            Require(battle.ChargePower(),
                "Tutorial power charge action failed.");
            Require(playerManager.playerData.tutorialStep == 2,
                "Tutorial power charge transition failed.");

            bool upgraded = growth
                .TryUpgradeAsync(UpgradeType.Attack)
                .GetAwaiter()
                .GetResult();

            Require(upgraded, "Tutorial growth action failed.");
            Require(playerManager.playerData.tutorialStep == 3,
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
            GachaManager.Instance = null;
            InventoryManager.Instance = null;
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

        string[] artReadyCharacters = { "Pip", "Nib", "Taro" };
        foreach (string characterName in artReadyCharacters)
        {
            CharacterData character = database.characters.Find(
                item => item != null &&
                    item.characterName == characterName);
            Require(
                character != null &&
                character.icon != null &&
                character.battleSprite != null,
                characterName + " prototype art is not connected.");
        }

        Require(hasR && hasSR && hasSSR,
            "Gacha placeholder rarity pools are incomplete.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    private static Transform FindDescendant(Transform root, string name)
    {
        if (root == null || string.IsNullOrEmpty(name))
            return null;

        foreach (Transform child in root.GetComponentsInChildren<Transform>(
            true))
        {
            if (child.name == name)
                return child;
        }

        return null;
    }
}
