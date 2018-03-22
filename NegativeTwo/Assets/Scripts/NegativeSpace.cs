using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointingDistortionInfo
{
    public Matrix4x4 matrix;
    public Vector3 midPoint;
    public float distance;
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

    public PointingDistortionInfo rightPointingInfo;
    public PointingDistortionInfo leftPointingInfo;


    void Awake()
    {
        _negativeSpaceObjects = new Dictionary<string, GameObject>();

        rightPointingInfo = new PointingDistortionInfo();
        leftPointingInfo = new PointingDistortionInfo();
    }

    void Start ()
    {
        _main = GetComponent<Main>();
        _properties = GetComponent<Properties>();
        _filteredHandPosition = new AdaptiveDoubleExponentialFilterVector3();
        _bodiesManager = GameObject.Find("BodiesManager").GetComponent<BodiesManager>();
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
        GameObject bottomWall = _negativeSpaceWalls("bottomWall", _localSurface.SurfaceBottomLeft, _localSurface.SurfaceBottomRight, _remoteSurfaceProxy.SurfaceBottomRight, _remoteSurfaceProxy.SurfaceBottomLeft);
        GameObject leftWall = _negativeSpaceWalls("leftWall", _localSurface.SurfaceBottomLeft, _remoteSurfaceProxy.SurfaceBottomLeft, _remoteSurfaceProxy.SurfaceTopLeft, _localSurface.SurfaceTopLeft);
        GameObject rightWall = _negativeSpaceWalls("rightWall", _remoteSurfaceProxy.SurfaceBottomRight, _localSurface.SurfaceBottomRight, _localSurface.SurfaceTopRight, _remoteSurfaceProxy.SurfaceTopRight);
        GameObject topWall =  _negativeSpaceWalls("topWall", _remoteSurfaceProxy.SurfaceTopLeft, _remoteSurfaceProxy.SurfaceTopRight, _localSurface.SurfaceTopRight, _localSurface.SurfaceTopLeft);

        GameObject workspace = new GameObject("workspaceCollider");
        workspace.AddComponent<BoxCollider>();
        workspace.transform.position = (_localSurface.SurfaceBottomLeft + _remoteSurfaceProxy.SurfaceBottomRight) * 0.5f;
        workspace.transform.localScale = new Vector3(Vector3.Distance(_localSurface.SurfaceBottomLeft, _localSurface.SurfaceBottomRight) * 1.2f, 0.001f, 2f);
        //workspace.transform.position += Vector3.back * 0.5f * Vector3.Distance(_localSurface.SurfaceBottomLeft, _remoteSurfaceProxy.SurfaceBottomLeft);
        workspaceCollider = workspace;

        NegativeSpaceCenter = new GameObject("NegativeSpaceCenter");
        NegativeSpaceCenter.transform.position = (_localSurface.SurfaceBottomLeft + _remoteSurfaceProxy.SurfaceTopRight) * 0.5f;
        NegativeSpaceCenter.transform.rotation = workspace.transform.rotation = GameObject.Find("localScreenCenter").transform.rotation;

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

    private GameObject _negativeSpaceWalls(string name, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        GameObject o = new GameObject(name);
        o.transform.position = Vector3.zero;
        o.transform.rotation = Quaternion.identity;
        o.transform.parent = transform;

        MeshFilter meshFilter = (MeshFilter)o.AddComponent(typeof(MeshFilter));
        Mesh m = new Mesh();
        m.name = "NegativeSpaceMesh";
        m.vertices = new Vector3[] { a, b, c, d };
        m.triangles = new int[]
            {
                0, 1, 2,
                0, 2, 3
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
        MeshRenderer renderer = o.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        renderer.material = negativeSpaceMaterial;
        //MeshCollider collider = o.AddComponent(typeof(MeshCollider)) as MeshCollider;
        //Rigidbody rigidbody = o.AddComponent(typeof(Rigidbody)) as Rigidbody;
        //rigidbody.useGravity = false;
        //rigidbody.isKinematic = true;

        return o;
    }

    private void _createNegativeSpaceMesh()
    {
        MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent(typeof(MeshFilter));
        Mesh m = new Mesh();
        m.name = "NegativeSpaceMesh";
        m.vertices = new Vector3[]
            {
                _localSurface.SurfaceBottomLeft, _localSurface.SurfaceBottomRight, _localSurface.SurfaceTopRight, _localSurface.SurfaceTopLeft,
                _remoteSurfaceProxy.SurfaceBottomLeft, _remoteSurfaceProxy.SurfaceBottomRight, _remoteSurfaceProxy.SurfaceTopRight, _remoteSurfaceProxy.SurfaceTopLeft
            };

        m.triangles = new int[]
            {
                    0, 4, 3,
                    0, 1, 4,
                    1, 5, 4,
                    1, 2, 5,
                    2, 6, 5,
                    2, 7, 6,
                    3, 7, 2,
                    3, 4, 7
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
        }
    }

    private void updateBody(Human human, GameObject go)
    {
        Vector3 head = human.body.Joints[BodyJointType.head];
        Vector3 leftHand = human.body.Joints[BodyJointType.leftHand];
        Vector3 rightHand = human.body.Joints[BodyJointType.rightHand];

        //_handCursor.transform.position = _handheldListener.Message.Hand == HandType.Left ? leftHand : rightHand;
        _filteredHandPosition.Value = _handheldListener.Message.Hand == HandType.Left ? leftHand : rightHand;
        
        //_handCursor.transform.position = _filteredHandPosition.Value;

        if (_main.location == Location.Assembler) // Instructor cannot interact yolo
        {
            //_handCursor.GetComponent<HandCursor>().Update(_handheldListener.Message);
        }


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
                _applyDisplacement(human, go);
            }

        }
    }

    private void _applyDisplacement(Human human, GameObject go)
    {
        Vector3 leftPointingA = go.transform.Find("LEFTELBOW").position;
        Vector3 leftPointingB = go.transform.Find("LEFTHANDTIP").position;

        Vector3 rightPointingA = go.transform.Find("RIGHTELBOW").position;
        Vector3 rightPointingB = go.transform.Find("RIGHTHANDTIP").position;

        Vector3 leftHit = Vector3.zero;
        bool leftPointing = _isPointing(new Ray(leftPointingB, leftPointingB - leftPointingA), workspaceCollider, out leftHit);

        Vector3 rightHit = Vector3.zero;
        bool rightPointing = _isPointing(new Ray(rightPointingB, rightPointingB - rightPointingA), workspaceCollider, out rightHit);


        Plane bottomWallPlane = new Plane(_localSurface.SurfaceBottomLeft, _localSurface.SurfaceBottomRight, _remoteSurfaceProxy.SurfaceBottomRight);

        Transform leftHand_d = go.transform.Find("LEFTHAND_D");
        leftHand_d.position = leftPointingB;

        Transform rightHand_d = go.transform.Find("RIGHTHAND_D");
        rightHand_d.position = rightPointingB;

        Matrix4x4 rM = Matrix4x4.identity;


        if (leftPointing)
        {
            Vector3 leftHitToLocal = workspaceCollider.transform.InverseTransformPoint(leftHit);
            Vector3 reflectedPoint = new Vector3(leftHitToLocal.x, leftHitToLocal.y, -leftHitToLocal.z);

            Vector3 reflectedPointWorld = workspaceCollider.transform.TransformPoint(reflectedPoint);

            Debug.DrawLine(leftPointingA, leftHit, Color.cyan);
            Debug.DrawLine(leftPointingA, reflectedPointWorld, Color.yellow);

            Vector3 oldVector = leftHit - leftPointingA;
            Vector3 newVector = reflectedPointWorld - leftPointingA;

            Vector3 axis = Vector3.Cross(oldVector, newVector);
            float angle = Vector3.Angle(oldVector, newVector);

            rM = Matrix4x4.Translate(leftPointingA) * Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(angle, axis), Vector3.one) * Matrix4x4.Translate(-leftPointingA);
            leftHand_d.position = rM.MultiplyPoint(leftHand_d.position);

            leftPointingInfo.matrix = rM;
            leftPointingInfo.midPoint = (leftPointingA + leftPointingB) * 0.5f;
            leftPointingInfo.distance = 0.15f; //(leftPointingB - leftPointingA).magnitude * 0.5f + 0.1f;
            leftPointingInfo.Elbow = leftPointingA;
            leftPointingInfo.Elbow = go.transform.Find("LEFTELBOW").transform.position;
            leftPointingInfo.Wrist = go.transform.Find("LEFTWRIST").transform.position;
            leftPointingInfo.Hand = go.transform.Find("LEFTHAND").transform.position;
            leftPointingInfo.HandTip = go.transform.Find("LEFTHANDTIP").transform.position;
            leftPointingInfo.pointing = true;
        }
        else
        {
            leftPointingInfo.pointing = false;
        }


        if (rightPointing)
        {
            Vector3 rightHitToLocal = workspaceCollider.transform.InverseTransformPoint(rightHit);
            Vector3 reflectedPoint = new Vector3(rightHitToLocal.x, rightHitToLocal.y, -rightHitToLocal.z);
            Vector3 reflectedPointWorld = workspaceCollider.transform.TransformPoint(reflectedPoint);

            Debug.DrawLine(rightPointingA, rightHit, Color.cyan);
            Debug.DrawLine(rightPointingA, reflectedPointWorld, Color.yellow);

            Vector3 oldVector = rightHit - rightPointingA;
            Vector3 newVector = reflectedPointWorld - rightPointingA;

            Vector3 axis = Vector3.Cross(oldVector, newVector);
            float angle = Vector3.Angle(oldVector, newVector);

            rM = Matrix4x4.Translate(rightPointingA) * Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(angle, axis), Vector3.one) * Matrix4x4.Translate(-rightPointingA);
            rightHand_d.position = rM.MultiplyPoint(rightHand_d.position);


            rightPointingInfo.matrix = rM;
            rightPointingInfo.midPoint = (rightPointingA + rightPointingB) * 0.5f;
            rightPointingInfo.distance = 0.15f;//(rightPointingB - rightPointingA).magnitude * 0.5f + 0.1f;
            rightPointingInfo.Elbow = go.transform.Find("RIGHTELBOW").transform.position;
            rightPointingInfo.Wrist = go.transform.Find("RIGHTWRIST").transform.position;
            rightPointingInfo.Hand = go.transform.Find("RIGHTHAND").transform.position;
            rightPointingInfo.HandTip = go.transform.Find("RIGHTHANDTIP").transform.position;
            rightPointingInfo.pointing = true;
        }
        else
        {
            rightPointingInfo.pointing = false;
        }
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

    private static bool _isPointing(Ray ray, GameObject workspace, out Vector3 hitpoint)
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

}
