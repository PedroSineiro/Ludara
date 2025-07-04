using System.Collections.Generic;
using UnityEngine;

public class UnlockHability : MonoBehaviour
{
    public List<Hability> habilitiesToUnlock;
    void Start()
    {
        foreach(var hability in habilitiesToUnlock){
            hability.discovered = true;
        }
    }
}
