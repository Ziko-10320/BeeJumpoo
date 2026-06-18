using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private Rigidbody2D rb;
    [Header("Knockback")]
    public float knockbackForce = 8f;
    public float knockbackUpForce = 5f;

    public void Init(Vector2 dir, float spd, float lifetime)
    {
        direction = dir.normalized;
        speed = spd;
        Destroy(gameObject, lifetime);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void Update()
    {
        rb.MovePosition(rb.position + direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();

            float knockDir = -Mathf.Sign(direction.x);
            if (knockDir == 0f) knockDir = -1f;

            playerRb.linearVelocity = new Vector2(
                knockDir * knockbackForce,
                knockbackUpForce
            );

            player.OnHitHazard();
        }

        Destroy(gameObject);
    }
}