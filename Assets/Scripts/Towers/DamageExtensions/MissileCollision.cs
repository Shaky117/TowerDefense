using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileCollision : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem explosionSystem;

    [SerializeField]
    private ParticleSystem missileSystem;

    [SerializeField]
    private float explosionRadius;

    [SerializeField]
    private MissileDamage baseClass;

    private List<ParticleCollisionEvent> missileCollisions;

    // Start is called before the first frame update
    void Start()
    {
        missileCollisions = new List<ParticleCollisionEvent>();
    }

    private void OnParticleCollision(GameObject other)
    {
        missileSystem.GetCollisionEvents(other, missileCollisions);

        for(int collisionEvent = 0; collisionEvent < missileCollisions.Count; collisionEvent++)
        {
            explosionSystem.transform.position = missileCollisions[collisionEvent].intersection;
            explosionSystem.Play();

            Collider[] enemiesInRadius = Physics.OverlapSphere(missileCollisions[collisionEvent].intersection, explosionRadius, baseClass.enemiesLayer);

            for(int i = 0; i < enemiesInRadius.Length; i++)
            {
                Enemy enemyToDamage = EntitySummoner.enemyTransformPairs[enemiesInRadius[i].transform.parent];
                EnemyDamageData damageToApply = new EnemyDamageData(enemyToDamage, baseClass.damage, enemyToDamage.damageResistance);
                GameManager.EnqueueEnemyDamageData(damageToApply);
            }
        }
    }
}
