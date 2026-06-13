using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float fireRate = 2f;            // seconds between shots

    [Header("Direction")]
    public ShootDirection shootDirection = ShootDirection.Left;

    public enum ShootDirection { Up, Down, Left, Right }

    private float fireTimer = 0f;

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

        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
            p.SetDirection(GetDirectionVector());
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
        Gizmos.DrawLine(transform.position,
            transform.position + (Vector3)(dir * 1.5f));
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}