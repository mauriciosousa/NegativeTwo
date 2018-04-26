using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodiesManager : MonoBehaviour
{

    private Dictionary<string, Human> _humans;
    private bool _humanLocked = false;
    public Human _localHuman = null;
    public Human _remoteHuman = null;

    //private PerspectiveProjection _perspectiveProjection;


    private  Dictionary<string, Human> _remoteHumans;
    private bool _remoteHumanLocked = false;
    public Human remoteHuman = null;

    void Start()
    {
        //_perspectiveProjection = Camera.main.GetComponent<PerspectiveProjection>();
        _humans = new Dictionary<string, Human>();
        _remoteHumans = new Dictionary<string, Human>();
    }

    private string GetHumanIdWithHandUp()
    {
        foreach (Human h in _humans.Values)
        {
            if (h.body.Joints[BodyJointType.leftHand].y > h.body.Joints[BodyJointType.head].y ||
                h.body.Joints[BodyJointType.rightHand].y > h.body.Joints[BodyJointType.head].y)
            {
                return h.id;
            }
        }
        return string.Empty;
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.L))
        {
            _humanLocked = !_humanLocked;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.PageDown) || Input.GetKeyDown(KeyCode.Joystick1Button1)) // Mouse tap
        {
            string currentHumanId = GetHumanIdWithHandUp();
            foreach (KeyValuePair<String,Human> h in _humans)
            {
                if (h.Key == currentHumanId)
                {
                    _localHuman = h.Value;
                }
                else
                {
                    _remoteHuman = h.Value;
                }
            }
        }


        // finally
        _cleanDeadHumans();

        if (_localHuman != null) Camera.main.transform.position = _localHuman.body.Joints[BodyJointType.head];


    }

    public void setNewFrame(Body[] bodies)
    {
        foreach (Body b in bodies)
        {
            try
            {
                string bodyID = b.Properties[BodyPropertiesType.UID];
                //print(bodyID + " " + b.Joints[BodyJointType.head]);
               
                if (!_humans.ContainsKey(bodyID))
                {
                    _humans.Add(bodyID, new Human());
                }
                _humans[bodyID].Update(b);
               
            }
            catch (Exception e) { }
        }
    }

    void _cleanDeadHumans()
    {
        List<Human> deadhumans = new List<Human>();

        foreach (Human h in _humans.Values)
        {
            if (DateTime.Now > h.lastUpdated.AddMilliseconds(1000))
                deadhumans.Add(h);
        }

        foreach (Human h in deadhumans)
        {
            _humans.Remove(h.id);
        }

        deadhumans = new List<Human>();

        foreach (Human h in _remoteHumans.Values)
        {
            if (DateTime.Now > h.lastUpdated.AddMilliseconds(1000))
                deadhumans.Add(h);
        }

        foreach (Human h in deadhumans)
        {
            _remoteHumans.Remove(h.id);
        }
    }

    void OnGUI()
    {
        //GUI.Label(new Rect(10, 10, 1000, 1000), "" + _humans.Count);
    }
}
