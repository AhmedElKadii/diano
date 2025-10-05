using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;
using System;

public class Leaderboard : MonoBehaviour
{
	public PlayerEntry[] playerEntries;
	
	public enum RequestType
	{
		GET = 0,
		POST = 1
	}
	
	[Serializable]
	public class PlayerEntry
	{
		public string username;
		public int score;
		public long time;
		
		public PlayerEntry(string username, int score, long time)
		{
			this.username = username;
			this.score = score;
			this.time = time;
		}
	}
	
	[Serializable]
	private class PlayerEntryArray
	{
		public PlayerEntry[] entries;
	}
	
	public void AddPlayerEntry(PlayerEntry entry)
	{
		StartCoroutine(AddPlayerEntryCoroutine(entry));
	}
	
	public IEnumerator AddPlayerEntryCoroutine(PlayerEntry entry)
	{
		var postRequest = CreateRequest("https://api.kodeflowstudios.com/diano/leaderboard/add", RequestType.POST, entry);
		yield return postRequest.SendWebRequest();
		
		if (postRequest.result == UnityWebRequest.Result.Success)
		{
			Debug.Log("Posted successfully: " + entry.username + " " + entry.score + " " + entry.time);
			Debug.Log("Server response: " + postRequest.downloadHandler.text);
		}
		else
		{
			Debug.LogError("Error posting: " + postRequest.error);
		}
	}
	
	public void DisplayLeaderboard()
	{
		StartCoroutine(GetPlayerEntries());
	}
	
	public IEnumerator GetPlayerEntries()
	{
		var getRequest = CreateRequest("https://api.kodeflowstudios.com/diano/leaderboard/fetch", RequestType.GET);
		yield return getRequest.SendWebRequest();
		
		if (getRequest.result == UnityWebRequest.Result.Success)
		{
			string jsonArray = "{\"entries\":" + getRequest.downloadHandler.text + "}";
			var wrapper = JsonUtility.FromJson<PlayerEntryArray>(jsonArray);
			playerEntries = wrapper.entries;
			
			Debug.Log("Fetched " + playerEntries.Length + " entries");
			foreach (var entry in playerEntries)
			{
				Debug.Log($"{entry.username}: {entry.score} (Time: {entry.time}ms)");
			}
		}
		else
		{
			Debug.LogError("Error fetching leaderboard: " + getRequest.error);
		}
	}
	
	private UnityWebRequest CreateRequest(string path, RequestType type, object data = null) 
	{
		var request = new UnityWebRequest(path, type.ToString());
		if (data != null) 
		{
			var bodyRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
			request.uploadHandler = new UploadHandlerRaw(bodyRaw);
		}
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		return request;
	}
}
