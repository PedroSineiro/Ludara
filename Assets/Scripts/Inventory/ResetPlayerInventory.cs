using System;
using System.Collections.Generic;
using UnityEngine;

public class ResetPlayerInventory : MonoBehaviour
{
    
    public List<CardData> characters;
    public List<MenuItemCardData> items;
    public List<Hability> habilities;
    public List<string> initialCharacters;
    public List<string> initialHabilities;

    public EventMemory eventMemory;

    void Start()
    {
        foreach (var character in characters)
        {
            character.currenthealth = character.health;
            character.obtained = initialCharacters.Contains(character.cardName);
            character.health = character.initialHealth;
            character.damage = character.initialDamage;
            character.died = false;
            character.isSupported = false;
            character.supportedCharacter = null;
            character.supporterCharacter = null;
            character.canGoInvisible = character.specialProperties == "Camuflagem Assassina";
            character.isInvisible = false;
            character.canTransform = character.specialProperties == "Transformar";
            character.numberOfTurns = 0;
            character.needRecharge = false;
        }

        foreach (var item in items)
        {
            item.discovered = false;
            item.quantity = 0;
        }

        foreach (var hability in habilities)
        {
            hability.discovered = initialHabilities.Contains(hability.name);
        }

        eventMemory.newEnemy = false;
        eventMemory.newEnemySpawned = false;
    }
}
