using System;

[Serializable]
public sealed class StoryIntroCut
{
    public int cutIndex;
    public string title;
    public string body;
    public string artDirection;
    public string placeholderColorHex;
    public bool requiresArt;
    public string artResourcePath;

    public StoryIntroCut(
        int cutIndex,
        string title,
        string body,
        string artDirection,
        string placeholderColorHex,
        bool requiresArt = true,
        string artResourcePath = null)
    {
        this.cutIndex = cutIndex;
        this.title = title;
        this.body = body;
        this.artDirection = artDirection;
        this.placeholderColorHex = placeholderColorHex;
        this.requiresArt = requiresArt;
        this.artResourcePath = artResourcePath;
    }
}
