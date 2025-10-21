using UnityEngine;

public enum GameState
{
	MainMenu,
	Playing,
	Paused,
	GameOver
}

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }

	[SerializeField] private Board board;

	public GameState CurrentState { get; private set; } = GameState.MainMenu;

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

	public void StartGame()
	{
		SetState(GameState.Playing);
		if (board != null)
		{
			board.enabled = true;
		}
	}

	public void ReturnToMenu()
	{
		SetState(GameState.MainMenu);
		Time.timeScale = 1f;
	}

	public void PauseGame()
	{
		if (CurrentState != GameState.Playing)
		{
			return;
		}
		SetState(GameState.Paused);
		Time.timeScale = 0f;
	}

	public void ResumeGame()
	{
		if (CurrentState != GameState.Paused)
		{
			return;
		}
		SetState(GameState.Playing);
		Time.timeScale = 1f;
	}

	public void GameOver()
	{
		SetState(GameState.GameOver);
		Time.timeScale = 0f;
	}

	public void Restart()
	{
		Time.timeScale = 1f;
		SetState(GameState.Playing);
		if (board != null)
		{
			board.tilemap.ClearAllTiles();
			board.SpawnPiece();
		}
		
		if (ScoreManager.Instance != null)
		{
			ScoreManager.Instance.ResetScore();
		}
	}

	private void SetState(GameState newState)
	{
		CurrentState = newState;
	}
}


