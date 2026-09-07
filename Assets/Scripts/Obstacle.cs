using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (other.CompareTag("Player"))
        {
            triggered = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoseLife();
            }
            Invoke(nameof(ResetTrigger), 1.5f);
        }
    }

    void ResetTrigger()
    {
        triggered = false;
    }
}
