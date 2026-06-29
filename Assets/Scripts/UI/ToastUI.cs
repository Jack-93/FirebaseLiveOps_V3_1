using TMPro;
using UnityEngine;

public sealed class ToastUI
{
    private GameObject panel;
    private TMP_Text text;
    private float timer;

    public GameObject GameObject => panel;

    public ToastUI(RectTransform root, bool usePrefab = true)
    {
        if (usePrefab &&
            RuntimeUiBinder.TryInstantiatePrefab(
                "ToastPanel",
                root,
                out RectTransform toast))
        {
            Bind(toast);
            return;
        }

        BuildGenerated(root);
    }

    public void BuildGenerated(RectTransform root)
    {
        RectTransform toast = RuntimeUiFactory.CreatePanel(
            "ToastPanel",
            root,
            new Color32(10, 15, 26, 235),
            new Vector2(0.2f, 0.82f),
            new Vector2(0.8f, 0.88f));
        panel = toast.gameObject;

        text = RuntimeUiFactory.CreateText(
            "ToastText",
            toast,
            "",
            29,
            Vector2.zero,
            Vector2.one,
            TextAlignmentOptions.Center,
            Color.white);

        panel.SetActive(false);
    }

    public void Show(string message)
    {
        if (text != null)
            text.text = message;
        panel?.SetActive(true);
        timer = 2f;
    }

    public void Update(float deltaTime)
    {
        if (timer <= 0f)
            return;

        timer -= deltaTime;
        if (timer <= 0f)
            panel?.SetActive(false);
    }

    private void Bind(RectTransform toast)
    {
        panel = toast.gameObject;
        text = RuntimeUiBinder.FindText(toast, "ToastText");
        panel.SetActive(false);
    }
}
