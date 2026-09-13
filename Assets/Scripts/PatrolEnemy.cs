using UnityEngine;

public class PatrolEnemy : MonoBehaviour
{
    public float speed = 2f;
    public float range = 1.5f;
    public float stompBounce = 8f; // upward velocity given to the player after a successful stomp

    private Vector3 startPos;
    private int direction = 1;
    private bool triggered = false;
    private Collider2D col;

    void Start()
    {
        startPos = transform.position;
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (GameManager.Instance != null && (GameManager.Instance.IsGameOver() || GameManager.Instance.IsLevelComplete()))
            return;

        transform.position += new Vector3(direction * speed * Time.deltaTime, 0f, 0f);

        if (transform.position.x > startPos.x + range) direction = -1;
        else if (transform.position.x < startPos.x - range) direction = 1;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        if (IsStomp(other))
        {
            // Player jumped on top of the enemy: the enemy dies, player gets a small bounce.
            Rigidbody2D playerRb = other.attachedRigidbody;
            if (playerRb != null)
            {
                Vector2 v = playerRb.linearVelocity;
                playerRb.linearVelocity = new Vector2(v.x, stompBounce);
            }
            Destroy(gameObject);
            return;
        }

        // Touched from the side (or from below): the enemy hurts the player instead.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife();
        }
        Invoke(nameof(ResetTrigger), 1.5f);
    }

    // A "stomp" is when the player's feet are at or above the enemy's vertical
    // center while the player isn't moving upward (i.e. falling/landing on it).
    bool IsStomp(Collider2D playerCol)
    {
        if (col == null) return false;

        Rigidbody2D playerRb = playerCol.attachedRigidbody;
        bool falling = playerRb == null || playerRb.linearVelocity.y <= 0.01f;
        bool fromAbove = playerCol.bounds.min.y >= col.bounds.center.y;

        return falling && fromAbove;
    }

    void ResetTrigger()
    {
        triggered = false;
    }
}
