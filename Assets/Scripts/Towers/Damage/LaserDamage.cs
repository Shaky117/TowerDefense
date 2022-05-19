using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserDamage : MonoBehaviour, IDamageMethod
{
    [SerializeField]
    private Transform laserPivot;

    [SerializeField]
    private LineRenderer laserRenderer;

    private float damage;
    private float fireRate;
    private float delay;

    public void Init(float damage, float fireRate)
    {
        this.damage = damage;
        this.fireRate = fireRate;
        delay = 1f / fireRate;
    }

    public void DamageTick(Enemy target)
    {
        if (target)
        {
            laserRenderer.enabled = true;
            laserRenderer.SetPosition(0, laserPivot.position);
            laserRenderer.SetPosition(1, target.root.position);

            if (delay > 0)
            {
                delay -= Time.deltaTime;
                return;
            }

            GameManager.EnqueueEnemyDamageData(new EnemyDamageData(target, damage, target.damageResistance));

            delay = 1f / fireRate;

            return;
        }

        laserRenderer.enabled = false;
    }
}
