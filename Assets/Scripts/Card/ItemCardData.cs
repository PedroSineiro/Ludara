using UnityEngine;

[CreateAssetMenu(fileName = "ItemCardData", menuName = "Scriptable Objects/ItemCardData")]
public class ItemCardData : ScriptableObject
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

    public Sprite cardImage;
    public Sprite thumbnailImage;
    
}
