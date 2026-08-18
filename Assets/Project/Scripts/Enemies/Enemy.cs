using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Một con quái đi theo waypoint về Base.
///
/// Đối tượng này được POOL: nó không bao giờ bị Destroy. Chết hoặc về tới Base
/// thì chỉ tắt đi và trả lại pool, nên mọi trạng thái (máu, chỉ số waypoint, cờ)
/// đều phải reset trong <see cref="SetPath"/> — quên reset thì lần tái sử dụng
/// sau con quái sẽ sống lại với máu bằng 0.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Thông số")]
    [SerializeField] private EnemyStats stats;

    [Header("Thanh máu (không bắt buộc)")]
    [Tooltip("Kéo Transform phần ruột thanh máu vào đây. Script co giãn scale X theo % máu.")]
    [SerializeField] private Transform healthBarFill;

    /// <summary>Mọi con quái đang sống trên map. Tháp ngắm mục tiêu qua danh sách này.</summary>
    private static readonly List<Enemy> activeEnemies = new List<Enemy>();

    public static IReadOnlyList<Enemy> ActiveEnemies => activeEnemies;

    private Transform[] waypoints;
    private int currentWaypointIndex;
    private bool isDying;
    private int currentHealth;

    private Animator animator;
    private EnemySpawner spawner;
    private SpriteRenderer spriteRenderer;

    public EnemyStats Stats => stats;
    public bool IsAlive => !isDying && currentHealth > 0;

    /// <summary>
    /// Đi được bao xa trên đường — dùng để tháp ưu tiên bắn con gần Base nhất.
    /// Càng lớn nghĩa là càng nguy hiểm.
    /// </summary>
    public float PathProgress { get; private set; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (!activeEnemies.Contains(this))
        {
            activeEnemies.Add(this);
        }
    }

    private void OnDisable()
    {
        activeEnemies.Remove(this);
    }

    /// <summary>Dọn danh sách static khi vào ván mới (static không tự sạch khi nạp lại scene).</summary>
    public static void ClearRegistry()
    {
        activeEnemies.Clear();
    }

    public void SetSpawner(EnemySpawner enemySpawner)
    {
        spawner = enemySpawner;
    }

    /// <summary>
    /// Gán đường đi và đưa con quái về trạng thái mới tinh.
    /// Spawner gọi hàm này ngay trước khi bật đối tượng lên.
    /// </summary>
    public void SetPath(Transform[] path)
    {
        waypoints = path;
        currentWaypointIndex = 0;
        PathProgress = 0f;
        isDying = false;

        currentHealth = stats != null ? stats.maxHealth : 30;
        UpdateHealthBar();

        // Pool tái dùng đối tượng cũ nên sprite có thể còn mờ từ lần chết trước.
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
        }

        if (animator != null)
        {
            animator.ResetTrigger("Die");
            animator.Play("Run");
        }
    }

    private void Update()
    {
        if (isDying)
            return;

        if (waypoints == null || waypoints.Length == 0)
            return;

        MoveToWaypoint();
    }

    private void MoveToWaypoint()
    {
        Transform target = waypoints[currentWaypointIndex];

        if (target == null)
            return;

        float speed = stats != null ? stats.moveSpeed : 2f;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // Lật mặt quái theo hướng đi cho đỡ vô lý.
        if (spriteRenderer != null)
        {
            float dx = target.position.x - transform.position.x;
            if (Mathf.Abs(dx) > 0.01f)
            {
                spriteRenderer.flipX = dx < 0f;
            }
        }

        PathProgress = currentWaypointIndex + (1f - Mathf.Clamp01(distanceToTarget));

        if (distanceToTarget < 0.05f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Length)
            {
                ReachBase();
            }
        }
    }

    // ───────────────────────────── Nhận sát thương ─────────────────────────────

    public void TakeDamage(int amount)
    {
        if (isDying || amount <= 0)
            return;

        currentHealth -= amount;
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDying)
            return;

        isDying = true;

        if (stats != null)
        {
            GameManager.Instance?.AddCoins(stats.coinReward);
        }

        AudioManager.Instance?.PlayEnemyDeath();

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        StartCoroutine(ReturnToPoolAfterDeath());
    }

    private void ReachBase()
    {
        if (isDying)
            return;

        isDying = true;

        int damage = stats != null ? stats.damageToBase : 1;
        GameManager.Instance?.TakeDamage(damage);

        // Lọt được tới Base thì biến mất luôn, không diễn cảnh chết.
        ReturnToPool();
    }

    private IEnumerator ReturnToPoolAfterDeath()
    {
        yield return new WaitForSeconds(0.6f);

        ReturnToPool();
    }

    public void ReturnToPool()
    {
        StopAllCoroutines();

        isDying = true;

        if (spawner != null)
        {
            spawner.ReturnEnemyToPool(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill == null || stats == null)
            return;

        float ratio = Mathf.Clamp01((float)currentHealth / stats.maxHealth);

        Vector3 scale = healthBarFill.localScale;
        scale.x = ratio;
        healthBarFill.localScale = scale;
    }
}
