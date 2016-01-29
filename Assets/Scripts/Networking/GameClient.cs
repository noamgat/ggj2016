using UnityEngine;
using System.Collections;
using WebSocketSharp;
using System.Collections.Generic;
using System.Linq;
using System;

public class GameClient : IGameClient {

	private string url;
	private WebSocket webSocket;

	public GameClient(string url) {
		this.url = url;
	}
		
	void WebSocket_OnError (object sender, ErrorEventArgs e)
	{
		Debug.Log("Websocket error : " + e.Message);
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
		case "load":
			HandleServerLoadMessage (dict ["data"]);
			break;
		case "fill":
			HandleServerFillMessage (dict ["data"]);
			break;
		case "win":
			HandleServerWinMessage ();
			break;
		}
	}

	void HandleServerLoadMessage (object obj)
	{
		IDictionary<string, object> data = (IDictionary<string, object>)obj;
		int playerID = Convert.ToInt32(data ["player_id"]);
		data = (IDictionary<string, object>)data ["pattern"];
		PatternModel pattern = new PatternModel ();
		List<object> points = (List<object>)data ["points"];
		List<object> edges = (List<object>)data ["edges"];
		pattern.points = points.ConvertAll<Vector2>(ParseVector2).ToArray();
		pattern.edges = edges.ConvertAll<int[]>(ParseEdge);
		onLoaded.Invoke (pattern, playerID);

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
		int playerID = (int)data ["player_id"];
		int edgeID = (int)data ["edge_id"];
		onEdgeFilled.Invoke(playerID, edgeID);
	}

	void HandleServerWinMessage ()
	{
		onLevelWon.Invoke ();
	}

	private byte[] CreateMessage(string action, IDictionary<string, object> data) {
		IDictionary<string, object> messageDict = new Dictionary<string, object> ();
		messageDict ["action"] = action;
		if (data != null) {
			messageDict ["data"] = data;
		}
		string jsonString = MiniJSON.Json.Serialize (messageDict);
		return System.Text.Encoding.UTF8.GetBytes(jsonString);
	}

	#region IGameClient implementation
	public event System.Action<PatternModel, int> onLoaded;
	public event System.Action<int, int> onEdgeFilled;
	public event System.Action onLevelWon;

	public void NotifyFilledEdge (int edgeID)
	{
		webSocket.Send(CreateMessage("fill", new Dictionary<string, object>() {
			{"edge_id", edgeID}
		}));
	}

	public void Load ()
	{
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
		webSocket.Connect ();
	}
	#endregion
}
