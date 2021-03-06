﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Holoville.HOTween;

public class PatternProxy : MonoBehaviour {
    
    public Image ProgBar;

    public Vector2[] TargetPoints;
    public int[] TargetSegments;

    public Segment SegmentPF;
    public Vertex VertexPF;

    private List<Vertex> _verteces = new List<Vertex>();
    private List<Segment> _segments = new List<Segment>();

	private Vertex _lastContact;
	private Dictionary<Vertex, float> _lastContactTimes;
	public float segmentTouchTimeThreshold = 0.2f;

    internal int myID;

    public float SegmentLifetime;
    public List<int> Players;
    

    internal bool _patternNedsUpdate = false;
    internal PatternModel _patternModel;
    
    private bool _isInGame = false;

    public GameManager GameManagerInst;

    public bool useFakePattern;

    public Candle[] Candles;

    private Tweener _barTween;

    internal void StartRound() {
        _isInGame = true;
        ShowInitialSplash(Color.white);

        ProgBar.fillAmount = 1;
        if (_barTween != null) _barTween.Kill();

        _barTween = HOTween.To(ProgBar, 30, new TweenParms().Prop("fillAmount", 0).Ease(EaseType.Linear));
		_lastContactTimes = new Dictionary<Vertex, float> ();
		_lastContact = null;
    }

    internal void EndRound(bool win) {
        _isInGame = false;
        ShowInitialSplash((win)? Color.green: Color.red);
    }
        

    internal void EdgeFilledByPlayer(int playerID, int edgeId) {
        if (playerID != myID) {
            _segments[edgeId].AddPlayer(Players.IndexOf(playerID));
        }
        
    }

   
    

    private void AdjustCollidersSize() {
        foreach (Vertex verA in _verteces) {
            foreach (Vertex verB in _verteces) {
                if (verA != verB) {
                    float dist = (verA.location - verB.location).magnitude;
                    if (dist < 0.09f) {
                        verA.SetColliderSize(dist * 0.35f);
                        verB.SetColliderSize(dist * 0.35f);
                    }
                }
            }
        }
    }

    private void CreatePattern(PatternModel patternModel) {
        for (int i = 0; i < patternModel.points.Length; i++) {
            _verteces.Add(Instantiate(VertexPF));
            _verteces[_verteces.Count - 1].location = patternModel.points[i];
            _verteces[_verteces.Count - 1].Place();
        }


        for (int i = 0; i < patternModel.edges.Count; i++) {
            Segment seg = Instantiate(SegmentPF);

            _segments.Add(seg);
            seg.Init(Players.Count, SegmentLifetime);
            seg.VertexA = _verteces[patternModel.edges[i][0]];
            seg.VertexB = _verteces[patternModel.edges[i][1]];
            seg.Place();
        }
    }

    internal void UpdatePattern(PatternModel patternModel) {
        _patternModel = patternModel;
        _patternNedsUpdate = true;
    }

    public void CreatePattern() {

        //stub place

        for (int i = 0; i < TargetPoints.Length; i++) {
            _verteces.Add(Instantiate(VertexPF));
            _verteces[_verteces.Count - 1].location = TargetPoints[i];
            _verteces[_verteces.Count - 1].Place();
        }


        for (int i = 0; i < TargetSegments.Length / 3; i++) {

            int numOfDivisions = TargetSegments[i * 3 + 2];

            Vertex absFrom = _verteces[TargetSegments[i * 3]];
            Vertex absTo = _verteces[TargetSegments[i * 3 + 1]];
            Vertex from;
            Vertex to = absFrom;

            for (int j = 0; j < numOfDivisions; j++) {

                from = to;

                if (j < numOfDivisions - 1) {

                    to = Instantiate(VertexPF);

                    _verteces.Add(to);

                    Vector2 trg = (absTo.location - absFrom.location);
                    trg = (trg / (float)numOfDivisions);
                    trg = trg * (float)(j + 1);

                    to.location = absFrom.location + trg;
                    to.Place();

                    
                } else {
                    to = absTo;
                }
                
                Segment seg = Instantiate(SegmentPF);

                _segments.Add(seg);
                seg.Init(Players.Count, SegmentLifetime);
                seg.VertexA = from;
                seg.VertexB = to;
                seg.Place();
                
            }

            
        }

            
    }

	private float GetLastVertexTouchTime(Vertex v) {
		float val;
		_lastContactTimes.TryGetValue (v, out val);
		return val;
	}

    internal void CheckCollision(Collider collider) {
        if (!_isInGame) return;

        Vertex vertex = collider.GetComponent<Vertex>();
        if (vertex != null && _lastContact != vertex && _verteces.IndexOf(vertex) > -1) {
			_lastContactTimes [vertex] = Time.realtimeSinceStartup;
            foreach (Segment seg in _segments) {
				float otherVertexTouchTime = -1;
				if (seg.VertexA == vertex) {
					otherVertexTouchTime = GetLastVertexTouchTime (seg.VertexB);
				} else if (seg.VertexB == vertex) {
					otherVertexTouchTime = GetLastVertexTouchTime (seg.VertexA);
				}
				if (otherVertexTouchTime > Time.realtimeSinceStartup - segmentTouchTimeThreshold) {
					seg.AddPlayer(Players.IndexOf(myID));
					//seg.AddPlayer(Players.IndexOf(UnityEngine.Random.Range(1, 3)));
					GameManagerInst.NotifyFilledEdge(_segments.IndexOf(seg));
				}
            }

            _lastContact = vertex;
            
        }
    }

    void Update() {
        if (_patternNedsUpdate) {

            foreach(Segment seg in _segments) { 
                Destroy(seg.gameObject);
            }

            _segments.Clear();


            foreach (Vertex ver in _verteces) {
                Destroy(ver.gameObject);
            }

            _verteces.Clear();

            if (useFakePattern) {
                CreatePattern();
            }else {
                CreatePattern(_patternModel);
            }

            //AdjustCollidersSize();
            _patternNedsUpdate = false;
        }

        int workngSegments = 0;

        foreach (Segment seg in _segments) {
            if (seg.IsWorking) workngSegments++;
        }
        
        
    }

    private void ShowInitialSplash(Color splashColor) {
        foreach (Segment seg in _segments) {
            seg.Splash(0.5f * (seg.VertexA.location.y + seg.VertexB.location.y), splashColor);
        }
    }

    public void DebugAction() {
        /*
        string s = "{\n";
        s += "\"points\": [\n";
        
        foreach (Vertex ver in _verteces) {
            s += "[" + ver.location.x + ',' + ver.location.y + "],\n";
        }
        s += "]\n";
        s += "\"edges\": [\n";

        foreach (Segment seg in _segments) {
            s += "[" + _verteces.IndexOf(seg.VertexA) + ',' + _verteces.IndexOf(seg.VertexB) + "],\n";
        }
        s += "]\n";

        s += "}\n";

        print( s);
        */
        _patternNedsUpdate = true;

    }

    public void Stopcandles() {
        
        for (int i = 0; i < Candles.Length; i++) {
            Candles[i].Stop();

        }
    }


    public void StartAnimationEnded() {
        for (int i = 0; i < Candles.Length; i++) {
            Candles[i].Lightdelay(2 + 0.08f * i);
            
        }
    }
   

    public void StartAnimComplete() {
        GameManagerInst.StartRound();
    }
}
