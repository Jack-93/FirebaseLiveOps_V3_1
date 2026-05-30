using System.Collections.Generic;

[System.Serializable]
public class InventoryData
{
    public Dictionary<string, int> items =
        new Dictionary<string, int>();

    public InventoryData()
    {
        items.Add("Potion", 3);
        items.Add("Gem", 100);
    }
}