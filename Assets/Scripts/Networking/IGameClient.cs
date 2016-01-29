using System;
using System.Collections.Generic;
using UnityEngine;

public class PatternModel {
	public Vector2[] points;
	public List<int[]> edges;
}

public interface IGameClient
{
	//Load finished successfully
	event System.Action<PatternModel, int> onLoaded;
	//PlayerID filled EdgeID
	event System.Action<int, int> onEdgeFilled;
	//Level won
	event System.Action onLevelWon;

	void NotifyFilledEdge (int edgeID);
	void Load();
}


