using UnityEngine;
using UnityEngine.UIElements;

public class HUD : MonoBehaviour
{
	private VisualElement ui;
	private VisualElement hudContainer;
	private Label scoreLabel;
	private Label enemyLabel;
	private Label ammoLabel;
	private ProgressBar healthBar;
	private ProgressBar staminaBar;
	private Label speechLabel;
	private VisualElement inputContainer;
	private TextField inputField;
	private Button submitButton;
	private Button cancelButton;
	private Button quitButton;
	private VisualElement crossHair;

	private Leaderboard leaderboard;

	void Awake()
	{
		ui = GetComponent<UIDocument>().rootVisualElement;
		leaderboard = GameManager.Instance.GetComponent<Leaderboard>();
	}

	void OnEnable()
	{
		InitializeHUD();
		InitializeInputContainer();
	}

	void OnDisable()
	{
		if (submitButton != null)
			submitButton.clicked -= OnSubmitClicked;
	}

	void InitializeHUD()
	{
		hudContainer = ui.Q<VisualElement>("HUD");
		scoreLabel = ui.Q<Label>("ScoreCounter");
		enemyLabel = ui.Q<Label>("EnemyCounter");
		ammoLabel = ui.Q<Label>("AmmoCounter");
		healthBar = ui.Q<ProgressBar>("HealthBar");
		staminaBar = ui.Q<ProgressBar>("StaminaBar");
		speechLabel = ui.Q<Label>("SpeechLabel");

		if (hudContainer == null)
		{
			Debug.LogError("HUD container not found!");
		}
	}

	void InitializeInputContainer()
	{
		inputContainer = ui.Q<VisualElement>("DeathScreen");
		inputField = ui.Q<TextField>("InputField");
		submitButton = ui.Q<Button>("SubmitButton");
		cancelButton = ui.Q<Button>("CancelButton");
		quitButton = ui.Q<Button>("QuitButton");
		crossHair = ui.Q<VisualElement>("Crosshair");

		inputField.value = GameManager.Instance.playerName;

		if (inputContainer == null)
		{
			Debug.LogWarning("InputContainer not found!");
			return;
		}

		inputContainer.style.display = DisplayStyle.None;

		if (submitButton != null)
		{
			submitButton.clicked += OnSubmitClicked;
		}

		if (cancelButton != null)
		{
			cancelButton.clicked += OnCancelClicked;
		}

		if (quitButton != null)
		{
			quitButton.clicked += OnQuitClicked;
		}

		if (inputField != null)
		{
			inputField.RegisterCallback<KeyDownEvent>(evt =>
			{
				if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
				{
					OnSubmitClicked();
				}
			});
		}
	}

	void OnCancelClicked()
	{
		HideInputContainer();
		StartCoroutine(GameManager.Instance.playerController.TransitionScenes());
	}

	public void ToggleCrosshair(bool value)
	{
		crossHair.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
	}

	void OnSubmitClicked()
	{
		if (inputField == null) return;

		string username = inputField.value;

		if (string.IsNullOrWhiteSpace(username))
		{
			Debug.LogWarning("Username cannot be empty!");
			return;
		}

		if (GameManager.Instance == null)
		{
			Debug.LogError("GameManager instance not found!");
			return;
		}

		if (leaderboard == null)
		{
			Debug.LogError("Leaderboard not found!");
			return;
		}

		int score = GameManager.Instance.GetScore();
		long time = GameManager.Instance.GetTime();

		var entry = new Leaderboard.PlayerEntry(username, score, time);
		leaderboard.AddPlayerEntry(entry);

		inputField.value = string.Empty;
		HideInputContainer();

		Debug.Log($"Submitted to leaderboard: {username} - Score: {score}, Time: {time}");

		StartCoroutine(GameManager.Instance.playerController.TransitionScenes());
	}

	void OnQuitClicked()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

	public void ShowInputContainer()
	{
		if (GameManager.Instance.wantsLeaderboard == false) return;
		if (inputContainer != null)
		{
			inputContainer.style.display = DisplayStyle.Flex;

			if (inputField != null)
			{
				inputField.value = string.Empty;

				inputField.Focus();
			}
		}
	}

	public void HideInputContainer()
	{
		if (inputContainer != null)
		{
			inputContainer.style.display = DisplayStyle.None;

			if (inputField != null)
			{
				inputField.value = string.Empty;
			}
		}
	}

	public string GetInputValue()
	{
		return inputField?.value ?? string.Empty;
	}

	public void SetInputValue(string value)
	{
		if (inputField != null)
		{
			inputField.value = value;
		}
	}

	public void Update()
	{
		UpdateScore(GameManager.Instance.GetScore());
		UpdateEnemyCount(GameManager.Instance.enemySpawner.currentEnemies, GameManager.Instance.enemySpawner.totalEnemiesSpawned);
	}

	public void UpdateScore(int score)
	{
		if (scoreLabel != null)
			scoreLabel.text = $"Score: {score}";
	}

	public void UpdateEnemyCount(int current, int enemies)
	{
		if (enemyLabel != null)
			enemyLabel.text = $"{current}/{enemies}";
	}

	public void UpdateAmmo(int current, int max)
	{
		if (ammoLabel != null)
			ammoLabel.text = $"{current}/{max}";
	}

	public void UpdateHealth(float current, float max)
	{
		if (healthBar != null)
		{
			healthBar.value = (current / max) * 100f;
			healthBar.title = $"HP: {current:F0}/{max:F0}";
			healthBar.highValue = 100f;
		}
	}

	public void UpdateStamina(float current, float max)
	{
		if (staminaBar != null)
		{
			staminaBar.value = (current / max) * 100f;
			staminaBar.title = $"Stamina: {current:F0}/{max:F0}";
			staminaBar.highValue = 100f;
		}
	}

	public void ShowBottomLabel(string message)
	{
		if (speechLabel != null)
		{
			speechLabel.text = message;
			speechLabel.style.display = DisplayStyle.Flex;
		}
	}

	public void HideBottomLabel()
	{
		if (speechLabel != null)
		{
			speechLabel.style.display = DisplayStyle.None;
		}
	}

	public void SetBottomLabelText(string text)
	{
		if (speechLabel != null)
		{
			speechLabel.text = text;
		}
	}

	public void ShowHUD()
	{
		if (hudContainer != null)
			hudContainer.style.display = DisplayStyle.Flex;
	}

	public void HideHUD()
	{
		if (hudContainer != null)
			hudContainer.style.display = DisplayStyle.None;
	}
}
