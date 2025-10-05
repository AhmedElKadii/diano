using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessManager : MonoBehaviour
{
    public static PostProcessManager Instance { get; private set; }
    
    private Volume volume;
    
    [Header("Bloom Settings (Muzzle Flashes & Explosions)")]
    public float bloomIntensity = 1.2f;
    public float bloomThreshold = 0.8f;
    public float bloomScatter = 0.7f;
    
    [Header("Vignette Settings")]
    public float vignetteIntensity = 0.3f;
    public float vignetteSmoothness = 0.5f;
    public Color vignetteColor = Color.black;
    
    [Header("Color Grading")]
    public float colorSaturation = 15f;
    public float colorContrast = 10f;
    public float colorTemperature = -5f;
    public float colorTint = 0f;
    
    [Header("Chromatic Aberration (Motion/Impact)")]
    public float chromaticIntensity = 0.3f;
    
    [Header("Film Grain (Gritty Look)")]
    public float filmGrainIntensity = 0.25f;
    
    [Header("Lens Distortion")]
    public float lensDistortion = -0.1f;
    
    [Header("Motion Blur")]
    public float motionBlurIntensity = 0.3f;
    public int motionBlurQuality = 4;
    
    [Header("Depth of Field (ADS)")]
    public float dofFocusDistance = 10f;
    public float dofAperture = 5.6f;
    public float dofFocalLength = 50f;
    
    private Bloom bloom;
    private Vignette vignette;
    private ColorAdjustments colorAdjustments;
    private WhiteBalance whiteBalance;
    private ChromaticAberration chromaticAberration;
    private FilmGrain filmGrain;
    private MotionBlur motionBlur;
    private DepthOfField depthOfField;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

	void Update()
	{
		ApplySettings();
	}
    
    void Start()
    {
        // Create volume as child
        GameObject volumeObject = new GameObject("URP Volume");
        volumeObject.transform.SetParent(transform);
        
        volume = volumeObject.AddComponent<Volume>();
        volumeObject.layer = LayerMask.NameToLayer("Postprocessing");
        
        volume.isGlobal = true;
        volume.priority = 1;
        volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
        
        InitializeEffects();
        ApplySettings();
    }
    
    void InitializeEffects()
    {
        // Bloom - for muzzle flashes, explosions, hit markers
        bloom = volume.profile.Add<Bloom>();
        bloom.intensity.overrideState = true;
        bloom.threshold.overrideState = true;
        bloom.scatter.overrideState = true;
        bloom.tint.overrideState = true;
        bloom.tint.value = Color.white;
        
        // Vignette - focus attention, low health effect
        vignette = volume.profile.Add<Vignette>();
        vignette.intensity.overrideState = true;
        vignette.smoothness.overrideState = true;
        vignette.color.overrideState = true;
        
        // Color Adjustments - stylistic grading
        colorAdjustments = volume.profile.Add<ColorAdjustments>();
        colorAdjustments.saturation.overrideState = true;
        colorAdjustments.contrast.overrideState = true;
        
        // White Balance - color temperature
        whiteBalance = volume.profile.Add<WhiteBalance>();
        whiteBalance.temperature.overrideState = true;
        whiteBalance.tint.overrideState = true;
        
        // Chromatic Aberration - damage/impact effect
        chromaticAberration = volume.profile.Add<ChromaticAberration>();
        chromaticAberration.intensity.overrideState = true;
        
        // Film Grain - gritty tactical feel
        filmGrain = volume.profile.Add<FilmGrain>();
        filmGrain.intensity.overrideState = true;
        filmGrain.type.overrideState = true;
        filmGrain.type.value = FilmGrainLookup.Medium3;
        
        // Motion Blur - sprint/slide
        motionBlur = volume.profile.Add<MotionBlur>();
        motionBlur.intensity.overrideState = true;
        motionBlur.quality.overrideState = true;
        
        // Depth of Field - ADS (aiming down sights)
        depthOfField = volume.profile.Add<DepthOfField>();
        depthOfField.mode.overrideState = true;
        depthOfField.mode.value = DepthOfFieldMode.Bokeh;
        depthOfField.focusDistance.overrideState = true;
        depthOfField.aperture.overrideState = true;
        depthOfField.focalLength.overrideState = true;
        depthOfField.active = false; // Disabled by default, enable when ADS
    }
    
    public void ApplySettings()
    {
        // Bloom
        bloom.intensity.value = bloomIntensity;
        bloom.threshold.value = bloomThreshold;
        bloom.scatter.value = bloomScatter;
        
        // Vignette
        vignette.intensity.value = vignetteIntensity;
        vignette.smoothness.value = vignetteSmoothness;
        vignette.color.value = vignetteColor;
        
        // Color Adjustments
        colorAdjustments.saturation.value = colorSaturation;
        colorAdjustments.contrast.value = colorContrast;
        
        // White Balance
        whiteBalance.temperature.value = colorTemperature;
        whiteBalance.tint.value = colorTint;
        
        // Chromatic Aberration
        chromaticAberration.intensity.value = chromaticIntensity;
        
        // Film Grain
        filmGrain.intensity.value = filmGrainIntensity;
        
        // Motion Blur
        motionBlur.intensity.value = motionBlurIntensity;
        
        // Depth of Field
        depthOfField.focusDistance.value = dofFocusDistance;
        depthOfField.aperture.value = dofAperture;
        depthOfField.focalLength.value = dofFocalLength;
    }
    
    // ===== QUALITY PRESETS =====
    
    public void SetQualityLow()
    {
        // Minimal effects for performance
        bloomIntensity = 0.5f;
        bloomThreshold = 1.0f;
        bloomScatter = 0.5f;
        
        vignetteIntensity = 0.2f;
        vignetteSmoothness = 0.3f;
        
        colorSaturation = 10f;
        colorContrast = 5f;
        colorTemperature = 0f;
        
        chromaticIntensity = 0.1f;
        filmGrainIntensity = 0.1f;
        lensDistortion = 0f;
        
        motionBlurIntensity = 0f; // Disable for performance
        motionBlur.active = false;
        
        ApplySettings();
        Debug.Log("Post Processing: Low Quality (Performance Mode)");
    }
    
    public void SetQualityMedium()
    {
        // Balanced for most systems
        bloomIntensity = 1.0f;
        bloomThreshold = 0.85f;
        bloomScatter = 0.7f;
        
        vignetteIntensity = 0.3f;
        vignetteSmoothness = 0.5f;
        
        colorSaturation = 15f;
        colorContrast = 10f;
        colorTemperature = -5f;
        
        chromaticIntensity = 0.2f;
        filmGrainIntensity = 0.2f;
        lensDistortion = -0.05f;
        
        motionBlurIntensity = 0.2f;
        motionBlurQuality = 3;
        motionBlur.active = true;
        
        ApplySettings();
        Debug.Log("Post Processing: Medium Quality (Balanced)");
    }
    
    public void SetQualityHigh()
    {
        // Maximum visual fidelity
        bloomIntensity = 1.5f;
        bloomThreshold = 0.8f;
        bloomScatter = 0.85f;
        
        vignetteIntensity = 0.35f;
        vignetteSmoothness = 0.6f;
        
        colorSaturation = 20f;
        colorContrast = 15f;
        colorTemperature = -8f;
        
        chromaticIntensity = 0.4f;
        filmGrainIntensity = 0.3f;
        lensDistortion = -0.15f;
        
        motionBlurIntensity = 0.4f;
        motionBlurQuality = 6;
        motionBlur.active = true;
        
        ApplySettings();
        Debug.Log("Post Processing: High Quality (Ultra)");
    }
    
    // ===== STYLISTIC PRESETS =====
    
    public void SetStyleRealistic()
    {
        colorSaturation = 5f;
        colorContrast = 5f;
        colorTemperature = 0f;
        filmGrainIntensity = 0.15f;
        vignetteIntensity = 0.2f;
        
        ApplySettings();
        Debug.Log("Style: Realistic");
    }
    
    public void SetStyleCinematic()
    {
        colorSaturation = 20f;
        colorContrast = 20f;
        colorTemperature = -10f;
        filmGrainIntensity = 0.35f;
        vignetteIntensity = 0.45f;
        lensDistortion = -0.2f;
        
        ApplySettings();
        Debug.Log("Style: Cinematic");
    }
    
    public void SetStyleArcade()
    {
        colorSaturation = 30f;
        colorContrast = 15f;
        colorTemperature = 5f;
        filmGrainIntensity = 0.1f;
        vignetteIntensity = 0.25f;
        bloomIntensity = 2.0f;
        
        ApplySettings();
        Debug.Log("Style: Arcade");
    }
    
    // ===== GAMEPLAY EFFECTS =====
    
    public void EnableADS(bool enable)
    {
        depthOfField.active = enable;
        if (enable)
        {
            Debug.Log("ADS: Depth of Field Enabled");
        }
    }
    
    public void DamageEffect(float healthPercent)
    {
        // Increase vignette and desaturate as health drops
        vignette.intensity.value = Mathf.Lerp(0.3f, 0.7f, 1f - healthPercent);
        vignette.color.value = Color.Lerp(Color.black, Color.red, 1f - healthPercent);
        colorAdjustments.saturation.value = Mathf.Lerp(-20f, colorSaturation, healthPercent);
    }
    
    public void ResetDamageEffect()
    {
        vignette.intensity.value = vignetteIntensity;
        vignette.color.value = vignetteColor;
        colorAdjustments.saturation.value = colorSaturation;
    }
    
    public void ExplosionFlash(float duration = 0.2f)
    {
        StartCoroutine(FlashRoutine(duration));
    }
    
    private System.Collections.IEnumerator FlashRoutine(float duration)
    {
        float originalIntensity = bloom.intensity.value;
        bloom.intensity.value = 5f;
        chromaticAberration.intensity.value = 1f;
        
        yield return new WaitForSeconds(duration);
        
        bloom.intensity.value = originalIntensity;
        chromaticAberration.intensity.value = chromaticIntensity;
    }
    
    public void SprintEffect(bool sprinting)
    {
        if (sprinting)
        {
            motionBlur.intensity.value = Mathf.Lerp(motionBlur.intensity.value, 0.6f, Time.deltaTime * 5f);
        }
        else
        {
            motionBlur.intensity.value = Mathf.Lerp(motionBlur.intensity.value, motionBlurIntensity, Time.deltaTime * 5f);
        }
    }
    
    // ===== HELPER METHODS =====
    
    public Bloom GetBloom() => bloom;
    public Vignette GetVignette() => vignette;
    public ColorAdjustments GetColorAdjustments() => colorAdjustments;
    public WhiteBalance GetWhiteBalance() => whiteBalance;
    public ChromaticAberration GetChromaticAberration() => chromaticAberration;
    public FilmGrain GetFilmGrain() => filmGrain;
    public MotionBlur GetMotionBlur() => motionBlur;
    public DepthOfField GetDepthOfField() => depthOfField;
}
