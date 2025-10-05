using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
	public UnityEvent onInteract;

	public void Interact(Transform initiator)
	{
		onInteract?.Invoke();

		Weapon weapon = GetComponent<Weapon>();
		if (weapon != null)
		{
			weapon.Interacted(initiator);
		}
	}
}
