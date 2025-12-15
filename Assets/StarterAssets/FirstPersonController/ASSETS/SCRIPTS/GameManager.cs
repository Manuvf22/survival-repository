using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public float spawnDelay = 2f;
    public int maxEnemiesAtOnce = 15;

    [Header("Enemy Difficulty")]
    public float gameTimeForDifficultyIncrease = 30f;
    public int baseEnemyHealth = 50;
    public int baseEnemyDamage = 10;
    public float difficultyMultiplier = 1.2f;

    [Header("Score System")]
    public int score = 0;
    public int enemiesKilled = 0;
    public int pointsPerHit = 10;
    public int pointsPerKill = 100;

    [Header("UI - In Game")]
    public TMP_Text scoreText;
    public TMP_Text killsText;
    public TMP_Text gameTimeText;
    
    [Header("UI - Pause Panel")]
    public TMP_Text pauseScoreText;
    public TMP_Text pauseKillsText;
    public TMP_Text pauseTimeText;
    
    [Header("UI - Game Over Panel")]
    public TMP_Text gameOverScoreText;
    public TMP_Text gameOverKillsText;
    public TMP_Text gameOverTimeText;
    
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;

    private float gameTime = 0f;
    private int currentDifficultyLevel = 1;
    private bool isGameOver = false;
    private bool isPaused = false;
    private bool gameStarted = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (!gameStarted || isGameOver) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }

        if (!isPaused)
        {
            gameTime += Time.deltaTime;
            currentDifficultyLevel = Mathf.FloorToInt(gameTime / gameTimeForDifficultyIncrease) + 1;
        }

        UpdateUI();
    }

    public void StartGame()
    {
        StopAllCoroutines();
        
        gameStarted = true;
        score = 0;
        enemiesKilled = 0;
        gameTime = 0f;
        isGameOver = false;
        isPaused = false;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.currentHealth = playerHealth.maxHealth;
            playerHealth.Heal(0);
        }

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine(SpawnEnemies());
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        
        UpdatePauseStats();
        
        if (pausePanel != null) pausePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RestartGame()
    {
        StopAllCoroutines();
        
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        StopAllCoroutines();
        
        Time.timeScale = 1f;
        
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        
        gameStarted = false;
        isGameOver = false;
        isPaused = false;
        
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void GameOver()
    {
        if (!gameStarted || isGameOver) return;
        
        isGameOver = true;
        gameStarted = false;
        
        StopAllCoroutines();
        
        UpdateGameOverStats();
        
        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    IEnumerator SpawnEnemies()
    {
        while (!isGameOver && gameStarted)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            int validEnemyCount = 0;

            foreach (GameObject enemy in enemies)
            {
                if (enemy != null) validEnemyCount++;
            }

            if (validEnemyCount < maxEnemiesAtOnce)
            {
                SpawnEnemy();
            }

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 finalPosition = spawn.position;

        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(spawn.position, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
        {
            finalPosition = hit.position;
        }

        GameObject enemy = Instantiate(enemyPrefab, finalPosition, spawn.rotation);
        enemy.tag = "Enemy";

        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            int hp = Mathf.RoundToInt(baseEnemyHealth * Mathf.Pow(difficultyMultiplier, currentDifficultyLevel - 1));
            health.maxHealth = hp;
            health.currentHealth = hp;
        }

        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
        {
            int dmg = Mathf.RoundToInt(baseEnemyDamage * Mathf.Pow(difficultyMultiplier, currentDifficultyLevel - 1));
            ai.attackDamage = dmg;
        }
    }

    public void EnemyKilled()
    {
        enemiesKilled++;
        score += pointsPerKill;
    }

    public void AddScore(int points)
    {
        score += points;
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
        if (killsText != null) killsText.text = "Kills: " + enemiesKilled;
        if (gameTimeText != null)
        {
            int min = Mathf.FloorToInt(gameTime / 60);
            int sec = Mathf.FloorToInt(gameTime % 60);
            gameTimeText.text = string.Format("{0:00}:{1:00}", min, sec);
        }
    }
    
    void UpdatePauseStats()
    {
        int min = Mathf.FloorToInt(gameTime / 60);
        int sec = Mathf.FloorToInt(gameTime % 60);
        
        if (pauseScoreText != null) pauseScoreText.text = "SCORE: " + score;
        if (pauseKillsText != null) pauseKillsText.text = "KILLS: " + enemiesKilled;
        if (pauseTimeText != null) pauseTimeText.text = "TIME: " + string.Format("{0:00}:{1:00}", min, sec);
    }
    
    void UpdateGameOverStats()
    {
        int min = Mathf.FloorToInt(gameTime / 60);
        int sec = Mathf.FloorToInt(gameTime % 60);
        
        if (gameOverScoreText != null) gameOverScoreText.text = "SCORE: " + score;
        if (gameOverKillsText != null) gameOverKillsText.text = "KILLS: " + enemiesKilled;
        if (gameOverTimeText != null) gameOverTimeText.text = "TIME: " + string.Format("{0:00}:{1:00}", min, sec);
    }
}