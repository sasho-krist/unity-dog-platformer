using UnityEngine;

public class BonusPickup : MonoBehaviour
{
    public int amount = 100;
    public float spinSpeed = 160f;

    void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(amount);
            }
            Destroy(gameObject);
        }
    }
}
