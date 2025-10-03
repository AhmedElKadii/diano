using UnityEngine;

public class Entity : MonoBehaviour
{
	public float health;

	public void Hit(float damage)
	{
		Debug.Log($"{gameObject.name} was hit for {damage} damage.");

		health -= damage;
		if (health <= 0f)
		{
			Destroy(gameObject);
		}
	}
}
