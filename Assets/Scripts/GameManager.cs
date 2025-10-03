using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("Player")]
    public GameObject player;
    
    [Header("Game State")]
    public bool isPaused = false;
    public bool gameOver = false;
    
    // Singleton instance
    public static GameManager Instance { get; private set; }
    
    // Events
    public System.Action OnGameStart;
    public System.Action OnGamePause;
    public System.Action OnGameResume;
    public System.Action OnGameOver;

	public InputAction pauseAction;
    
    void Awake()
    {
        // Singleton pattern
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
        // Find player if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        
        // Start the game
        StartGame();

		pauseAction = InputSystem.actions.FindAction("Pause");
    }
    
    void Update()
    {
        // Handle pause input
        if (pauseAction.WasPressedThisFrame())
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }
    
    void InitializeGame()
    {
        // Set initial game state
        isPaused = false;
        gameOver = false;
        Time.timeScale = 1f;
        
        // Lock cursor for FPS gameplay
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
    
    // Utility methods for common game operations
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
