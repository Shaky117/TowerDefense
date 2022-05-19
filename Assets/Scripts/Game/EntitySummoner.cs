using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySummoner : MonoBehaviour
{
    public static List<Enemy> enemiesInGame;
    public static List<Transform> enemiesInGameTransform;

    public static Dictionary<int, GameObject> enemyPrefabs;
    public static Dictionary<int, Queue<Enemy>> enemyObjectPool;
    public static Dictionary<Transform, Enemy> enemyTransformPairs;

    private static bool isInitialized;
    public static  void Init()
    {
        if (!isInitialized)
        {
            enemyTransformPairs = new Dictionary<Transform, Enemy>();
            enemyPrefabs = new Dictionary<int, GameObject>();
            enemyObjectPool = new Dictionary<int, Queue<Enemy>>();
            enemiesInGameTransform = new List<Transform>();
            enemiesInGame = new List<Enemy>();

            EnemySummon[] enemies = Resources.LoadAll<EnemySummon>("Enemies");

            foreach (EnemySummon enemy in enemies)
            {
                enemyPrefabs.Add(enemy.enemyID, enemy.enemyPrefab);
                enemyObjectPool.Add(enemy.enemyID, new Queue<Enemy>());
            }
        }
        else 
        {
            Debug.Log("EntitySummoner : Is already initialized");
        }

    }

    public static Enemy SummonEnemy(int enemyId) {

        Enemy summonedEnemy = null;

        if (enemyPrefabs.ContainsKey(enemyId))
        {
            Queue<Enemy> referencedQueue = enemyObjectPool[enemyId];

            if (referencedQueue.Count > 0)
            {
                summonedEnemy = referencedQueue.Dequeue();
                summonedEnemy.Init();

                summonedEnemy.gameObject.SetActive(true);
            }
            else 
            {
                GameObject newEnemy = Instantiate(enemyPrefabs[enemyId], GameManager.nodePositions[0], Quaternion.identity);
                summonedEnemy = newEnemy.GetComponent<Enemy>();
                summonedEnemy.Init();
            }
        }
        else 
        {
            Debug.Log("EntitySummoner : Enemy {enemyId} doesnt exist");

            return null;
        }

        if(!enemiesInGame.Contains(summonedEnemy)) enemiesInGame.Add(summonedEnemy);
        if(!enemiesInGameTransform.Contains(summonedEnemy.transform)) enemiesInGameTransform.Add(summonedEnemy.transform);
        if(!enemyTransformPairs.ContainsKey(summonedEnemy.transform)) enemyTransformPairs.Add(summonedEnemy.transform, summonedEnemy);

        summonedEnemy.iD = enemyId;
        return summonedEnemy;
    }


    public static void RemoveEnemy(Enemy enemyToRemove) 
    {
        enemyObjectPool[enemyToRemove.iD].Enqueue(enemyToRemove);
        enemyToRemove.gameObject.SetActive(false);

        enemyTransformPairs.Remove(enemyToRemove.transform);
        enemiesInGameTransform.Remove(enemyToRemove.transform);
        enemiesInGame.Remove(enemyToRemove);
    }
}
