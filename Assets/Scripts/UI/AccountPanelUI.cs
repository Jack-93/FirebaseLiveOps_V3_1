using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class AccountPanelUI
{
    private readonly RectTransform panel;
    private readonly TMP_Text accountDetailText;
    private readonly Button googleLinkButton;

    public GameObject GameObject => panel.gameObject;

    private static readonly Color OverlayBackground =
        new Color32(12, 18, 30, 218);
    private static readonly Color Panel =
        new Color32(37, 49, 73, 245);
    private static readonly Color PanelLight =
        new Color32(52, 68, 96, 255);
    private static readonly Color Accent =
        new Color32(82, 188, 255, 255);
    private static readonly Color Gold =
        new Color32(255, 201, 77, 255);
    private static readonly Color Danger =
        new Color32(238, 83, 106, 255);
    private static readonly Color MutedText =
        new Color32(190, 203, 225, 255);

    public AccountPanelUI(
        RectTransform root,
        Action showMore,
        Action linkGoogle,
        Action startNewGuest)
    {
        panel = RuntimeUiFactory.CreatePanel(
            "AccountPanel",
            root,
            OverlayBackground,
            new Vector2(0f, 0.12f),
            new Vector2(1f, 0.9f));

        RuntimeUiFactory.CreateButton(
            "AccountBackButton",
            panel,
            "BACK",
            new Vector2(0.05f, 0.9f),
            new Vector2(0.25f, 0.98f),
            PanelLight,
            () => showMore?.Invoke());

        RuntimeUiFactory.CreateText(
            "AccountTitle",
            panel,
            "ACCOUNT LINK",
            48,
            new Vector2(0.27f, 0.9f),
            new Vector2(0.82f, 0.98f),
            TextAlignmentOptions.Center,
            Accent);

        RuntimeUiFactory.CreateText(
            "AccountSubtitle",
            panel,
            "Link Google to protect guest progress on Android.",
            24,
            new Vector2(0.15f, 0.85f),
            new Vector2(0.85f, 0.9f),
            TextAlignmentOptions.Center,
            MutedText);

        RectTransform card = RuntimeUiFactory.CreatePanel(
            "AccountCard",
            panel,
            Panel,
            new Vector2(0.06f, 0.22f),
            new Vector2(0.94f, 0.84f));

        RuntimeUiFactory.CreateText(
            "AccountCardTitle",
            card,
            "CURRENT ACCOUNT",
            28,
            new Vector2(0.08f, 0.86f),
            new Vector2(0.92f, 0.96f),
            TextAlignmentOptions.Left,
            Gold);

        accountDetailText = RuntimeUiFactory.CreateText(
            "AccountDetailText",
            card,
            "Account status",
            25,
            new Vector2(0.22f, 0.5f),
            new Vector2(0.92f, 0.83f),
            TextAlignmentOptions.TopLeft,
            Color.white);

        Image accountIcon = RuntimeUiFactory.CreateSpriteImage(
            "AccountIcon",
            card,
            PrototypeUiArt.GetButtonIcon("MenuButton"),
            new Vector2(0.08f, 0.62f),
            new Vector2(0.2f, 0.8f));
        accountIcon.color = Accent;

        googleLinkButton = RuntimeUiFactory.CreateButton(
            "GoogleLinkButton",
            card,
            "LINK GOOGLE",
            new Vector2(0.08f, 0.34f),
            new Vector2(0.92f, 0.47f),
            Accent,
            () => linkGoogle?.Invoke());

        RuntimeUiFactory.CreateText(
            "AccountNotice",
            card,
            "Android build uses Google login only.\n" +
            "Linking keeps the current UID and all guest progress.",
            25,
            new Vector2(0.08f, 0.17f),
            new Vector2(0.92f, 0.31f),
            TextAlignmentOptions.Center,
            new Color32(174, 189, 214, 255));

        RuntimeUiFactory.CreateButton(
            "NewGuestButton",
            card,
            "START NEW GUEST",
            new Vector2(0.2f, 0.02f),
            new Vector2(0.8f, 0.12f),
            Danger,
            () => startNewGuest?.Invoke());
    }

    public void Refresh(AccountLinkManager accounts)
    {
        if (accounts == null)
        {
            accountDetailText.text = LocalizationManager.Text(
                "Account service is unavailable.",
                "계정 서비스를 사용할 수 없습니다.");
            googleLinkButton.interactable = false;
            return;
        }

        accountDetailText.text =
            accounts.GetAccountSummary() + "\n\n" +
            GoogleCredentialTokenProvider.GetSetupStatus() + "\n" +
            FirebaseManager.GetDiagnosticsStatus() + "\n" +
            (PushNotificationManager.Instance != null
                ? PushNotificationManager.Instance.GetTokenStatus()
                : "FCM: Pending");

        googleLinkButton.interactable =
            !accounts.IsBusy &&
            !accounts.IsLinked(AccountLinkProvider.Google);
    }
}
