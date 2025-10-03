using UnityEngine;

public class Grenade : MonoBehaviour
{
	public float explosionRadius = 5f;
	public float damage;
	public GameObject explosionEffect;
	public Vector3 direction;

    void Start()
    {
		Rigidbody rb = GetComponent<Rigidbody>();
		rb.AddForce(direction * 10f, ForceMode.VelocityChange);
		
		Invoke("Explode", 3f);
    }

	void Explode()
	{
		Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
		foreach (Collider nearbyObject in colliders)
		{
			nearbyObject.GetComponent<Entity>()?.Hit(damage);
		}
		Instantiate(explosionEffect, transform.position, transform.rotation);
		Destroy(gameObject);
	}
}
