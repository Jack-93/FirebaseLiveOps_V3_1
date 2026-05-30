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
    }

    public void SaveNickname()
    {
        string nickname =
            nicknameInput.text;

        PlayerDataManager.Instance
            .playerData.nickname = nickname;

        FirestoreManager.Instance
            .UpdateNickname(nickname);

        RefreshUI();
    }

    public void AddGold()
    {
        FirestoreManager.Instance
            .AddGold(100);

        RefreshUI();
    }

    public void RefreshUI()
    {
        PlayerData data =
            PlayerDataManager.Instance.playerData;

        playerInfoText.text =
            $"Nickname: {data.nickname}\\n" +
            $"Level: {data.level}";

        goldText.text =
            $"Gold: {data.gold}";
    }
}