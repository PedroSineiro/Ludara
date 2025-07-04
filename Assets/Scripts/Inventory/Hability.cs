using UnityEngine;

[CreateAssetMenu(fileName = "Hability", menuName = "Scriptable Objects/Hability")]
public class Hability : ScriptableObject
{
    public int habilityId;
    public string habilityName;
    public bool discovered;
    public string description;
}
