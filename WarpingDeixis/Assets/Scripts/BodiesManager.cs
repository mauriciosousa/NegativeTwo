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

    private Evaluation _evaluation;
    private NetworkCommunication _network;

    public GameObject LeftHumanGO;
    public GameObject RightHumanGO;

    void Start()
    {
        //_perspectiveProjection = Camera.main.GetComponent<PerspectiveProjection>();
        _humans = new Dictionary<string, Human>();
        _remoteHumans = new Dictionary<string, Human>();
        _evaluation = GameObject.Find("PointingEvaluation").GetComponent<Evaluation>();
        _network = GameObject.Find("PointingEvaluation").GetComponent<NetworkCommunication>();
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

        // finally
        _cleanDeadHumans();


        if (LeftHuman != null) updateBody(LeftHuman, LeftHumanGO);
        if (RightHuman != null) updateBody(RightHuman, RightHumanGO);
    }

    public Vector3 getHeadPosition(out bool canApplyHeadPosition)
    {
        canApplyHeadPosition = false;

        if (_evaluation.clientPosition == EvaluationPosition.ON_THE_LEFT && LeftHuman != null)
        {
            canApplyHeadPosition = true;
            return transform.position = LeftHuman.body.Joints[BodyJointType.head];
        }

        if (_evaluation.clientPosition == EvaluationPosition.ON_THE_RIGHT && RightHuman != null)
        {
            canApplyHeadPosition = true;
            return transform.position = RightHuman.body.Joints[BodyJointType.head];
        }

        return Vector3.zero;
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

    internal int remoteHead()
    {
        if (_evaluation.clientPosition == EvaluationPosition.ON_THE_LEFT)
        {
            return LeftHuman == null ? 0 : 1;
        }
        else
        {
            return RightHuman == null ? 0 : 1;
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

    internal string calibrateRightHuman()
    {
        Human h = getHumanWithHandUp();
        if (h == null)
        {
            throw new Exception("Cannot find that human!");
        }
        else
        {
            if (_evaluation.clientPosition == EvaluationPosition.ON_THE_RIGHT)
            {
                UnityEngine.XR.InputTracking.Recenter();
            }
            RightHuman = h;
            return h.id;
        }
    }

    internal string calibrateLeftHuman()
    {
        Human h = getHumanWithHandUp();
        if (h == null)
        {
            throw new Exception("Cannot find that human!");
        }
        else
        {
            if (_evaluation.clientPosition == EvaluationPosition.ON_THE_LEFT)
            {
                UnityEngine.XR.InputTracking.Recenter();
            }
            LeftHuman = h;
            return h.id;
        }


    }

    internal void calibrateHumans(EvaluationPosition evaluationPosition)
    {
        /**
        if (_evaluation.clientPosition == evaluationPosition)
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
        */
        if (evaluationPosition == EvaluationPosition.ON_THE_LEFT)
        {
            calibrateLeftHuman();
        }
        if (evaluationPosition == EvaluationPosition.ON_THE_RIGHT)
        {
            calibrateRightHuman();
        }
    }

    internal Vector3 getLocalHead()
    {
        if (_evaluation.clientPosition == EvaluationPosition.ON_THE_LEFT)
            return LeftHuman == null ? Vector3.zero : LeftHuman.body.Joints[BodyJointType.head];
        else
            return RightHuman == null ? Vector3.zero : RightHuman.body.Joints[BodyJointType.head];
    }

    private void updateBody(Human human, GameObject go)
    {
        Vector3 head = human.body.Joints[BodyJointType.head];
        Vector3 leftHand = human.body.Joints[BodyJointType.leftHand];
        Vector3 rightHand = human.body.Joints[BodyJointType.rightHand];

        if (go != null)// && go.activeSelf)
        {

            go.transform.Find("HEAD").localPosition = head;
            go.transform.Find("LEFTHAND").localPosition = leftHand;
            go.transform.Find("RIGHTHAND").localPosition = rightHand;
            go.transform.Find("NECK").localPosition = human.body.Joints[BodyJointType.neck];
            go.transform.Find("SPINESHOULDER").localPosition = human.body.Joints[BodyJointType.spineShoulder];
            go.transform.Find("LEFTSHOULDER").localPosition = human.body.Joints[BodyJointType.leftShoulder];
            go.transform.Find("RIGHTSHOULDER").localPosition = human.body.Joints[BodyJointType.rightShoulder];
            go.transform.Find("LEFTELBOW").localPosition = human.body.Joints[BodyJointType.leftElbow];
            go.transform.Find("RIGHTELBOW").localPosition = human.body.Joints[BodyJointType.rightElbow];
            go.transform.Find("LEFTWRIST").localPosition = human.body.Joints[BodyJointType.leftWrist];
            go.transform.Find("RIGHTWRIST").localPosition = human.body.Joints[BodyJointType.rightWrist];
            go.transform.Find("SPINEBASE").localPosition = human.body.Joints[BodyJointType.spineBase];
            go.transform.Find("SPINEMID").localPosition = human.body.Joints[BodyJointType.spineMid];
            go.transform.Find("LEFTHIP").localPosition = human.body.Joints[BodyJointType.leftHip];
            go.transform.Find("RIGHTHIP").localPosition = human.body.Joints[BodyJointType.rightHip];
            go.transform.Find("LEFTKNEE").localPosition = human.body.Joints[BodyJointType.leftKnee];
            go.transform.Find("RIGHTKNEE").localPosition = human.body.Joints[BodyJointType.rightKnee];
            go.transform.Find("LEFTFOOT").localPosition = human.body.Joints[BodyJointType.leftFoot];
            go.transform.Find("RIGHTFOOT").localPosition = human.body.Joints[BodyJointType.rightFoot];
            go.transform.Find("LEFTHANDTIP").localPosition = human.body.Joints[BodyJointType.leftHandTip];
            go.transform.Find("RIGHTHANDTIP").localPosition = human.body.Joints[BodyJointType.rightHandTip];
        }
    }

    private void _renderServerBody(Human human, GameObject go)
    {
        if (human == null)
        {
            go.SetActive(false);
        }
        else
        {
            go.SetActive(true);

            go.transform.Find("HEAD").localPosition = human.body.Joints[BodyJointType.head];
            go.transform.Find("LEFTHAND").localPosition = human.body.Joints[BodyJointType.leftHand];
            go.transform.Find("RIGHTHAND").localPosition = human.body.Joints[BodyJointType.rightHand];
            go.transform.Find("NECK").localPosition = human.body.Joints[BodyJointType.neck];
            go.transform.Find("SPINESHOULDER").localPosition = human.body.Joints[BodyJointType.spineShoulder];
            go.transform.Find("LEFTSHOULDER").localPosition = human.body.Joints[BodyJointType.leftShoulder];
            go.transform.Find("RIGHTSHOULDER").localPosition = human.body.Joints[BodyJointType.rightShoulder];
            go.transform.Find("LEFTELBOW").localPosition = human.body.Joints[BodyJointType.leftElbow];
            go.transform.Find("RIGHTELBOW").localPosition = human.body.Joints[BodyJointType.rightElbow];
            go.transform.Find("LEFTWRIST").localPosition = human.body.Joints[BodyJointType.leftWrist];
            go.transform.Find("RIGHTWRIST").localPosition = human.body.Joints[BodyJointType.rightWrist];
            go.transform.Find("SPINEBASE").localPosition = human.body.Joints[BodyJointType.spineBase];
            go.transform.Find("SPINEMID").localPosition = human.body.Joints[BodyJointType.spineMid];
            go.transform.Find("LEFTHIP").localPosition = human.body.Joints[BodyJointType.leftHip];
            go.transform.Find("RIGHTHIP").localPosition = human.body.Joints[BodyJointType.rightHip];
            go.transform.Find("LEFTKNEE").localPosition = human.body.Joints[BodyJointType.leftKnee];
            go.transform.Find("RIGHTKNEE").localPosition = human.body.Joints[BodyJointType.rightKnee];
            go.transform.Find("LEFTFOOT").localPosition = human.body.Joints[BodyJointType.leftFoot];
            go.transform.Find("RIGHTFOOT").localPosition = human.body.Joints[BodyJointType.rightFoot];
            go.transform.Find("LEFTHANDTIP").localPosition = human.body.Joints[BodyJointType.leftHandTip];
            go.transform.Find("RIGHTHANDTIP").localPosition = human.body.Joints[BodyJointType.rightHandTip];
        }

    }
}
