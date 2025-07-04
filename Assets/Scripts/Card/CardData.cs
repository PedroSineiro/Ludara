using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData")]
public class CardData : ScriptableObject
{

    public int cardId;
    public string cardName;

    public bool obtained;
    public bool died;
    public int initialHealth;
    public int health;
    public int currenthealth;
    public int initialDamage;
    public int damage;
    public int movement;
    public int range;
    [TextArea]
    public string specialProperties;

    public AudioClip attackAudio;

    //habilidade especial Camuflagem
    public bool isInvisible;

    public bool canGoInvisible;

    //habilidade especial Telepatia Apoiadora

    public CardData supporterCharacter;
    public CardData supportedCharacter;
    public bool isSupported;

    //habilidade especial Mecha

    public bool isMechaCard;

    public CardData mechaCard;

    public CardData nonMechaCard;

    //habilidade especial Transformar

    public bool isTransformed;

    public bool canTransform;

    public CardData nonTransformedCard;

    public CardData transformedCard;

    public AudioClip transformAudio;

    //habilidade especial Recarregar

    public bool needRecharge;

    //habilidades especiais de geração de item

    public int numberOfTurns;

    public List<MenuItemCardData> generateableItems; 

    public Sprite cardImage;
    public Sprite thumbnailImage;
}
