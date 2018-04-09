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
}

public class NegativeSpace : MonoBehaviour {

    private Main _main;
    private Properties _properties;
    private BodiesManager _bodiesManager;

    private SimpleHandCursor _cursors;
    private SimpleHandCursor _cursors2;


    private bool _spaceCreated = false;
    private Location _location;
    private SurfaceRectangle _localSurface;
    public SurfaceRectangle LocalSurface { get { return _localSurface; } }
    private SurfaceRectangle _remoteSurfaceProxy;
    public SurfaceRectangle RemoteSurface { get { return _remoteSurfaceProxy; } }
    private float _negativeSpaceLength;

    public Material negativeSpaceMaterial;

    private UDPHandheldListener _handheldListener;

    public GameObject NegativeSpaceCenter { get; private set;}

    private Dictionary<string, GameObject> _negativeSpaceObjects;
    private Dictionary<string, GameObject> negativeSpaceObjects { get { return _negativeSpaceObjects; } }

    //private GameObject _handCursor;
    public Vector3 bottomCenterPosition { get; private set; }

    private AdaptiveDoubleExponentialFilterVector3 _filteredHandPosition;

    private GameObject workspaceCollider = null;

    private GameObject _screen = null;

    public PointingDistortionInfo rightPointingInfo;
    public PointingDistortionInfo leftPointingInfo;

    private Vector3 lastRightHandPos = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
    private Vector3 lastLeftHandPos = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

    public float maxHandVelocity = 1.0f;

    public bool isUsingDeitics = false;

    private bool mirroring = false;
    public bool Mirroring
    {
        get
        {
            return mirroring;
        }

        set
        {
            mirroring = value;
            _main.RemoteOrigin.transform.parent.localScale = new Vector3(mirroring ? -1.0f : 1.0f, 1.0f, 1.0f);
        }
    }
    public bool correctingPointing = false;

    private EvaluationClient _client;
    public Vector3 remoteRightHit;
    public Vector3 remoteLeftHit;

    void Awake()
    {
        _negativeSpaceObjects = new Dictionary<string, GameObject>();

        rightPointingInfo = new PointingDistortionInfo();
        leftPointingInfo = new PointingDistortionInfo();

        remoteLeftHit = remoteRightHit = Vector3.positiveInfinity;
    }

    void Start ()
    {
        _main = GetComponent<Main>();
        _properties = GetComponent<Properties>();
        _filteredHandPosition = new AdaptiveDoubleExponentialFilterVector3();
        _bodiesManager = GameObject.Find("BodiesManager").GetComponent<BodiesManager>();
        _cursors = GameObject.Find("Cursors").GetComponent<SimpleHandCursor>();
        _cursors2 = GameObject.Find("Cursors2").GetComponent<SimpleHandCursor>();
        _client = GameObject.Find("WhackAMole").GetComponent<EvaluationClient>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="localSurface"></param>
    /// <param name="remoteSurfaceProxy"></param>
    /// <param name="length"></param>
    internal void create(Location location, SurfaceRectangle localSurface, SurfaceRectangle remoteSurfaceProxy, float length)
    {
        _handheldListener = new UDPHandheldListener(int.Parse(_properties.localSetupInfo.receiveHandheldPort));
        Debug.Log(this.ToString() + ": Receiving Handheld data in " + _properties.localSetupInfo.receiveHandheldPort);

        _location = location;
        _localSurface = localSurface;
        _remoteSurfaceProxy = remoteSurfaceProxy;
        _negativeSpaceLength = length;

        //_createNegativeSpaceMesh();
        GameObject bottomWall = _negativeSpaceWalls("bottomWall", _localSurface.SurfaceBottomLeft, _localSurface.SurfaceBottomRight, _remoteSurfaceProxy.SurfaceBottomRight, _remoteSurfaceProxy.SurfaceBottomLeft, true);
        GameObject leftWall = _negativeSpaceWalls("leftWall", _localSurface.SurfaceBottomLeft, _remoteSurfaceProxy.SurfaceBottomLeft, _remoteSurfaceProxy.SurfaceTopLeft, _localSurface.SurfaceTopLeft, true);
        GameObject rightWall = _negativeSpaceWalls("rightWall", _remoteSurfaceProxy.SurfaceBottomRight, _localSurface.SurfaceBottomRight, _localSurface.SurfaceTopRight, _remoteSurfaceProxy.SurfaceTopRight, true);
        GameObject topWall =  _negativeSpaceWalls("topWall", _remoteSurfaceProxy.SurfaceTopLeft, _remoteSurfaceProxy.SurfaceTopRight, _localSurface.SurfaceTopRight, _localSurface.SurfaceTopLeft, true);
        GameObject screen = _negativeSpaceWalls("Screen", _localSurface.SurfaceBottomLeft, _localSurface.SurfaceBottomRight, _localSurface.SurfaceTopRight, _localSurface.SurfaceTopLeft, false);
        
        MeshCollider collider = screen.AddComponent(typeof(MeshCollider)) as MeshCollider;
        _screen = screen;
        _screen.SetActive(false);


        GameObject workspace = new GameObject("workspaceCollider");
        workspace.AddComponent<BoxCollider>();
        workspace.transform.position = (_localSurface.SurfaceBottomLeft + _remoteSurfaceProxy.SurfaceBottomRight) * 0.5f;
        workspace.transform.localScale = new Vector3(
            Vector3.Distance(_localSurface.SurfaceBottomLeft, _localSurface.SurfaceBottomRight) * 4.0f, 
            0.001f, 
            Vector3.Distance(_localSurface.SurfaceBottomLeft, _localSurface.SurfaceTopLeft) * 1.5f);
        //workspace.transform.position += Vector3.back * 0.5f * Vector3.Distance(_localSurface.SurfaceBottomLeft, _remoteSurfaceProxy.SurfaceBottomLeft);
        workspaceCollider = workspace;

        NegativeSpaceCenter = new GameObject("NegativeSpaceCenter");
        NegativeSpaceCenter.transform.position = (_localSurface.SurfaceBottomLeft + _remoteSurfaceProxy.SurfaceTopRight) * 0.5f;
        NegativeSpaceCenter.transform.rotation = workspace.transform.rotation = GameObject.Find("localScreenCenter").transform.rotation;

        //workspace.transform.Rotate(workspace.transform.right, 45.0f);

        bottomCenterPosition = (_localSurface.SurfaceBottomLeft + _remoteSurfaceProxy.SurfaceBottomRight) * 0.5f;



        /*_handCursor = new GameObject("HandCursor");
        _handCursor.transform.position = Vector3.zero;
        _handCursor.transform.rotation = Quaternion.identity;
        _handCursor.transform.parent = _main.LocalOrigin.transform;
        _handCursor.AddComponent<HandCursor>();

        GameObject projector = GameObject.Find("Projector");
        projector.transform.parent = _handCursor.transform;
        projector.transform.localPosition = Vector3.zero;
        projector.transform.rotation = _handCursor.transform.rotation;

    */


        


        _spaceCreated = true;
    }

    private GameObject _negativeSpaceWalls(string name, Vector3 a, Vector3 b, Vector3 c, Vector3 d, bool haveRenderer)
    {
        GameObject o = new GameObject(name);
        o.transform.position = Vector3.zero;
        o.transform.rotation = Quaternion.identity;
        o.transform.parent = transform;

        MeshFilter meshFilter = (MeshFilter)o.AddComponent(typeof(MeshFilter));
        Mesh m = new Mesh()
        {
            name = "NegativeSpaceMesh",
            vertices = new Vector3[] { a, b, c, d },
            triangles = new int[]
            {
                0, 1, 2,
                0, 2, 3
            }
        };
        Vector2[] uv = new Vector2[m.vertices.Length];
        for (int i = 0; i < uv.Length; i++)
        {
            uv[i] = new Vector2(m.vertices[i].x, m.vertices[i].z);
        }
        m.uv = uv;

        m.RecalculateNormals();
        m.RecalculateBounds();

        meshFilter.mesh = m;
        if (haveRenderer)
        {
            MeshRenderer renderer = o.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
            renderer.material = negativeSpaceMaterial;
        }
            //MeshCollider collider = o.AddComponent(typeof(MeshCollider)) as MeshCollider;
        //Rigidbody rigidbody = o.AddComponent(typeof(Rigidbody)) as Rigidbody;
        //rigidbody.useGravity = false;
        //rigidbody.isKinematic = true;

        return o;
    }

    private void _createNegativeSpaceMesh()
    {
        MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent(typeof(MeshFilter));
        Mesh m = new Mesh()
        {
            name = "NegativeSpaceMesh",
            vertices = new Vector3[]
            {
                _localSurface.SurfaceBottomLeft, _localSurface.SurfaceBottomRight, _localSurface.SurfaceTopRight, _localSurface.SurfaceTopLeft,
                _remoteSurfaceProxy.SurfaceBottomLeft, _remoteSurfaceProxy.SurfaceBottomRight, _remoteSurfaceProxy.SurfaceTopRight, _remoteSurfaceProxy.SurfaceTopLeft
            },

            triangles = new int[]
            {
                    0, 4, 3,
                    0, 1, 4,
                    1, 5, 4,
                    1, 2, 5,
                    2, 6, 5,
                    2, 7, 6,
                    3, 7, 2,
                    3, 4, 7
            }
        };
        Vector2[] uv = new Vector2[m.vertices.Length];
        for (int i = 0; i < uv.Length; i++)
        {
            uv[i] = new Vector2(m.vertices[i].x, m.vertices[i].z);
        }
        m.uv = uv;

        m.RecalculateNormals();
        m.RecalculateBounds();

        meshFilter.mesh = m;
        MeshRenderer renderer = gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        renderer.material = negativeSpaceMaterial;
        MeshCollider collider = gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
    }

    void Update()
    {
        if (_spaceCreated)
        {
            CommonUtils.drawSurface(_localSurface.SurfaceBottomLeft, _localSurface.SurfaceBottomRight, _localSurface.SurfaceTopRight, _localSurface.SurfaceTopLeft, Color.red);
            CommonUtils.drawSurface(_remoteSurfaceProxy.SurfaceBottomLeft, _remoteSurfaceProxy.SurfaceBottomRight, _remoteSurfaceProxy.SurfaceTopRight, _remoteSurfaceProxy.SurfaceTopLeft, Color.green);

            if (_bodiesManager.human != null) updateBody(_bodiesManager.human, GameObject.Find("LocalBody"));
            if (_bodiesManager.remoteHuman != null) updateBody(_bodiesManager.remoteHuman, GameObject.Find("RemoteBody"));


            if (_bodiesManager.human != null)
            {
                Vector3 head = _bodiesManager.human.body.Joints[BodyJointType.head];
                Vector3 leftElbow = _bodiesManager.human.body.Joints[BodyJointType.leftElbow];
                Vector3 leftHand = _bodiesManager.human.body.Joints[BodyJointType.leftHand];
                Vector3 rightElbow = _bodiesManager.human.body.Joints[BodyJointType.rightElbow];
                Vector3 rightHand = _bodiesManager.human.body.Joints[BodyJointType.rightHand];

                Vector3 dummy;

                isUsingDeitics = _isLocalPointingToScreen(head, leftHand, out dummy)
                                || _isLocalPointingToScreen(head, rightHand, out dummy)
                                || _isLocalPointingToScreen(leftElbow, leftHand, out dummy)
                                || _isLocalPointingToScreen(rightElbow, rightHand, out dummy);
            }
        }
    }

    private void updateBody(Human human, GameObject go)
    {
        Vector3 head = human.body.Joints[BodyJointType.head];
        Vector3 leftHand = human.body.Joints[BodyJointType.leftHand];
        Vector3 rightHand = human.body.Joints[BodyJointType.rightHand];

        _filteredHandPosition.Value = _handheldListener.Message.Hand == HandType.Left ? leftHand : rightHand;

        if (Application.isEditor && go != null && go.activeSelf)
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



            if (go.name == "RemoteBody")
            {
                if (correctingPointing) _applyDisplacement(human, go);
            }
            else
            {
                if (_main.location == Location.Instructor)
                {
                    Ray ray;
                    Vector3 hit;
                    RaycastHit hitInfo;

                    // right
                    ray = new Ray(human.body.Joints[BodyJointType.rightHandTip], human.body.Joints[BodyJointType.rightHandTip] - human.body.Joints[BodyJointType.head]);
                    if (Physics.Raycast(ray, out hitInfo)) _client.sendRightHit(workspaceCollider.transform.worldToLocalMatrix.MultiplyPoint(hitInfo.point));
                    else _client.sendRightHit(Vector3.positiveInfinity);

                    // left
                    ray = new Ray(human.body.Joints[BodyJointType.rightHandTip], human.body.Joints[BodyJointType.rightHandTip] - human.body.Joints[BodyJointType.head]);
                    if (Physics.Raycast(ray, out hitInfo)) _client.sendLeftHit(workspaceCollider.transform.worldToLocalMatrix.MultiplyPoint(hitInfo.point));
                    else _client.sendLeftHit(Vector3.positiveInfinity);

                    // cursors2 experiment - Ziz workz =)

                    // right
                    if (_isLocalPointingToScreen(human.body.Joints[BodyJointType.head], human.body.Joints[BodyJointType.rightHandTip], out hit))
                        _cursors.rightHandPixel = Camera.main.WorldToScreenPoint(hit);
                    else
                        _cursors.rightHandPixel = Vector2.positiveInfinity;

                    // left
                    if (_isLocalPointingToScreen(human.body.Joints[BodyJointType.head], human.body.Joints[BodyJointType.leftHandTip], out hit))
                        _cursors.leftHandPixel = Camera.main.WorldToScreenPoint(hit);
                    else
                        _cursors.leftHandPixel = Vector2.positiveInfinity;
                }
            }
        }
    }

    private void _applyDisplacement(Human human, GameObject go)
    {
        Vector3 head = go.transform.Find("HEAD").position;

        Vector3 leftShoulder = go.transform.Find("LEFTSHOULDER").position;
        Vector3 leftTip = go.transform.Find("LEFTHANDTIP").position;

        Vector3 rightShoulder = go.transform.Find("RIGHTSHOULDER").position;
        Vector3 rightTip = go.transform.Find("RIGHTHANDTIP").position;

        if(float.IsPositiveInfinity(lastRightHandPos.x))
        {
            lastRightHandPos = rightTip;
        }

        if (float.IsPositiveInfinity(lastLeftHandPos.x))
        {
            lastLeftHandPos = leftTip;
        }

        Vector3 leftHit = Vector3.zero;
        //bool leftPointing = _isPointingToWorkspace(new Ray(leftTip, leftTip - head), workspaceCollider, out leftHit);
        bool leftPointing = !float.IsPositiveInfinity(remoteLeftHit.x);
        if (leftPointing) leftHit = workspaceCollider.transform.localToWorldMatrix.MultiplyPoint(remoteLeftHit);

        Vector3 rightHit = Vector3.zero;
        //bool rightPointing = _isPointingToWorkspace(new Ray(rightTip, rightTip - head), workspaceCollider, out rightHit);
        bool rightPointing = !float.IsPositiveInfinity(remoteRightHit.x);
        if (rightPointing) rightHit = workspaceCollider.transform.localToWorldMatrix.MultiplyPoint(remoteRightHit);

        Transform leftHand_d = go.transform.Find("LEFTHAND_D");
        leftHand_d.position = leftTip;

        Transform rightHand_d = go.transform.Find("RIGHTHAND_D");
        rightHand_d.position = rightTip;

        Matrix4x4 m = Matrix4x4.identity;

        // Left Pointing

        m = _correctPointing(leftShoulder, leftTip, head, leftPointing, leftHit, ref lastLeftHandPos);

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

        m = _correctPointing(rightShoulder, rightTip, head, rightPointing, rightHit, ref lastRightHandPos);

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

    private Matrix4x4 _correctPointing(Vector3 elbow, Vector3 tip, Vector3 head, bool isPointing, Vector3 hit, ref Vector3 lastHandPosition)
    {
        Vector3 oldVector = tip - elbow;
        Vector3 newVector = oldVector;
        Vector3 newHandPosition;

        if (isPointing)
        {
            Vector3 rightHitToLocal = workspaceCollider.transform.InverseTransformPoint(hit);
            Vector3 reflectedPoint = new Vector3(rightHitToLocal.x, rightHitToLocal.y, -rightHitToLocal.z);
            Vector3 reflectedPointWorld = workspaceCollider.transform.TransformPoint(reflectedPoint);

            Debug.DrawLine(head, hit, Color.cyan);
            Debug.DrawLine(elbow, reflectedPointWorld, Color.yellow);

            newVector = reflectedPointWorld - elbow;

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
    }

    private List<Vector3> _calcPoints(Vector3 A, Vector3 B, float inc)
    {
        List<Vector3> points = new List<Vector3>();

        Vector3 dir = B - A;
        for (float i = 0f; i*inc <= Vector3.Distance(A, B); i += 1)
        {
            points.Add(A + dir.normalized * i * inc);
            print(A + dir * i);
        }
        return points;
    }

    private static bool _isPointingToWorkspace(Ray ray, GameObject workspace, out Vector3 hitpoint)
    {
        hitpoint = Vector3.zero;
        RaycastHit hit;
        if (workspace.GetComponent<Collider>().Raycast(ray, out hit, 1000.0f))
        {
            //print("hit = " + hit.transform.name + " " + hit.point);
            hitpoint = hit.point;
            return true;
        }

        return false;
    }

    private bool _isLocalPointingToScreen(Vector3 A, Vector3 B, out Vector3 hitPoint)
    {
        Vector3 dir = (B - A).normalized;
        Ray ray = new Ray(A, dir);

        if (_screen != null)
        {
            _screen.SetActive(true);
            RaycastHit hit;
            if (_screen.GetComponent<Collider>().Raycast(ray, out hit, 1000.0f))
            {
                _screen.SetActive(false);
                hitPoint = hit.point;
                return true;
            }
            _screen.SetActive(false);
        }
        hitPoint = Vector3.positiveInfinity;
        return false;
    }

}
