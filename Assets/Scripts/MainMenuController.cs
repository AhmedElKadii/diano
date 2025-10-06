using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
	private VisualElement ui;
	private VisualElement mainMenu;
	private VisualElement menuPanel;
	private VisualElement settingsMenu;
	private VisualElement splashPanel;
	private VisualElement splashContainer;
	
	private Button playButton;
	private Button settingsButton;
	private Button quitButton;
	private Button backButton;
	private Button applyButton;
	
	private ListView leaderboard;
	
	private Toggle bloomToggle;
	private Toggle vignetteToggle;
	private Toggle chromaticAberrationToggle;
	private Toggle filmGrainToggle;
	private Toggle motionBlurToggle;
	private Toggle aimAssistToggle;
	private Slider mouseSensitivitySlider;
	
	private SettingsController settingsController;
	private Leaderboard leaderboardController;

	void Awake()
	{
		ui = GetComponent<UIDocument>().rootVisualElement;
		settingsController = GetComponent<SettingsController>();
	}

	void OnEnable()
	{
		splashPanel = ui.Q<VisualElement>("SplashScreen");
		splashContainer = ui.Q<VisualElement>("SplashContainer");
		menuPanel = ui.Q<VisualElement>("Panel");
		menuPanel.style.display = DisplayStyle.None;
	}
	
	void init()
	{
		InitializeMainMenu();
		InitializeSettingsMenu();
		LoadLeaderboard();
	}

	void Start()
	{
		splashPanel.style.display = DisplayStyle.Flex;
		splashContainer.style.display = DisplayStyle.Flex;
		splashContainer.style.opacity = 0.0f;

		GameManager.Instance.gameOver = true;
		leaderboardController = GameManager.Instance.GetComponent<Leaderboard>();
		StartCoroutine(SplashScreen());
	}

	IEnumerator SplashScreen()
	{
		yield return new WaitForSeconds(0.5f);
		
		float fadeDuration = 2.0f;
		float elapsedTime = 0.0f;

		while (elapsedTime < fadeDuration)
		{
			elapsedTime += Time.deltaTime;
			float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
			splashContainer.style.opacity = alpha;
			yield return null;
		}

		yield return new WaitForSeconds(4.0f);
		elapsedTime = 0.0f;
		
		while (elapsedTime < fadeDuration)
		{
			elapsedTime += Time.deltaTime;
			float alpha = Mathf.Clamp01(1.0f - (elapsedTime / fadeDuration));
			splashContainer.style.opacity = alpha;
			yield return null;
		}
		
		splashPanel.style.display = DisplayStyle.None;
		splashContainer.style.display = DisplayStyle.None;
		splashContainer.style.opacity = 0.0f;
		splashPanel.style.opacity = 0.0f;

		init();
	}

	void OnDisable()
	{
		if (playButton != null) playButton.clicked -= OnPlayClicked;
		if (quitButton != null) quitButton.clicked -= OnQuitClicked;
		if (backButton != null) backButton.clicked -= OnBackClicked;
		if (applyButton != null) applyButton.clicked -= OnApplyClicked;
	}

	void InitializeMainMenu()
	{
		menuPanel.style.display = DisplayStyle.Flex;
		mainMenu = ui.Q<VisualElement>("MainMenu");
		if (mainMenu == null)
		{
			Debug.LogError("MainMenu element not found!");
			return;
		}
		mainMenu.style.display = DisplayStyle.Flex;

		playButton = ui.Q<Button>("PlayButton");
		settingsButton = ui.Q<Button>("SettingsButton");
		quitButton = ui.Q<Button>("QuitButton");

		if (playButton != null) playButton.clicked += OnPlayClicked;
		if (settingsButton != null) settingsButton.clicked += OnSettingsClicked;
		if (quitButton != null) quitButton.clicked += OnQuitClicked;
		
		leaderboard = ui.Q<ListView>("LeaderboardEntries");
		if (leaderboard == null)
		{
			Debug.LogWarning("LeaderboardEntries ListView not found!");
		}
	}

	void InitializeSettingsMenu()
	{
		settingsMenu = ui.Q<VisualElement>("SettingsMenu");
		if (settingsMenu == null)
		{
			Debug.LogError("SettingsMenu element not found!");
			return;
		}
		settingsMenu.style.display = DisplayStyle.None;
		
		// Post Processing Toggles
		bloomToggle = settingsMenu.Q<Toggle>("BloomToggle");
		if (bloomToggle != null)
		{
			bloomToggle.value = GetPlayerPrefBool("Bloom", true);
		}
		
		vignetteToggle = settingsMenu.Q<Toggle>("VignetteToggle");
		if (vignetteToggle != null)
		{
			vignetteToggle.value = GetPlayerPrefBool("Vignette", true);
		}
		
		chromaticAberrationToggle = settingsMenu.Q<Toggle>("ChromaticAberrationToggle");
		if (chromaticAberrationToggle != null)
		{
			chromaticAberrationToggle.value = GetPlayerPrefBool("ChromaticAberration", true);
		}
		
		filmGrainToggle = settingsMenu.Q<Toggle>("FilmGrainToggle");
		if (filmGrainToggle != null)
		{
			filmGrainToggle.value = GetPlayerPrefBool("FilmGrain", true);
		}
		
		motionBlurToggle = settingsMenu.Q<Toggle>("MotionBlurToggle");
		if (motionBlurToggle != null)
		{
			motionBlurToggle.value = GetPlayerPrefBool("MotionBlur", true);
		}
		
		// Gameplay Settings
		aimAssistToggle = settingsMenu.Q<Toggle>("AimAssistToggle");
		if (aimAssistToggle != null)
		{
			aimAssistToggle.value = GetPlayerPrefBool("AimAssist", true);
		}
		
		mouseSensitivitySlider = settingsMenu.Q<Slider>("MouseSensitivitySlider");
		if (mouseSensitivitySlider != null)
		{
			mouseSensitivitySlider.value = GetPlayerPrefFloat("MouseSensitivity", 1.0f, 0.1f, 5.0f);
		}
		
		// Apply Button
		applyButton = settingsMenu.Q<Button>("ApplyButton");
		if (applyButton != null)
		{
			applyButton.clicked += OnApplyClicked;
		}
		
		// Back Button
		backButton = settingsMenu.Q<Button>("BackButton");
		if (backButton != null)
		{
			backButton.clicked += OnBackClicked;
		}
		
		// Apply loaded settings to the controller immediately
		ApplyLoadedSettings();
	}

	void ApplyLoadedSettings()
	{
		if (settingsController == null) return;
		
		if (bloomToggle != null) settingsController.ToggleBloom(bloomToggle.value);
		if (vignetteToggle != null) settingsController.ToggleVignette(vignetteToggle.value);
		if (chromaticAberrationToggle != null) settingsController.ToggleChromaticAberration(chromaticAberrationToggle.value);
		if (filmGrainToggle != null) settingsController.ToggleFilmGrain(filmGrainToggle.value);
		if (motionBlurToggle != null) settingsController.ToggleMotionBlur(motionBlurToggle.value);
		if (aimAssistToggle != null) settingsController.ToggleAimAssist(aimAssistToggle.value);
		if (mouseSensitivitySlider != null) settingsController.SetMouseSensitivity(mouseSensitivitySlider.value);
	}

	void OnApplyClicked()
	{
		if (settingsController == null) return;

		if (bloomToggle != null) settingsController.ToggleBloom(bloomToggle.value);
		if (vignetteToggle != null) settingsController.ToggleVignette(vignetteToggle.value);
		if (chromaticAberrationToggle != null) settingsController.ToggleChromaticAberration(chromaticAberrationToggle.value);
		if (filmGrainToggle != null) settingsController.ToggleFilmGrain(filmGrainToggle.value);
		if (motionBlurToggle != null) settingsController.ToggleMotionBlur(motionBlurToggle.value);
		if (aimAssistToggle != null) settingsController.ToggleAimAssist(aimAssistToggle.value);
		if (mouseSensitivitySlider != null) settingsController.SetMouseSensitivity(mouseSensitivitySlider.value);
		
		settingsController.SaveSettings();
		
		Debug.Log("Settings applied and saved!");
	}

	void LoadLeaderboard()
	{
		if (leaderboardController == null)
		{
			Debug.LogError("Leaderboard controller is null!");
			return;
		}

		if (leaderboard == null)
		{
			Debug.LogWarning("Leaderboard ListView not found, skipping load.");
			return;
		}

		leaderboard.style.flexGrow = 1;
		leaderboard.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

		leaderboardController.GetPlayerEntries(
			onSuccess: (entries) => 
			{
				Debug.Log($"Successfully fetched {entries.Length} entries!");
				PopulateLeaderboard(entries);
			},
			onError: (error) => 
			{
				Debug.LogError($"Failed to fetch leaderboard: {error}");
			}
		);
	}

	void PopulateLeaderboard(Leaderboard.PlayerEntry[] entries)
	{
		if (leaderboard == null || entries == null)
		{
			Debug.LogWarning("Cannot populate leaderboard: ListView or entries is null");
			return;
		}

		List<Leaderboard.PlayerEntry> leaderboardData = new List<Leaderboard.PlayerEntry>(entries);

		string currentUsername = PlayerPrefs.GetString("Username", "Player");
		for (int i = 0; i < leaderboardData.Count; i++)
		{
			if (leaderboardData[i].username == currentUsername)
			{
				leaderboardData[i].username = "YOU";
				break;
			}
		}
		
		leaderboard.Clear();
		leaderboard.itemsSource = null;
		
		CreateLeaderboardHeader();
		
		leaderboard.itemsSource = leaderboardData;
		leaderboard.makeItem = () => 
		{
			var item = new VisualElement();
			item.AddToClassList("entry");
			item.style.flexDirection = FlexDirection.Row;
			
			var usernameLabel = new Label();
			usernameLabel.AddToClassList("username");
			
			var scoreLabel = new Label();
			scoreLabel.AddToClassList("score");
			
			var timeLabel = new Label();
			timeLabel.AddToClassList("time");
			
			item.Add(usernameLabel);
			item.Add(scoreLabel);
			item.Add(timeLabel);
			
			return item;
		};
		
		leaderboard.bindItem = (element, index) => 
		{
			if (index < 0 || index >= leaderboardData.Count)
			{
				Debug.LogWarning($"Invalid leaderboard index: {index}");
				return;
			}

			var entry = leaderboardData[index];
			
			var usernameLabel = element.Q<Label>(className: "username");
			var scoreLabel = element.Q<Label>(className: "score");
			var timeLabel = element.Q<Label>(className: "time");
			
			if (usernameLabel != null) usernameLabel.text = entry.username ?? "Unknown";
			if (scoreLabel != null) scoreLabel.text = entry.score.ToString();
			if (timeLabel != null) timeLabel.text = FormatTime(entry.time);
		};
	}

	private string FormatTime(long time)
	{
		int totalSeconds = (int)(time);

		int hours = totalSeconds / 3600;
		int minutes = (totalSeconds % 3600) / 60;
		int seconds = totalSeconds % 60;
		
		return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
	}

	void CreateLeaderboardHeader()
	{
		var leaderboardContainer = leaderboard.parent;
		if (leaderboardContainer == null) return;
		
		var oldHeader = leaderboardContainer.Q<VisualElement>("leaderboard-header");
		if (oldHeader != null)
		{
			leaderboardContainer.Remove(oldHeader);
		}
		
		var header = new VisualElement();
		header.name = "leaderboard-header";
		header.AddToClassList("leaderboard-header");
		header.style.flexDirection = FlexDirection.Row;
		
		var nameLabel = new Label("NAME");
		nameLabel.AddToClassList("header-label");
		nameLabel.AddToClassList("username");
		
		var scoreLabel = new Label("SCORE");
		scoreLabel.AddToClassList("header-label");
		scoreLabel.AddToClassList("score");
		
		var timeLabel = new Label("TIME");
		timeLabel.AddToClassList("header-label");
		timeLabel.AddToClassList("time");
		
		header.Add(nameLabel);
		header.Add(scoreLabel);
		header.Add(timeLabel);
		
		int leaderboardIndex = leaderboardContainer.IndexOf(leaderboard);
		leaderboardContainer.Insert(leaderboardIndex, header);
	}

	private bool GetPlayerPrefBool(string key, bool defaultValue)
	{
		try
		{
			return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
		}
		catch (System.Exception e)
		{
			Debug.LogWarning($"Error reading PlayerPref '{key}': {e.Message}. Using default: {defaultValue}");
			return defaultValue;
		}
	}

	private void SetPlayerPrefBool(string key, bool value)
	{
		try
		{
			PlayerPrefs.SetInt(key, value ? 1 : 0);
			PlayerPrefs.Save();
		}
		catch (System.Exception e)
		{
			Debug.LogError($"Error saving PlayerPref '{key}': {e.Message}");
		}
	}

	private float GetPlayerPrefFloat(string key, float defaultValue, float minValue = float.MinValue, float maxValue = float.MaxValue)
	{
		try
		{
			float value = PlayerPrefs.GetFloat(key, defaultValue);
			return Mathf.Clamp(value, minValue, maxValue);
		}
		catch (System.Exception e)
		{
			Debug.LogWarning($"Error reading PlayerPref '{key}': {e.Message}. Using default: {defaultValue}");
			return defaultValue;
		}
	}

	private void SetPlayerPrefFloat(string key, float value)
	{
		try
		{
			PlayerPrefs.SetFloat(key, value);
			PlayerPrefs.Save();
		}
		catch (System.Exception e)
		{
			Debug.LogError($"Error saving PlayerPref '{key}': {e.Message}");
		}
	}

	void OnPlayClicked()
	{
		mainMenu.style.display = DisplayStyle.None;
		settingsMenu.style.display = DisplayStyle.None;
		GameManager.Instance.gameOver = false;
		SceneManager.LoadScene(2);
		GameManager.Instance?.StartGame();
	}

	void OnSettingsClicked()
	{
		if (mainMenu != null) mainMenu.style.display = DisplayStyle.None;
		if (settingsMenu != null) settingsMenu.style.display = DisplayStyle.Flex;
	}

	void OnBackClicked()
	{
		if (settingsMenu != null) settingsMenu.style.display = DisplayStyle.None;
		if (mainMenu != null) mainMenu.style.display = DisplayStyle.Flex;
	}

	void OnQuitClicked()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}
}
