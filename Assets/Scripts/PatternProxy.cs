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


        for (int i = 0; i < TargetSegments.Length / 2; i++) {
            Segments.Add(Instantiate(SegmentPF));
            Segments[i].VertexA = Verteces[TargetSegments[i * 2]];
            Segments[i].VertexB = Verteces[TargetSegments[i * 2 + 1]];
            Segments[i].Place();
        }

            
    }

    internal void CheckCollision(Collider collider) {
        Vertex vertex = collider.GetComponent<Vertex>();
        if (vertex != null && _lastContact != vertex && Verteces.IndexOf(vertex) > -1) {
            _lastContact = vertex;
            print("123");
        }
    }
}
