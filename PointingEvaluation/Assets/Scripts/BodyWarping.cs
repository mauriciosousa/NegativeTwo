using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointingDistortionInfo
{
    public Matrix4x4 matrix;
    public Vector3 midPoint;
    public float distance;
    public Vector3 Shoulder;
    public Vector3 Elbow;
    public Vector3 Wrist;
    public Vector3 Hand;
    public Vector3 HandTip;
    internal bool pointing;

    public PointingDistortionInfo()
    {
        matrix = Matrix4x4.identity;
        midPoint = Vector3.zero;
        distance = 0;
        Shoulder = Vector3.zero;
        Elbow = Vector3.zero;
        Wrist = Vector3.zero;
        Hand = Vector3.zero;
        HandTip = Vector3.zero;
        pointing = false;
    }
}

public class BodyWarping : MonoBehaviour {

    public BodiesManager bodies;
    public Evaluation evaluation;
    public NetworkCommunication network;

    public GameObject wall;

    private Human _human;

    public GameObject leftBody;
    public GameObject rightBody;

    public PointingDistortionInfo rightPointingInfo;
    public PointingDistortionInfo leftPointingInfo;

    private Vector3 lastRightHandPos;
    private Vector3 lastLeftHandPos;

    public float headSize;

    public float maxHandVelocity = 1.0f;

    public bool applyWarp = true;

    void Start ()
    {
        _human = null;

        rightPointingInfo = new PointingDistortionInfo();
        leftPointingInfo = new PointingDistortionInfo();
    }

    void Update ()
    {
        if (network.evaluationPeerType != EvaluationPeertype.CLIENT) return;


        if (bodies.LeftHuman != null) updateBody(bodies.LeftHuman, leftBody);
        if (bodies.RightHuman != null) updateBody(bodies.RightHuman, rightBody);


        // applyWarp = evaluation.taskInProgress && evaluation.condition == EvaluationCondition.DEICTICS_CORRECTION)
        
        GameObject go = null;
        if (evaluation.clientPosition == EvaluationPosition.ON_THE_LEFT)
        {
            _human = bodies.RightHuman;
            go = rightBody;
        }
        else
        {
            _human = bodies.LeftHuman;
            go = leftBody;
        }

        if (_human != null && go != null)
        {
            _applyWarp(_human, go);
        }
	}

    public Vector3 getLocalHead()
    {
        return bodies.getLocalHead();
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

    private void _applyWarp(Human human, GameObject go)
    {
        if (go == null) return;

        Vector3 head = go.transform.Find("HEAD").position;

        Vector3 leftShoulder = go.transform.Find("LEFTSHOULDER").position;
        Vector3 leftTip = go.transform.Find("LEFTHANDTIP").position;

        Vector3 rightShoulder = go.transform.Find("RIGHTSHOULDER").position;
        Vector3 rightTip = go.transform.Find("RIGHTHANDTIP").position;

        if (float.IsPositiveInfinity(lastRightHandPos.x))
        {
            lastRightHandPos = rightTip;
        }

        if (float.IsPositiveInfinity(lastLeftHandPos.x))
        {
            lastLeftHandPos = leftTip;
        }

        Vector3 leftHit;
        bool leftPointing = _isPointingToWall(new Ray(leftTip, leftTip - head), out leftHit);
        Vector3 rightHit;
        bool rightPointing = _isPointingToWall(new Ray(rightTip, rightTip - head), out rightHit);

        Transform leftHand_d = go.transform.Find("LEFTHAND_D");
        leftHand_d.position = leftTip;

        Transform rightHand_d = go.transform.Find("RIGHTHAND_D");
        rightHand_d.position = rightTip;

        Matrix4x4 m = Matrix4x4.identity;

        // Left Pointing

        
        if (applyWarp) m = _correctPointing(leftShoulder, leftTip, head, leftPointing, leftHit, ref lastLeftHandPos);

        leftHand_d.position = m.MultiplyPoint(leftHand_d.position);

        leftPointingInfo.matrix = m;
        leftPointingInfo.midPoint = (leftShoulder + leftTip) * 0.5f;
        leftPointingInfo.distance = 0.15f;//(leftPointingB - leftPointingA).magnitude * 0.5f + 0.1f;
        leftPointingInfo.Shoulder = go.transform.Find("LEFTSHOULDER").transform.position;
        leftPointingInfo.Elbow = go.transform.Find("LEFTELBOW").transform.position;
        leftPointingInfo.Wrist = go.transform.Find("LEFTWRIST").transform.position;
        leftPointingInfo.Hand = go.transform.Find("LEFTHAND").transform.position;
        leftPointingInfo.HandTip = go.transform.Find("LEFTHANDTIP").transform.position;
        leftPointingInfo.pointing = leftPointing;// true;

        // Right Pointing

        if (applyWarp) m = _correctPointing(rightShoulder, rightTip, head, rightPointing, rightHit, ref lastRightHandPos);

        rightHand_d.position = m.MultiplyPoint(rightHand_d.position);

        rightPointingInfo.matrix = m;
        rightPointingInfo.midPoint = (rightShoulder + rightTip) * 0.5f;
        rightPointingInfo.distance = 0.15f;//(rightPointingB - rightPointingA).magnitude * 0.5f + 0.1f;
        rightPointingInfo.Shoulder = go.transform.Find("RIGHTSHOULDER").transform.position;
        rightPointingInfo.Elbow = go.transform.Find("RIGHTELBOW").transform.position;
        rightPointingInfo.Wrist = go.transform.Find("RIGHTWRIST").transform.position;
        rightPointingInfo.Hand = go.transform.Find("RIGHTHAND").transform.position;
        rightPointingInfo.HandTip = go.transform.Find("RIGHTHANDTIP").transform.position;
        rightPointingInfo.pointing = rightPointing;// true;

        if (leftPointing)
        {
            Debug.DrawLine(head, leftTip, Color.cyan);
            Debug.DrawLine(head, leftHand_d.position, Color.cyan);
        }

        if (rightPointing)
        {
            Debug.DrawLine(head, rightTip, Color.cyan);
            Debug.DrawLine(head, rightHand_d.position, Color.cyan);
        }
    }

    private bool _isPointingToWall(Ray ray, out Vector3 hitpoint)
    {
        hitpoint = Vector3.zero;
        RaycastHit hit;
        if (wall.GetComponent<Collider>().Raycast(ray, out hit, 1000.0f))
        {
            //print("hit = " + hit.transform.name + " " + hit.point);
            hitpoint = hit.point;
            return true;
        }

        return false;
    }

    private Matrix4x4 _correctPointing(Vector3 elbow, Vector3 tip, Vector3 head, bool isPointing, Vector3 hit, ref Vector3 lastHandPosition)
    {
        Vector3 oldVector = tip - elbow;
        Vector3 newVector = oldVector;
        Vector3 newHandPosition;
        if (isPointing)
        {
            newVector = hit - elbow;

            Vector3 reflectedHandPosition = elbow + newVector.normalized * Vector3.Distance(tip, elbow);
            newHandPosition = _constraintHandDisplacement(reflectedHandPosition, lastHandPosition);
        }
        else
        {
            //rightPointingInfo.pointing = false;

            newHandPosition = _constraintHandDisplacement(tip, lastHandPosition);
        }

        lastHandPosition = newHandPosition;
        newVector = newHandPosition - elbow;

        Vector3 axis = Vector3.Cross(oldVector, newVector);
        float angle = Vector3.Angle(oldVector, newVector);

        return Matrix4x4.Translate(elbow) * Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(angle, axis), Vector3.one) * Matrix4x4.Translate(-elbow);

    }

    private Vector3 _constraintHandDisplacement(Vector3 newPos, Vector3 oldPos)
    {
        Vector3 handDisplacement = newPos - oldPos;
        float displacementMag = handDisplacement.magnitude;

        //print("Vel: " + (displacementMag / Time.deltaTime) + "; Time: " + Time.deltaTime);

        float maxMag = maxHandVelocity * Time.deltaTime;
        return oldPos + handDisplacement.normalized * (displacementMag > maxMag ? maxMag : displacementMag);
        //return Vector3.zero;
    }
}
