using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public List<Effect> activeEffects;

    public Transform root;
    public float damageResistance = 1f;
    public int nodeIndex;
    public float maxHealth;
    public float health;
    public float speed;
    public int iD;
    

    public void Init() 
    {
        activeEffects = new List<Effect>();
        health = maxHealth;
        transform.position = GameManager.nodePositions[0];
        nodeIndex = 0; 
    }
    public void Tick() 
    {
        for(int i = 0; i < activeEffects.Count; i++)
        {
            if (activeEffects[i].expireTime > 0)
            {
                if(activeEffects[i].damageDelay > 0) 
                {
                    activeEffects[i].damageDelay -= Time.deltaTime;
                }
                else 
                {
                    GameManager.EnqueueEnemyDamageData(new EnemyDamageData(this, activeEffects[i].damage, 1f));
                    activeEffects[i].damageDelay = 1f / activeEffects[i].damageRate;
                }

                activeEffects[i].expireTime -= Time.deltaTime;
            }
        }

        activeEffects.RemoveAll(x => x.expireTime <= 0f);
    }
}
