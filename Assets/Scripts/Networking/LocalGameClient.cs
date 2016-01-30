using System;
using UnityEngine;
using System.Collections.Generic;

public class LocalGameClient : IGameClient
{
	public LocalGameClient ()
	{
		patternModel = CreatePatternModel ();
	}

	public event Action<int> onConnected;

	public event Action<string> onClientError;

	public event Action<int> onNumberOfPlayersChanged;

	public event Action<PatternModel> onLevelStarted;

	public event Action onLevelLost;

	public event Action onGameCompleted;

	public void Connect ()
	{
		
		onConnected.Invoke(1);
		onNumberOfPlayersChanged.Invoke(2);
	}

	public void RequestStartGame ()
	{
		filledEdges = new HashSet<int>();
		onLevelStarted.Invoke (patternModel);
	}

	private HashSet<int> filledEdges;

	public event Action<PatternModel, int> onLoaded;

	public event Action<int, int> onEdgeFilled;

	public event Action onLevelWon;

	public void NotifyFilledEdge (int edgeID)
	{
		if (filledEdges.Add (edgeID)) {
			if (filledEdges.Count == this.patternModel.edges.Count) {
				onLevelWon.Invoke ();
				onGameCompleted.Invoke ();
			}
		}
	}

	private PatternModel patternModel;
	private PatternModel CreatePatternModel ()
	{
		return new PatternModel () {
			points = new Vector2[] {
				new Vector2(0, 0),
				new Vector2(1, 1)
			},
			edges = new List<int[]>() {
				new int[] { 0, 1 }
			}
		};
	}
}


