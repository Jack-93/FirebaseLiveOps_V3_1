using System;
using TMPro;
using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    public TMP_InputField nicknameInput;
    public TMP_Text goldText;
    public TMP_Text playerInfoText;

    private void Start()
    {
        RefreshUI();

        if (PlayerDataManager.Instance != null)
            PlayerDataManager.Instance.OnPlayerDataChanged += RefreshUI;
    }

    public async void SaveNickname()
    {
        if (nicknameInput == null)
            return;

        string nickname = nicknameInput.text.Trim();
        if (string.IsNullOrEmpty(nickname))
            return;

        try
        {
            await FirestoreManager.Instance.UpdateNicknameAsync(nickname);
            PlayerDataManager.Instance.NotifyPlayerDataChanged();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    public async void AddGold()
    {
        try
        {
            await FirestoreManager.Instance.AddGoldAsync(100);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    public void RefreshUI()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        if (playerInfoText != null)
        {
            playerInfoText.text =
                $"Nickname: {data.nickname}\n" +
                $"Level: {data.level}";
        }

        if (goldText != null)
            goldText.text = $"Gold: {data.gold}";

        if (nicknameInput != null &&
            !nicknameInput.isFocused)
        {
            nicknameInput.text = data.nickname;
        }
    }

    private void OnDestroy()
    {
        if (PlayerDataManager.Instance != null)
            PlayerDataManager.Instance.OnPlayerDataChanged -= RefreshUI;
    }
}
