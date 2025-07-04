using System.Collections.Generic;
using UnityEngine;

public class GiveNewCharacter : MonoBehaviour
{
    public string type;
    public List<CardData> medievalCaracters;
    public List<CardData> militarCaracters;
    public List<CardData> futuristicCaracters;
    public List<Hability> habilities;
    public void GiveCharacter(){

        var characters = type=="Medieval"?medievalCaracters:(type=="Militar"?militarCaracters:futuristicCaracters);

        List<CardData> nonObtainedCards = new();
        
        foreach(var character in characters){
            if(!character.obtained){
                nonObtainedCards.Add(character);
            }
        }

        if(nonObtainedCards.Count>0){
            int index = Random.Range(0, nonObtainedCards.Count);
            nonObtainedCards[index].obtained = true;

            foreach(var hability in habilities){
                if(nonObtainedCards[index].specialProperties==hability.habilityName)
                    hability.discovered = true;
            }
        }
    }
}
