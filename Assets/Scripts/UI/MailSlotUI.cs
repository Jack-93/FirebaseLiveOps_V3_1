using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MailSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private Button claimButton;

    private MailData mailData;

    public void Setup(MailData mail)
    {
        mailData = mail;

        titleText.text = mail.title;
        rewardText.text = $"{mail.itemName} x{mail.amount}";

        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(OnClickClaim);
    }

    private async void OnClickClaim()
    {
        bool claimed =
            await MailboxManager.Instance.ClaimMailAsync(mailData);

        if (claimed)
            Destroy(gameObject);
    }
}
