using TMPro;
using UnityEngine;
using System.Text;

public class InventoryUIManager : MonoBehaviour
{
    public TMP_Text inventoryText;

    private void Start()
    {
        RefreshInventoryUI();

        InventoryManager.Instance.OnInventoryChanged += RefreshInventoryUI;
    }

    public void AddPotion()
    {
        InventoryManager.Instance
            .AddItem("Potion", 1);

        RefreshInventoryUI();
    }

    public void AddGem()
    {
        InventoryManager.Instance
            .AddItem("Gem", 100);

        RefreshInventoryUI();
    }

    public void RefreshInventoryUI()
    {
        var inventory =
            PlayerDataManager.Instance
            .playerData.inventory.items;

        StringBuilder sb =
            new StringBuilder();

        sb.AppendLine("Inventory");

        foreach (var item in inventory)
        {
            sb.AppendLine(
                $"{item.Key} x{item.Value}"
            );
        }

        inventoryText.text = sb.ToString();
    }

    // 이벤트 중복 등록 방지 메서드 (씬 전환 시)
    private void OnDestroy()
    {
        if(InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= RefreshInventoryUI;
        }
}
}