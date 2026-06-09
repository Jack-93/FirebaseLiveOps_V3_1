using System;
using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainGameBootstrap : MonoBehaviour
{
    public bool IsReady { get; private set; }

    private BattleManager battleManager;
    private GrowthManager growthManager;
    private TutorialManager tutorialManager;
    private MainGameUI mainGameUI;
    private CompanionManager companionManager;
    private bool isInitializing;
    private float autosaveTimer;

    private void Awake()
    {
        GameSettingsManager.ApplySavedSettings();
        MobileScreenLayout.ApplyPortraitSettings();
        MobileScreenLayout.EnsureMainCamera(
            new Color32(20, 28, 45, 255));

        EnsurePersistentManager<GameSettingsManager>(
            "GameSettingsManager");
        EnsurePersistentManager<AudioManager>("AudioManager");
        EnsurePersistentManager<PlayerDataManager>("PlayerDataManager");
        EnsurePersistentManager<FirebaseManager>("FirebaseManager");
        EnsurePersistentManager<FirestoreManager>("FirestoreManager");
        EnsurePersistentManager<PushNotificationManager>(
            "PushNotificationManager");
        EnsurePersistentManager<InventoryManager>("InventoryManager");
        EnsurePersistentManager<MailboxManager>("MailboxManager");
        EnsurePersistentManager<DailyRewardManager>("DailyRewardManager");
        EnsurePersistentManager<AnalyticsManager>("AnalyticsManager");
        EnsurePersistentManager<AccountLinkManager>("AccountLinkManager");
        EnsurePersistentManager<EquipmentManager>("EquipmentManager");
        EnsurePersistentManager<QuestManager>("QuestManager");
        EnsurePersistentManager<ShopManager>("ShopManager");
        EnsurePersistentManager<MonetizationManager>(
            "MonetizationManager");
        EnsurePersistentManager<EventMissionManager>(
            "EventMissionManager");
        companionManager =
            EnsurePersistentManager<CompanionManager>("CompanionManager");

        battleManager = gameObject.AddComponent<BattleManager>();
        growthManager = gameObject.AddComponent<GrowthManager>();
        tutorialManager = gameObject.AddComponent<TutorialManager>();
        mainGameUI = gameObject.AddComponent<MainGameUI>();

        mainGameUI.Configure(
            this,
            battleManager,
            growthManager,
            tutorialManager,
            companionManager);
    }

    private async void Start()
    {
        await InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        if (isInitializing)
            return;

        isInitializing = true;
        IsReady = false;
        mainGameUI.SetLoading(true, "Connecting to game services...");

        try
        {
            await FirebaseManager.WaitUntilReadyAsync();

            FirebaseAuth auth = FirebaseAuth.DefaultInstance;
            FirebaseUser user = auth.CurrentUser;

            if (user == null)
            {
                mainGameUI.SetLoading(false, "");
                mainGameUI.ShowTitleScreen(
                    "Choose how to start.\n" +
                    GoogleCredentialTokenProvider.GetSetupStatus());
                return;
            }

            await InitializeSignedInSessionAsync(user);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            mainGameUI.ShowInitializationError(exception.Message);
        }
        finally
        {
            isInitializing = false;
        }
    }

    public async void StartGuestLogin()
    {
        if (isInitializing)
            return;

        isInitializing = true;
        IsReady = false;
        mainGameUI.SetTitleBusy(true, "Creating guest account...");
        mainGameUI.SetLoading(true, "Creating guest account...");

        try
        {
            await FirebaseManager.WaitUntilReadyAsync();
            AuthResult result =
                await FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync();
            await InitializeSignedInSessionAsync(result.User);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            mainGameUI.SetLoading(false, "");
            mainGameUI.ShowTitleScreen(
                "Guest login failed.\n" + exception.Message);
        }
        finally
        {
            isInitializing = false;
            mainGameUI.SetTitleBusy(false, "");
        }
    }

    public async void StartGoogleLogin()
    {
        if (isInitializing)
            return;

        isInitializing = true;
        IsReady = false;
        mainGameUI.SetTitleBusy(true, "Waiting for Google...");
        mainGameUI.SetLoading(true, "Waiting for Google...");

        try
        {
            await FirebaseManager.WaitUntilReadyAsync();
            AccountLinkTokens tokens =
                await new GoogleCredentialTokenProvider()
                    .RequestTokensAsync();

            if (tokens == null ||
                string.IsNullOrWhiteSpace(tokens.IdToken))
            {
                throw new InvalidOperationException(
                    "Google did not return a valid ID token.");
            }

            Credential credential = GoogleAuthProvider.GetCredential(
                tokens.IdToken,
                tokens.AccessToken);
            FirebaseUser user =
                await FirebaseAuth.DefaultInstance
                    .SignInWithCredentialAsync(credential);
            await InitializeSignedInSessionAsync(user);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            mainGameUI.SetLoading(false, "");
            mainGameUI.ShowTitleScreen(
                "Google login failed.\n" + exception.Message);
        }
        finally
        {
            isInitializing = false;
            mainGameUI.SetTitleBusy(false, "");
        }
    }

    private async Task InitializeSignedInSessionAsync(FirebaseUser user)
    {
        if (user == null)
            throw new InvalidOperationException(
                "No signed-in user is available.");

        mainGameUI.HideTitleScreen();
        mainGameUI.SetLoading(true, "Loading player data...");
        PlayerData data =
            await FirestoreManager.Instance.LoadPlayerDataAsync();

        if (data == null)
            throw new InvalidOperationException(
                "Player data could not be loaded.");

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        int offlineGold = CalculateOfflineGold(data, now);
        long offlineSeconds = data.lastOnlineUnixTime > 0
            ? Math.Max(0, now - data.lastOnlineUnixTime)
            : 0;

        data.uid = user.UserId;
        data.lastLoginDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        data.lastOnlineUnixTime = now;
        data.gold += offlineGold;
        data.EnsureInitialized();
        data.gold = Math.Max(
            data.gold,
            GameBalanceConfig.PrototypeMinimumGold);
        data.inventory.items["Gem"] = Math.Max(
            GachaEconomy.GetItemCount(data, "Gem"),
            GameBalanceConfig.PrototypeMinimumGems);

        PlayerDataManager.Instance.SetPlayerData(data);
        await MonetizationManager.Instance.InitializeAsync();
        await PushNotificationManager.Instance.InitializeAsync();
        EquipmentManager.Instance.InitializeStarterEquipment();
        companionManager.Initialize();

        mainGameUI.SetLoading(true, "Checking rewards...");
        await FirestoreManager.Instance.LoadGlobalMailsAsync();

        growthManager.Initialize(battleManager);
        battleManager.Initialize();
        tutorialManager.Initialize(battleManager, growthManager);
        QuestManager.Instance.Initialize(
            battleManager,
            growthManager,
            EquipmentManager.Instance);
        EventMissionManager.Instance.Initialize(battleManager);

        await FirestoreManager.Instance.SavePlayerDataAsync(data);

        IsReady = true;
        autosaveTimer = 60f;
        mainGameUI.SetLoading(false, "");
        mainGameUI.RefreshAll();

        if (offlineGold > 0)
        {
            mainGameUI.ShowOfflineReward(
                offlineSeconds,
                offlineGold);
        }

        AnalyticsManager.Instance?.LogLogin();
        Debug.Log("[MainGame] Session initialized.");
    }

    public async void RetryInitialization()
    {
        await InitializeAsync();
    }

    public async void OpenGacha()
    {
        if (!IsReady)
            return;

        try
        {
            await SaveNowAsync();
            SceneManager.LoadScene("VerticalGachaScene");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    public async void SaveNow()
    {
        try
        {
            await SaveNowAsync();
            mainGameUI.ShowToast("Progress saved.");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            mainGameUI.ShowToast(
                "Save failed. Progress will retry automatically.");
        }
    }

    public async Task SaveNowAsync()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null || FirestoreManager.Instance == null)
            return;

        data.lastOnlineUnixTime =
            DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await FirestoreManager.Instance.SavePlayerDataAsync(data);
    }

    public async void Logout()
    {
        try
        {
            await SaveNowAsync();
            FirebaseAuth.DefaultInstance.SignOut();
            PlayerDataManager.Instance.SetPlayerData(new PlayerData());
            SceneManager.LoadScene("MainGameScene");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    public async Task<AccountLinkResult> LinkAccountAsync(
        AccountLinkProvider provider)
    {
        if (!IsReady || AccountLinkManager.Instance == null)
        {
            return AccountLinkResult.Failed(
                "Game services are not ready.");
        }

        await SaveNowAsync();
        AccountLinkResult result =
            await AccountLinkManager.Instance.LinkAsync(provider);

        if (result.Success)
        {
            PlayerData data = PlayerDataManager.Instance?.playerData;
            FirebaseUser user =
                FirebaseAuth.DefaultInstance.CurrentUser;
            if (data != null && user != null)
            {
                data.uid = user.UserId;
                await FirestoreManager.Instance.SavePlayerDataAsync(data);
                PlayerDataManager.Instance.NotifyPlayerDataChanged();
            }
        }

        return result;
    }

    private void Update()
    {
        if (!IsReady)
            return;

        autosaveTimer -= Time.unscaledDeltaTime;
        if (autosaveTimer <= 0f)
        {
            autosaveTimer = 60f;
            _ = SaveInBackgroundAsync();
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && IsReady)
            _ = SaveInBackgroundAsync();
    }

    private void OnApplicationQuit()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data != null)
        {
            data.lastOnlineUnixTime =
                DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }

    private async Task SaveInBackgroundAsync()
    {
        try
        {
            await SaveNowAsync();
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"[MainGame] Background save deferred: " +
                exception.Message);
        }
    }

    private static int CalculateOfflineGold(PlayerData data, long now)
    {
        if (data.lastOnlineUnixTime <= 0)
            return 0;

        long maxSeconds = GameBalance.MaxOfflineHours * 60L * 60L;
        long elapsedSeconds = Math.Min(
            maxSeconds,
            Math.Max(0, now - data.lastOnlineUnixTime));
        long minutes = elapsedSeconds / 60L;
        long reward =
            minutes * GameBalance.GetOfflineGoldPerMinute(data);

        return (int)Math.Min(int.MaxValue, reward);
    }

    private static T EnsurePersistentManager<T>(string objectName)
        where T : Component
    {
        T existing = FindAnyObjectByType<T>();
        if (existing != null)
            return existing;

        GameObject managerObject = new GameObject(objectName);
        return managerObject.AddComponent<T>();
    }

}
