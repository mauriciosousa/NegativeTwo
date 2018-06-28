using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable()]
public class NegativeRuntimeException : System.Exception
{
    public NegativeRuntimeException() : base() { }
    public NegativeRuntimeException(string message) : base(message) { }
    public NegativeRuntimeException(string message, System.Exception inner) : base(message, inner) { }
    protected NegativeRuntimeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
}

public class Main : MonoBehaviour {

    public Location location;

    public Properties properties;

    public Evaluation evaluation;
  
    private UdpBodiesListener _udpLocalBodiesListener;
    private BodiesManager _bodies;
    public bool correctingPointing = true;

    //public PointingDistortionInfo rightPointingInfo;
    //public PointingDistortionInfo leftPointingInfo;

    public bool mirrorPessoa = false;
    public float headSize;
    Vector3 lastRightHandPos;
    Vector3 lastLeftHandPos;
    Vector3 remoteLeftHit;
    Vector3 remoteRightHit;
    public GameObject Wall;
    public float maxHandVelocity = 1.0f;

     


    public Vector3 getLocalHead()
    {
        return _bodies.getLocalHead();
    }

    public int remoteHead()
    {
        return _bodies.remoteHead();
    }

    void Awake()
    {
        Application.runInBackground = true;

        properties = GetComponent<Properties>();
        try
        {
            properties.load();
        }
        catch (NegativeRuntimeException e)
        {
            Debug.LogException(e);
            strategicExit();
        }

        UdpBodiesListener[] udpListeners = GameObject.Find("BodiesManager").GetComponents<UdpBodiesListener>();
        foreach (UdpBodiesListener l in udpListeners)
        {
            if (l.humansType == HumansType.Local)
            {
                _udpLocalBodiesListener = l;
            }
        }

        //_wall = GameObject.Find("Wall");
        _bodies = GameObject.Find("BodiesManager").GetComponent<BodiesManager>();
        _udpLocalBodiesListener.startListening(int.Parse(properties.info.trackerBroadcastPort));
        //rightPointingInfo = new PointingDistortionInfo();
        //leftPointingInfo = new PointingDistortionInfo();

		//if (GameObject.Find ("DeixisEvaluation").GetComponent<EvaluationConfigProperties> ().Peer != EvaluationPeer.SERVER) {
			GameObject.Find("RavatarManager").GetComponent<TcpDepthListener>().Init(int.Parse(properties.info.avatarListenPort));
			GameObject.Find("RavatarManager").GetComponent<Tracker>().Init(int.Parse(properties.info.avatarListenPort), int.Parse(properties.info.trackerListenPort));
		//}
	
	}

	void Start ()
    {
    }

    private static void strategicExit()
    {
        if (Application.isEditor)
        {
            Debug.Break();
        }
    }

    void Update()
    {

        //if (_bodies._localHuman != null) updateBody(_bodies._localHuman, GameObject.Find("LocalBody"));
        //if (_bodies._remoteHuman!= null) updateBody(_bodies._remoteHuman, GameObject.Find("RemoteBody"));

        /*if (true) // evaluation.taskInProgress && evaluation.condition == EvaluationCondition.DEICTICS_CORRECTION)
        {
            Human human;
            GameObject go;
            if (evaluation.clientPosition == EvaluationPosition.ON_THE_LEFT)
            {
                human = _bodies.RightHuman;
                go = GameObject.Find("RightBody");
            }
            else
            {
                human = _bodies.LeftHuman;
                go = GameObject.Find("RightBody");
            }

            _applyDisplacement(human, go);
        }*/
    }

    /*
    private void updateBody(Human human, GameObject go)
    {
        Debug.Log("update body");

        Vector3 head = human.body.Joints[BodyJointType.head];
        Vector3 leftHand = human.body.Joints[BodyJointType.leftHand];
        Vector3 rightHand = human.body.Joints[BodyJointType.rightHand];

        // _filteredHandPosition.Value = _handheldListener.Message.Hand == HandType.Left ? leftHand : rightHand;
        if (go != null && go.activeSelf)
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

            if(go.name == "LocalBody")
            {
                //_localHead = head;
            }


            if (go.name == "RemoteBody" && correctingPointing)
            {
                Ray ray;
                Vector3 hit;
                RaycastHit hitInfo;

                // right
                ray = new Ray(human.body.Joints[BodyJointType.rightHandTip], human.body.Joints[BodyJointType.rightHandTip] - human.body.Joints[BodyJointType.head]);
                if (Wall.GetComponent<Collider>().Raycast(ray, out hitInfo, 1000.0f))
                    remoteRightHit = hitInfo.point;
                else
                    remoteRightHit = Vector3.positiveInfinity;

                // left
                ray = new Ray(human.body.Joints[BodyJointType.leftHandTip], human.body.Joints[BodyJointType.leftHandTip] - human.body.Joints[BodyJointType.head]);
                if (Wall.GetComponent<Collider>().Raycast(ray, out hitInfo, 1000.0f))
                    remoteLeftHit = hitInfo.point;
                else
                    remoteLeftHit = Vector3.positiveInfinity;

                //_applyDisplacement(human, go);
            }
          
        }
    }

    private void _applyDisplacement(Human human, GameObject go)
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


        //bool leftPointing = _isPointingToWorkspace(new Ray(leftTip, leftTip - head), workspaceCollider, out leftHit);
        //bool leftPointing = !float.IsPositiveInfinity(remoteLeftHit.x);
        //if (leftPointing) leftHit = _wall.transform.TransformPoint(remoteLeftHit);

        //bool rightPointing = _isPointingToWorkspace(new Ray(rightTip, rightTip - head), workspaceCollider, out rightHit);
        //bool rightPointing = !float.IsPositiveInfinity(remoteRightHit.x);
        //if (rightPointing) rightHit = _wall.transform.TransformPoint(remoteRightHit);

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

        m = _correctPointing(leftShoulder, leftTip, head, leftPointing, remoteLeftHit, ref lastLeftHandPos);

        leftHand_d.position = m.MultiplyPoint(leftHand_d.position);

        leftPointingInfo.matrix = m;
        leftPointingInfo.midPoint = (leftShoulder + leftTip) * 0.5f;
        leftPointingInfo.distance = 0.15f;//(leftPointingB - leftPointingA).magnitude * 0.5f + 0.1f;
        leftPointingInfo.Shoulder = go.transform.Find("LEFTSHOULDER").transform.position;
        leftPointingInfo.Elbow = go.transform.Find("LEFTELBOW").transform.position;
        leftPointingInfo.Wrist = go.transform.Find("LEFTWRIST").transform.position;
        leftPointingInfo.Hand = go.transform.Find("LEFTHAND").transform.position;
        leftPointingInfo.HandTip = go.transform.Find("LEFTHANDTIP").transform.position;
        leftPointingInfo.pointing = true;

        // Right Pointing

        m = _correctPointing(rightShoulder, rightTip, head, rightPointing, remoteRightHit, ref lastRightHandPos);

        rightHand_d.position = m.MultiplyPoint(rightHand_d.position);

        rightPointingInfo.matrix = m;
        rightPointingInfo.midPoint = (rightShoulder + rightTip) * 0.5f;
        rightPointingInfo.distance = 0.15f;//(rightPointingB - rightPointingA).magnitude * 0.5f + 0.1f;
        rightPointingInfo.Shoulder = go.transform.Find("RIGHTSHOULDER").transform.position;
        rightPointingInfo.Elbow = go.transform.Find("RIGHTELBOW").transform.position;
        rightPointingInfo.Wrist = go.transform.Find("RIGHTWRIST").transform.position;
        rightPointingInfo.Hand = go.transform.Find("RIGHTHAND").transform.position;
        rightPointingInfo.HandTip = go.transform.Find("RIGHTHANDTIP").transform.position;
        rightPointingInfo.pointing = true;
    }

    private bool _isPointingToWall(Ray ray, out Vector3 hitpoint)
    {
        hitpoint = Vector3.zero;
        RaycastHit hit;
        if (Wall.GetComponent<Collider>().Raycast(ray, out hit, 1000.0f))
        {
            //print("hit = " + hit.transform.name + " " + hit.point);
            hitpoint = hit.point;
            return true;
        }

        return false;
    }

    private Vector3 reflectWorkspacePoint(Vector3 point) // receives world point, returns world point (yes, that's world 2 times)
    {
        Vector3 pointToLocal = _wall.transform.InverseTransformPoint(point);
        Vector3 reflectedPoint = new Vector3(pointToLocal.x, pointToLocal.y, -pointToLocal.z);
        return _wall.transform.TransformPoint(reflectedPoint);
    }

    private Matrix4x4 _correctPointing(Vector3 elbow, Vector3 tip, Vector3 head, bool isPointing, Vector3 hit, ref Vector3 lastHandPosition)
    {
        Vector3 oldVector = tip - elbow;
        Vector3 newVector = oldVector;
        Vector3 newHandPosition;
        if (isPointing)
        {

            Debug.DrawLine(head, hit, Color.cyan);
            Debug.DrawLine(elbow, hit, Color.yellow);

            newVector = hit - elbow;

            Vector3 reflectedHandPosition = elbow + newVector.normalized * Vector3.Distance(tip, elbow);
            newHandPosition = _constraintHandDisplacement(reflectedHandPosition, lastHandPosition);
        }
        else
        {
            rightPointingInfo.pointing = false;

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
        return Vector3.zero;
    }

    */

}
