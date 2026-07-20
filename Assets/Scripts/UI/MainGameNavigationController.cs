using UnityEngine;

public sealed class MainGameNavigationController
{
    private readonly BottomNavigationUI bottomNavigation;
    private readonly GameObject battlePanel;
    private readonly GameObject growthPanel;
    private readonly GameObject gachaPanel;
    private readonly GameObject morePanel;
    private readonly GameObject collectionPanel;
    private readonly GameObject equipmentPanel;
    private readonly GameObject suppliesPanel;
    private readonly GameObject questPanel;
    private readonly GameObject shopPanel;
    private readonly GameObject eventPanel;
    private readonly GameObject settingsPanel;
    private readonly GameObject accountPanel;

    public MainGameNavigationController(
        BottomNavigationUI bottomNavigation,
        GameObject battlePanel,
        GameObject growthPanel,
        GameObject gachaPanel,
        GameObject morePanel,
        GameObject collectionPanel,
        GameObject equipmentPanel,
        GameObject suppliesPanel,
        GameObject questPanel,
        GameObject shopPanel,
        GameObject eventPanel,
        GameObject settingsPanel,
        GameObject accountPanel)
    {
        this.bottomNavigation = bottomNavigation;
        this.battlePanel = battlePanel;
        this.growthPanel = growthPanel;
        this.gachaPanel = gachaPanel;
        this.morePanel = morePanel;
        this.collectionPanel = collectionPanel;
        this.equipmentPanel = equipmentPanel;
        this.suppliesPanel = suppliesPanel;
        this.questPanel = questPanel;
        this.shopPanel = shopPanel;
        this.eventPanel = eventPanel;
        this.settingsPanel = settingsPanel;
        this.accountPanel = accountPanel;
    }

    public void ShowBattle()
    {
        Show(battlePanel, BottomNavigationTab.Battle);
    }

    public void ShowGrowth()
    {
        Show(growthPanel, BottomNavigationTab.Growth);
    }

    public void ShowGacha()
    {
        Show(gachaPanel, BottomNavigationTab.Gacha);
    }

    public void ShowMore()
    {
        Show(morePanel, BottomNavigationTab.More);
    }

    public void ShowCollection()
    {
        Show(collectionPanel, BottomNavigationTab.Collection);
    }

    public void ShowEquipment()
    {
        Show(equipmentPanel, BottomNavigationTab.Equipment);
    }

    public void ShowSupplies()
    {
        Show(suppliesPanel, BottomNavigationTab.Supplies);
    }

    public void ShowQuests()
    {
        Show(questPanel, BottomNavigationTab.More);
    }

    public void ShowShop()
    {
        Show(shopPanel, BottomNavigationTab.More);
    }

    public void ShowEvent()
    {
        Show(eventPanel, BottomNavigationTab.More);
    }

    public void ShowSettings()
    {
        Show(settingsPanel, BottomNavigationTab.More);
    }

    public void ShowAccount()
    {
        Show(accountPanel, BottomNavigationTab.More);
    }

    private void Show(GameObject active, BottomNavigationTab tab)
    {
        SetActive(active);
        bottomNavigation?.SetActive(tab);
    }

    private void SetActive(GameObject active)
    {
        if (battlePanel != null)
            battlePanel.SetActive(active != null);
        if (growthPanel != null)
            growthPanel.SetActive(active == growthPanel);
        if (gachaPanel != null)
            gachaPanel.SetActive(active == gachaPanel);
        if (morePanel != null)
            morePanel.SetActive(active == morePanel);
        if (collectionPanel != null)
            collectionPanel.SetActive(active == collectionPanel);
        if (equipmentPanel != null)
            equipmentPanel.SetActive(active == equipmentPanel);
        if (suppliesPanel != null)
            suppliesPanel.SetActive(active == suppliesPanel);
        if (questPanel != null)
            questPanel.SetActive(active == questPanel);
        if (shopPanel != null)
            shopPanel.SetActive(active == shopPanel);
        if (eventPanel != null)
            eventPanel.SetActive(active == eventPanel);
        if (settingsPanel != null)
            settingsPanel.SetActive(active == settingsPanel);
        if (accountPanel != null)
            accountPanel.SetActive(active == accountPanel);
    }
}
