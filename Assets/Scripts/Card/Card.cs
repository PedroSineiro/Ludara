using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public GameObject image;
    public CardData card;
    public bool canShowInfo;

    public GameObject cardInfo;

    public TextMeshProUGUI nameLabel;

    public TextMeshProUGUI healthLabel;

    public TextMeshProUGUI atkLabel;

    public TextMeshProUGUI rangeLabel;

    public TextMeshProUGUI movLabel;

    public TextMeshProUGUI habilityLabel;


    public void ShowCardInfo(){
        if(canShowInfo){
            nameLabel.text = "Nome: " + card.cardName;
            healthLabel.text = "Pontos de Vida: " + card.health;
            atkLabel.text = "Ataque: " + card.damage;
            rangeLabel.text = "Alcance de Ataque: " + card.range;
            movLabel.text = "Movimento: " + card.movement + " blocos";
            habilityLabel.text = "Habilidade: " + card.specialProperties;
            cardInfo.SetActive(true);
        }
    }
    public void HideCardInfo(){
        if(canShowInfo){
            cardInfo.SetActive(false);
        }
    }
}
