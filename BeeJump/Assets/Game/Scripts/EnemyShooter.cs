using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Projectile")]
    public GameObject projectilePrefab;

    [Header("Fire Settings")]
    public float fireRate = 2f;
    public float projectileSpeed = 6f;
    public float projectileLifetime = 5f;

    [Header("Direction")]
    public ShootDirection shootDirection = ShootDirection.Left;
    public enum ShootDirection { Up, Down, Left, Right }

    private float fireTimer;

    void Start()
    {
        // Random offset so multiple enemies dont fire in sync
        fireTimer = Random.Range(0f, fireRate);
    }

    void Update()
    {
        fireTimer += Time.deltaTime;

        if (fireTimer >= fireRate)
        {
            fireTimer = 0f;
            Shoot();
        }
    }

    void Shoot()
    {
        if (projectilePrefab == null) return;

        Vector2 dir = GetDirectionVector();
        Vector3 spawnPos = transform.position + (Vector3)(dir * 0.6f);

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Projectile p = proj.GetComponent<Projectile>();

        if (p != null)
            p.Init(dir, projectileSpeed, projectileLifetime);
    }

    Vector2 GetDirectionVector()
    {
        return shootDirection switch
        {
            ShootDirection.Up => Vector2.up,
            ShootDirection.Down => Vector2.down,
            ShootDirection.Left => Vector2.left,
            ShootDirection.Right => Vector2.right,
            _ => Vector2.left
        };
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector2 dir = GetDirectionVector();
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(dir * 1.5f));
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}