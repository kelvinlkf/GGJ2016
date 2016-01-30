using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (LineRenderer))]
public class PlayerLogic : NetworkBehaviour
{
	// GAME VARIABLES

	public enum GameState
	{
		None,
		Init,
		WaitingForPlayers,
		WaitingForNextRound,
		PlayingRound,
		GameOver,
		RestartRound
	}

	public Text timer;
	public Text score;

	[SyncVar]
	public GameState gameState;
	[SyncVar]
	int playerScore;

	float nextRoundCountdown = 0;
	public float currentTime;
	int currentTurn;
	int currentRound;
	int totalTurns;
	int totalTurnPerPlayer;
	public int playerCount;

	public int currentPlayer;
	List<NetworkInstanceId> playerIdQueue = new List<NetworkInstanceId>();

	List<int> availableIdArray = new List<int>();



	// PLAYER VARIABLES

	private LineRenderer _lineRenderer;
	private SpringJoint _springJoint;
	private MeshRenderer _meshRenderer;
	private Material _material;
	public Color mColor;

	private Transform _transform;
	private SphereCollider _sphereCollider;
	private Transform _parentObj;
	private SphereCollider _parentsphereCollider;

	public float Hvalue;
	private int mPlayerId;
	public Vector3 posRange;





	void Awake()
	{
		timer = timer.GetComponent<Text>();
		score = score.GetComponent<Text>();

		_transform = GetComponent<Transform>();
		SetPosition();
		_sphereCollider = GetComponent<SphereCollider>();
		_lineRenderer = GetComponent<LineRenderer>();
		_springJoint = GetComponent<SpringJoint>();
		_meshRenderer = GetComponent<MeshRenderer>();
		_material = _meshRenderer.material;

		_lineRenderer.SetVertexCount(2);
	}

	[Command]
	void CmdDoSomething ()
	{
		Debug.Log("do something");
	}

	void OnEnable()
	{
	}

	void Start () 
	{
		if (GetComponent<NetworkIdentity>().isServer)
		{
			Debug.Log("LOGGED IN AS SERVER");
			SetGameState(GameState.Init);
		}
		else
		{
			Debug.Log("Not logged in as server");
		}

		CenterHub = GameObject.FindGameObjectWithTag("MainGlobe").GetComponent<Transform>();
		UpdateColor();

		CmdDoSomething();
	}

	void Update () 
	{
		SetLine();

		UpdateGame();
	}

	void UpdateGame ()
	{
		Debug.Log("Update : " + gameState);

		if (playerCount < 2)
		SetGameState(GameState.WaitingForPlayers);

		switch (gameState)
		{
			case GameState.WaitingForPlayers:

			if (playerCount > 1)
			SetGameState(GameState.WaitingForNextRound);

			break;

			case GameState.WaitingForNextRound:

			nextRoundCountdown -= Time.deltaTime;

			if (nextRoundCountdown <= 0f)
			SetGameState(GameState.RestartRound);

			break;

			case GameState.PlayingRound:

			currentTime -= Time.deltaTime;
			if (currentTime <= 0)
			{
				currentTime = 5f;
				if (playerScore <= 0)
				{
					playerScore = 0;
				}
				else
				{
					playerScore -= 5;
				}
			}

			timer.text = "" + currentTime.ToString("f0");
			score.text = "" + playerScore.ToString("f0");

			break;
		}
	}

	void SetGameState(GameState newState)
	{
		Debug.Log("SetGameState : " + newState);

		gameState = newState;

		switch (gameState)
		{
			case GameState.Init:

			// This is when the the first player (host) is created,
			// so setup the server variables.

			currentTurn = 0;
			currentRound = 0;
			SetGameState(GameState.WaitingForPlayers);

				break;

			case GameState.WaitingForPlayers:

				// This only happens when there's only one player,
				// either on init or all other players left.

				// In this state, the game doesn't start/countdown
				// until another player joins.

				break;

			case GameState.WaitingForNextRound:

				// This is when there are enough players, and we
				// wait for a while before starting next round

				// TODO display the countdown to let everyone get ready
				nextRoundCountdown = 2f;

				break;

			case GameState.RestartRound:

				currentTurn = 0;
				currentRound++;

				// Set up total turn for each player count
				if (playerCount < 5)
				totalTurnPerPlayer = Random.Range(5, 10);
				else if (playerCount < 10)
				totalTurnPerPlayer = Random.Range(3, 5);
				else if (playerCount >= 10)
				totalTurnPerPlayer = Random.Range(2, 3);

				GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
				totalTurns = playerCount * totalTurnPerPlayer;

				CreateAvailableId(totalTurns);

				//! Assign available id to each player

				Debug.Log("players : " + players.Length);

				for (int i = 0; i < playerCount; i++)
				{
					players[i].GetComponent<PlayerScript>().ClearNumbers();

					for (int j = 0; j < totalTurnPerPlayer; j++)
					{
						int number = j+(i*totalTurnPerPlayer);
						players[i].GetComponent<PlayerScript>().AddNumber(availableIdArray[number]);

						Debug.Log("assigned number : " + i + " , " + availableIdArray[number]);
					}

					players[i].GetComponent<PlayerScript>().SortNumbers();
				}

				SetGameState(GameState.PlayingRound);

				break;

			case GameState.PlayingRound:

				// Init variables before starting the round

				currentTime = 5f;

				break;

			case GameState.GameOver:

				// TODO

				break;

			default:

				Debug.Log("State not found : " + newState);

				break;
		}
	}

	void CreateAvailableId(int length)
	{
		availableIdArray = new List<int>();
		//! Assign 0 - length
		for (int i = 0; i < length; i++)
		{
			availableIdArray.Add(i);
		}

		//! Random swtich the index
		for (int i = length-1; i > 0; i--)
		{
			// Randomize a number between 0 and i (so that the range decreases each time)
			int rnd = Random.Range(0,i);

			// Save the value of the current i, otherwise it'll overright when we swap the values
			int temp = availableIdArray[i];

			// Swap the new and old values
			availableIdArray[i] = availableIdArray[rnd];
			availableIdArray[rnd] = temp;
		}
	}

	public Transform CenterHub
	{
		get
		{
			return _parentObj;
		}
		set
		{
			_parentObj = value;
			_parentsphereCollider = _parentObj.GetComponent<SphereCollider>();
			_springJoint.connectedBody = _parentObj.GetComponent<Rigidbody>();
		}
	}

	public void SetPosition()
	{
		_transform.position = Random.insideUnitSphere * Random.Range(6f, 11f);
	}

	public void SetLine()
	{
		_lineRenderer.SetPosition(0, _transform.position + ((_parentObj.position - _transform.position).normalized *_sphereCollider.radius));
		_lineRenderer.SetPosition(1, _parentObj.position + ((_transform.position - _parentObj.position).normalized * _parentsphereCollider.radius)* _parentObj.localScale.x);
	}

	public void SetID(int _value)
	{
		mPlayerId = _value;
	}

	public void UpdateColor()
	{
		Vector3 direction = _parentObj.position - _transform.position;
		float angle = Vector3.Angle(Vector3.up, _transform.position);
		//Quaternion qua = Quaternion.Euler(new Vector3(0f,0f,direction));
		//float angle = Vector3.Angle(_parentObj.position, _transform.position);
		//Debug.Log(angle);
		HSBColor newColor = new HSBColor(Mathf.Sin (angle + Mathf.PI / 2 ), 1f, 1f);
		_material.color = newColor.ToColor();
	}



	void OnStartClient ()
	{
		Debug.Log("OnStartClient");
	}

	void OnServerConnect (NetworkConnection conn)
	{
		Debug.Log("OnServerConnect");
	}

	void OnServerDisconnect ()
	{
		Debug.Log("OnServerDisconnect");
	}

	void OnConnectedToServer ()
	{
		Debug.Log("OnConnectedToServer");
	}

	void OnPlayerConnected(NetworkPlayer player)
	{
		Debug.Log("Player connect");
	}

	void OnPlayerDisconnected(NetworkPlayer player)
	{
		Debug.Log("Player dc");

		// Cleanup stuff, from http://docs.unity3d.com/ScriptReference/MonoBehaviour.OnPlayerDisconnected.html
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
	}
}
