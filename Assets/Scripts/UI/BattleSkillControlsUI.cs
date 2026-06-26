using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BattleSkillControlsUI
{
    private readonly BattleManager battleManager;
    private readonly CompanionManager companionManager;
    private readonly Action<string> showToast;
    private readonly Color panelLight;
    private readonly Color accent;
    private readonly Color gold;
    private readonly Color success;

    private TMP_Text skillStatusText;
    private TMP_Text powerChargeButtonText;
    private Button powerChargeButton;
    private readonly TMP_Text[] skillButtonTexts =
        new TMP_Text[CompanionManager.PartySize];
    private readonly Image[] skillButtonImages =
        new Image[CompanionManager.PartySize];
    private readonly Image[] skillPortraitImages =
        new Image[CompanionManager.PartySize];
    private readonly RectTransform[] skillCooldownOverlays =
        new RectTransform[CompanionManager.PartySize];

    public BattleSkillControlsUI(
        BattleManager battleManager,
        CompanionManager companionManager,
        Action<string> showToast,
        Color panelLight,
        Color accent,
        Color gold,
        Color success)
    {
        this.battleManager = battleManager;
        this.companionManager = companionManager;
        this.showToast = showToast;
        this.panelLight = panelLight;
        this.accent = accent;
        this.gold = gold;
        this.success = success;
    }

    public void Build(RectTransform parent)
    {
        skillStatusText = RuntimeUiFactory.CreateText(
            "SkillStatus",
            parent,
            "AUTO SKILL",
            21,
            new Vector2(0.58f, 0.205f),
            new Vector2(0.94f, 0.25f),
            TextAlignmentOptions.Right,
            gold);
        skillStatusText.gameObject.SetActive(false);

        powerChargeButton = RuntimeUiFactory.CreateButton(
            "PowerChargeButton",
            parent,
            "CHARGE POWER",
            new Vector2(0.25f, 0.1f),
            new Vector2(0.49f, 0.2f),
            success,
            ChargePower);
        powerChargeButtonText =
            powerChargeButton.GetComponentInChildren<TMP_Text>();

        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            BuildSkillButton(parent, slot);
        }
    }

    public void Refresh()
    {
        if (skillStatusText == null || companionManager == null)
            return;

        RefreshPowerButton();
        RefreshSkillButtons();
        RefreshSkillStatusText();
    }

    private void BuildSkillButton(RectTransform parent, int slot)
    {
        int capturedSlot = slot;
        float left = 0.61f + slot * 0.115f;
        Button skillButton = RuntimeUiFactory.CreateButton(
            $"CompanionSkillButton{slot + 1}",
            parent,
            $"S{slot + 1}",
            new Vector2(left, 0.1f),
            new Vector2(left + 0.1f, 0.2f),
            panelLight,
            () => UseCompanionSkill(capturedSlot));
        skillButtonTexts[slot] =
            skillButton.GetComponentInChildren<TMP_Text>();
        skillButtonImages[slot] =
            skillButton.targetGraphic as Image;
        skillPortraitImages[slot] = RuntimeUiFactory.CreateSpriteImage(
            "Portrait",
            skillButton.transform,
            null,
            new Vector2(0.2f, 0.34f),
            new Vector2(0.8f, 0.9f));
        skillCooldownOverlays[slot] = RuntimeUiFactory.CreatePanel(
            "CooldownOverlay",
            skillButton.transform,
            new Color32(5, 8, 16, 145),
            Vector2.zero,
            Vector2.one);
        skillCooldownOverlays[slot]
            .GetComponent<Image>()
            .raycastTarget = false;
        skillCooldownOverlays[slot].gameObject.SetActive(false);

        TMP_Text label = skillButtonTexts[slot];
        if (label == null)
            return;

        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.06f, 0.03f);
        labelRect.anchorMax = new Vector2(0.94f, 0.34f);
        label.fontSizeMax = 18f;
        label.fontSizeMin = 10f;
        label.transform.SetAsLastSibling();
    }

    private void RefreshPowerButton()
    {
        if (powerChargeButtonText != null)
        {
            powerChargeButtonText.text =
                $"{LocalizationManager.Text("CHARGE POWER", "CHARGE POWER")}\n" +
                "+12";
        }

        if (powerChargeButton != null && battleManager != null)
        {
            powerChargeButton.interactable =
                battleManager.IsRunning &&
                !battleManager.IsRecovering;
        }
    }

    private void RefreshSkillStatusText()
    {
        StringBuilder builder = new StringBuilder();
        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            CharacterData character =
                companionManager.GetEquippedAtSlot(slot);
            if (character == null)
                continue;

            if (builder.Length > 0)
                builder.Append("   ");

            float cooldown =
                slot < battleManager.SkillCooldowns.Count
                    ? battleManager.SkillCooldowns[slot]
                    : 0f;
            builder.Append(
                cooldown <= 0f
                    ? $"{character.characterName}: " +
                      LocalizationManager.Text("READY", "READY")
                    : $"{character.characterName}: {cooldown:0.0}s");
        }

        bool hasSkillStatus = builder.Length > 0;
        skillStatusText.gameObject.SetActive(hasSkillStatus);
        skillStatusText.text = hasSkillStatus
            ? builder.ToString()
            : LocalizationManager.Text(
                "No companion skills equipped.",
                "No companion skills equipped.");
    }

    private void RefreshSkillButtons()
    {
        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            CharacterData character =
                companionManager.GetEquippedAtSlot(slot);
            float cooldown =
                slot < battleManager.SkillCooldowns.Count
                    ? battleManager.SkillCooldowns[slot]
                    : 0f;
            bool hasPower =
                battleManager.PowerCharge >=
                BattleManager.CompanionSkillPowerCost;
            bool ready =
                character != null &&
                cooldown <= 0f &&
                hasPower;

            if (skillButtonTexts[slot] != null)
            {
                skillButtonTexts[slot].text = character == null
                    ? "EMPTY"
                    : ready
                        ? $"{character.characterName}\nREADY"
                        : cooldown > 0f
                            ? $"{character.characterName}\n{cooldown:0.0}s"
                            : $"{character.characterName}\nPWR " +
                              $"{BattleManager.CompanionSkillPowerCost:0}";
            }

            if (skillButtonImages[slot] != null)
                skillButtonImages[slot].color =
                    ready ? accent : panelLight;

            RefreshSkillPortrait(slot, character);
            RefreshCooldownOverlay(slot, character, cooldown);
        }
    }

    private void RefreshSkillPortrait(int slot, CharacterData character)
    {
        if (skillPortraitImages[slot] == null)
            return;

        Sprite portrait = character == null
            ? null
            : character.icon ?? character.battleSprite;
        skillPortraitImages[slot].sprite = portrait;
        skillPortraitImages[slot].color =
            portrait == null ? Color.clear : Color.white;
    }

    private void RefreshCooldownOverlay(
        int slot,
        CharacterData character,
        float cooldown)
    {
        if (skillCooldownOverlays[slot] == null)
            return;

        bool coolingDown = character != null && cooldown > 0f;
        skillCooldownOverlays[slot].gameObject.SetActive(coolingDown);
        if (!coolingDown)
            return;

        float ratio = Mathf.Clamp01(
            cooldown /
            Mathf.Max(1f, character.skillCooldown));
        skillCooldownOverlays[slot].anchorMin =
            new Vector2(0f, 1f - ratio);
        skillCooldownOverlays[slot].anchorMax = Vector2.one;
    }

    private void UseCompanionSkill(int slot)
    {
        if (battleManager != null &&
            battleManager.TryUseCompanionSkill(slot))
        {
            return;
        }

        if (battleManager != null &&
            battleManager.PowerCharge <
            BattleManager.CompanionSkillPowerCost)
        {
            showToast?.Invoke(
                LocalizationManager.Text(
                    "Not enough power charge.",
                    "Not enough power charge."));
            return;
        }

        showToast?.Invoke(
            LocalizationManager.Text(
                "Skill is not ready.",
                "Skill is not ready."));
    }

    private void ChargePower()
    {
        if (battleManager == null || !battleManager.ChargePower())
        {
            showToast?.Invoke(
                LocalizationManager.Text(
                    "Power charger is not ready.",
                    "Power charger is not ready."));
            return;
        }

        Refresh();
    }
}
