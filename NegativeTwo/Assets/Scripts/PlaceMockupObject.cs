using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceMockupObject : MonoBehaviour {

    public Transform target;
    private Transform origin;

	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (origin != null)
        {
            target.position = origin.position;
            target.rotation = origin.rotation;
            target.forward = -target.forward;
        }
        else
        {
            origin = GameObject.Find("workspaceCollider").transform;
        }
	}
}
