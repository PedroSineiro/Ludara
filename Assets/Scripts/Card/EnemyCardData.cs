using UnityEngine;

[CreateAssetMenu(fileName = "EnemyCardData", menuName = "Scriptable Objects/EnemyCardData")]
public class EnemyCardData : ScriptableObject
{
    public string cardName;
    public int health;
    public int currenthealth;
    public int damage;
    public int movement;
    public int range;
    [TextArea]
    public string specialProperties;

    public AudioClip attackAudio;

    public AudioClip deathAudio;
    //habilidade especial Camuflagem
    public bool isInvisible;

    public bool canGoInvisible;

    //habilidade especial Telepatia Apoiadora

    public EnemyCardData supporterCharacter;
    public EnemyCardData supportedCharacter;
    public bool isSupported;

    //habilidade especial Recarregar

    public bool needRecharge;

    public Sprite cardImage;
    public Sprite thumbnailImage;

}
