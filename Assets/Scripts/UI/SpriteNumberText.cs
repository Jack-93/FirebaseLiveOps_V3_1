using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class SpriteNumberText
{
    private const float CharacterSpacing = -2f;

    private readonly RectTransform root;
    private readonly string resourceRoot;
    private readonly float characterHeight;
    private readonly List<Image> images = new List<Image>();
    private readonly Dictionary<char, Sprite> cache =
        new Dictionary<char, Sprite>();

    public SpriteNumberText(
        Transform parent,
        string name,
        string resourceRoot,
        float characterHeight,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        this.resourceRoot = resourceRoot.TrimEnd('/') + "/";
        this.characterHeight = characterHeight;
        root = RuntimeUiFactory.CreatePanel(
            name,
            parent,
            new Color32(0, 0, 0, 0),
            anchorMin,
            anchorMax);
        root.GetComponent<Image>().raycastTarget = false;
    }

    public SpriteNumberText(
        RectTransform existingRoot,
        string resourceRoot,
        float characterHeight)
    {
        this.resourceRoot = resourceRoot.TrimEnd('/') + "/";
        this.characterHeight = characterHeight;
        root = existingRoot;
        if (root == null)
            return;

        foreach (Image image in root.GetComponentsInChildren<Image>(true))
        {
            if (image.transform == root)
                continue;

            images.Add(image);
        }
    }

    public void SetText(string value)
    {
        if (root == null)
            return;

        value = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
        List<Sprite> sprites = new List<Sprite>();
        float totalWidth = 0f;

        foreach (char character in value)
        {
            Sprite sprite = GetSprite(character);
            if (sprite == null)
                continue;

            sprites.Add(sprite);
            totalWidth += GetWidth(sprite);
        }

        if (sprites.Count > 1)
            totalWidth += CharacterSpacing * (sprites.Count - 1);

        EnsureImageCount(sprites.Count);
        float cursor = -totalWidth * 0.5f;
        for (int index = 0; index < images.Count; index++)
        {
            bool active = index < sprites.Count;
            images[index].gameObject.SetActive(active);
            if (!active)
                continue;

            Sprite sprite = sprites[index];
            float width = GetWidth(sprite);
            RectTransform rect =
                images[index].GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, characterHeight);
            rect.anchoredPosition =
                new Vector2(cursor + width * 0.5f, 0f);
            images[index].sprite = sprite;
            images[index].color = Color.white;
            cursor += width + CharacterSpacing;
        }
    }

    public void SetAlpha(float alpha)
    {
        if (root == null)
            return;

        alpha = Mathf.Clamp01(alpha);
        foreach (Image image in images)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }

    public void SetActive(bool active)
    {
        if (root == null)
            return;

        root.gameObject.SetActive(active);
    }

    public void SetAsLastSibling()
    {
        if (root == null)
            return;

        root.SetAsLastSibling();
    }

    private void EnsureImageCount(int count)
    {
        while (images.Count < count)
        {
            GameObject imageObject = new GameObject(
                "Digit",
                typeof(RectTransform),
                typeof(Image));
            imageObject.transform.SetParent(root, false);

            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            Image image = imageObject.GetComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;
            images.Add(image);
        }
    }

    private Sprite GetSprite(char character)
    {
        if (cache.TryGetValue(character, out Sprite sprite))
            return sprite;

        string name = character >= 'A' && character <= 'Z'
            ? character.ToString()
            : character switch
        {
            '0' => "Num0",
            '1' => "Num1",
            '2' => "Num2",
            '3' => "Num3",
            '4' => "Num4",
            '5' => "Num5",
            '6' => "Num6",
            '7' => "Num7",
            '8' => "Num8",
            '9' => "Num9",
            '-' => "Minus",
            '+' => "Plus",
            ',' => "Comma",
            '.' => "Dot",
            _ => null
        };

        sprite = string.IsNullOrEmpty(name)
            ? null
            : Resources.Load<Sprite>(resourceRoot + name);
        cache[character] = sprite;
        return sprite;
    }

    private float GetWidth(Sprite sprite)
    {
        if (sprite == null || sprite.rect.height <= 0f)
            return characterHeight;

        return characterHeight * sprite.rect.width / sprite.rect.height;
    }
}
