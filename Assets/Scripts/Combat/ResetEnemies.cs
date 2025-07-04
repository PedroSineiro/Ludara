using System.Collections.Generic;
using UnityEngine;

public class ResetEnemies : MonoBehaviour
{
    public List<EnemyCardData> enemies;
    void Start()
    {
        foreach(var enemy in enemies){
            enemy.currenthealth = enemy.health;
            enemy.isSupported = false;
            enemy.supportedCharacter = null;
            enemy.supporterCharacter = null;
            enemy.needRecharge = false;
            enemy.isInvisible = false;
            enemy.canGoInvisible = enemy.specialProperties=="Camuflagem"?true:false;
        }
    }

}
