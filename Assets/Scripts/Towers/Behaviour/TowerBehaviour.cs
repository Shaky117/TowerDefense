using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBehaviour : MonoBehaviour
{
    public LayerMask enemiesLayer;
    public Transform towerPivot;
    public Enemy target;

    public float damage;
    public float fireRate;
    public float range;

    private float delay;

    private IDamageMethod currentDamageMethodClass;

    // Start is called before the first frame update
    void Start()
    {
        currentDamageMethodClass = GetComponent<IDamageMethod>();

        if (currentDamageMethodClass == null)
        {
            Debug.Log("Tower Behaviour : Current Damage Method Class not initialized somehow");
        }
        else 
        {
            currentDamageMethodClass.Init(damage, fireRate);
        }

        delay = 1 / fireRate;
    }

    public void Tick()
    {

        if(target != null)
        {
            currentDamageMethodClass.DamageTick(target);

            towerPivot.transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position);
        }

    }
}
