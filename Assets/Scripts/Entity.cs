using UnityEngine;
using UnityEngine.Events;

public class Entity : MonoBehaviour
{
	public float health;
	public UnityEvent onDeath;

	public void Hit(float damage)
	{
		Debug.Log($"{gameObject.name} was hit for {damage} damage.");

		health -= damage;
		if (health <= 0)
		{
			onDeath?.Invoke();
		}
	}
}
