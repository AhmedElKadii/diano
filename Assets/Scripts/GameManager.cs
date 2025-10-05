using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Player")]
    public GameObject player;
    
    [Header("Game State")]
    public bool isPaused = false;
    public bool gameOver = false;

	[Header("Gameplay Settings")]
	public bool aimAssistEnabled = true;

	[Header("Player Abilities")]
	public bool canDash = false;
	public int maxJumps = 1;
	public float speedBoostMultiplier = 1f;

	public string currentWeapon = "Pistol";

	public EnemySpawner enemySpawner;
    
    public static GameManager Instance { get; private set; }
    
    public System.Action OnGameStart;
    public System.Action OnGamePause;
    public System.Action OnGameResume;
    public System.Action OnGameOver;

	public List<GameObject> trash;

	public InputAction pauseAction;

	bool spawningEnemies = false;
    
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

		pauseAction = InputSystem.actions.FindAction("Pause");

		if (enemySpawner != null) enemySpawner.SpawnEnemies();
    }
    
    void Update()
    {
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
			StartCoroutine(SpawnNextWave());
		}
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
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
    }
    
    public void ResumeGame()
    {
        if (gameOver) return;
        
        isPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        OnGameResume?.Invoke();
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
