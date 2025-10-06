using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Player")]
    public GameObject player;
	public string playerName;
	public PlayerController playerController;
    
    [Header("Game State")]
    public bool isPaused = false;
    public bool gameOver = true;

	[Header("Gameplay Settings")]
	public bool aimAssistEnabled = true;

	[Header("Player Abilities")]
	public bool canDash = false;
	public int maxJumps = 1;
	public float speedBoostMultiplier = 1f;
	public float maxStamina = 100f;
	public float maxHealth = 100f;

	public string currentWeapon = "Pistol";

	public int score = 0;
	public long time  = 0;

	public EnemySpawner enemySpawner;
    
    public static GameManager Instance { get; private set; }
    
    public System.Action OnGameStart;
    public System.Action OnGamePause;
    public System.Action OnGameResume;
    public System.Action OnGameOver;

	public List<GameObject> trash;

	public InputAction pauseAction;

	bool spawningEnemies = false;

	public PauseMenu pauseMenu;

    public int GetScore()
    {
        return score;
    }
    
    public long GetTime()
    {
        return time;
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        StartGame();

		if (PlayerPrefs.HasKey("PlayerName"))
		{
			playerName = PlayerPrefs.GetString("PlayerName");
		}
		else
		{
			playerName = "Player";
			PlayerPrefs.SetString("PlayerName", playerName);
		}

		pauseAction = InputSystem.actions.FindAction("Pause");

		if (enemySpawner != null) enemySpawner.SpawnEnemies();

		if (player != null) player.GetComponent<PlayerController>().sensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
    }
    
    void Update()
    {
		if (gameOver) return;

        if (pauseAction.WasPressedThisFrame())
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }

		if (enemySpawner != null && enemySpawner.currentEnemies == 0 && !spawningEnemies)
		{
			spawningEnemies = true;
			playerController = player.GetComponent<PlayerController>();
			playerController.health += 25;
			StartCoroutine(SpawnNextWave());
		}

		time = (long)Time.timeSinceLevelLoad;
    }

	IEnumerator SpawnNextWave()
	{
		foreach (var obj in trash)
		{
			Destroy(obj);
		}

		yield return new WaitForSeconds(5f);
		enemySpawner.SpawnEnemies();
		yield return new WaitForSeconds(0.1f);
		spawningEnemies = false;
	}
    
    void InitializeGame()
    {
        isPaused = false;
        gameOver = false;
        Time.timeScale = 1f;
    }
    
    public void StartGame()
    {
        gameOver = false;
        isPaused = false;
        Time.timeScale = 1f;
        OnGameStart?.Invoke();
    }
    
    public void PauseGame()
    {
        if (gameOver) return;
        
        isPaused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        OnGamePause?.Invoke();
		pauseMenu.ShowPauseMenu();
		if (playerController) playerController.hud.HideHUD();
    }
    
    public void ResumeGame()
    {
        if (gameOver) return;
        
        isPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        OnGameResume?.Invoke();
		pauseMenu.HidePauseMenu();
		playerController.hud.ShowHUD();

		playerController = player.GetComponent<PlayerController>();	
		playerController.sensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
		playerController.aimAssistEnabled = PlayerPrefs.GetInt("AimAssist", 1) == 1;
    }
    
    public void GameOver()
    {
        if (gameOver) return;
        
        gameOver = true;
        isPaused = false;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        OnGameOver?.Invoke();
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
    
    public void QuitGame()
    {
        Time.timeScale = 1f;
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public Vector3 GetPlayerPosition()
    {
        return player != null ? player.transform.position : Vector3.zero;
    }
    
    public Transform GetPlayerTransform()
    {
        return player != null ? player.transform : null;
    }
    
    public bool IsPlayerAlive()
    {
        if (player == null) return false;
        
        var playerController = player.GetComponent<PlayerController>();
        return playerController != null && !gameOver;
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && !isPaused)
        {
            PauseGame();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && !isPaused)
        {
            PauseGame();
        }
    }
}
