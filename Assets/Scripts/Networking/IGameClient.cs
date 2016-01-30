using System;
using System.Collections.Generic;
using UnityEngine;

public class PatternModel {
	public Vector2[] points;
	public List<int[]> edges;
}

public interface IGameClient
{
	//First call to connect to the server
	void Connect();

	//Successfully connected to server as PlayerID
	event System.Action<int> onConnected;

	//Could not connect or other problem (reason as string)
	event System.Action<string> onClientError;

	//The number of players changed
	event System.Action<int> onNumberOfPlayersChanged;

	//If the game has not started yet, request to start it
	void RequestStartGame ();

	//Level started (will be called for subsequent levels as well)
	event System.Action<PatternModel> onLevelStarted;

	//Tell the server that you filled an edge
	void NotifyFilledEdge (int edgeID);

	//PlayerID filled EdgeID
	event System.Action<int, int> onEdgeFilled;

	//Level won
	event System.Action onLevelWon;

	//Level lost
	event System.Action onLevelLost;

	//All the levels were completed
	event System.Action onGameCompleted;
}


