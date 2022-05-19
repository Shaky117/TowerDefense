using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New EnemySummon", menuName = "Create EnemySummon")]

public class EnemySummon : ScriptableObject
{

    public GameObject enemyPrefab;
    public int enemyID;
}
