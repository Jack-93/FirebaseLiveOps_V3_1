using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "CharacterDatabase",
    menuName = "Game/Character Database")]
public class CharacterDatabase : ScriptableObject
{
    public List<CharacterData> characters;
}