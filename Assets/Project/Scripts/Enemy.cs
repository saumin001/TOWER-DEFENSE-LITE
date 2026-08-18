using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;

    private bool reachedBase = false;

    private Animator animator;
    private EnemySpawner spawner;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetSpawner(EnemySpawner enemySpawner)
    {
        spawner = enemySpawner;
    }

    public void SetPath(Transform[] path)
    {
        waypoints = path;
        currentWaypointIndex = 0;
        reachedBase = false;

        // Reset vị trí/animation nếu cần
        if (animator != null)
        {
            animator.ResetTrigger("Die");
            animator.Play("Run");
        }
    }

    private void Update()
    {
        if (reachedBase)
            return;

        if (waypoints == null || waypoints.Length == 0)
            return;

        MoveToWaypoint();
    }

    private void MoveToWaypoint()
    {
        Transform target = waypoints[currentWaypointIndex];

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Length)
            {
                ReachBase();
            }
        }
    }

    private void ReachBase()
    {
        if (reachedBase)
            return;

        reachedBase = true;

        Debug.Log("Enemy reached the Base!");

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        StartCoroutine(ReturnToPoolAfterDeath());
    }

    private IEnumerator ReturnToPoolAfterDeath()
    {
        yield return new WaitForSeconds(0.6f);

        ReturnToPool();
    }

    public void ReturnToPool()
    {
        StopAllCoroutines();

        reachedBase = true;

        if (spawner != null)
        {
            spawner.ReturnEnemyToPool(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}