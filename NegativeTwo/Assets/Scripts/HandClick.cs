using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandClick : MonoBehaviour {

    private AdaptiveDoubleExponentialFilterVector3 _A;
    private AdaptiveDoubleExponentialFilterVector3 _B;

    public WhackAMole waka;
    public HandType arm;

	void Start ()
    {
        _A = new AdaptiveDoubleExponentialFilterVector3();
        _B = new AdaptiveDoubleExponentialFilterVector3();
	}
	
	void Update ()
    {
        if (!waka.IsInit) return;

        string strB = arm == HandType.Left ? "LEFTWRIST" : "RIGHTWRIST";
        string strA = arm == HandType.Left ? "LEFTELBOW" : "RIGHTELBOW";

        _A.Value = transform.Find(strA).position;
        _B.Value = transform.Find(strB).position;

        if (Input.GetMouseButtonDown(0))
        {
            waka.PointingEvent(_B.Value, _B.Value - _A.Value);
        }
	}
}
