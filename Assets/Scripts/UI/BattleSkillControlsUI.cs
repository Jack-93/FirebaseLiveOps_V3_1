using System;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BattleSkillControlsUI
{
    private const string NumberResourceRoot =
        "PrototypeArt/Numbers/DamageGold";

    private readonly BattleManager battleManager;
    private readonly CompanionManager companionManager;
    private readonly Action<string> showToast;
    private readonly Color panelLight;
    private readonly Color accent;
    private readonly Color gold;
    private readonly Color success;

    private TMP_Text skillStatusText;
    private TMP_Text powerChargeButtonText;
    private TMP_Text powerChargeSlashText;
    private RectTransform controlPanel;
    private Button powerChargeButton;
    private Image powerChargeButtonImage;
    private SpriteNumberText powerChargeCurrentNumberText;
    private SpriteNumberText powerChargeMaxNumberText;
    private SpriteNumberText powerChargeTapNumberText;
    private readonly Button[] skillButtons =
        new Button[CompanionManager.PartySize];
    private readonly TMP_Text[] skillButtonTexts =
        new TMP_Text[CompanionManager.PartySize];
    private readonly SpriteNumberText[] skillStateNumberTexts =
        new SpriteNumberText[CompanionManager.PartySize];
    private readonly Image[] skillButtonImages =
        new Image[CompanionManager.PartySize];
    private readonly Image[] skillPortraitImages =
        new Image[CompanionManager.PartySize];
    private readonly Image[] skillReadyGlowImages =
        new Image[CompanionManager.PartySize];
    private readonly RectTransform[] skillCooldownOverlays =
        new RectTransform[CompanionManager.PartySize];
    private readonly RectTransform[] skillReadyGlowRects =
        new RectTransform[CompanionManager.PartySize];
    private float readyPulseTime;

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
        controlPanel = BuildControlPanel(parent);
        skillStatusText = RuntimeUiFactory.CreateText(
            "SkillStatus",
            controlPanel,
            "\uB3D9\uB8CC \uC2A4\uD0AC",
            21,
            new Vector2(0.41f, 0.76f),
            new Vector2(0.96f, 0.86f),
            TextAlignmentOptions.Right,
            gold);

        powerChargeButton = RuntimeUiFactory.CreateButton(
            "PowerChargeButton",
            controlPanel,
            "CHARGE POWER",
            new Vector2(0.04f, 0.16f),
            new Vector2(0.36f, 0.88f),
            success,
            ChargePower);
        powerChargeButtonText =
            powerChargeButton.GetComponentInChildren<TMP_Text>();
        powerChargeButtonImage =
            powerChargeButton.targetGraphic as Image;
        ConfigurePowerChargeButtonText();
        BuildPowerChargeNumbers();

        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            BuildSkillButton(controlPanel, slot);
        }
    }

    public void Bind(RectTransform parent)
    {
        controlPanel = RuntimeUiBinder.FindRect(parent, "BattleControlCard") ??
            BuildControlPanel(parent);
        skillStatusText =
            RuntimeUiBinder.FindText(parent, "SkillStatus");
        if (skillStatusText == null)
        {
            skillStatusText = RuntimeUiFactory.CreateText(
                "SkillStatus",
                controlPanel,
                "\uB3D9\uB8CC \uC2A4\uD0AC",
                21,
                new Vector2(0.41f, 0.76f),
                new Vector2(0.96f, 0.86f),
                TextAlignmentOptions.Right,
                gold);
        }
        ReparentAndAnchor(
            skillStatusText.GetComponent<RectTransform>(),
            controlPanel,
            new Vector2(0.41f, 0.76f),
            new Vector2(0.96f, 0.86f));
        powerChargeButton =
            RuntimeUiBinder.FindButton(parent, "PowerChargeButton");
        bool createdPowerChargeButton = powerChargeButton == null;
        if (powerChargeButton == null)
        {
            powerChargeButton = RuntimeUiFactory.CreateButton(
                "PowerChargeButton",
                controlPanel,
                "CHARGE POWER",
                new Vector2(0.04f, 0.16f),
                new Vector2(0.36f, 0.88f),
                success,
                ChargePower);
        }
        ReparentAndAnchor(
            powerChargeButton == null
                ? null
                : powerChargeButton.GetComponent<RectTransform>(),
            controlPanel,
            new Vector2(0.04f, 0.16f),
            new Vector2(0.36f, 0.88f));
        RuntimeUiBinder.ReplaceButtonAction(
            powerChargeButton,
            ChargePower);
        powerChargeButtonText = powerChargeButton == null
            ? null
            : powerChargeButton.GetComponentInChildren<TMP_Text>(true);
        powerChargeButtonImage = powerChargeButton == null
            ? null
            : powerChargeButton.targetGraphic as Image;
        ConfigurePowerChargeButtonText();
        powerChargeSlashText =
            RuntimeUiBinder.FindText(parent, "PowerChargeSlashText");
        if (createdPowerChargeButton)
            BuildPowerChargeNumbers();
        else
            BindPowerChargeNumbers();

        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
            BindSkillButton(parent, slot);
    }

    public void Refresh()
    {
        if (skillStatusText == null ||
            companionManager == null ||
            battleManager == null)
            return;

        readyPulseTime += Time.deltaTime;
        RefreshPowerButton();
        RefreshSkillButtons();
        RefreshSkillStatusText();
    }

    private void BuildSkillButton(RectTransform parent, int slot)
    {
        int capturedSlot = slot;
        float left = 0.42f + slot * 0.18f;
        Button skillButton = RuntimeUiFactory.CreateButton(
            $"CompanionSkillButton{slot + 1}",
            parent,
            $"S{slot + 1}",
            new Vector2(left, 0.16f),
            new Vector2(left + 0.16f, 0.78f),
            panelLight,
            () => UseCompanionSkill(capturedSlot));
        skillButtons[slot] = skillButton;
        skillButtonTexts[slot] =
            skillButton.GetComponentInChildren<TMP_Text>();
        skillButtonImages[slot] =
            skillButton.targetGraphic as Image;
        skillReadyGlowImages[slot] = RuntimeUiFactory.CreateSpriteImage(
            "ReadyGlow",
            skillButton.transform,
            PrototypeUiArt.SkillFrame,
            new Vector2(-0.08f, -0.08f),
            new Vector2(1.08f, 1.08f));
        skillReadyGlowImages[slot].type = Image.Type.Simple;
        skillReadyGlowImages[slot].preserveAspect = true;
        skillReadyGlowImages[slot].color = Color.clear;
        skillReadyGlowRects[slot] =
            skillReadyGlowImages[slot].GetComponent<RectTransform>();
        skillPortraitImages[slot] = RuntimeUiFactory.CreateSpriteImage(
            "Portrait",
            skillButton.transform,
            null,
            new Vector2(0.13f, 0.34f),
            new Vector2(0.87f, 0.9f));
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
        skillReadyGlowImages[slot].transform.SetAsLastSibling();

        TMP_Text label = skillButtonTexts[slot];
        if (label == null)
            return;

        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.06f, 0.03f);
        labelRect.anchorMax = new Vector2(0.94f, 0.32f);
        label.fontSizeMax = 16f;
        label.fontSizeMin = 10f;
        label.transform.SetAsLastSibling();

        skillStateNumberTexts[slot] = new SpriteNumberText(
            skillButton.transform,
            "SkillStateNumberText",
            NumberResourceRoot,
            17f,
            new Vector2(0.12f, 0.03f),
            new Vector2(0.88f, 0.28f));
        skillStateNumberTexts[slot].SetActive(false);
    }

    private void BindSkillButton(RectTransform parent, int slot)
    {
        int capturedSlot = slot;
        float left = 0.42f + slot * 0.18f;
        Button skillButton = RuntimeUiBinder.FindButton(
            parent,
            $"CompanionSkillButton{slot + 1}");
        if (skillButton == null)
        {
            BuildSkillButton(controlPanel, slot);
            return;
        }
        skillButtons[slot] = skillButton;
        ReparentAndAnchor(
            skillButton.GetComponent<RectTransform>(),
            controlPanel,
            new Vector2(left, 0.16f),
            new Vector2(left + 0.16f, 0.78f));
        RuntimeUiBinder.ReplaceButtonAction(
            skillButton,
            () => UseCompanionSkill(capturedSlot));

        skillButtonTexts[slot] = skillButton == null
            ? null
            : skillButton.GetComponentInChildren<TMP_Text>(true);
        skillButtonImages[slot] = skillButton == null
            ? null
            : skillButton.targetGraphic as Image;
        skillReadyGlowImages[slot] = skillButton == null
            ? null
            : RuntimeUiBinder.FindImage(skillButton.transform, "ReadyGlow");
        skillReadyGlowRects[slot] =
            skillReadyGlowImages[slot] == null
                ? null
                : skillReadyGlowImages[slot].GetComponent<RectTransform>();
        skillPortraitImages[slot] = skillButton == null
            ? null
            : RuntimeUiBinder.FindImage(skillButton.transform, "Portrait");
        skillCooldownOverlays[slot] = skillButton == null
            ? null
            : RuntimeUiBinder.FindRect(
                skillButton.transform,
                "CooldownOverlay");
        skillStateNumberTexts[slot] = skillButton == null
            ? null
            : new SpriteNumberText(
                RuntimeUiBinder.FindRect(
                    skillButton.transform,
                    "SkillStateNumberText"),
                NumberResourceRoot,
                17f);
        ConfigureSkillButtonLayout(slot);
    }

    private RectTransform BuildControlPanel(RectTransform parent)
    {
        RectTransform existing =
            RuntimeUiBinder.FindRect(parent, "BattleControlCard");
        if (existing != null)
        {
            ReparentAndAnchor(
                existing,
                parent,
                new Vector2(0.02f, 0.02f),
                new Vector2(0.98f, 0.325f));
            existing.SetAsLastSibling();
            return existing;
        }

        RectTransform card = RuntimeUiFactory.CreatePanel(
            "BattleControlCard",
            parent,
            new Color32(63, 48, 36, 245),
            new Vector2(0.02f, 0.02f),
            new Vector2(0.98f, 0.325f));
        card.SetAsLastSibling();

        RuntimeUiFactory.CreateText(
            "PowerChargeHeader",
            card,
            "\uC804\uB825 \uCDA9\uC804",
            20,
            new Vector2(0.04f, 0.86f),
            new Vector2(0.36f, 0.98f),
            TextAlignmentOptions.Center,
            gold);
        RuntimeUiFactory.CreateText(
            "SkillHeader",
            card,
            "\uCE90\uB9AD\uD130 \uC2A4\uD0AC",
            20,
            new Vector2(0.42f, 0.86f),
            new Vector2(0.96f, 0.98f),
            TextAlignmentOptions.Center,
            gold);
        RectTransform divider = RuntimeUiFactory.CreatePanel(
            "BattleControlDivider",
            card,
            new Color32(22, 18, 16, 180),
            new Vector2(0.385f, 0.08f),
            new Vector2(0.395f, 0.92f));
        divider.GetComponent<Image>().raycastTarget = false;

        return card;
    }

    private static void ReparentAndAnchor(
        RectTransform rect,
        RectTransform parent,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        if (rect == null || parent == null)
            return;

        if (rect.parent != parent)
            rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void ConfigureSkillButtonLayout(int slot)
    {
        TMP_Text label = skillButtonTexts[slot];
        if (label != null)
        {
            RectTransform labelRect = label.GetComponent<RectTransform>();
            ReparentAndAnchor(
                labelRect,
                skillButtons[slot].transform as RectTransform,
                new Vector2(0.06f, 0.03f),
                new Vector2(0.94f, 0.32f));
            label.fontSizeMax = 16f;
            label.fontSizeMin = 10f;
            label.alignment = TextAlignmentOptions.Center;
            label.transform.SetAsLastSibling();
        }

        if (skillPortraitImages[slot] != null)
        {
            ReparentAndAnchor(
                skillPortraitImages[slot].GetComponent<RectTransform>(),
                skillButtons[slot].transform as RectTransform,
                new Vector2(0.13f, 0.34f),
                new Vector2(0.87f, 0.9f));
        }

        if (skillCooldownOverlays[slot] != null)
        {
            ReparentAndAnchor(
                skillCooldownOverlays[slot],
                skillButtons[slot].transform as RectTransform,
                Vector2.zero,
                Vector2.one);
        }
    }

    private void RefreshPowerButton()
    {
        if (battleManager == null)
            return;

        bool fullPower =
            battleManager.PowerCharge >= battleManager.PowerChargeMax;
        if (powerChargeButtonText != null)
        {
            powerChargeButtonText.text = fullPower
                ? LocalizationManager.Text(
                    "FULL POWER",
                    "\uC804\uB825 \uCD5C\uB300") +
                  "\n" +
                  LocalizationManager.Text(
                    "COOLDOWN DOWN",
                    "\uC7AC\uC0AC\uC6A9 \uAC10\uC18C")
                : LocalizationManager.Text(
                    "CHARGE POWER",
                    "\uC804\uB825 \uCDA9\uC804");
        }
        RefreshPowerChargeNumbers(fullPower);

        if (powerChargeButton != null)
        {
            powerChargeButton.interactable =
                battleManager.IsRunning &&
                !battleManager.IsRecovering;
        }

        if (powerChargeButtonImage != null)
            powerChargeButtonImage.color = fullPower ? gold : success;
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
            bool hasPower =
                battleManager.PowerCharge >=
                BattleManager.CompanionSkillPowerCost;
            builder.Append(
                $"{character.characterName}: " +
                GetSkillStateLabel(cooldown, hasPower));
        }

        bool hasSkillStatus = builder.Length > 0;
        skillStatusText.gameObject.SetActive(hasSkillStatus);
        skillStatusText.text = hasSkillStatus
            ? builder.ToString()
            : LocalizationManager.Translate(
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
            bool powerBlocked =
                character != null &&
                cooldown <= 0f &&
                !hasPower;
            bool hasCharacter = character != null;

            if (skillButtonTexts[slot] != null)
            {
                skillButtonTexts[slot].text = character == null
                    ? LocalizationManager.Text("EMPTY", "\uBE48 \uC2AC\uB86F")
                    : ready
                        ? $"{character.characterName}\n" +
                          $"{LocalizationManager.Text("READY", "\uC900\uBE44")}"
                        : cooldown > 0f
                            ? $"{character.characterName}\nCD"
                            : $"{character.characterName}\n" +
                              $"{LocalizationManager.Text("CHARGE", "\uCDA9\uC804")}";
                skillButtonTexts[slot].color =
                    ready ? Color.white :
                    powerBlocked ? new Color32(185, 205, 230, 255) :
                    character == null ? new Color32(130, 142, 162, 255) :
                    Color.white;
            }
            RefreshSkillButtonNumber(slot, character, ready, cooldown);

            if (skillButtons[slot] != null)
            {
                skillButtons[slot].interactable =
                    hasCharacter &&
                    battleManager.IsRunning &&
                    !battleManager.IsRecovering;
            }

            if (skillButtonImages[slot] != null)
                skillButtonImages[slot].color =
                    ready
                        ? Color.Lerp(accent, gold, GetReadyPulse())
                        : powerBlocked
                            ? success
                            : panelLight;

            RefreshSkillPortrait(slot, character);
            RefreshCooldownOverlay(slot, character, cooldown);
            RefreshReadyGlow(slot, ready);
        }
    }

    private void RefreshSkillPortrait(int slot, CharacterData character)
    {
        if (skillPortraitImages[slot] == null)
            return;

        Sprite portrait = character == null
            ? null
            : character.ResolvePortraitSprite();
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

    private void RefreshReadyGlow(int slot, bool ready)
    {
        Image glow = skillReadyGlowImages[slot];
        RectTransform rect = skillReadyGlowRects[slot];
        if (glow == null || rect == null)
            return;

        glow.gameObject.SetActive(ready);
        if (!ready)
            return;

        float pulse = GetReadyPulse();
        glow.color = new Color(
            gold.r,
            gold.g,
            gold.b,
            Mathf.Lerp(0.45f, 0.95f, pulse));
        rect.localScale =
            Vector3.one * Mathf.Lerp(0.96f, 1.12f, pulse);
        glow.transform.SetAsLastSibling();
        skillButtonTexts[slot]?.transform.SetAsLastSibling();
    }

    private float GetReadyPulse()
    {
        return (Mathf.Sin(readyPulseTime * 6f) + 1f) * 0.5f;
    }

    private void ConfigurePowerChargeButtonText()
    {
        if (powerChargeButtonText == null)
            return;

        RectTransform rect =
            powerChargeButtonText.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.08f, 0.48f);
        rect.anchorMax = new Vector2(0.92f, 0.94f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        powerChargeButtonText.fontSizeMax = 22f;
        powerChargeButtonText.fontSizeMin = 12f;
        powerChargeButtonText.alignment = TextAlignmentOptions.Center;
    }

    private void BuildPowerChargeNumbers()
    {
        if (powerChargeButton == null)
            return;

        powerChargeCurrentNumberText = new SpriteNumberText(
            powerChargeButton.transform,
            "PowerChargeCurrentNumberText",
            NumberResourceRoot,
            18f,
            new Vector2(0.08f, 0.12f),
            new Vector2(0.31f, 0.42f));
        powerChargeSlashText = RuntimeUiFactory.CreateText(
            "PowerChargeSlashText",
            powerChargeButton.transform,
            "/",
            18,
            new Vector2(0.31f, 0.12f),
            new Vector2(0.38f, 0.42f),
            TextAlignmentOptions.Center,
            Color.white);
        powerChargeMaxNumberText = new SpriteNumberText(
            powerChargeButton.transform,
            "PowerChargeMaxNumberText",
            NumberResourceRoot,
            18f,
            new Vector2(0.38f, 0.12f),
            new Vector2(0.61f, 0.42f));
        powerChargeTapNumberText = new SpriteNumberText(
            powerChargeButton.transform,
            "PowerChargeTapNumberText",
            NumberResourceRoot,
            18f,
            new Vector2(0.64f, 0.12f),
            new Vector2(0.94f, 0.42f));
    }

    private void BindPowerChargeNumbers()
    {
        if (powerChargeButton == null)
            return;

        powerChargeCurrentNumberText = new SpriteNumberText(
            RuntimeUiBinder.FindRect(
                powerChargeButton.transform,
                "PowerChargeCurrentNumberText"),
            NumberResourceRoot,
            18f);
        powerChargeMaxNumberText = new SpriteNumberText(
            RuntimeUiBinder.FindRect(
                powerChargeButton.transform,
                "PowerChargeMaxNumberText"),
            NumberResourceRoot,
            18f);
        powerChargeTapNumberText = new SpriteNumberText(
            RuntimeUiBinder.FindRect(
                powerChargeButton.transform,
                "PowerChargeTapNumberText"),
            NumberResourceRoot,
            18f);
    }

    private void RefreshPowerChargeNumbers(bool fullPower)
    {
        powerChargeCurrentNumberText?.SetActive(!fullPower);
        powerChargeMaxNumberText?.SetActive(!fullPower);
        if (powerChargeSlashText != null)
            powerChargeSlashText.gameObject.SetActive(!fullPower);

        if (!fullPower)
        {
            powerChargeCurrentNumberText?.SetText(
                CompactNumberFormatter.Format(
                    Mathf.RoundToInt(battleManager.PowerCharge)));
            powerChargeMaxNumberText?.SetText(
                CompactNumberFormatter.Format(
                    Mathf.RoundToInt(battleManager.PowerChargeMax)));
            powerChargeTapNumberText?.SetText(
                CompactNumberFormatter.Format(
                    Mathf.RoundToInt(
                        BattleManager.PowerChargePerTapAmount),
                    "+"));
            return;
        }

        powerChargeTapNumberText?.SetText(
            "-" +
            BattleManager.FullChargeCooldownBoost.ToString(
                "0.##",
                CultureInfo.InvariantCulture));
    }

    private void RefreshSkillButtonNumber(
        int slot,
        CharacterData character,
        bool ready,
        float cooldown)
    {
        SpriteNumberText numberText = skillStateNumberTexts[slot];
        if (numberText == null)
            return;

        bool showNumber = character != null && !ready;
        numberText.SetActive(showNumber);
        if (!showNumber)
            return;

        numberText.SetAsLastSibling();
        numberText.SetText(cooldown > 0f
            ? cooldown.ToString("0.0", CultureInfo.InvariantCulture)
            : CompactNumberFormatter.Format(
                Mathf.RoundToInt(
                    BattleManager.CompanionSkillPowerCost)));
    }

    private static string GetSkillStateLabel(float cooldown, bool hasPower)
    {
        if (cooldown > 0f)
            return LocalizationManager.Text(
                "COOLDOWN",
                "\uC7AC\uC0AC\uC6A9");

        if (!hasPower)
        {
            return LocalizationManager.Text(
                "POWER NEEDED",
                "\uC804\uB825 \uD544\uC694");
        }

        return LocalizationManager.Text("READY", "\uC900\uBE44");
    }

    private void UseCompanionSkill(int slot)
    {
        if (battleManager == null)
        {
            showToast?.Invoke(
                LocalizationManager.Translate(
                    "Battle is not ready."));
            return;
        }

        CompanionSkillUseResult result =
            battleManager.TryUseCompanionSkill(
                slot,
                out float remainingCooldown);
        if (result == CompanionSkillUseResult.Success)
            return;

        if (result == CompanionSkillUseResult.Cooldown)
        {
            showToast?.Invoke(
                $"{LocalizationManager.Translate("Skill cooldown remains.")} " +
                remainingCooldown.ToString("0.0", CultureInfo.InvariantCulture) +
                "s");
            return;
        }

        showToast?.Invoke(
            LocalizationManager.Translate(
                GetSkillUseFailureMessage(result)));
    }

    private static string GetSkillUseFailureMessage(
        CompanionSkillUseResult result)
    {
        switch (result)
        {
            case CompanionSkillUseResult.NotEnoughPower:
                return "Not enough power charge.";
            case CompanionSkillUseResult.NoCompanion:
                return "No companion in this slot.";
            case CompanionSkillUseResult.BattleNotRunning:
                return "Battle is not running.";
            case CompanionSkillUseResult.Recovering:
                return "Power charger is not ready.";
            case CompanionSkillUseResult.NoEnemy:
                return "No enemy target.";
            case CompanionSkillUseResult.InvalidSlot:
                return "Invalid companion slot.";
            default:
                return "Skill is not ready.";
        }
    }

    private void ChargePower()
    {
        if (battleManager == null || !battleManager.ChargePower())
        {
            showToast?.Invoke(
                LocalizationManager.Translate(
                    "Power charger is not ready."));
            return;
        }

        Refresh();
    }
}
