using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 20f;
    public float doubleJumpMultiplier = 2f; // 200%
    public float doubleTapWindow = 2.5f;

    public AudioClip jumpClip;
    public AudioClip doubleJumpClip;

    private Rigidbody2D rb;
    private Collider2D col;
    private AudioSource audioSource;
    private float jumpBufferTimer;
    private const float JumpBufferTime = 0.15f;

    private float lastJumpPressTime = -10f;
    private bool hasDoubleJumped = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (GameManager.Instance != null && (GameManager.Instance.IsGameOver() || GameManager.Instance.IsLevelComplete()))
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        float move = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        Bounds b = col.bounds;
        Vector2 boxCenter = new Vector2(b.center.x, b.min.y);
        Vector2 boxSize = new Vector2(b.size.x * 0.9f, 0.3f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f);
        bool isGrounded = false;
        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;
            if (hit == col) continue;
            if (hit.attachedRigidbody == rb) continue;
            if (hit.isTrigger) continue;
            isGrounded = true;
            break;
        }

        if (isGrounded)
        {
            hasDoubleJumped = false;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdateCheckpoint(transform.position);
            }
        }

        bool jumpPressed = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);

        if (jumpPressed)
        {
            float timeSinceLastPress = Time.time - lastJumpPressTime;
            bool isDoubleTap = timeSinceLastPress <= doubleTapWindow;

            if (isGrounded)
            {
                jumpBufferTimer = JumpBufferTime;
            }
            else if (isDoubleTap && !hasDoubleJumped)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * doubleJumpMultiplier);
                hasDoubleJumped = true;
                PlayClip(doubleJumpClip);
            }

            lastJumpPressTime = Time.time;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        if (jumpBufferTimer > 0f && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferTimer = 0f;
            PlayClip(jumpClip);
        }
    }

    void PlayClip(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (col == null) return;
        Bounds b = col.bounds;
        Gizmos.color = Color.red;
        Vector2 boxCenter = new Vector2(b.center.x, b.min.y);
        Vector2 boxSize = new Vector2(b.size.x * 0.9f, 0.3f);
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
}
