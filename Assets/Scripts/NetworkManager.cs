using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using SocketIO;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
	public static NetworkManager instance;
	public Canvas canvas;
	public SocketIOComponent socket;
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
		socket.On("enemies", OnEnemies);
		socket.On("other player connected", OnOtherPlayerConnected);
		socket.On("play", OnPlay);
		socket.On("player move", OnPlayerMove);
		socket.On("player turn", OnPlayerTurn);
		socket.On("player shoot", OnPlayerShoot);
		socket.On("health", OnHealth);
		socket.On("other player disconnected", OnOtherPlayerDisconnected);
	}

	public void JoinGame()
	{
		StartCoroutine(ConnectToServer());
	}

	IEnumerator ConnectToServer()
	{
		yield return new WaitForSeconds(0.5f);
		
		socket.Emit("player connect");

		yield return new WaitForSeconds(1f);

		string playerName = playerNameInput.text;
		List<SpawnPoint> playerSpawnPoints = GetComponent<PlayerSpawner>().playerSpawnPoints;
		List<SpawnPoint> enemySpawnPoints = GetComponent<EnemySpawner>().enemySpawnPoints;
		PlayerJSON playerJson = new PlayerJSON(playerName, playerSpawnPoints, enemySpawnPoints);
		string data = JsonUtility.ToJson(playerName);
		socket.Emit("play", new JSONObject(data));
		canvas.gameObject.SetActive(false);
	}
	
	#region Listining

	void OnEnemies(SocketIOEvent socketIoEvent)
	{
		
	}
	
	void OnOtherPlayerConnected(SocketIOEvent socketIoEvent)
	{
		print("Someone else joined");
		string data = socketIoEvent.data.ToString();
		UserJSON userJson = UserJSON.CreateFromJSON(data);
		Vector3 position = new Vector3(userJson.position[0], userJson.position[1], userJson.position[2]);
		Quaternion rotation = Quaternion.Euler(userJson.rotation[0], userJson.rotation[1], userJson.rotation[2]);
		GameObject o = GameObject.Find(userJson.name) as GameObject;
		if (o != null)
		{
			return;
		}
		GameObject p = Instantiate(player, position, rotation) as GameObject;
		PlayerController pc = p.GetComponent<PlayerController>();
		Transform t = p.transform.Find("Healthbar Canvas");
		Transform t1 = t.transform.Find("Player Name");
		Text playerName = t1.GetComponent<Text>();
		playerName.text = userJson.name;
		pc.isLocalPlayer = false;
		p.name = userJson.name;
		Health h = p.GetComponent<Health>();
		h.currentHealth = userJson.health;
		h.OnChangeHealth();
	}
	
	void OnPlay(SocketIOEvent socketIoEvent)
	{
		print("you joined");
		string data = socketIoEvent.data.ToString();
		UserJSON currentUserJSON = UserJSON.CreateFromJSON(data);
		Vector3 positon = new Vector3(currentUserJSON.position[0], currentUserJSON.position[1], currentUserJSON.position[2]);
		Quaternion rotation = Quaternion.Euler(currentUserJSON.rotation[0], currentUserJSON.rotation[1], currentUserJSON.rotation[2]);
		GameObject p = GameObject.Find(currentUserJSON.name) as GameObject;
		PlayerController pc = p.GetComponent<PlayerController>();
		Transform t = p.transform.Find("Healthbar Canvas");
		Transform t1 = t.transform.Find("Player Name");
		Text playerName = t1.GetComponent<Text>();
		playerName.text = currentUserJSON.name;
		pc.isLocalPlayer = false;
		p.name = currentUserJSON.name;
	}
	
	void OnPlayerMove(SocketIOEvent socketIoEvent)
	{
		
	}
	
	void OnPlayerTurn(SocketIOEvent socketIoEvent)
	{
		
	}
	
	void OnPlayerShoot(SocketIOEvent socketIoEvent)
	{
		
	}
	
	void OnHealth(SocketIOEvent socketIoEvent)
	{
		
	}
	
	void OnOtherPlayerDisconnected(SocketIOEvent socketIoEvent)
	{
		
	}
	
	#endregion

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
		public float[] position;
		public float[] rotation;
	
		public PointJSON(SpawnPoint spawnPoint)
		{
			position = new float[]
			{
				spawnPoint.transform.position.x,
				spawnPoint.transform.position.y,
				spawnPoint.transform.position.z
			};
			
			rotation = new float[]
			{
				spawnPoint.transform.eulerAngles.x,
				spawnPoint.transform.eulerAngles.y,
				spawnPoint.transform.eulerAngles.z
			};	
		}
	}

	[Serializable]
	public class PositionJSON
	{
		public float[] positoin;

		public PositionJSON(Vector3 _position)
		{
			positoin = new float[]
			{
				_position.x,
				_position.y,
				_position.z
			};
		}
	}

	[Serializable]
	public class RotationJSON
	{
		public float[] rotation;

		public RotationJSON(Quaternion _rotation)
		{
			rotation = new float[]
			{
				_rotation.eulerAngles.x,
				_rotation.eulerAngles.y,
				_rotation.eulerAngles.z
			};
		}
	}

	[Serializable]
	public class UserJSON
	{
		public string name;
		public float[] position;
		public float[] rotation;
		public int health;

		public static UserJSON CreateFromJSON(string data)
		{
			return JsonUtility.FromJson<UserJSON>(data);
		}
	}

	[Serializable]
	public class HealthChangeJSON
	{
		public string name;
		public int healthChange;
		public string from;
		public bool isEnemy;

		public HealthChangeJSON(string _name, int _healthChange, string _from, bool _isEnemy)
		{
			name = _name;
			healthChange = _healthChange;
			from = _from;
			isEnemy = _isEnemy;
		}
	}

	[Serializable]
	public class EnemiesJSON
	{
		public List<UserJSON> enemies;

		public static EnemiesJSON CreateFromJSON(string data)
		{
			return JsonUtility.FromJson<EnemiesJSON>(data);
		}
	}

	[Serializable]
	public class ShootJSON
	{
		public string name;

		public static ShootJSON CreateFromJSON(string data)
		{
			return JsonUtility.FromJson<ShootJSON>(data);
		}
	}

	[Serializable]
	public class UserHealthJSON
	{
		public string name;
		public int health;

		public static UserHealthJSON CreateFromJSON(string data)
		{
			return JsonUtility.FromJson<UserHealthJSON>(data);
		}
	}
	
	#endregion
}
