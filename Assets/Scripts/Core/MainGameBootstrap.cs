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
    private bool isInitializing;
    private float autosaveTimer;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Screen.orientation = ScreenOrientation.Portrait;
        EnsureMainCamera();

        EnsurePersistentManager<PlayerDataManager>("PlayerDataManager");
        EnsurePersistentManager<FirebaseManager>("FirebaseManager");
        EnsurePersistentManager<FirestoreManager>("FirestoreManager");
        EnsurePersistentManager<InventoryManager>("InventoryManager");
        EnsurePersistentManager<MailboxManager>("MailboxManager");
        EnsurePersistentManager<DailyRewardManager>("DailyRewardManager");
        EnsurePersistentManager<AnalyticsManager>("AnalyticsManager");

        battleManager = gameObject.AddComponent<BattleManager>();
        growthManager = gameObject.AddComponent<GrowthManager>();
        tutorialManager = gameObject.AddComponent<TutorialManager>();
        mainGameUI = gameObject.AddComponent<MainGameUI>();

        mainGameUI.Configure(
            this,
            battleManager,
            growthManager,
            tutorialManager);
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
                mainGameUI.SetLoading(true, "Creating guest account...");
                AuthResult result = await auth.SignInAnonymouslyAsync();
                user = result.User;
            }

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

            PlayerDataManager.Instance.SetPlayerData(data);

            mainGameUI.SetLoading(true, "Checking rewards...");
            await FirestoreManager.Instance.LoadGlobalMailsAsync();

            growthManager.Initialize(battleManager);
            battleManager.Initialize();
            tutorialManager.Initialize(battleManager, growthManager);

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
            mainGameUI.ShowToast("Save failed.");
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

    private void Update()
    {
        if (!IsReady)
            return;

        autosaveTimer -= Time.unscaledDeltaTime;
        if (autosaveTimer <= 0f)
        {
            autosaveTimer = 60f;
            _ = SaveNowAsync();
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && IsReady)
            _ = SaveNowAsync();
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

    private static void EnsureMainCamera()
    {
        Camera existing = Camera.main;
        if (existing != null)
            return;

        GameObject cameraObject = new GameObject(
            "Main Camera",
            typeof(Camera),
            typeof(AudioListener));
        cameraObject.tag = "MainCamera";

        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(20, 28, 45, 255);
        camera.orthographic = true;
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
    }
}
