using UnityEditor;
using UnityEngine;

/// <summary>
/// Tạo sẵn bộ asset thông số (quái, tháp, đợt) với số liệu đã cân bằng thô,
/// khỏi phải ngồi tạo tay từng cái rồi gõ số.
///
/// Menu: Tower Defense ▸ Tạo asset thông số mặc định
///
/// Chạy nhiều lần cũng không sao: asset nào đã tồn tại thì bỏ qua, không ghi đè,
/// nên số liệu bạn tự chỉnh sẽ không bị mất.
/// </summary>
public static class TowerDefenseAssetSetup
{
    private const string Folder = "Assets/Project/ScriptableObjects";
    private const string SlimePrefabPath = "Assets/Project/Prefabs/Enemy.prefab";
    private const string OrcPrefabPath = "Assets/Project/Prefabs/EnemyOrc.prefab";
    private const string BossPrefabPath = "Assets/Project/Prefabs/EnemyBoss.prefab";

    [MenuItem("Tower Defense/Tạo asset thông số mặc định")]
    public static void CreateDefaultAssets()
    {
        EnsureFolder();

        EnemyStats slime = CreateOrGet<EnemyStats>("EnemyStats_Slime", asset =>
        {
            asset.displayName = "Slime";
            asset.isBoss = false;
            asset.maxHealth = 40;
            asset.moveSpeed = 1.8f;
            asset.coinReward = 8;
            asset.damageToBase = 1;
        });

        EnemyStats orc = CreateOrGet<EnemyStats>("EnemyStats_Orc", asset =>
        {
            asset.displayName = "Orc";
            asset.isBoss = false;
            asset.maxHealth = 80;
            asset.moveSpeed = 1.4f;
            asset.coinReward = 14;
            asset.damageToBase = 2;
        });

        CreateOrGet<EnemyStats>("EnemyStats_Boss", asset =>
        {
            asset.displayName = "Boss";
            asset.isBoss = true;
            asset.maxHealth = 600;
            asset.moveSpeed = 1.0f;
            asset.coinReward = 100;
            asset.damageToBase = 10;
        });

        CreateOrGet<TowerStats>("TowerStats_Archer", asset =>
        {
            asset.displayName = "Cung thủ";
            asset.description = "Tầm xa, bắn một mục tiêu. Rẻ, dựng sớm.";
            asset.attackType = TowerAttackType.Ranged;
            asset.damage = 12;
            asset.range = 3.2f;
            asset.fireRate = 1.4f;
            asset.cost = 50;
        });

        CreateOrGet<TowerStats>("TowerStats_Melee", asset =>
        {
            asset.displayName = "Lính cận chiến";
            asset.description = "Tầm rất ngắn nhưng sát thương cao, đánh trúng ngay.";
            asset.attackType = TowerAttackType.Melee;
            asset.damage = 25;
            asset.range = 1.4f;
            asset.fireRate = 2.0f;
            asset.cost = 75;
        });

        CreateOrGet<TowerStats>("TowerStats_Cannon", asset =>
        {
            asset.displayName = "Pháo";
            asset.description = "Bắn chậm, đạn nổ lan sát thương cả cụm quái.";
            asset.attackType = TowerAttackType.Splash;
            asset.damage = 18;
            asset.range = 2.8f;
            asset.fireRate = 0.7f;
            asset.splashRadius = 1.3f;
            asset.cost = 100;
        });

        CreateWaveData(slime, orc);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[Tower Defense] Đã tạo asset thông số trong {Folder}. " +
                  "Bước tiếp: gán EnemyStats vào từng prefab quái, gán sprite vào TowerStats.");
    }

    private static void CreateWaveData(EnemyStats slime, EnemyStats orc)
    {
        string path = $"{Folder}/WaveData_Level1.asset";

        if (AssetDatabase.LoadAssetAtPath<WaveData>(path) != null)
            return;

        GameObject slimePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SlimePrefabPath);
        GameObject orcPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(OrcPrefabPath);
        GameObject bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BossPrefabPath);

        if (bossPrefab == null)
        {
            Debug.LogWarning("[Tower Defense] Chưa có EnemyBoss.prefab — đợt 5 sẽ để trống ô boss, " +
                             "tạo prefab boss xong nhớ kéo vào WaveData.");
        }

        var waveData = ScriptableObject.CreateInstance<WaveData>();

        waveData.waves = new[]
        {
            MakeWave("Đợt 1 — dò đường", 3f,
                Entry(slimePrefab, 6, 0.9f)),

            MakeWave("Đợt 2 — đông hơn", 8f,
                Entry(slimePrefab, 10, 0.7f)),

            MakeWave("Đợt 3 — có Orc", 10f,
                Entry(slimePrefab, 6, 0.6f),
                Entry(orcPrefab, 5, 1.0f)),

            MakeWave("Đợt 4 — Orc tràn", 12f,
                Entry(orcPrefab, 12, 0.7f)),

            MakeWave("Đợt 5 — BOSS", 14f,
                Entry(orcPrefab, 8, 0.6f),
                Entry(bossPrefab, 1, 1f))
        };

        AssetDatabase.CreateAsset(waveData, path);
    }

    private static Wave MakeWave(string name, float delay, params WaveEntry[] entries)
    {
        return new Wave
        {
            waveName = name,
            delayBeforeWave = delay,
            entries = entries
        };
    }

    private static WaveEntry Entry(GameObject prefab, int count, float interval)
    {
        return new WaveEntry
        {
            enemyPrefab = prefab,
            count = count,
            spawnInterval = interval
        };
    }

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Project"))
        {
            AssetDatabase.CreateFolder("Assets", "Project");
        }

        if (!AssetDatabase.IsValidFolder(Folder))
        {
            AssetDatabase.CreateFolder("Assets/Project", "ScriptableObjects");
        }
    }

    private static T CreateOrGet<T>(string assetName, System.Action<T> configure) where T : ScriptableObject
    {
        string path = $"{Folder}/{assetName}.asset";

        T existing = AssetDatabase.LoadAssetAtPath<T>(path);

        if (existing != null)
        {
            // Đã có rồi thì giữ nguyên số liệu người dùng đã chỉnh.
            return existing;
        }

        T asset = ScriptableObject.CreateInstance<T>();
        configure(asset);
        AssetDatabase.CreateAsset(asset, path);

        return asset;
    }
}
