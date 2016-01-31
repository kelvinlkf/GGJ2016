using UnityEngine;

public enum GameState {
	None,
	Init,
	WaitingForPlayers,
	WaitingForNextRound,
	PlayingRound,
	GameOver,
	RestartRound
}

public class Game : MonoBehaviour
{
	public static int currentTurn = 0;
	public static int currentRound = 0;
	public static float currentTime = 0;
	public static int playerCount = 0;
	public static int totalTurns = 0;
	public static float nextRoundCountdown = 0;

	public static GameState gameState;
}
