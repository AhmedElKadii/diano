using UnityEngine;

public class AbilityRoulette : MonoBehaviour
{
	public string GetRandomAbility()
	{
		GameManager.Instance.maxJumps = 1;
		GameManager.Instance.canDash = false;
		GameManager.Instance.speedBoostMultiplier = 1.0f;

		string[] abilities = { "Double Jump", "Dash", "Speed Boost", "Health Boost", "Stamina Boost" };
		int randomIndex = Random.Range(0, abilities.Length);
		string selectedAbility = abilities[randomIndex];

		switch (selectedAbility)
		{
			case "Double Jump":
				GameManager.Instance.maxJumps = 2;
				Debug.Log("Ability Unlocked: Double Jump");
				break;
			case "Dash":
				GameManager.Instance.canDash = true;
				Debug.Log("Ability Unlocked: Dash");
				break;
			case "Speed Boost":
				GameManager.Instance.speedBoostMultiplier = 1.5f;
				Debug.Log("Ability Unlocked: Speed Boost");
				break;
			case "Health Boost":
				GameManager.Instance.maxHealth += 100f;
				Debug.Log("Ability Unlocked: Health Boost");
				break;
			case "Stamina Boost":
				GameManager.Instance.maxStamina += 100f;
				Debug.Log("Ability Unlocked: Stamina Boost");
				break;
		}

		return selectedAbility;
	}
}
