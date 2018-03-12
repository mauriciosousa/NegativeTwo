using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointingDistortionInfo
{
    public Matrix4x4 matrix;
    public Vector3 midPoint;
    public float distance;
}

public class NegativeSpace : MonoBehaviour {

    private Main _main;
    private Properties _properties;
    private BodiesManager _bodiesManager;

    private bool _spaceCreated = false;
    private Location _location;
    private SurfaceRectangle _localSurface;
    private SurfaceRectangle _remoteSurfaceProxy;
    private float _negativeSpaceLength;

    public Material negativeSpaceMaterial;
    

    private UDPHandheldListener _handheldListener;

    public GameObject NegativeSpaceCenter { get; private set;}

    private Dictionary<string, GameObject> _negativeSpaceObjects;
    private Dictionary<string, GameObject> negativeSpaceObjects { get { return _negativeSpaceObjects; } }

    private GameObject _handCursor;
    public Vector3 bottomCenterPosition { get; private set; }

    private AdaptiveDoubleExponentialFilterVector3 _filteredHandPosition;

    private GameObject workspaceCollider = null;

    public PointingDistortionInfo rightPointingInfo;
    public PointingDistortionInfo leftPointingInfo;

    public SurfaceRectangle LocalSurface
    {
        get
        {
            return _localSurface;
        }
    }

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



        //workspace.transform.localPosition = 0.5f*(_localSurface.SurfaceBottomLeft + _remoteSurfaceProxy.SurfaceBottomRight);
        //Plane bottomWallPlane = new Plane(_localSurface.SurfaceBottomLeft, _localSurface.SurfaceBottomRight, _remoteSurfaceProxy.SurfaceBottomRight);
        //workspace.transform.up = bottomWallPlane.normal;

        
        workspace.transform.localScale = new Vector3(Vector3.Distance(_localSurface.SurfaceBottomLeft, _localSurface.SurfaceBottomRight) * 1.5f, 0.001f, 1.5f);
        //workspace.transform.parent = bottomWall.transform;
        workspaceCollider = workspace;

        NegativeSpaceCenter = new GameObject("NegativeSpaceCenter");
        NegativeSpaceCenter.transform.position = (_localSurface.SurfaceBottomLeft + _remoteSurfaceProxy.SurfaceTopRight) * 0.5f;
        NegativeSpaceCenter.transform.rotation = workspace.transform.rotation = GameObject.Find("localScreenCenter").transform.rotation;

        bottomCenterPosition = (_localSurface.SurfaceBottomLeft + _remoteSurfaceProxy.SurfaceBottomRight) * 0.5f;

        _handCursor = new GameObject("HandCursor");
        _handCursor.transform.position = Vector3.zero;
        _handCursor.transform.rotation = Quaternion.identity;
        _handCursor.transform.parent = _main.LocalOrigin.transform;
        _handCursor.AddComponent<HandCursor>();

        GameObject projector = GameObject.Find("Projector");
        projector.transform.parent = _handCursor.transform;
        projector.transform.localPosition = Vector3.zero;
        projector.transform.rotation = _handCursor.transform.rotation;

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

            if (_bodiesManager.human != null) updateDebugBody(_bodiesManager.human, GameObject.Find("LocalBody"));
            if (_bodiesManager.remoteHuman != null) updateDebugBody(_bodiesManager.remoteHuman, GameObject.Find("RemoteBody"));


        }
    }

    private void updateDebugBody(Human human, GameObject go)
    {
        Vector3 head = human.body.Joints[BodyJointType.head];
        Vector3 leftHand = human.body.Joints[BodyJointType.leftWrist];
        Vector3 rightHand = human.body.Joints[BodyJointType.rightWrist];

        //_handCursor.transform.position = _handheldListener.Message.Hand == HandType.Left ? leftHand : rightHand;
        _filteredHandPosition.Value = _handheldListener.Message.Hand == HandType.Left ? leftHand : rightHand;
        _handCursor.transform.position = _filteredHandPosition.Value;

        if (_main.location == Location.Assembler) // Instructor cannot interact yolo
        {
            //_handCursor.GetComponent<HandCursor>().Update(_handheldListener.Message);
        }


        if (Application.isEditor && go != null && go.activeSelf)
        {

            go.transform.Find("HEAD").localPosition = head;
            go.transform.Find("LEFTHAND").localPosition = leftHand;
            go.transform.Find("RIGHTHAND").localPosition = rightHand;
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


            if (go.name == "RemoteBody")
            {
                _applyDisplacement(human, go);
            }

        }
    }

    private void _applyDisplacement(Human human, GameObject go)
    {

        Transform leftHand = go.transform.Find("LEFTHAND");
        Transform rightHand = go.transform.Find("RIGHTHAND");

        Transform leftElbow = go.transform.Find("LEFTELBOW");
        Transform rightElbow = go.transform.Find("RIGHTELBOW");

        //Vector3 leftShoulder = go.transform.Find("LEFTSHOULDER").position;
        //Vector3 rightShoulder = go.transform.Find("RIGHTSHOULDER").position;

        Vector3 leftHit = Vector3.zero;
        bool leftPointing = _isPointing(new Ray(leftHand.position, leftHand.position - leftElbow.position), workspaceCollider, out leftHit);

        Vector3 rightHit = Vector3.zero;
        bool rightPointing = _isPointing(new Ray(rightHand.position, rightHand.position - rightElbow.position), workspaceCollider, out rightHit);



        Plane bottomWallPlane = new Plane(_localSurface.SurfaceBottomLeft, _localSurface.SurfaceBottomRight, _remoteSurfaceProxy.SurfaceBottomRight);

        Transform leftHand_d = go.transform.Find("LEFTHAND_D");
        leftHand_d.position = leftHand.position;
        leftHand_d.rotation = leftHand.rotation;
        Transform rightHand_d = go.transform.Find("RIGHTHAND_D");
        rightHand_d.position = rightHand.position;
        rightHand_d.rotation = rightHand.rotation;

        Matrix4x4 rM = Matrix4x4.identity;

        if (rightPointing)
        {

            Vector3 rightHitToLocal = workspaceCollider.transform.InverseTransformPoint(rightHit);
            Vector3 reflectedPoint = new Vector3(rightHitToLocal.x, rightHitToLocal.y, -rightHitToLocal.z);
            Vector3 reflectedPointWorld = workspaceCollider.transform.TransformPoint(reflectedPoint);

            Debug.DrawLine(rightElbow.position, rightHit, Color.cyan);
            Debug.DrawLine(rightElbow.position, reflectedPointWorld, Color.yellow);

            Vector3 oldVector = rightHit - rightElbow.position;
            Vector3 newVector = reflectedPointWorld - rightElbow.position;

            Vector3 axis = Vector3.Cross(oldVector, newVector);
            float angle = Vector3.Angle(oldVector, newVector);
            
            rM = Matrix4x4.Translate(rightElbow.position) * Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(angle, axis), Vector3.one) * Matrix4x4.Translate(-rightElbow.position);
            rightHand_d.position = rM.MultiplyPoint(rightHand_d.position);
        }

        rightPointingInfo.matrix = rM;
        rightPointingInfo.midPoint = (rightElbow.position + human.body.Joints[BodyJointType.rightHandTip]) * 0.5f;
        rightPointingInfo.distance = (human.body.Joints[BodyJointType.rightHandTip] - rightElbow.position).magnitude * 0.5f;


        if (leftPointing)
        {

        }
        




        // todo: mudar as vars para as vars_D
        //go.transform.Find("LEFTSHOULDER_D").localPosition = leftShoulder;
        //go.transform.Find("RIGHTSHOULDER_D").localPosition = rightShoulder;

        //go.transform.Find("LEFTELBOW_D").localPosition = leftElbow;
        //go.transform.Find("RIGHTELBOW_D").localPosition = rightElbow;

        //go.transform.Find("LEFTHAND_D").localPosition = leftHand;
        //go.transform.Find("RIGHTHAND_D").localPosition = rightHand;
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
