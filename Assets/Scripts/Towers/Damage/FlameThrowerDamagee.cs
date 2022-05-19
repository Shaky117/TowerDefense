using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameThrowerDamagee : MonoBehaviour, IDamageMethod
{
    [SerializeField]
    private Collider fireTrigger;

    [SerializeField]
    private ParticleSystem fireEffect;

    [HideInInspector] public float damage;
    [HideInInspector] public float fireRate;

    public void Init(float damage, float fireRate)
    {
        this.damage = damage;
        this.fireRate = fireRate;
    }

    public void DamageTick(Enemy target)
    {
      
        fireTrigger.enabled = target != null;

        if (target)
        {
            if(!fireEffect.isPlaying) fireEffect.Play();
            return;
        }

        fireEffect.Stop();
        
    }
}
