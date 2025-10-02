using UnityEngine;

public class Bullet : MonoBehaviour
{
	float speed = 2f;
	public float damage;

    void Update()
    {
		transform.Translate(Vector3.forward * damage * speed * Time.deltaTime);
    }

	void OnCollisionEnter(Collision collision)
	{
		collision.gameObject.GetComponent<Entity>()?.Hit(damage);
		Destroy(gameObject);
	}
}
