using UnityEngine;

public class SettingsController : MonoBehaviour
{
    public enum QualityPreset
    {
        High,
        Medium,
        Low,
		Custom
    }
    
    [Header("Quality Preset")]
    public QualityPreset qualityPreset = QualityPreset.Medium;
    
    [Header("Post Processing Effects")]
    public bool enableBloom = true;
    public bool enableVignette = true;
    public bool enableColorAdjustments = true;
    public bool enableWhiteBalance = true;
    public bool enableChromaticAberration = true;
    public bool enableFilmGrain = true;
    public bool enableMotionBlur = true;
    public bool enableDepthOfField = false;

	public float mouseSensitivity = 1.0f;
	public bool aimAssist = true;

	void Start()
	{
		LoadSettings();
	}
    
    public void ApplySettings()
    {
        switch (qualityPreset)
        {
            case QualityPreset.Low:
                PostProcessManager.Instance.SetQualityLow();
                break;
            case QualityPreset.Medium:
                PostProcessManager.Instance.SetQualityMedium();
                break;
            case QualityPreset.High:
                PostProcessManager.Instance.SetQualityHigh();
                break;
        }
        
        PostProcessManager.Instance.GetBloom().active = enableBloom;
        PostProcessManager.Instance.GetVignette().active = enableVignette;
        PostProcessManager.Instance.GetColorAdjustments().active = enableColorAdjustments;
        PostProcessManager.Instance.GetWhiteBalance().active = enableWhiteBalance;
        PostProcessManager.Instance.GetChromaticAberration().active = enableChromaticAberration;
        PostProcessManager.Instance.GetFilmGrain().active = enableFilmGrain;
        PostProcessManager.Instance.GetMotionBlur().active = enableMotionBlur;
        PostProcessManager.Instance.GetDepthOfField().active = enableDepthOfField;
        
        Debug.Log($"Settings Applied: {qualityPreset} Quality");
    }
    
    public void ToggleBloom(bool value)
    {
        enableBloom = value;
        if (PostProcessManager.Instance != null) PostProcessManager.Instance.GetBloom().active = value;
    }
    
    public void ToggleVignette(bool value)
    {
        enableVignette = value;
        if (PostProcessManager.Instance != null) PostProcessManager.Instance.GetVignette().active = value;
    }
    
    public void ToggleChromaticAberration(bool value)
    {
        enableChromaticAberration = value;
        if (PostProcessManager.Instance != null) PostProcessManager.Instance.GetChromaticAberration().active = value;
    }
    
    public void ToggleFilmGrain(bool value)
    {
        enableFilmGrain = value;
        if (PostProcessManager.Instance != null) PostProcessManager.Instance.GetFilmGrain().active = value;
    }
    
    public void ToggleMotionBlur(bool value)
    {
        enableMotionBlur = value;
        if (PostProcessManager.Instance != null) PostProcessManager.Instance.GetMotionBlur().active = value;
    }
    
    public void SetQuality(QualityPreset preset)
	{
		qualityPreset = preset;
		ApplySettings();
	}

	public void ToggleAimAssist(bool value)
	{
		aimAssist = value;
	}

	public void SetMouseSensitivity(float value)
	{
		mouseSensitivity = value;
	}

	public void SaveSettings()
	{
		PlayerPrefs.SetInt("Bloom", enableBloom ? 1 : 0);
		PlayerPrefs.SetInt("Vignette", enableVignette ? 1 : 0);
		PlayerPrefs.SetInt("ChromaticAberration", enableChromaticAberration ? 1 : 0);
		PlayerPrefs.SetInt("FilmGrain", enableFilmGrain ? 1 : 0);
		PlayerPrefs.SetInt("MotionBlur", enableMotionBlur ? 1 : 0);
		PlayerPrefs.SetInt("AimAssist", aimAssist ? 1 : 0);
		PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);

		PlayerPrefs.Save();
		ApplySettings();
	}

	public void LoadSettings()
	{
		enableBloom = PlayerPrefs.GetInt("Bloom", 1) == 1;
		enableVignette = PlayerPrefs.GetInt("Vignette", 1) == 1;
		enableChromaticAberration = PlayerPrefs.GetInt("ChromaticAberration", 1) == 1;
		enableFilmGrain = PlayerPrefs.GetInt("FilmGrain", 1) == 1;
		enableMotionBlur = PlayerPrefs.GetInt("MotionBlur", 1) == 1;
		aimAssist = PlayerPrefs.GetInt("AimAssist", 1) == 1;
		mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1.0f);

		ApplySettings();
	}
}
