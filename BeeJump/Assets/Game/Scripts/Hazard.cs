using UnityEngine;

public class Hazard : MonoBehaviour
{
    public float knockbackForce = 8f;
    public float knockbackUpForce = 5f;

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

        // Opposite of whatever horizontal direction player was moving
        float knockDir = -Mathf.Sign(rb.linearVelocity.x);
        if (knockDir == 0f) knockDir = -1f;   // fallback

        rb.linearVelocity = new Vector2(
            knockDir * knockbackForce,
            knockbackUpForce
        );

        player.OnHitHazard();
    }
}