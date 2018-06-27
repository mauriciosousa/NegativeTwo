using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PoleCellStateType
{
    OBSERVER_SIDE,
    POINTER_SIDE,
    POINTER_SIDE_TARGET
}

public class PoleNumber : MonoBehaviour {

    public Transform observerQuad;
    public Transform observerText;
    public TextMesh textMesh;

    public Transform pointerQuad;

    public PoleCellStateType state;

    public int Number = 0;

	void Start () {
		
	}
	
	void Update ()
    {
        observerText.gameObject.GetComponent<Renderer>().enabled = state == PoleCellStateType.OBSERVER_SIDE;
        observerQuad.gameObject.GetComponent<Renderer>().enabled = state != PoleCellStateType.POINTER_SIDE_TARGET;
        pointerQuad.gameObject.GetComponent<Renderer>().enabled = state == PoleCellStateType.POINTER_SIDE_TARGET;
    }
}
