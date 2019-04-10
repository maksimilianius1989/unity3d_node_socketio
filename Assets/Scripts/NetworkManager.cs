using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

	#region Commands
	
	IEnumerator ConnectToServer()
	{
		yield return new WaitForSeconds(0.5f);
		
		socket.Emit("player connect");

		yield return new WaitForSeconds(1f);

		string playerName = playerNameInput.text;
		List<SpawnPoint> playerSpawnPoints = GetComponent<PlayerSpawner>().playerSpawnPoints;
		List<SpawnPoint> enemySpawnPoints = GetComponent<EnemySpawner>().enemySpawnPoints;
		PlayerJSON playerJson = new PlayerJSON(playerName, playerSpawnPoints, enemySpawnPoints);
		string data = JsonUtility.ToJson(playerJson);

		JSONObject dataJson = new JSONObject(data);
		socket.Emit("play", dataJson);
		canvas.gameObject.SetActive(false);
	}

	public void CommandMove(Vector3 vector3)
	{
		string data = JsonUtility.ToJson(new PositionJSON(vector3));
		socket.Emit("player move", new JSONObject(data));
	}

	public void CommandRotate(Quaternion quaternion)
	{
		string data = JsonUtility.ToJson(new RotationJSON(quaternion));
		socket.Emit("player turn", new JSONObject(data));
	}

	public void CommandShoot()
	{
		print("shoot");
		socket.Emit("player shoot");
	}

	public void CommandHealthChange(GameObject playerFrom, GameObject playerTo, int healthChange, bool isEnemy)
	{
		print("health change cmd");
		HealthChangeJSON healthChangeJson = new HealthChangeJSON(playerTo.name, healthChange, playerFrom.name, isEnemy);
		socket.Emit("health", new JSONObject(JsonUtility.ToJson(healthChangeJson)));
	}
	
	#endregion
	
	#region Listininghealth

	void OnEnemies(SocketIOEvent socketIoEvent)
	{
		string data = socketIoEvent.data.ToString();
		EnemiesJSON enemiesJson = EnemiesJSON.CreateFromJSON(data);
		EnemySpawner es = GetComponent<EnemySpawner>();
		es.SpawnEnemies(enemiesJson);
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
		Transform t = p.transform.Find("Halthbar Canvas");
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
		GameObject p = Instantiate(player, positon, rotation) as GameObject;
		PlayerController pc = p.GetComponent<PlayerController>();
		Transform t = p.transform.Find("Halthbar Canvas");
		Transform t1 = t.transform.Find("Player Name");
		Text playerName = t1.GetComponent<Text>();
		playerName.text = currentUserJSON.name;
		pc.isLocalPlayer = true;
		p.name = currentUserJSON.name;
	}
	
	void OnPlayerMove(SocketIOEvent socketIoEvent)
	{
		string data = socketIoEvent.data.ToString();
		UserJSON userJson = UserJSON.CreateFromJSON(data);
		Vector3 position = new Vector3(userJson.position[0], userJson.position[1], userJson.position[2]);
		if (userJson.name == playerNameInput.text)
		{
			return;
		}

		GameObject p = GameObject.Find(userJson.name) as GameObject;
		if (p != null)
		{
			p.transform.position = position;
		}
	}
	
	void OnPlayerTurn(SocketIOEvent socketIoEvent)
	{
		string data = socketIoEvent.data.ToString();
		UserJSON userJson = UserJSON.CreateFromJSON(data);
		Quaternion rotation = Quaternion.Euler(userJson.rotation[0], userJson.rotation[1], userJson.rotation[2]);
		if (userJson.name == playerNameInput.text)
		{
			return;
		}
		GameObject p = GameObject.Find(userJson.name) as GameObject;
		if (p != null)
		{
			p.transform.rotation = rotation;
		}
	}
	
	void OnPlayerShoot(SocketIOEvent socketIoEvent)
	{
		string data = socketIoEvent.data.ToString();
		ShootJSON shootJson = ShootJSON.CreateFromJSON(data);
		GameObject p = GameObject.Find(shootJson.name);
		PlayerController pc = p.GetComponent<PlayerController>();
		pc.CmdFire();
	}
	
	void OnHealth(SocketIOEvent socketIoEvent)
	{
		print("changing the health");
		var data = socketIoEvent.data.ToString();
		UserHealthJSON userHealthJson = UserHealthJSON.CreateFromJSON(data);
		GameObject p = GameObject.Find(userHealthJson.name);
		Health h = p.GetComponent<Health>();
		h.currentHealth = userHealthJson.health;
		h.OnChangeHealth();
	}
	
	void OnOtherPlayerDisconnected(SocketIOEvent socketIoEvent)
	{
		print("user disconnected");
		string data = socketIoEvent.data.ToString();
		UserJSON userJson = UserJSON.CreateFromJSON(data);
		Destroy(GameObject.Find(userJson.name));
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
		public string[] position;
		public string[] rotation;
	
		public PointJSON(SpawnPoint spawnPoint)
		{
			position = new string[]
			{
				spawnPoint.transform.position.x.ToString(),
				spawnPoint.transform.position.y.ToString(),
				spawnPoint.transform.position.z.ToString()
			};
			
			rotation = new string[]
			{
				spawnPoint.transform.eulerAngles.x.ToString(),
				spawnPoint.transform.eulerAngles.y.ToString(),
				spawnPoint.transform.eulerAngles.z.ToString()
			};	
		}
	}

	[Serializable]
	public class PositionJSON
	{
		public string[] position;

		public PositionJSON(Vector3 _position)
		{
			position = new string[]
			{
				_position.x.ToString(),
				_position.y.ToString(),
				_position.z.ToString()
			};
		}
	}

	[Serializable]
	public class RotationJSON
	{
		public string[] rotation;

		public RotationJSON(Quaternion _rotation)
		{
			rotation = new string[]
			{
				_rotation.eulerAngles.x.ToString(),
				_rotation.eulerAngles.y.ToString(),
				_rotation.eulerAngles.z.ToString()
			};
		}
	}

	[Serializable]
	public class BaseUser
	{
		public string name;
		public string[] position;
		public string[] rotation;
		public int health;
	}
	
	public class UserJSON
	{
		public string name;
		public float[] position;
		public float[] rotation;
		public int health;

		public static UserJSON CreateFromJSON(string data)
		{
			var baseUser = JsonUtility.FromJson<BaseUser>(data);
			
			UserJSON userJson = new UserJSON();
			userJson.name = baseUser.name;
			userJson.health = baseUser.health;
			userJson.position = new float[3];
			userJson.rotation = new float[3];
			for (int i = 0; i < baseUser.position.Length; i++)
			{
				userJson.position[i] = float.Parse(baseUser.position[i]);
			}

			for (int i = 0; i < baseUser.rotation.Length; i++)
			{
				userJson.rotation[i] = float.Parse(baseUser.rotation[i]);
			}
			
			return userJson;
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
		public List<BaseUser> enemies;
		
		public List<UserJSON> enemyPlayers;

		public static EnemiesJSON CreateFromJSON(string data)
		{
			var enimalsJson = JsonUtility.FromJson<EnemiesJSON>(data);
			enimalsJson.enemyPlayers = new List<UserJSON>();

			foreach (BaseUser baseEnemy in enimalsJson.enemies)
			{
				UserJSON someEnemyPlayer = new UserJSON();
				someEnemyPlayer.name = baseEnemy.name;
				someEnemyPlayer.health = baseEnemy.health;
				someEnemyPlayer.position = new float[3];
				someEnemyPlayer.rotation = new float[3];
				for (int i = 0; i < baseEnemy.position.Length; i++)
				{
					someEnemyPlayer.position[i] = float.Parse(baseEnemy.position[i]);
				}

				for (int i = 0; i < baseEnemy.rotation.Length; i++)
				{
					someEnemyPlayer.rotation[i] = float.Parse(baseEnemy.position[i]);
				}
				
				enimalsJson.enemyPlayers.Add(someEnemyPlayer);
			}
			
			return enimalsJson;
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
