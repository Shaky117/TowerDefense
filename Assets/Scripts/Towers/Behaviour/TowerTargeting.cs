using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class TowerTargeting
{
    public enum TargetType
    {
        First,
        Last,
        Close
    }

    public static Enemy GetTarget(TowerBehaviour currentTower, TargetType targetMethod)
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(currentTower.transform.position, currentTower.range, currentTower.enemiesLayer);
        NativeArray<EnemyData> enemiesToCalculate = new NativeArray<EnemyData>(enemiesInRange.Length, Allocator.TempJob);
        NativeArray<Vector3> nodePositions = new NativeArray<Vector3>(GameManager.nodePositions, Allocator.TempJob);
        NativeArray<float> nodeDistances = new NativeArray<float>(GameManager.nodeDistances, Allocator.TempJob);
        NativeArray<int> enemyToIndex = new NativeArray<int>(new int[] { -1 }, Allocator.TempJob);
        int enemyIndexToReturn = -1;

        for (int i = 0; i < enemiesToCalculate.Length; i++)
        {
            Enemy currentEnemty = enemiesInRange[i].transform.parent.GetComponent<Enemy>();
            int enemyIndexInList = EntitySummoner.enemiesInGame.FindIndex(x => x == currentEnemty);

            enemiesToCalculate[i] = new EnemyData(currentEnemty.transform.position, currentEnemty.nodeIndex, currentEnemty.health, enemyIndexInList);
        }

        SearchForEnemy EnemySearchJob = new SearchForEnemy
        {
            _enemiesToCalculate = enemiesToCalculate,
            _nodeDistances = nodeDistances,
            _nodePositions = nodePositions,
            _enemyToIndex = enemyToIndex,
            targetingType = (int)targetMethod,
            towerPosition = currentTower.transform.position
        };

        switch ((int)targetMethod)
        {
            case 0:
                EnemySearchJob.compareValue = Mathf.Infinity;
                break;
            case 1:
                EnemySearchJob.compareValue = Mathf.NegativeInfinity;
                break;
            case 2:
                goto case 0;
        }

        JobHandle dependency = new JobHandle();
        JobHandle searchJobHandle = EnemySearchJob.Schedule(enemiesToCalculate.Length, dependency);

        searchJobHandle.Complete();

        if (enemiesToCalculate.Length > 0)
        {
            enemyIndexToReturn = enemiesToCalculate[0].EnemyIndex;

            enemiesToCalculate.Dispose();
            nodePositions.Dispose();
            nodeDistances.Dispose();
            enemyToIndex.Dispose();

            return EntitySummoner.enemiesInGame[enemyIndexToReturn];
        }
        else
        {
            enemiesToCalculate.Dispose();
            nodePositions.Dispose();
            nodeDistances.Dispose();
            enemyToIndex.Dispose();

            return null;
        }

       
    }

    struct EnemyData
    {
        public EnemyData(Vector3 position, int nodeindex, float hp, int enemyIndex)
        {
            enemyPosition = position;
            nodeIndex = nodeindex;
            health = hp;
            EnemyIndex = enemyIndex;
        }

        public Vector3 enemyPosition;
        public int EnemyIndex;
        public int nodeIndex;
        public float health;
    }

    struct SearchForEnemy : IJobFor
    {
        public NativeArray<EnemyData> _enemiesToCalculate;
        public NativeArray<Vector3> _nodePositions;
        public NativeArray<float> _nodeDistances;
        public NativeArray<int> _enemyToIndex;
        public Vector3 towerPosition;
        public float compareValue;
        public int targetingType;
        public void Execute(int index)
        {
            float currentEnemyDistanceToEnd = 0;
            switch (targetingType)
            {
                case 0:

                    currentEnemyDistanceToEnd = GetDistanceToEnd(_enemiesToCalculate[index]);
                    if(currentEnemyDistanceToEnd < compareValue) 
                    {
                        _enemyToIndex[0] = index;
                        compareValue = currentEnemyDistanceToEnd;

                    }

                    break;
                case 1:
                    currentEnemyDistanceToEnd = GetDistanceToEnd(_enemiesToCalculate[index]);
                    if (currentEnemyDistanceToEnd > compareValue)
                    {
                        _enemyToIndex[0] = index;
                        compareValue = currentEnemyDistanceToEnd;

                    }

                    break;
                case 2:
                    currentEnemyDistanceToEnd = Vector3.Distance(towerPosition, _enemiesToCalculate[index].enemyPosition);
                    if (currentEnemyDistanceToEnd > compareValue)
                    {
                        _enemyToIndex[0] = index;
                        compareValue = currentEnemyDistanceToEnd;

                    }
                    break;
            }
        }

        private float GetDistanceToEnd(EnemyData enemyToEvaluate)
        {
            float finalDistance = Vector3.Distance(enemyToEvaluate.enemyPosition, _nodePositions[enemyToEvaluate.nodeIndex]);

            for(int i = enemyToEvaluate.nodeIndex; i < _nodeDistances.Length; i++)
            {
                finalDistance += _nodeDistances[i];
            }

            return finalDistance;
        }
    }
}
