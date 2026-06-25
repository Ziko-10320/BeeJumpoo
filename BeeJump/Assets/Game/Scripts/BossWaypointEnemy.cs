using UnityEngine;
using System.Collections;

public class BossWaypointEnemy : MonoBehaviour
{
    [Header("Preview")]
    public float previewDuration = 1.5f;   // how long path glows before enemy appears
    public GameObject pathLinePrefab;       // a LineRenderer prefab
    public GameObject leftWarningIndicator;   // assign in prefab — an arrow or ! sprite on left
    public GameObject rightWarningIndicator;  // assign in prefab — an arrow or ! sprite on right
    private bool comingFromLeft;
    [Header("Knockback")]
    public float knockbackForce = 8f;
    public float knockbackUpForce = 5f;

    private float speed;
    private Vector3[] path;
    private LineRenderer lineRenderer;
    private SpriteRenderer spriteRenderer;
    private Bounds roomBounds;
    public void Init(float spd, bool fromLeft, Bounds bounds)
    {
        speed = spd;
        roomBounds = bounds;
        comingFromLeft = fromLeft;
        spriteRenderer = GetComponent<SpriteRenderer>();
        path = GeneratePath(fromLeft, bounds);

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        StartCoroutine(PreviewThenRun());
    }

    Vector3[] GeneratePath(bool fromLeft, Bounds bounds)
    {
        // Start off screen, end off screen other side
        // Middle points are random positions inside the room
        int midPoints = Random.Range(1, 4);
        Vector3[] points = new Vector3[midPoints + 2];

        points[0] = transform.position;  // spawn point (off screen)

        for (int i = 1; i <= midPoints; i++)
        {
            points[i] = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                0f);
        }

        // Exit off the opposite side
        points[points.Length - 1] = new Vector3(
            fromLeft ? bounds.max.x + 2f : bounds.min.x - 2f,
            Random.Range(bounds.min.y, bounds.max.y),
            0f);

        return points;
    }

    IEnumerator PreviewThenRun()
    {
        // Setup line AFTER Init so comingFromLeft is already set
        SetupLineRenderer();
        lineRenderer.enabled = true;

        // Flash side indicator
        if (comingFromLeft && leftWarningIndicator != null)
            StartCoroutine(FlashIndicator(leftWarningIndicator));
        else if (!comingFromLeft && rightWarningIndicator != null)
            StartCoroutine(FlashIndicator(rightWarningIndicator));

        yield return new WaitForSeconds(previewDuration);

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        StartCoroutine(FollowPath());
    }
    IEnumerator FlashIndicator(GameObject indicator)
    {
        indicator.SetActive(true);
        float t = 0f;
        SpriteRenderer sr = indicator.GetComponent<SpriteRenderer>();

        while (t < previewDuration)
        {
            t += Time.deltaTime;
            if (sr != null)
            {
                float alpha = Mathf.PingPong(t * 4f, 1f);
                sr.color = new Color(1f, 0.3f, 0.3f, alpha);
            }
            yield return null;
        }

        indicator.SetActive(false);
    }
    void SetupLineRenderer()
    {
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.positionCount = path.Length;
            lineRenderer.SetPositions(path);
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            Color pathColor = comingFromLeft ? Color.cyan : new Color(0.6f, 0f, 1f);
            lineRenderer.startColor = pathColor;
            lineRenderer.endColor = pathColor;
            lineRenderer.sortingOrder = 10;
        }
    }

    IEnumerator FollowPath()
    {
        for (int i = 1; i < path.Length; i++)
        {
            Vector3 target = path[i];

            while (Vector3.Distance(transform.position, target) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, target, speed * Time.deltaTime);
                yield return null;
            }

            transform.position = target;
        }

        // Path done — destroy, line renderer goes with it
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