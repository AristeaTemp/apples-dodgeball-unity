using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class LevelSettings
{
    public string levelName = "Level X";
    public int numberOfZombies = 5;
    public int numberOfBalls = 10;
    public bool zombiesCanDodge = false;
    public bool zombiesCanCatch = false;
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Level Configuration")]
    public List<LevelSettings> levels;
    private int currentLevelIndex = 0;

    [Header("Scene References")]
    public SimpleAISpawner spawner;
    public GameObject ballPrefab;
    public Transform ballSpawnPoint;
    public PlayerController player;

    [Header("UI Elements")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI zombiesRemainingText;
    public TextMeshProUGUI ballsRemainingText;
    public GameObject levelCompletePanel;
    public GameObject gameWinPanel;
    public GameObject gameOverPanel;

    private int zombiesRemaining;
    private int ballsSpawnedThisLevel;
    private LevelSettings currentSettings;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (gameWinPanel != null) gameWinPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        StartLevel(currentLevelIndex);
    }

    void StartLevel(int levelIndex)
    {
        if (levelIndex >= levels.Count) { return; }

        currentLevelIndex = levelIndex;
        levelCompletePanel.SetActive(false);
        gameWinPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        Time.timeScale = 1;

        GameObject[] existingZombies = GameObject.FindGameObjectsWithTag("Zombie");
        foreach (var zombie in existingZombies) Destroy(zombie);

        currentSettings = levels[levelIndex];

        if (spawner != null)
        {
            spawner.numberOfZombies = currentSettings.numberOfZombies;
            spawner.SpawnZombies();
        }

        GameObject[] oldBalls = GameObject.FindGameObjectsWithTag("Ball");
        foreach (var ball in oldBalls) Destroy(ball);

        ballsSpawnedThisLevel = 0;
        SpawnNextBall(); // Δημιουργούμε την πρώτη μπάλα

        UpdateUI();
    }

    public void SetInitialZombieCount(int count)
    {
        zombiesRemaining = count;
        GameObject[] newZombies = GameObject.FindGameObjectsWithTag("Zombie");
        foreach (GameObject zombie in newZombies)
        {
            CompleteZombieAI zombieAI = zombie.GetComponent<CompleteZombieAI>();
            if (zombieAI != null)
            {
                zombieAI.ConfigureAI(currentSettings.zombiesCanDodge, currentSettings.zombiesCanCatch);
            }
        }
        UpdateZombiesRemainingUI();
    }

    // Καλείται από τον παίκτη όταν πιάνει μια μπάλα
    public void PlayerTookBall()
    {
        SpawnNextBall();
        UpdateBallsRemainingUI();
    }

    // Δημιουργεί την επόμενη διαθέσιμη μπάλα για το level
    void SpawnNextBall()
    {
        if (currentSettings.numberOfBalls == 0) // Άπειρες μπάλες
        {
            if (ballPrefab != null && ballSpawnPoint != null && GameObject.FindGameObjectsWithTag("Ball").Length < 1)
                Instantiate(ballPrefab, ballSpawnPoint.position, ballSpawnPoint.rotation);
            return;
        }

        if (ballsSpawnedThisLevel < currentSettings.numberOfBalls)
        {
            if (ballPrefab == null || ballSpawnPoint == null) return;
            Instantiate(ballPrefab, ballSpawnPoint.position, ballSpawnPoint.rotation);
            ballsSpawnedThisLevel++;
        }
    }

    // Καλείται από το BallController όταν μια μπάλα καταστρέφεται
    public void BallWasDestroyed()
    {
        CheckForLossCondition();
    }

    void CheckForLossCondition()
    {
        // Χρησιμοποιούμε Invoke για να δώσουμε χρόνο στη Unity να καταστρέψει το αντικείμενο πριν ελέγξουμε
        Invoke("PerformLossCheck", 0.1f);
    }

    void PerformLossCheck()
    {
        int ballsLeftInWorld = GameObject.FindGameObjectsWithTag("Ball").Length;

        // Αν έχουν δημιουργηθεί όλες οι μπάλες του level, και δεν υπάρχουν άλλες στη σκηνή,
        // και ο παίκτης δεν κρατάει μπάλα, και υπάρχουν ακόμα ζόμπι... τότε έχασες.
        if (currentSettings.numberOfBalls != 0 && ballsSpawnedThisLevel >= currentSettings.numberOfBalls && ballsLeftInWorld == 0 && !player.HasBall() && zombiesRemaining > 0)
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void ZombieWasDestroyed()
    {
        zombiesRemaining--;
        UpdateUI();
        if (zombiesRemaining <= 0)
        {
            if (currentLevelIndex >= levels.Count - 1)
            {
                if (gameWinPanel != null) gameWinPanel.SetActive(true);
            }
            else
            {
                if (levelCompletePanel != null) levelCompletePanel.SetActive(true);
            }
            Time.timeScale = 0;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        StartLevel(0);
    }

    public void GoToNextLevel()
    {
        currentLevelIndex++;
        if (currentLevelIndex < levels.Count)
        {
            StartLevel(currentLevelIndex);
        }
        else
        {
            Debug.Log("YOU WIN THE GAME!");
        }
    }

    void UpdateUI()
    {
        UpdateZombiesRemainingUI();
        UpdateBallsRemainingUI();
        if (levelText != null) levelText.text = currentSettings.levelName;
    }

    void UpdateZombiesRemainingUI()
    {
        if (zombiesRemainingText != null) zombiesRemainingText.text = "Zombies: " + zombiesRemaining;
    }

    void UpdateBallsRemainingUI()
    {
        if (ballsRemainingText != null)
        {
            if (currentSettings.numberOfBalls == 0)
            {
                ballsRemainingText.text = "Balls: ∞";
            }
            else
            {
                int ballsLeft = currentSettings.numberOfBalls - ballsSpawnedThisLevel;
                if (player.HasBall()) ballsLeft++;
                ballsRemainingText.text = "Balls: " + ballsLeft;
            }
        }
    }
    public void PlayerHit() { /* ... */ }
}