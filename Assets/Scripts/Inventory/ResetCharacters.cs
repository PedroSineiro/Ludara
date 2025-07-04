using System.Collections.Generic;
using UnityEngine;

public class ResetCharacters : MonoBehaviour
{
    public List<CardData> characters;
    void Start()
    {
        foreach(var character in characters){
            character.currenthealth = character.health;
            character.isSupported = false;
            character.supportedCharacter = null;
            character.supporterCharacter = null;
            character.canGoInvisible = character.specialProperties=="Camuflagem Assassina";
            character.isInvisible = false;
            character.canTransform = character.specialProperties=="Transformar";
            character.numberOfTurns = 0;
            character.needRecharge = false;
        }

    }
}
