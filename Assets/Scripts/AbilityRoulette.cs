using UnityEngine;

public class AbilityRoulette : MonoBehaviour
{
	public void GetRandomAbility()
	{
		GameManager.Instance.maxJumps = 1;
		GameManager.Instance.canDash = false;
		GameManager.Instance.speedBoostMultiplier = 1.0f;

		string[] abilities = { "Double Jump", "Dash", "Speed Boost" };
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
		}
	}
}
