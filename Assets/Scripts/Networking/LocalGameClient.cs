using System;
using UnityEngine;
using System.Collections.Generic;

public class LocalGameClient : IGameClient
{
	private HashSet<int> filledEdges;

	public event Action<PatternModel, int> onLoaded;

	public event Action<int, int> onEdgeFilled;

	public event Action onLevelWon;

	public void NotifyFilledEdge (int edgeID)
	{
		if (filledEdges.Add (edgeID)) {
			if (filledEdges.Count == this.patternModel.edges.Count) {
				onLevelWon.Invoke ();
			}
		}
	}
		
	public void Load ()
	{
		filledEdges = new HashSet<int>();
		onLoaded.Invoke (patternModel, 1);
	}

	public LocalGameClient ()
	{
		patternModel = CreatePatternModel ();
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


