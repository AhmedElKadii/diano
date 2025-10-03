using UnityEngine;

public class Bullet : MonoBehaviour
{
	float speed = 5f;
	public float damage;
	public Vector3 direction;

	void Start()
	{
		Destroy(gameObject, 15f);
	}

    void Update()
    {
		transform.position += direction * speed * damage * Time.deltaTime;
    }

	void OnTriggerEnter(Collider other)
    {
		if (!other.CompareTag("Gun"))
		{
			Debug.Log("Bullet hit " + other.gameObject.name);
			other.gameObject.GetComponent<Entity>()?.Hit(damage);
			Destroy(gameObject);
		}
    }
}
