using UnityEngine;

public class HitBlock : MonoBehaviour
{
    public int requiredHits = 10;
    public int rewardBones = 10;

    private int hitCount = 0;
    private bool rewarded = false;
    private Renderer rend;
    private Color freshColor;
    private Color usedColor = new Color(0.45f, 0.45f, 0.45f);

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null) freshColor = rend.material.color;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (rewarded) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        Rigidbody2D prb = collision.rigidbody;
        if (prb == null) return;
        // Use the collision's relative velocity at the moment of impact, not the
        // Rigidbody2D's live velocity: by the time this callback runs, Unity has
        // already resolved the contact and the live velocity is back near zero,
        // which meant a hit could never register no matter how fast the jump was.
        if (collision.relativeVelocity.y <= 0.05f) return;

        Collider2D myCol = GetComponent<Collider2D>();
        if (myCol == null) return;

        // Only count it as a "bump" if the player hit the block from underneath
        // (contact point is near the block's bottom edge) while moving upward.
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.point.y <= myCol.bounds.min.y + 0.3f)
            {
                RegisterHit();
                break;
            }
        }
    }

    void RegisterHit()
    {
        hitCount++;

        if (rend != null && !rewarded)
        {
            float t = Mathf.Clamp01((float)hitCount / requiredHits);
            rend.material.color = Color.Lerp(freshColor, usedColor, t);
        }

        if (hitCount >= requiredHits && !rewarded)
        {
            rewarded = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(rewardBones);
            }
            if (rend != null) rend.material.color = usedColor;
        }
    }
}
