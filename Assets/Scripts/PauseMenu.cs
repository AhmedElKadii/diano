using UnityEngine;
using UnityEngine.UIElements;

public class PauseMenu : MonoBehaviour
{
	private VisualElement ui;
	private VisualElement mainMenu;
	private VisualElement settingsMenu;
	private VisualElement root;
	
	private Button backButton;
	private Button quitButton;
	private Button applyButton;
	
	private Toggle bloomToggle;
	private Toggle vignetteToggle;
	private Toggle chromaticAberrationToggle;
	private Toggle filmGrainToggle;
	private Toggle motionBlurToggle;
	private Toggle aimAssistToggle;
	private Slider mouseSensitivitySlider;
	
	private SettingsController settingsController;

	void Awake()
	{
		ui = GetComponent<UIDocument>().rootVisualElement;
		root = ui.Q<VisualElement>("Panel");
		settingsController = GetComponent<SettingsController>();
	}

	void OnEnable()
	{
		InitializeSettingsMenu();
		root.style.display = DisplayStyle.None;
	}

	void Start()
	{
		HidePauseMenu();
	}

	void OnDisable()
	{
		if (backButton != null) backButton.clicked -= OnBackClicked;
		if (quitButton != null) quitButton.clicked -= OnQuitClicked;
		if (applyButton != null) applyButton.clicked -= OnApplyClicked;
	}

	public void ShowPauseMenu()
	{
		root.style.display = DisplayStyle.Flex;
		if (settingsMenu != null) settingsMenu.style.display = DisplayStyle.Flex;
	}

	public void HidePauseMenu()
	{
		root.style.display = DisplayStyle.None;
		if (settingsMenu != null) settingsMenu.style.display = DisplayStyle.None;
	}

	void InitializeSettingsMenu()
	{
		settingsMenu = ui.Q<VisualElement>("SettingsMenu");
		mainMenu = ui.Q<VisualElement>("MainMenu");
		if (settingsMenu == null)
		{
			Debug.LogError("SettingsMenu element not found!");
			return;
		}
		settingsMenu.style.display = DisplayStyle.None;
		mainMenu.style.display = DisplayStyle.None;
		
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
		
		applyButton = settingsMenu.Q<Button>("ApplyButton");
		if (applyButton != null)
		{
			applyButton.clicked += OnApplyClicked;
		}

		quitButton = settingsMenu.Q<Button>("SettingsQuitButton");
		if (quitButton != null)
		{
			quitButton.clicked += OnQuitClicked;
		}
		quitButton.style.display = DisplayStyle.Flex;
		
		backButton = settingsMenu.Q<Button>("BackButton");
		if (backButton != null)
		{
			backButton.clicked += OnBackClicked;
		}
		
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

		// Just apply settings, don't save individually
		if (bloomToggle != null) settingsController.ToggleBloom(bloomToggle.value);
		if (vignetteToggle != null) settingsController.ToggleVignette(vignetteToggle.value);
		if (chromaticAberrationToggle != null) settingsController.ToggleChromaticAberration(chromaticAberrationToggle.value);
		if (filmGrainToggle != null) settingsController.ToggleFilmGrain(filmGrainToggle.value);
		if (motionBlurToggle != null) settingsController.ToggleMotionBlur(motionBlurToggle.value);
		if (aimAssistToggle != null) settingsController.ToggleAimAssist(aimAssistToggle.value);
		if (mouseSensitivitySlider != null) settingsController.SetMouseSensitivity(mouseSensitivitySlider.value);
		
		// Save everything once at the end
		settingsController.SaveSettings();
		
		Debug.Log("Settings applied and saved!");
	}

	// ===== PLAYERPREFS HELPER METHODS =====

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

	void OnBackClicked()
	{
		GameManager.Instance.ResumeGame();
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
