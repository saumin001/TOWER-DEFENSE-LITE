using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Path")]
    [SerializeField] private WaypointPath waypointPath;

    [Header("Spawn")]
    [SerializeField] private float spawnDelay = 2f;

    [Header("Pooling")]
    [SerializeField] private int poolSize = 10;

    private List<GameObject> enemyPool = new List<GameObject>();

    private void Start()
    {
        CreatePool();

        InvokeRepeating(
            nameof(SpawnEnemy),
            1f,
            spawnDelay
        );
    }

    private void CreatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(
                enemyPrefab,
                transform
            );

            enemy.SetActive(false);

            Enemy enemyScript = enemy.GetComponent<Enemy>();

            if (enemyScript != null)
            {
                enemyScript.SetSpawner(this);
            }

            enemyPool.Add(enemy);
        }
    }

    private void SpawnEnemy()
    {
        GameObject enemyObject = GetEnemyFromPool();

        if (enemyObject == null)
        {
            Debug.Log("Enemy Pool is full!");
            return;
        }

        enemyObject.transform.position = spawnPoint.position;
        enemyObject.transform.rotation = Quaternion.identity;

        enemyObject.SetActive(true);

        Enemy enemy = enemyObject.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.SetPath(waypointPath.waypoints);
        }
    }

    private GameObject GetEnemyFromPool()
    {
        foreach (GameObject enemy in enemyPool)
        {
            if (!enemy.activeInHierarchy)
            {
                return enemy;
            }
        }

        return null;
    }

    public void ReturnEnemyToPool(GameObject enemy)
    {
        enemy.SetActive(false);
    }
}