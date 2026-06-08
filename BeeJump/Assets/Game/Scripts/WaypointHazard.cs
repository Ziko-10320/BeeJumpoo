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
        if (waypoints.Length == 0) return;
        transform.position = waypoints[0].position;
    }

    void Update()
    {
        if (waypoints.Length < 2) return;

        // ?? idle at waypoint ???????????????????????????????????????????????????
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
                isWaiting = false;
            return;
        }

        // ?? move toward current target waypoint ????????????????????????????????
        Vector3 target = waypoints[currentIndex].position;
        transform.position = Vector3.MoveTowards(
            transform.position, target, speed * Time.deltaTime);

        // ?? reached waypoint ???????????????????????????????????????????????????
        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            transform.position = target;
            isWaiting = true;
            waitTimer = waitTime;
            AdvanceIndex();
        }
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
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] == null || waypoints[i + 1] == null) continue;
            Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            Gizmos.DrawWireSphere(waypoints[i].position, 0.15f);
        }
        Gizmos.DrawWireSphere(waypoints[waypoints.Length - 1].position, 0.15f);

        // Draw loop closing line
        if (loop && waypoints[0] != null && waypoints[waypoints.Length - 1] != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
        }
    }
}