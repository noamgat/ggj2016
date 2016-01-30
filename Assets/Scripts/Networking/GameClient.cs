using UnityEngine;
using System.Collections;
using WebSocketSharp;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading;

public class GameClient : IGameClient, IDisposable {
	


	public event System.Action<int, int> onEdgeFilled;
	public event System.Action onLevelWon;
	public event Action<int> onConnected;
	public event Action<string> onClientError;
	public event Action<int> onNumberOfPlayersChanged;
	public event Action<PatternModel> onLevelStarted;
	public event Action onLevelLost;
	public event Action onGameStarted;
	public event Action onGameCompleted;


	private string url;
	private WebSocket webSocket;

	public GameClient(string url) {
		this.url = url;
	}
		
	void WebSocket_OnError (object sender, ErrorEventArgs e)
	{
		Debug.Log("Websocket error : " + e.Message);
		onClientError.Invoke (e.Message);
	}

	void WebSocket_OnClose (object sender, CloseEventArgs e)
	{
		Debug.Log ("Websocket close : " + e.Reason);
	}

	void WebSocket_OnOpen (object sender, System.EventArgs e)
	{
		Debug.Log ("Websocket open");
	}

	void WebSocket_OnMessage (object sender, MessageEventArgs e)
	{
		Debug.Log ("Websocket message: " + e.Data);
		object json = MiniJSON.Json.Deserialize (e.Data);
		HandleServerObjectReceived (json);
	}

	void HandleServerObjectReceived (object json)
	{
		IDictionary<string, object> dict = (IDictionary<string, object>)json;
		string action = (string)dict ["action"];
		switch (action) {
		case "connect":
			HandleServerConnectMessage (dict ["data"]);
			break;
		case "num_players_changed":
			HandleServerNumPlayersChangedMessage (dict ["data"]);
			break;
		case "start_game":
			HandleGameStartedMessage ();
			break;
		case "start_level":
			HandleLevelStartMessage (dict ["data"]);
			break;
		case "fill":
			HandleServerFillMessage (dict ["data"]);
			break;
		case "win_level":
			HandleServerWinLevelMessage ();
			break;
		case "lose_level":
			HandleServerLoseLevelMessage ();
			break;
		case "complete":
			HandleServerCompleteGameMessage ();
			break;
		}
	}

	void HandleServerConnectMessage (object obj)
	{
		IDictionary<string, object> data = (IDictionary<string, object>)obj;
		int playerID = Convert.ToInt32(data ["player_id"]);
		onConnected.Invoke (playerID);
	}

	void HandleServerNumPlayersChangedMessage (object obj)
	{
		IDictionary<string, object> data = (IDictionary<string, object>)obj;
		int numPlayers = Convert.ToInt32(data ["num_players"]);
		onNumberOfPlayersChanged.Invoke (numPlayers);
	}

	void HandleLevelStartMessage (object obj)
	{
		IDictionary<string, object> data = (IDictionary<string, object>)obj;
		data = (IDictionary<string, object>)data ["pattern"];
		PatternModel pattern = new PatternModel ();
		List<object> points = (List<object>)data ["points"];
		List<object> edges = (List<object>)data ["edges"];
		pattern.points = points.ConvertAll<Vector2>(ParseVector2).ToArray();
		pattern.edges = edges.ConvertAll<int[]>(ParseEdge);
		onLevelStarted.Invoke (pattern);
	}

	private static Vector2 ParseVector2(object obj) {
		List<object> floats = (List<object>)obj;
		return new Vector2 (Convert.ToSingle (floats [0]), Convert.ToSingle (floats [1]));
	}

	private static int[] ParseEdge(object obj) {
		List<object> ints = (List<object>)obj;
		return new int[] { Convert.ToInt32 (ints [0]), Convert.ToInt32(ints [1]) };
	}

	void HandleServerFillMessage (object obj)
	{
		IDictionary<string, object> data = (IDictionary<string, object>)obj;
		int playerID = Convert.ToInt32(data ["player_id"]);
		int edgeID = Convert.ToInt32(data ["edge_id"]);
		onEdgeFilled.Invoke(playerID, edgeID);
	}

	void HandleGameStartedMessage ()
	{
		onGameStarted.Invoke ();
	}


	void HandleServerWinLevelMessage ()
	{
		onLevelWon.Invoke ();
	}

	void HandleServerLoseLevelMessage ()
	{
		onLevelLost.Invoke ();
	}

	void HandleServerCompleteGameMessage ()
	{
		onGameCompleted.Invoke ();
	}

	private void SendMessage(string action, IDictionary<string, object> data) {
		IDictionary<string, object> messageDict = new Dictionary<string, object> ();
		messageDict ["action"] = action;
		if (data != null) {
			messageDict ["data"] = data;
		}
		string jsonString = MiniJSON.Json.Serialize (messageDict);
		byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
		//new Thread (SendBytes).Start (bytes);
		SendBytes(bytes);
	}

	private void SendBytes(object bytesObj) {
		byte[] bytes = (byte[])bytesObj;
		webSocket.Send (bytes);
	}

	public void RequestStartGame ()
	{
		SendMessage ("start", null);
	}

	public void NotifyFilledEdge (int edgeID)
	{
		SendMessage("fill", new Dictionary<string, object>() {
			{"edge_id", edgeID}
		});
	}
	public void Connect ()
	{
		CloseWebSocketIfOpen ();
		webSocket = new WebSocket (url);
		webSocket.OnOpen += WebSocket_OnOpen;
		webSocket.OnMessage += WebSocket_OnMessage;
		webSocket.OnClose += WebSocket_OnClose;
		webSocket.OnError += WebSocket_OnError;

		// To change the logging level.
		webSocket.Log.Level = LogLevel.Trace;

		// To change the wait time for the response to the Ping or Close.
		webSocket.WaitTime = System.TimeSpan.FromSeconds (10);

		Debug.Log ("About to connect");
		new Thread (webSocket.Connect).Start();
		Debug.Log ("Sent connection request");
	}

	void CloseWebSocketIfOpen ()
	{
		if (webSocket != null) {
			webSocket.Close ();
			webSocket = null;
		}
	}

	public void Dispose() {
		CloseWebSocketIfOpen ();
	}

	public void RequestStartLevel ()
	{
		SendMessage ("start_level", null);
	}
}
