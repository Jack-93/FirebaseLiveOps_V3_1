[System.Serializable]
public class CharacterData
{
    public string characterName;
    public string rarity;

    public CharacterData(string name, string rarityType)
    {
        characterName = name;
        rarity = rarityType;
    }
}
