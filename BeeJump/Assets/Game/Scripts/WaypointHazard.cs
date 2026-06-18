using UnityEngine;

public class WaypointHazard : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints;

    [Header("Movement")]
    public float speed = 3f;
    public float waitTime = 0.5f;        // how long to idle at each point

    [Header("Loop Type")]
    public bool loop = true;             // true = 1 2 3 4 1 2 3 4
                                         // false = 1 2 3 4 3 2 1 (ping pong)
    [Header("Path Type")]
    public PathType pathType = PathType.Waypoints;
    public enum PathType { Waypoints, Circle }

    [Header("Circle Settings")]
    public float circleRadius = 2f;
    public float circleSpeed = 90f;          // degrees per second
    public bool clockwise = true;

    private Vector3 circleCenter;
    private float currentAngle = 0f;
    [Header("Knockback")]
    public float knockbackForce = 8f;
    public float knockbackUpForce = 5f;

    // ?? private state ??????????????????????????????????????????????????????????
    private int currentIndex = 0;
    private int direction = 1;           // 1 = forward, -1 = backward (ping pong)
    private float waitTimer = 0f;
    private bool isWaiting = false;

    void Start()
    {
        if (pathType == PathType.Circle)
        {
            circleCenter = transform.position;
            return;
        }

        if (waypoints.Length == 0) return;
        transform.position = waypoints[0].position;
    }

    void Update()
    {
        if (pathType == PathType.Circle)
        {
            UpdateCircle();
            return;
        }

        if (waypoints.Length < 2) return;

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
                isWaiting = false;
            return;
        }

        Vector3 target = waypoints[currentIndex].position;
        transform.position = Vector3.MoveTowards(
            transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            transform.position = target;
            isWaiting = true;
            waitTimer = waitTime;
            AdvanceIndex();
        }
    }
    void UpdateCircle()
    {
        float dir = clockwise ? -1f : 1f;
        currentAngle += circleSpeed * dir * Time.deltaTime;

        float rad = currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * circleRadius;

        transform.position = circleCenter + offset;
    }

    void AdvanceIndex()
    {
        if (loop)
        {
            // 1 2 3 4 1 2 3 4 — wraps back to start
            currentIndex = (currentIndex + 1) % waypoints.Length;
        }
        else
        {
            // 1 2 3 4 3 2 1 — bounces back and forth
            currentIndex += direction;

            if (currentIndex >= waypoints.Length - 1)
            {
                currentIndex = waypoints.Length - 1;
                direction = -1;
            }
            else if (currentIndex <= 0)
            {
                currentIndex = 0;
                direction = 1;
            }
        }
    }

    // ?? knockback on contact ???????????????????????????????????????????????????
    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

        float knockDir = -Mathf.Sign(rb.linearVelocity.x);
        if (knockDir == 0f) knockDir = -1f;

        rb.linearVelocity = new Vector2(
            knockDir * knockbackForce,
            knockbackUpForce
        );

        player.OnHitHazard();
    }

    // ?? draw path in editor so u can see it ???????????????????????????????????
    void OnDrawGizmos()
    {
        if (pathType == PathType.Circle)
        {
            Vector3 center = Application.isPlaying ? circleCenter : transform.position;
            Gizmos.color = Color.cyan;

            Vector3 prevPoint = center + new Vector3(circleRadius, 0f, 0f);
            int segments = 32;

            for (int i = 1; i <= segments; i++)
            {
                float angle = (360f / segments) * i * Mathf.Deg2Rad;
                Vector3 point = center + new Vector3(
                    Mathf.Cos(angle) * circleRadius,
                    Mathf.Sin(angle) * circleRadius, 0f);
                Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }
            return;
        }

        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] == null || waypoints[i + 1] == null) continue;
            Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            Gizmos.DrawWireSphere(waypoints[i].position, 0.15f);
        }
        Gizmos.DrawWireSphere(waypoints[waypoints.Length - 1].position, 0.15f);

        if (loop && waypoints[0] != null && waypoints[waypoints.Length - 1] != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
        }
    }
}