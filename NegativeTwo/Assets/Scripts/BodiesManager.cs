using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodiesManager : MonoBehaviour
{

    private Dictionary<string, Human> _humans;
    private bool _humanLocked = false;
    public Human human = null;

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

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.L))
        {
            _humanLocked = !_humanLocked;
        }

        if (_humans.Count > 0)
        {
            if (human != null && _humanLocked && _humans.ContainsKey(human.id))
            {
                human = _humans[human.id];
            }
            else
            {
                _humanLocked = false;
                Human newHuman = null;
                foreach (Human h in _humans.Values)
                {
                    if (newHuman == null)
                    {
                        newHuman = h;
                    }
                    else
                    {
                        GameObject screenCenter = GameObject.Find("localScreenCenter");
                        if (screenCenter != null && Vector3.Distance(h.body.Joints[BodyJointType.head], screenCenter.transform.position) < Vector3.Distance(newHuman.body.Joints[BodyJointType.head], screenCenter.transform.position))
                        {
                            newHuman = h;
                        }
                    }
                }
                human = newHuman;
            }
        }

        if (_remoteHumans.Count > 0)
        {
            if (remoteHuman != null && _remoteHumanLocked && _remoteHumans.ContainsKey(remoteHuman.id))
            {
                remoteHuman = _remoteHumans[remoteHuman.id];
            }
            else
            {
                _remoteHumanLocked = false;
                Human newHuman = null;
                foreach (Human h in _remoteHumans.Values)
                {
                    if (newHuman == null)
                    {
                        newHuman = h;
                    }
                    else
                    {
                        GameObject screenCenter = GameObject.Find("remoteScreenCenter");
                        if (screenCenter != null && Vector3.Distance(h.body.Joints[BodyJointType.head], screenCenter.transform.position) < Vector3.Distance(newHuman.body.Joints[BodyJointType.head], screenCenter.transform.position))
                        {
                            newHuman = h;
                        }
                    }
                }
                remoteHuman = newHuman;
            }

            //if (_perspectiveProjection.Running && _perspectiveProjection.Active)
            //{
            //    Camera.main.transform.position = human.body.Joints[BodyJointType.head];
            //}
        }

        // finally
        _cleanDeadHumans();


        if (human != null) Camera.main.transform.position = human.body.Joints[BodyJointType.head];


    }

    public void setNewFrame(Body[] bodies, bool local)
    {
        foreach (Body b in bodies)
        {
            try
            {
                string bodyID = b.Properties[BodyPropertiesType.UID];
                //print(bodyID + " " + b.Joints[BodyJointType.head]);

                

                if (local)
                {
                    if (!_humans.ContainsKey(bodyID))
                    {
                        _humans.Add(bodyID, new Human());
                    }
                    _humans[bodyID].Update(b);
                }
                else
                {
                    if (!_remoteHumans.ContainsKey(bodyID))
                    {
                        _remoteHumans.Add(bodyID, new Human());
                    }
                    _remoteHumans[bodyID].Update(b);
                }
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
