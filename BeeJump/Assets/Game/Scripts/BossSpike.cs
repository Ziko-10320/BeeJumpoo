using UnityEngine;
using System.Collections;

public class BossSpike : MonoBehaviour
{
    [Header("Timing")]
    public float warningDuration = 1f;    // how long warning shows before spike appears
    public float stayDuration = 2f;       // how long spike stays out
    public float retractDuration = 0.3f;  // how fast it retracts

    [Header("Knockback")]
    public float knockbackForce = 8f;
    public float knockbackUpForce = 5f;

    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        // Start hidden
        if (spriteRenderer != null)
            spriteRenderer.color = new Color(1f, 1f, 1f, 0f);
        if (col != null)
            col.enabled = false;

        StartCoroutine(SpikeSequence());
    }

    IEnumerator SpikeSequence()
    {
        // Warning — flash red transparent
        float t = 0f;
        while (t < warningDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.PingPong(t * 4f, 1f);
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(1f, 0.2f, 0.2f, alpha);
            yield return null;
        }

        // Pop out — enable collider and show fully
        if (col != null) col.enabled = true;
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;

        yield return new WaitForSeconds(stayDuration);

        // Retract
        if (col != null) col.enabled = false;
        t = 0f;
        while (t < retractDuration)
        {
            t += Time.deltaTime;
            float alpha = 1f - (t / retractDuration);
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        float knockDir = -Mathf.Sign(rb.linearVelocity.x);
        if (knockDir == 0f) knockDir = -1f;

        rb.linearVelocity = new Vector2(knockDir * knockbackForce, knockbackUpForce);
        player.OnHitHazard();
    }
}