using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
	public static NetworkManager instance;
	public Canvas canvas;
//	public SocketIOComponent socket;
	public InputField playerNameInput;
	public GameObject player;

	void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject);
		}
		DontDestroyOnLoad(gameObject);
	}
	
	// Use this for initialization
	void Start () {
		
	}

	public void JoinGame()
	{
		StartCoroutine(ConnectToServer());
	}

	IEnumerator ConnectToServer()
	{
		yield return new WaitForSeconds(0.5f);
	}

	#region JSONMessageClasses

	[Serializable]
	public class PlayerJSON
	{
		public string name;
		public List<PointJSON> playerSpawnPoints;
		public List<PointJSON> enemySpawnPoints;

		public PlayerJSON(string _name, List<SpawnPoint> _palyerSpawnPoints, List<SpawnPoint> _enemySpawnPoints)
		{
			playerSpawnPoints = new List<PointJSON>();
			enemySpawnPoints = new List<PointJSON>();
			name = _name;
			foreach (SpawnPoint playerSpawnPoint in _palyerSpawnPoints)
			{
				PointJSON pointJson = new PointJSON(playerSpawnPoint);
				playerSpawnPoints.Add(pointJson);
			}

			foreach (SpawnPoint enemySpawnPoint in _enemySpawnPoints)
			{
				PointJSON pointJson = new PointJSON(enemySpawnPoint);
				enemySpawnPoints.Add(pointJson);
			}
		}
	}

	[Serializable]
	public class PointJSON
	{
		public PointJSON(SpawnPoint spawnPoint)
		{
			
		}
	}

	#endregion
}
