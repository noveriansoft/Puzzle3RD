using System.Collections.Generic;
using UnityEngine.Events;
using System;
using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Serializable]
    public class EnemySpawnEntry
    {
        public GameObject prefab;

        [Min(0f)]
        public float spawnRate = 1f;
    }

    [Header("Enemy Prefabs")]
    [SerializeField] private List<EnemySpawnEntry> enemyPrefabs = new();

    [Header("References")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform player;

    [Header("Spawn Limits")]
    [Min(1)]
    [SerializeField] private int maxEnemies = 5;

    [Min(0)]
    [SerializeField] private int replenishBelow = 2;

    [Min(0.1f)]
    [SerializeField] private float spawnInterval = 3f;

    [Header("Monitoring")]
    [Min(0.1f)]
    [SerializeField] private float checkInterval = 0.5f;

    private bool isRefilling;
    private float checkTimer;

    [Header("Kill Count Event")]
    [SerializeField, Min(1)] private int killCountToTrigger = 15;
    [SerializeField] private UnityEvent onKillCountReached;
    [SerializeField] private int totalSpawnedCount;
    [SerializeField]private int killedEnemyCount;
    private bool killEventTriggered;

    private readonly HashSet<EnemyAI> spawnedEnemies =
        new HashSet<EnemyAI>();

    private void Update()
    {
        if (PlayerGameOver.IsGameOver || isRefilling)
            return;

        checkTimer -= Time.deltaTime;

        if (checkTimer > 0f)
            return;

        checkTimer = checkInterval;

        int aliveCount = CountAliveEnemies();

        if (aliveCount < replenishBelow &&
            aliveCount < maxEnemies)
        {
            StartCoroutine(RefillEnemies());
        }
    }

    private int CountAliveEnemies()
    {
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(
            FindObjectsSortMode.None
        );

        int count = 0;

        foreach (EnemyAI enemy in enemies)
        {
            if (enemy != null &&
                enemy.gameObject.activeInHierarchy &&
                !enemy.IsDead)
            {
                count++;
            }
        }

        return count;
    }

    private IEnumerator RefillEnemies()
    {
        isRefilling = true;

        while (!PlayerGameOver.IsGameOver &&
               CountAliveEnemies() < maxEnemies)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (PlayerGameOver.IsGameOver)
                break;

            if (CountAliveEnemies() >= maxEnemies)
                break;

            SpawnEnemy();
        }

        isRefilling = false;
    }

    private GameObject SelectEnemyPrefab()
    {
        float totalWeight = 0f;

        foreach (EnemySpawnEntry entry in enemyPrefabs)
        {
            if (entry == null || entry.prefab == null)
                continue;

            totalWeight += Mathf.Max(0f, entry.spawnRate);
        }

        if (totalWeight <= 0f)
        {
            Debug.LogWarning(
                "[EnemySpawner] No valid enemy spawn rates!"
            );
            return null;
        }

        float randomValue = UnityEngine.Random.Range(
            0f,
            totalWeight
        );

        foreach (EnemySpawnEntry entry in enemyPrefabs)
        {
            if (entry == null || entry.prefab == null)
                continue;

            float weight = Mathf.Max(0f, entry.spawnRate);

            randomValue -= weight;

            if (randomValue < 0f)
                return entry.prefab;
        }

        return null;
    }

    private void SpawnEnemy()
    {
        if (totalSpawnedCount >= killCountToTrigger)
            return;

        GameObject prefab = SelectEnemyPrefab();

        if (prefab == null)
            return;

        Transform spawnPoint = transform;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            List<Transform> validPoints = new List<Transform>();

            foreach (Transform point in spawnPoints)
            {
                if (point != null)
                    validPoints.Add(point);
            }

            if (validPoints.Count > 0)
            {
                spawnPoint = validPoints[
                    UnityEngine.Random.Range(0, validPoints.Count)
                ];
            }
        }

        GameObject spawnedEnemy = Instantiate(
            prefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        totalSpawnedCount++;
        EnemyAI enemyAI = spawnedEnemy.GetComponent<EnemyAI>();

        if (enemyAI != null && player != null)
        {
            spawnedEnemies.Add(enemyAI);
            enemyAI.SetPlayer(player);
        }
        else if (enemyAI == null)
        {
            Debug.LogWarning(
                "[EnemySpawner] Spawned prefab has no EnemyAI!"
            );
        }
    }

    private void OnEnable()
    {
        EnemyAI.EnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        EnemyAI.EnemyDied -= HandleEnemyDied;
    }

    private void HandleEnemyDied(EnemyAI enemy)
    {
        // Only count enemies spawned by this spawner.
        if (enemy == null || !spawnedEnemies.Contains(enemy))
            return;

        killedEnemyCount++;

        Debug.Log(
            $"Enemies killed: {killedEnemyCount}/{killCountToTrigger}"
        );

        if (!killEventTriggered &&
            killedEnemyCount >= killCountToTrigger)
        {
            killEventTriggered = true;
            onKillCountReached?.Invoke();
        }
    }
}
