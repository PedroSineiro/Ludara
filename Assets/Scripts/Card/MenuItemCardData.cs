using UnityEngine;

[CreateAssetMenu(fileName = "MenuItemCardData", menuName = "Scriptable Objects/MenuItemCardData")]
public class MenuItemCardData : ScriptableObject
{
    public int itemId;

    public string itemName;

    public int quantity;

    public bool discovered;

    public string description;

    public Sprite cardImage;

    public Sprite thumbnailImage;

    public AudioClip useAudio;

    public AudioClip attackAudio;
}
