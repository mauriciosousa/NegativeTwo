using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodiesManager : MonoBehaviour
{

    private Dictionary<string, Human> _humans;
    private bool _humanLocked = false;

    public Human LeftHuman = null;
    public Human RightHuman = null;

    private  Dictionary<string, Human> _remoteHumans;
    private bool _remoteHumanLocked = false;
    public Human remoteHuman = null;

    public Evaluation _evaluation;

    void Start()
    {
        //_perspectiveProjection = Camera.main.GetComponent<PerspectiveProjection>();
        _humans = new Dictionary<string, Human>();
        _remoteHumans = new Dictionary<string, Human>();
        _evaluation = GameObject.Find("PointingEvaluation").GetComponent<Evaluation>();
    }

    private string _getHumanIdWithHandUp()
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

    void Update() // REDO
    {

        if (Input.GetKeyDown(KeyCode.L))
        {
            _humanLocked = !_humanLocked;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.PageDown) || Input.GetKeyDown(KeyCode.Joystick1Button1)) // Mouse tap
        {

        }


        // finally
        _cleanDeadHumans();

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

    public Human getHumanWithHandUp()
    {

        string id = _getHumanIdWithHandUp();
        foreach (KeyValuePair<String, Human> h in _humans)
        {
            if (h.Key == id) return h.Value;
        }
        return null;
    }

    internal void calibrateRightHuman()
    {
        Human h = getHumanWithHandUp();
        if (h == null)
        {
            throw new Exception("Cannot find that human!");
        }
        else
            RightHuman = h;
    }

    internal void calibrateLeftHuman()
    {
        Human h = getHumanWithHandUp();
        if (h == null)
        {
            throw new Exception("Cannot find that human!");
        }
        else
            LeftHuman = h;
    }

    internal void calibrateHumans(EvaluationPosition evaluationPosition)
    {
        if (_evaluation.evaluationPosition == evaluationPosition)
        {
            if (_evaluation.clientPosition == EvaluationPosition.ON_THE_LEFT)
            {
                calibrateLeftHuman();
            }
            else if (_evaluation.clientPosition == EvaluationPosition.ON_THE_RIGHT)
            {
                calibrateRightHuman();
            }
        }
    }
}
