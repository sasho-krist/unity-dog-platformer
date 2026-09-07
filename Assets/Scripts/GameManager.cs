using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Transform player;
    public Vector3 spawnPoint = new Vector3(0, 0, 0);
    public int startingLives = 3;
    public float fallDeathY = -6f;

    public AudioClip bonePickupClip;
    public AudioClip bonusPickupClip;
    public AudioClip lifePickupClip;
    public AudioClip hurtClip;
    public AudioClip finishClip;
    public AudioClip musicClip;

    private int bones = 0;
    private int lives;
    private bool levelComplete = false;
    private bool gameOver = false;
    private Vector3 checkpoint;

    private AudioSource sfxSource;
    private AudioSource musicSource;

    void Awake()
    {
        Instance = this;
        lives = startingLives;
        checkpoint = spawnPoint;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = 0.35f;
        musicSource.playOnAwake = false;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Start()
    {
        if (musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.Play();
        }
    }

    void Update()
    {
        if (gameOver && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (!gameOver && !levelComplete && player != null && player.position.y < fallDeathY)
        {
            LoseLife();
        }
    }

    public void UpdateCheckpoint(Vector3 pos)
    {
        checkpoint = pos;
    }

    public void AddScore(int amount)
    {
        if (gameOver || levelComplete) return;
        bones += amount;
        PlaySfx(amount >= 100 ? bonusPickupClip : bonePickupClip);
    }

    public void AddLife(int amount)
    {
        if (gameOver || levelComplete) return;
        lives += amount;
        PlaySfx(lifePickupClip);
    }

    public void LoseLife()
    {
        if (gameOver || levelComplete) return;
        lives -= 1;
        PlaySfx(hurtClip);
        if (lives <= 0)
        {
            lives = 0;
            gameOver = true;
        }
        else
        {
            RespawnPlayer();
        }
    }

    public void CompleteLevel()
    {
        if (levelComplete || gameOver) return;
        levelComplete = true;
        PlaySfx(finishClip);
    }

    public void RespawnPlayer()
    {
        if (player == null) return;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        player.position = checkpoint;
    }

    void PlaySfx(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public bool IsGameOver() { return gameOver; }
    public bool IsLevelComplete() { return levelComplete; }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 28;
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(20, 20, 300, 40), "Bones: " + bones, style);
        GUI.Label(new Rect(20, 55, 300, 40), "Lives: " + lives, style);

        if (gameOver)
        {
            GUIStyle big = new GUIStyle();
            big.fontSize = 48;
            big.normal.textColor = Color.red;
            big.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height / 2 - 60, 600, 80), "GAME OVER", big);

            GUIStyle small = new GUIStyle();
            small.fontSize = 24;
            small.normal.textColor = Color.white;
            small.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height / 2 + 20, 600, 40), "Press R to restart", small);
        }
        else if (levelComplete)
        {
            GUIStyle big = new GUIStyle();
            big.fontSize = 48;
            big.normal.textColor = Color.yellow;
            big.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height / 2 - 60, 600, 80), "LEVEL COMPLETE!", big);

            GUIStyle small = new GUIStyle();
            small.fontSize = 24;
            small.normal.textColor = Color.white;
            small.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height / 2 + 20, 600, 40), "Bones collected: " + bones, small);
        }
    }
}
