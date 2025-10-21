using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
	[SerializeField] private GameObject mainMenuPanel;
	[SerializeField] private GameObject hudPanel;
	[SerializeField] private GameObject pausePanel;
	[SerializeField] private GameObject gameOverPanel;

	private void Start()
	{
		ShowMainMenu();
	}

	private void Update()
	{
		if (GameManager.Instance == null)
		{
			return;
		}

		if (GameManager.Instance.CurrentState == GameState.Playing)
		{
			if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
			{
				Pause();
			}
		}
		else if (GameManager.Instance.CurrentState == GameState.Paused)
		{
			if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
			{
				Resume();
			}
		}

		if (GameManager.Instance.CurrentState == GameState.GameOver)
		{
			ShowGameOver();
		}
	}

	public void ShowMainMenu()
	{
		SetActive(mainMenuPanel, true);
		SetActive(hudPanel, false);
		SetActive(pausePanel, false);
		SetActive(gameOverPanel, false);
		if (GameManager.Instance != null)
		{
			GameManager.Instance.ReturnToMenu();
		}
	}

	public void StartGame()
	{
		SetActive(mainMenuPanel, false);
		SetActive(hudPanel, true);
		SetActive(pausePanel, false);
		SetActive(gameOverPanel, false);
		GameManager.Instance.StartGame();
	}

	public void Pause()
	{
		SetActive(pausePanel, true);
		GameManager.Instance.PauseGame();
	}

	public void Resume()
	{
		SetActive(pausePanel, false);
		GameManager.Instance.ResumeGame();
	}

	public void Restart()
	{
		SetActive(gameOverPanel, false);
		SetActive(pausePanel, false);
		SetActive(hudPanel, true);
		GameManager.Instance.Restart();
	}

	public void ShowGameOver()
	{
		SetActive(gameOverPanel, true);
		SetActive(hudPanel, false);
	}

	public void Quit()
	{
		Application.Quit();
	}

	public void ReturnToMainMenu()
	{
		ShowMainMenu();
	}

	private void SetActive(GameObject go, bool active)
	{
		if (go != null)
		{
			go.SetActive(active);
		}
	}
}


