using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Entity : MonoBehaviour
{
	public float health;
	public UnityEvent onDeath;
	public DissolveController dc;
	
	[Header("Hit Flash Settings")]
	public float flashDuration = 0.1f;
	public Color flashColor = Color.red;
	
	private Color originalColor;
	private Coroutine flashCoroutine;
	private bool isFlashing = false;
	
	public void Hit(float damage)
	{
		Debug.Log($"{gameObject.name} was hit for {damage} damage.");
		health -= damage;
		
		FlashRed();
		
		if (health <= 0)
		{
			onDeath?.Invoke();
		}
	}
	
	void FlashRed()
	{
		if (dc == null) return;
		
		if (flashCoroutine != null)
		{
			StopCoroutine(flashCoroutine);
			if (isFlashing)
			{
				dc.SetColor(originalColor);
			}
		}
		
		flashCoroutine = StartCoroutine(FlashCoroutine());
	}
	
	IEnumerator FlashCoroutine()
	{
		originalColor = dc.GetColor();
		isFlashing = true;
		
		dc.SetColor(flashColor);
		
		yield return new WaitForSeconds(flashDuration);
		
		dc.SetColor(originalColor);
		
		isFlashing = false;
		flashCoroutine = null;
	}
}
