using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Desktop : MonoBehaviour
{
	public GameObject cam;
	public GameObject camPos;
	public GameObject weaponCam;
	public PlayerController player;
	public AbilityRoulette abilityRoulette;
	public float speed = 5f;
	public enum MoveState { DESKTOP, WAITING, PLAYER} 
	MoveState moveState = MoveState.WAITING;
	
	private bool coroutineStarted = false;
	private Quaternion targetRotation = Quaternion.Euler(14, 0, 0);
	
	void Start()
	{
		player.canMove = false;
		cam.transform.position = camPos.transform.position;
		cam.transform.rotation = camPos.transform.rotation;
		weaponCam.SetActive(false);
	}
	
	void Update()
	{
		switch (moveState)
		{
			case MoveState.DESKTOP:
				GoToDesktop();
				break;
			case MoveState.PLAYER:
				ResetCam();
				break;
			case MoveState.WAITING:
				if (!coroutineStarted)
				{
					coroutineStarted = true;
					StartCoroutine(WaitBeforeMoving());
				}
				break;
			default:
				break;
		}
	}
	
	IEnumerator WaitBeforeMoving()
	{
		yield return new WaitForSeconds(0.3f);
		moveState = MoveState.PLAYER;
		coroutineStarted = false;
		weaponCam.SetActive(true);
	}
	
	public void SetEnum(int state)
	{
		moveState = (MoveState)state;
		coroutineStarted = false;
	}
	
	public void GoToDesktop()
	{
		player.canMove = false;
		weaponCam.SetActive(false);
		cam.transform.position = Vector3.Lerp(cam.transform.position, camPos.transform.position, 1 - Mathf.Exp(-speed * Time.deltaTime));
		cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, camPos.transform.rotation, 1 - Mathf.Exp(-speed * Time.deltaTime));
		if (!coroutineStarted)
		{
			coroutineStarted = true;
			StartCoroutine(TransitionScenes());
		}
	}

	public IEnumerator TransitionScenes()
	{
		abilityRoulette.GetRandomAbility();
		yield return new WaitForSeconds(1f);
		SceneManager.LoadScene(2);
	}
	
	public void ResetCam()
	{
		cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, Vector3.zero, 1 - Mathf.Exp(-speed * Time.deltaTime));
		cam.transform.localRotation = Quaternion.Slerp(cam.transform.localRotation, targetRotation, 1 - Mathf.Exp(-speed * Time.deltaTime));
		
		if (Vector3.Distance(cam.transform.localPosition, Vector3.zero) < 0.01f && 
		    Quaternion.Angle(cam.transform.localRotation, targetRotation) < 1f)
		{
			if (!player.canMove)
			{
				SyncPlayerRotation();
				player.canMove = true;
			}
		}
	}
	
	void SyncPlayerRotation()
	{
		Vector3 currentCamRot = cam.transform.localEulerAngles;
		Vector3 currentPlayerRot = player.transform.localEulerAngles;
		
		player.xRotation = currentCamRot.x;
		if (player.xRotation > 180f) player.xRotation -= 360f;
		
		player.yRotation = currentPlayerRot.y;
		if (player.yRotation > 180f) player.yRotation -= 360f;
	}
}
