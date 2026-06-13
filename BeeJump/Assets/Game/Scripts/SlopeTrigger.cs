using UnityEngine;

public class SlopeTrigger : MonoBehaviour
{
    [Tooltip("Direction the player slides. For a 35 degree slope going down-right: (1, -0.7). Down-left: (-1, -0.7)")]
    public Vector2 slideDirection = new Vector2(1f, -0.7f);

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position,
            transform.position + (Vector3)slideDirection.normalized * 1.5f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController p = other.GetComponent<PlayerController>();
        if (p != null) p.EnterSlope(slideDirection);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        PlayerController p = other.GetComponent<PlayerController>();
        if (p != null) p.ExitSlope();
    }
}