using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickEvent
{
    public bool Clicking { get; private set; }

    public ClickEvent()
    {
        Clicking = false;
    }

    public bool raiseEvent(bool input)
    {
        if (!Clicking && !input)
        { }
        else if (!Clicking && input)
        {
            Clicking = input;
            return true;
        }
        else if (Clicking && input)
        { }
        else if (Clicking && !input)
        { }

        Clicking = input;
        return false;
    }
}

public class HandCursor : MonoBehaviour {

    private HandheldMessage message = null;
    private float yawOffset = 0f;

    private Quaternion attitude;

    private ClickEvent clickEvent;

    private AdaptiveDoubleExponentialQuaternion _filteredRotation;

    private WhackAMole _waka;

    private bool _init = false;
    
	void Start ()
    {
        clickEvent = new ClickEvent();
        attitude = Quaternion.identity;
        _filteredRotation = new AdaptiveDoubleExponentialQuaternion();
        _waka = GameObject.Find("WhackAMole").GetComponent<WhackAMole>();
	}

    public void Init()
    {
        print(this.ToString() + ": starting yeah");
        _init = true;
    }
	
	void Update ()
    {
        if (!_init) return;

        /*if (message == null) return;

        print(this.ToString() + ": updatings");

        if (message.Reset)
        {
            Debug.Log(this.ToString() + ": reset");
            yawOffset += -transform.rotation.eulerAngles.y;
        }

        attitude = message.Attitude;
        _filteredRotation.Value = attitude;
        transform.localRotation = Quaternion.AngleAxis(yawOffset, Vector3.up) * new Quaternion(-_filteredRotation.Value.x, -_filteredRotation.Value.z, -_filteredRotation.Value.y, _filteredRotation.Value.w);

        Ray ray = new Ray(transform.position, transform.forward);

        SimpleProjector sp = GameObject.Find("Projector").GetComponent<SimpleProjector>();
        Transform screenCenter = GameObject.Find("localScreenCenter").transform;

        Plane surface = new Plane(screenCenter.forward, screenCenter.position);

        float distance;

        if (surface.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 pixel = Camera.main.WorldToScreenPoint(hitPoint);

            ray = Camera.main.ScreenPointToRay(pixel);

            sp.pixel = pixel;
            sp.didHit = true;
        }
        else
            sp.didHit = false;

        Vector3 hit = Vector3.zero;
        
        _waka.IAmPointing(ray, clickEvent.raiseEvent(message.Click), out hit);

        if (clickEvent.raiseEvent(message.Click))
        {
            Debug.Log(this.ToString() + ": selection event.");
        }
        else
        {

        }*/

        //_waka.IAmPointing(ray, clickEvent.raiseEvent(message.Click), out hit);
    }

    internal void Update(HandheldMessage message)
    {
        this.message = message;
    }
}
