using UnityEngine;

public class LifePickup : MonoBehaviour
{
    public float bobSpeed = 2f;
    public float bobHeight = 0.15f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float y = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddLife(1);
            }
            Destroy(gameObject);
        }
    }
}
