using UnityEngine;
using System.Collections;

public class DissolveController : MonoBehaviour
{
    [Header("Material Settings")]
    [SerializeField] private Material dissolveMaterial;
    [SerializeField] private Color baseColor = Color.white;
    
    [Header("Dissolve Settings")]
    [SerializeField] private float dissolveSpeed = 1f;
    [SerializeField] private bool autoDissolve = false;
    
    private float currentDissolve = 0f;
    
    private static readonly int DissolveProperty = Shader.PropertyToID("_Dissolve");
    private static readonly int BaseMapProperty = Shader.PropertyToID("_BaseMap");
    private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
    
    private void Start()
    {
		SetDissolve(0);

        if (dissolveMaterial != null)
        {
            dissolveMaterial.SetColor(BaseColorProperty, baseColor);
            
            currentDissolve = dissolveMaterial.GetFloat(DissolveProperty);
        }
    }
    
    private void Update()
    {
        if (autoDissolve && dissolveMaterial != null)
        {
            currentDissolve += Time.deltaTime * dissolveSpeed;
            currentDissolve = Mathf.Clamp01(currentDissolve);
            SetDissolve(currentDissolve);
        }
    }
    
    public void SetDissolve(float value)
    {
        if (dissolveMaterial != null)
        {
            dissolveMaterial.SetFloat(DissolveProperty, Mathf.Clamp01(value));
        }
    }
    
    public void SetBaseTexture(Texture2D texture)
    {
        if (dissolveMaterial != null && texture != null)
        {
            dissolveMaterial.SetTexture(BaseMapProperty, texture);
        }
    }
    
    public void SetBaseColor(Color color)
    {
        if (dissolveMaterial != null)
        {
            dissolveMaterial.SetColor(BaseColorProperty, color);
        }
    }
    
    public void AnimateDissolve(float targetValue, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(DissolveCoroutine(targetValue, duration));
    }
    
    private IEnumerator DissolveCoroutine(float target, float duration)
    {
        float start = dissolveMaterial.GetFloat(DissolveProperty);
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float value = Mathf.Lerp(start, target, t);
            SetDissolve(value);
            yield return null;
        }
        
        SetDissolve(target);
    }
    
    public float GetDissolve()
    {
        if (dissolveMaterial != null)
        {
            return dissolveMaterial.GetFloat(DissolveProperty);
        }
        return 0f;
    }
}
