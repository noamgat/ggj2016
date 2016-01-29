using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PatternProxy : MonoBehaviour {

    public Vector2[] TargetPoints;
    public int[] TargetSegments;

    public Segment SegmentPF;
    public Vertex VertexPF;

    protected List<Vertex> Verteces = new List<Vertex>();
    protected List<Segment> Segments = new List<Segment>();

    private Vertex _lastContact;

    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void CreatePattern() {

        //stub place\

        for (int i = 0; i < TargetPoints.Length; i++) {
            Verteces.Add(Instantiate(VertexPF));
            Verteces[Verteces.Count - 1].location = TargetPoints[i];
            Verteces[Verteces.Count - 1].Place();
        }


        for (int i = 0; i < TargetSegments.Length / 3; i++) {

            int numOfDivisions = TargetSegments[i * 3 + 2];

            for (int j = 0; j < numOfDivisions; j++) {

                if (j < numOfDivisions - 1) {

                } else {

                }

                Verteces.Add(Instantiate(VertexPF));
                Verteces[Verteces.Count - 1].location = TargetPoints[i];
                Verteces[Verteces.Count - 1].Place();

            }

            Segments.Add(Instantiate(SegmentPF));
            Segments[i].VertexA = Verteces[TargetSegments[i * 3]];
            Segments[i].VertexB = Verteces[TargetSegments[i * 3 + 1]];
            Segments[i].Place();
            Segments[i].State = Segment.States.Idle;
        }

            
    }

    internal void CheckCollision(Collider collider) {
        Vertex vertex = collider.GetComponent<Vertex>();
        if (vertex != null && _lastContact != vertex && Verteces.IndexOf(vertex) > -1) {

            foreach (Segment seg in Segments) {
                if ((seg.VertexA == vertex || seg.VertexB == vertex) && (seg.VertexA == _lastContact || seg.VertexB == _lastContact)) {
                    seg.State = Segment.States.Player;
                }
            }

            _lastContact = vertex;
            
        }
    }
}
