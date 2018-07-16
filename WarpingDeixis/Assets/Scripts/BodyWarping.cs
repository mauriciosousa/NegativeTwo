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
    private Human _human;

    public DeixisEvaluation deixisEvaluation;
    public DeixisNetwork deixisNetwork;
    public EvaluationConfigProperties config;

    public GameObject leftBody;
    public GameObject rightBody;

    public PointingDistortionInfo rightPointingInfo;
    public PointingDistortionInfo leftPointingInfo;

    private Vector3 lastRightHandPos;
    private Vector3 lastLeftHandPos;

    public float headSize;

    public float maxHandVelocity = 1.0f;

    private bool applyWarp = false;
    public bool overrideApplyWarp = false;

    void Start ()
    {
        _human = null;

        rightPointingInfo = new PointingDistortionInfo();
        leftPointingInfo = new PointingDistortionInfo();
    }

    void Update ()
    {
        if (config.Peer == EvaluationPeer.SERVER) return;

        applyWarp = deixisEvaluation.condition == WarpingCondition.WARPING;

        GameObject go = null;
        if (config.Peer == EvaluationPeer.LEFT)
        {
            _human = bodies.RightHuman;
            go = rightBody;
        }
        else
        {
            _human = bodies.LeftHuman;
            go = leftBody;
        }

        if (applyWarp && _human != null && go != null)
        {
            Debug.Log("APPLYing WARP");
            _applyWarp(_human, go); // <----------------REVIEWWW a partir daqui
        }
	}

    public Vector3 getLocalHead()
    {
        return bodies.getLocalHead();
    }

    private void _applyWarp(Human human, GameObject go)
    {
        if (go == null) return;

        Vector3 head = go.transform.Find(BodyJointType.head.ToString()).position;

        Vector3 leftShoulder = go.transform.Find(BodyJointType.leftShoulder.ToString()).position;
        Vector3 leftHandTip = go.transform.Find(BodyJointType.leftHandTip.ToString()).position;

        Vector3 rightShoulder = go.transform.Find(BodyJointType.rightShoulder.ToString()).position;
        Vector3 rightHandTip = go.transform.Find(BodyJointType.rightHandTip.ToString()).position;

        if (float.IsPositiveInfinity(lastRightHandPos.x))
        {
            lastRightHandPos = rightHandTip;
        }

        if (float.IsPositiveInfinity(lastLeftHandPos.x))
        {
            lastLeftHandPos = leftHandTip;
        }

        Vector3 leftHit;
        bool leftPointing = _isPointingToWall(new Ray(leftHandTip, leftHandTip - head), out leftHit);
        Vector3 rightHit;
        bool rightPointing = _isPointingToWall(new Ray(rightHandTip, rightHandTip - head), out rightHit);

        Transform leftHand_d = go.transform.Find(BodyJointType.leftHandTip.ToString() + "_WARP");
        leftHand_d.position = leftHandTip;

        Transform rightHand_d = go.transform.Find(BodyJointType.rightHandTip.ToString() + "_WARP");
        rightHand_d.position = rightHandTip;

        Matrix4x4 m = Matrix4x4.identity;

        // Left Pointing

        
        if (applyWarp) m = _correctPointing(leftShoulder, leftHandTip, head, leftPointing, leftHit, ref lastLeftHandPos);

        leftHand_d.position = m.MultiplyPoint(leftHand_d.position);

        leftPointingInfo.matrix = m;
        leftPointingInfo.midPoint = (leftShoulder + leftHandTip) * 0.5f;
        leftPointingInfo.distance = 0.15f;//(leftPointingB - leftPointingA).magnitude * 0.5f + 0.1f;
        leftPointingInfo.Shoulder = go.transform.Find(BodyJointType.leftShoulder.ToString()).transform.position;
        leftPointingInfo.Elbow = go.transform.Find(BodyJointType.leftElbow.ToString()).transform.position;
        leftPointingInfo.Wrist = go.transform.Find(BodyJointType.leftWrist.ToString()).transform.position;
        leftPointingInfo.Hand = go.transform.Find(BodyJointType.leftHand.ToString()).transform.position;
        leftPointingInfo.HandTip = go.transform.Find(BodyJointType.leftHandTip.ToString()).transform.position;
        leftPointingInfo.pointing = leftPointing;// true;

        // Right Pointing

        if (applyWarp) m = _correctPointing(rightShoulder, rightHandTip, head, rightPointing, rightHit, ref lastRightHandPos);

        rightHand_d.position = m.MultiplyPoint(rightHand_d.position);

        rightPointingInfo.matrix = m;
        rightPointingInfo.midPoint = (rightShoulder + rightHandTip) * 0.5f;
        rightPointingInfo.distance = 0.15f;//(rightPointingB - rightPointingA).magnitude * 0.5f + 0.1f;
        rightPointingInfo.Shoulder = go.transform.Find(BodyJointType.rightShoulder.ToString()).transform.position;
        rightPointingInfo.Elbow = go.transform.Find(BodyJointType.rightElbow.ToString()).transform.position;
        rightPointingInfo.Wrist = go.transform.Find(BodyJointType.rightWrist.ToString()).transform.position;
        rightPointingInfo.Hand = go.transform.Find(BodyJointType.rightHand.ToString()).transform.position;
        rightPointingInfo.HandTip = go.transform.Find(BodyJointType.rightHandTip.ToString()).transform.position;
        rightPointingInfo.pointing = rightPointing;// true;

        if (overrideApplyWarp)
        {
            if (leftPointing)
            {
                Debug.DrawLine(head, leftHandTip, Color.cyan);
                Debug.DrawLine(head, leftHand_d.position, Color.cyan);
            }

            if (rightPointing)
            {
                Debug.DrawLine(head, rightHandTip, Color.cyan);
                Debug.DrawLine(head, rightHand_d.position, Color.cyan);
            }
        }
    }

    private bool _isPointingToWall(Ray ray, out Vector3 hitpoint)
    {
        hitpoint = Vector3.zero;
        RaycastHit hit;
        if (deixisEvaluation.getCollider().Raycast(ray, out hit, 1000.0f))
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
