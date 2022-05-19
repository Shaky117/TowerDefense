using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTriggerManager : MonoBehaviour
{
    [SerializeField]
    private FlameThrowerDamagee baseClass;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Effect flameEffect = new Effect("Fire", baseClass.fireRate, baseClass.damage, 5f);
            ApplyEffectData effectData = new ApplyEffectData(EntitySummoner.enemyTransformPairs[other.transform.parent], flameEffect);
            GameManager.EnqueueEffectToApply(effectData);
        }
    }
}
