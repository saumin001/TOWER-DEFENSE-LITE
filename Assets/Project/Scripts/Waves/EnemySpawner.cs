using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Chạy các đợt quái và quản lý pool.
///
/// Mỗi loại quái có một pool riêng (cùng prefab thì chung pool, kể cả xuất hiện
/// ở nhiều đợt khác nhau). Không có chỗ nào gọi Destroy: quái chết chỉ tắt đi
/// và nằm chờ trong List&lt;GameObject&gt; của pool.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Đường đi")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private WaypointPath waypointPath;

    [Header("Các đợt")]
    [SerializeField] private WaveData waveData;

    [Header("Pooling")]
    [Tooltip("Số quái tạo sẵn cho MỖI loại lúc khởi động.")]
    [SerializeField] private int poolSizePerType = 10;

    [Tooltip("Bật: hết quái rảnh thì pool tự tạo thêm. Tắt: chờ có con chết mới spawn tiếp.")]
    [SerializeField] private bool poolCanGrow = true;

    /// <summary>Một pool cho mỗi prefab quái.</summary>
    private readonly List<GameObjectPool> pools = new List<GameObjectPool>();

    /// <summary>Số đợt đã bắt đầu (1-based). UI hiển thị "Đợt x/y".</summary>
    public int CurrentWave { get; private set; }

    public int TotalWaves => waveData != null ? waveData.WaveCount : 0;

    public event Action<int, int> OnWaveChanged;

    private void Start()
    {
        if (!ValidateSetup())
            return;

        BuildPools();

        // Đề bài: bắt đầu là chơi luôn.
        StartCoroutine(RunWaves());
    }

    private bool ValidateSetup()
    {
        if (spawnPoint == null)
        {
            Debug.LogError("[EnemySpawner] Chưa gán Spawn Point.", this);
            return false;
        }

        if (waypointPath == null || waypointPath.waypoints == null || waypointPath.waypoints.Length == 0)
        {
            Debug.LogError("[EnemySpawner] Chưa gán Waypoint Path hoặc path rỗng.", this);
            return false;
        }

        if (waveData == null || waveData.WaveCount == 0)
        {
            Debug.LogError("[EnemySpawner] Chưa gán Wave Data hoặc chưa có đợt nào.", this);
            return false;
        }

        return true;
    }

    // ───────────────────────────── Pool ─────────────────────────────

    /// <summary>Quét hết các đợt, gom prefab khác nhau, mỗi prefab dựng một pool.</summary>
    private void BuildPools()
    {
        foreach (Wave wave in waveData.waves)
        {
            if (wave?.entries == null)
                continue;

            foreach (WaveEntry entry in wave.entries)
            {
                if (entry?.enemyPrefab == null)
                    continue;

                if (FindPool(entry.enemyPrefab) != null)
                    continue;

                var pool = new GameObjectPool(entry.enemyPrefab, poolSizePerType, transform, poolCanGrow);
                pools.Add(pool);
            }
        }
    }

    private GameObjectPool FindPool(GameObject prefab)
    {
        for (int i = 0; i < pools.Count; i++)
        {
            if (pools[i].Prefab == prefab)
            {
                return pools[i];
            }
        }

        return null;
    }

    public void ReturnEnemyToPool(GameObject enemy)
    {
        // Không Destroy — chỉ tắt. Đây chính là điểm mấu chốt của pooling.
        if (enemy != null)
        {
            enemy.SetActive(false);
        }
    }

    // ───────────────────────────── Chạy đợt ─────────────────────────────

    private IEnumerator RunWaves()
    {
        for (int i = 0; i < waveData.waves.Length; i++)
        {
            Wave wave = waveData.waves[i];

            if (wave == null)
                continue;

            yield return new WaitForSeconds(wave.delayBeforeWave);

            CurrentWave = i + 1;
            OnWaveChanged?.Invoke(CurrentWave, TotalWaves);

            yield return StartCoroutine(SpawnWave(wave));
        }

        // Hết đợt cuối rồi vẫn phải đợi dọn sạch quái còn sống mới tính là thắng.
        while (Enemy.ActiveEnemies.Count > 0)
        {
            yield return null;
        }

        GameManager.Instance?.ReportAllWavesCleared();
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        if (wave.entries == null)
            yield break;

        foreach (WaveEntry entry in wave.entries)
        {
            if (entry?.enemyPrefab == null)
                continue;

            for (int i = 0; i < entry.count; i++)
            {
                yield return StartCoroutine(SpawnOne(entry.enemyPrefab));
                yield return new WaitForSeconds(entry.spawnInterval);
            }
        }
    }

    private IEnumerator SpawnOne(GameObject prefab)
    {
        GameObjectPool pool = FindPool(prefab);

        if (pool == null)
            yield break;

        GameObject enemyObject = pool.Get();

        // Pool không được phép nở thêm và đang hết sạch: đợi có con chết rồi spawn tiếp,
        // chứ không bỏ qua con quái này (bỏ qua là đợt thiếu quân so với cấu hình).
        while (enemyObject == null)
        {
            yield return null;
            enemyObject = pool.Get();
        }

        enemyObject.transform.position = spawnPoint.position;
        enemyObject.transform.rotation = Quaternion.identity;

        Enemy enemy = enemyObject.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.SetSpawner(this);

            // Reset trạng thái TRƯỚC khi bật, để Update chạy frame đầu đã có máu đúng.
            enemy.SetPath(waypointPath.waypoints);
        }

        enemyObject.SetActive(true);
    }
}
