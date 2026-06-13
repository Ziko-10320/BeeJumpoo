using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 6f;
    public float lifetime = 5f;            // auto destroy if it hits nothing

    [Header("Knockback")]
    public float knockbackForce = 8f;
    public float knockbackUpForce = 5f;

    private Vector2 direction;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Hit player
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

            float knockDir = -Mathf.Sign(rb.linearVelocity.x);
            if (knockDir == 0f) knockDir = -1f;

            rb.linearVelocity = new Vector2(
                knockDir * knockbackForce,
                knockbackUpForce
            );

            player.OnHitHazard();
            Destroy(gameObject);
            return;
        }

        // Hit ground or wall — destroy on any non-player collider
        if (!other.isTrigger)
            Destroy(gameObject);
    }
}