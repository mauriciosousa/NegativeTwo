using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandClick : MonoBehaviour {

    private AdaptiveDoubleExponentialFilterVector3 _A;
    private AdaptiveDoubleExponentialFilterVector3 _B;

    public WhackAMole waka;
    public HandType arm;

    public double timeToLive = 1000;
    public GUIStyle style = new GUIStyle();

    public DateTime timestamp;

	void Start ()
    {
        _A = new AdaptiveDoubleExponentialFilterVector3();
        _B = new AdaptiveDoubleExponentialFilterVector3();
	}
	
	void Update ()
    {
        if (!waka.IsInit) return;



        if (Input.GetKeyDown(KeyCode.Return))
        {
            timestamp = DateTime.Now;
            if (arm == HandType.Left) arm = HandType.Right;
            else arm = HandType.Left;
        }



        string strB = arm == HandType.Left ? "LEFTWRIST" : "RIGHTWRIST";
        string strA = arm == HandType.Left ? "LEFTELBOW" : "RIGHTELBOW";

        _A.Value = transform.Find(strA).position;
        _B.Value = transform.Find(strB).position;


        // No more pointing to select objects
        /*
        waka.PointingEvent(_B.Value, _B.Value - _A.Value);
        if (Input.GetMouseButtonDown(0))
        {
            waka.click();
        }
        */
	}

    void OnGUI()
    {
        if (timestamp.AddMilliseconds(timeToLive) > DateTime.Now)
        {
            GUI.Label(new Rect(1, 1, 1000, 10000), arm.ToString() + "Hand", style);
        }
    }
}
