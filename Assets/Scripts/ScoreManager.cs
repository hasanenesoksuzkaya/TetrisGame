using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    public int score = 0;
    public int lines = 0;
    public int level = 1;
    public int linesToNextLevel = 15;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Scoring")]
    [SerializeField] private int singleLineScore = 100;
    [SerializeField] private int doubleLineScore = 300;
    [SerializeField] private int tripleLineScore = 500;
    [SerializeField] private int tetrisScore = 800;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddScore(int linesCleared)
    {
        int points = 0;
        
        switch (linesCleared)
        {
            case 1:
                points = singleLineScore * level;
                break;
            case 2:
                points = doubleLineScore * level;
                break;
            case 3:
                points = tripleLineScore * level;
                break;
            case 4:
                points = tetrisScore * level;
                break;
        }

        score += points;
        lines += linesCleared;

        if (lines >= linesToNextLevel)
        {
            LevelUp();
        }

        UpdateUI();
    }

    private void LevelUp()
    {
        level++;
        linesToNextLevel += 15; 
        
        if (Piece.Instance != null)
        {
            Piece.Instance.UpdateSpeedForLevel(level);
        }
    }

    public void ResetScore()
    {
        score = 0;
        lines = 0;
        level = 1;
        linesToNextLevel = 15;
        UpdateUI();
        
        if (Piece.Instance != null)
        {
            Piece.Instance.UpdateSpeedForLevel(1);
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "SCORE: " + score.ToString("N0");
        
        if (levelText != null)
            levelText.text = "LEVEL: " + level.ToString();
    }
}
