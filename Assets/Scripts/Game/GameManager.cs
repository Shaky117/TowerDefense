using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Jobs;

public class GameManager : MonoBehaviour {

    public float timer;
    public float lives;

    public Text timerText;
    public Text livesText;
    public Text winMessage;
    public GameObject panel;

    public static List<TowerBehaviour> towersInGame;

    public static float[] nodeDistances;
    public static Vector3[] nodePositions;

    private static Queue<ApplyEffectData> effectQueue;
    private static Queue<EnemyDamageData> DamageData;
    private static Queue<Enemy> enemiesToRemove;
    private static Queue<int> enemyIdToSummon;

    private bool win = false;

    public Transform nodeParent;
    public bool gameShouldLoop;

    // Start is called before the first frame update
    void Start()
    {
        effectQueue = new Queue<ApplyEffectData>();
        DamageData = new Queue<EnemyDamageData>();
        towersInGame = new List<TowerBehaviour>();
        enemyIdToSummon = new Queue<int>();
        enemiesToRemove = new Queue<Enemy>();

        EntitySummoner.Init();

        nodePositions = new Vector3[nodeParent.childCount];
        for (int i = 0; i < nodePositions.Length; i++) 
        {
            nodePositions[i] = nodeParent.GetChild(i).position;
        }

        nodeDistances = new float[nodePositions.Length - 1];
        for (int i = 0; i < nodeDistances.Length; i++)
        {
            nodeDistances[i] = Vector3.Distance(nodePositions[i], nodePositions[i + 1]);
        }

        StartCoroutine(GameLoop());
        InvokeRepeating("SummonTest", 0f, 1f);
    }

    void SummonTest() 
    {
        int random = Random.Range(1, 4);
        EnqueueEnemyIdToSummon(random);
    }

    IEnumerator GameLoop() 
    {
        while (gameShouldLoop) 
        {
            int timerInt = (int)timer;
            timerText.text = timerInt.ToString();
            livesText.text = lives.ToString();

            timer -= Time.deltaTime;

            if(timer <= 0f) 
            {
                gameShouldLoop = false;
                win = true;
            }

            if(lives <= 0)
            {
                gameShouldLoop = false;
                win = false;
            }

            if (enemyIdToSummon.Count > 0) 
            {
                for (int i = 0; i < enemyIdToSummon.Count; i++) 
                {
                    EntitySummoner.SummonEnemy(enemyIdToSummon.Dequeue());
                } 
            }

            NativeArray<Vector3> nodesToUse = new NativeArray<Vector3>(nodePositions, Allocator.TempJob);
            NativeArray<float> enemySpeeds = new NativeArray<float>(EntitySummoner.enemiesInGame.Count, Allocator.TempJob);
            NativeArray<int> nodeIndices = new NativeArray<int>(EntitySummoner.enemiesInGame.Count, Allocator.TempJob);
            TransformAccessArray enemyAcces = new TransformAccessArray(EntitySummoner.enemiesInGameTransform.ToArray(), 2);

            for (int i = 0; i < EntitySummoner.enemiesInGame.Count; i++) 
            {
                enemySpeeds[i] = EntitySummoner.enemiesInGame[i].speed;
                nodeIndices[i] = EntitySummoner.enemiesInGame[i].nodeIndex;
            }

            MoveEnemiesJob moveJob = new MoveEnemiesJob
            {
                nodePositions = nodesToUse,
                enemySpeed = enemySpeeds,
                nodeIndex = nodeIndices,
                deltaTime = Time.deltaTime
            };

            JobHandle moveJobHandle = moveJob.Schedule(enemyAcces);
            moveJobHandle.Complete();

            for (int i = 0; i < EntitySummoner.enemiesInGame.Count; i++) 
            {
                EntitySummoner.enemiesInGame[i].nodeIndex = nodeIndices[i];

                if (EntitySummoner.enemiesInGame[i].nodeIndex == nodePositions.Length - 1)
                {
                    lives--;
                    EnqueueEnemyToRemove(EntitySummoner.enemiesInGame[i]);
                }
            }

            nodesToUse.Dispose();
            enemySpeeds.Dispose();
            nodeIndices.Dispose();
            enemyAcces.Dispose();

            foreach(TowerBehaviour tower in towersInGame)
            {
                tower.target = TowerTargeting.GetTarget(tower, TowerTargeting.TargetType.Close);
                tower.Tick();
            }

            if (effectQueue.Count > 0)
            {
                for (int i = 0; i < effectQueue.Count; i++)
                {
                    ApplyEffectData currentDamageData = effectQueue.Dequeue();
                    Effect effectToDuplicate = currentDamageData.enemyToAffect.activeEffects.Find(x => x.effectName == currentDamageData.effectToApply.effectName);
                    if (effectToDuplicate == null)
                    {
                        currentDamageData.enemyToAffect.activeEffects.Add(currentDamageData.effectToApply);
                    }
                    else 
                    {
                        effectToDuplicate.expireTime = currentDamageData.effectToApply.expireTime;
                    }

                }
            }

            foreach(Enemy currentEnemy in EntitySummoner.enemiesInGame)
            {
                currentEnemy.Tick();
            }

            if (DamageData.Count > 0)
            {
                for (int i = 0; i < DamageData.Count; i++)
                {
                    EnemyDamageData currentDamageData = DamageData.Dequeue();
                    currentDamageData.targetEnemy.health -= currentDamageData.totalDamage / currentDamageData.resistance;

                    if (currentDamageData.targetEnemy.health <= 0f)
                    {
                        if (!enemiesToRemove.Contains(currentDamageData.targetEnemy))
                        {
                            EnqueueEnemyToRemove(currentDamageData.targetEnemy);
                        }
                    }
                }
            }

            if (enemiesToRemove.Count > 0)
            {
                for (int i = 0; i < enemiesToRemove.Count; i++)
                {
                    EntitySummoner.RemoveEnemy(enemiesToRemove.Dequeue());
                }
            }

            yield return null;
        }


        panel.SetActive(true);

        Text winMessage = panel.transform.GetChild(0).transform.GetComponent<Text>();

        if (win) 
        {
            winMessage.text = "You win!!";
        }
        else
        {
            winMessage.text = "You Lose";
        }
    }

    public static void EnqueueEffectToApply(ApplyEffectData effectData)
    {
        effectQueue.Enqueue(effectData);
    }

    public static void EnqueueEnemyDamageData(EnemyDamageData damageData)
    {
        DamageData.Enqueue(damageData);
    }

    public static void EnqueueEnemyIdToSummon(int id) 
    {
        enemyIdToSummon.Enqueue(id);
    }

    public static void EnqueueEnemyToRemove(Enemy enemyToRemove)
    {
        enemiesToRemove.Enqueue(enemyToRemove);
    }
}

public class Effect
{
    public Effect(string effectName, float damageRate, float damage, float expireTime)
    {
        this.expireTime = expireTime;
        this.effectName = effectName;
        this.damageRate = damageRate;
        this.damage = damage;
    }

    public string effectName;

    public float damage;
    public float expireTime;
    public float damageRate;
    public float damageDelay;
}

public struct ApplyEffectData
{
    public ApplyEffectData(Enemy enemytoAffect, Effect effectToApply)
    {
        this.effectToApply = effectToApply;
        this.enemyToAffect = enemytoAffect;
    }

    public Enemy enemyToAffect;
    public Effect effectToApply;
}

public struct EnemyDamageData
{
    public EnemyDamageData(Enemy target, float damage, float resistance)
    {
        targetEnemy = target;
        totalDamage = damage;
        this.resistance = resistance;
    }

    public Enemy targetEnemy;
    public float totalDamage;
    public float resistance;
}

public struct MoveEnemiesJob : IJobParallelForTransform
{
    [NativeDisableParallelForRestriction]
    public NativeArray<int> nodeIndex;

    [NativeDisableParallelForRestriction]
    public NativeArray<float> enemySpeed;

    [NativeDisableParallelForRestriction]
    public NativeArray<Vector3> nodePositions;

    public float deltaTime;

    public void Execute(int index, TransformAccess transform)
    {
        if (nodeIndex[index] < nodePositions.Length)
        {
            Vector3 positionToMoveTo = nodePositions[nodeIndex[index]];

            transform.position = Vector3.MoveTowards(transform.position, positionToMoveTo, enemySpeed[index] * deltaTime);

            if (transform.position == positionToMoveTo)
            {
                nodeIndex[index]++;
            }
        }
    }
}