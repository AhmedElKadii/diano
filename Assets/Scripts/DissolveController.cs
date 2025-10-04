using UnityEngine;
using System.Collections;

public class DissolveController : MonoBehaviour
{
    [Header("Material Settings")]
    [SerializeField] private Material dissolveMaterial;
    [SerializeField] private Texture2D baseTexture;
    [SerializeField] private GameObject targetObject;
    
    [Header("Dissolve Effect Settings")]
    [SerializeField] private Color edgeColor = Color.yellow;
    
    private float currentDissolve = 0f;
    private Material materialInstance;
    private Renderer objectRenderer;
    
    private static readonly int DissolveProperty = Shader.PropertyToID("_Dissolve");
    private static readonly int BaseMapProperty = Shader.PropertyToID("_BaseMap");
    private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
    private static readonly int NormalMapProperty = Shader.PropertyToID("_NormalMap");
    private static readonly int SpecularMapProperty = Shader.PropertyToID("_RGB_SpecularMap_A_Smoothness");
    private static readonly int OcclusionMapProperty = Shader.PropertyToID("_OcclusionMap");
    private static readonly int EdgeColorProperty = Shader.PropertyToID("_EdgeColor");
    
    private void Awake()
    {
        // If target object is specified, use it. Otherwise use this gameObject
        GameObject target = targetObject != null ? targetObject : gameObject;
        objectRenderer = target.GetComponent<Renderer>();
        
        if (objectRenderer == null)
        {
            Debug.LogError($"DissolveController on {gameObject.name} - Target object '{target.name}' has no Renderer component!");
            return;
        }
        
        if (dissolveMaterial != null)
        {
            materialInstance = new Material(dissolveMaterial);
            objectRenderer.material = materialInstance;
        }
        else if (objectRenderer.sharedMaterial != null)
        {
            materialInstance = new Material(objectRenderer.sharedMaterial);
            objectRenderer.material = materialInstance;
        }
        else
        {
            Debug.LogError($"DissolveController on {gameObject.name} has no material assigned!");
        }
    }
    
    private void Start()
    {
        if (materialInstance != null)
        {
            // Apply base texture if provided
            if (baseTexture != null)
            {
                materialInstance.SetTexture(BaseMapProperty, baseTexture);
            }
            
            // Apply edge color
            materialInstance.SetColor(EdgeColorProperty, edgeColor);
            
            // Initialize dissolve to 0 (fully visible)
            SetDissolve(0f);
            currentDissolve = materialInstance.GetFloat(DissolveProperty);
        }
    }
    
    public void SetDissolve(float value)
    {
        if (materialInstance != null)
        {
            materialInstance.SetFloat(DissolveProperty, Mathf.Clamp01(value));
        }
    }
    
    public void SetBaseTexture(Texture2D texture)
    {
        if (materialInstance != null && texture != null)
        {
            materialInstance.SetTexture(BaseMapProperty, texture);
        }
    }
    
    public void SetEdgeColor(Color color)
    {
        if (materialInstance != null)
        {
            materialInstance.SetColor(EdgeColorProperty, color);
        }
    }
    
    public void AnimateDissolve(float targetValue, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(DissolveCoroutine(targetValue, duration));
    }
    
    private IEnumerator DissolveCoroutine(float target, float duration)
    {
        float start = materialInstance.GetFloat(DissolveProperty);
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
        if (materialInstance != null)
        {
            return materialInstance.GetFloat(DissolveProperty);
        }
        return 0f;
    }
    
    private void OnDestroy()
    {
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }
}
