using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WhackAMoleSessionType
{
    FOUR = 4,
    EIGHT = 8, 
    SIXTEEN = 16
}

public enum EvaluationConditionType
{
    NONE,
    SIMULATED_SIDE_SIDE, 
    MIRRORED_PERSON,
    SIMULATED_SIDE_SIDE_Depth,
    MIRRORED_PERSON_Depth
}

public class WhackAMole : MonoBehaviour {

    private bool _init = false;
    public bool IsInit { get { return _init; } }

    private Transform _workspace;
    private NegativeSpace _negativeSpace;

    public GameObject cubePrefab;
    public Vector3 CubesScale;

    private List<GameObject> _availableCubes;

    private EvaluationConditionType condition = EvaluationConditionType.NONE;

    void Start()
    {
        _availableCubes = null;
    }



    internal void Init()
    {
        _negativeSpace = GameObject.Find("Main").GetComponent<NegativeSpace>();
        transform.position = (_negativeSpace.LocalSurface.SurfaceBottomLeft + _negativeSpace.RemoteSurface.SurfaceBottomRight) * 0.5f;
        transform.rotation = _negativeSpace.NegativeSpaceCenter.transform.rotation;

        _init = true;
    }

    void Update ()
    {
        if (!_init) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _setupWorkspace(WhackAMoleSessionType.FOUR);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _setupWorkspace(WhackAMoleSessionType.EIGHT);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _setupWorkspace(WhackAMoleSessionType.SIXTEEN);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            _cleanCubes();
        }

 	}

    private void _setupWorkspace(WhackAMoleSessionType session)
    {
        int numberOfCubes = (int)session;

        _availableCubes = new List<GameObject>();
        for (int i = 1; i <= numberOfCubes; i++)
        {
            _availableCubes.Add(_instantiateNewCube("cube_" + i));
        }

        Vector3 origin = _negativeSpace.RemoteSurface.SurfaceBottomRight;
        Vector3 length = _negativeSpace.RemoteSurface.SurfaceBottomLeft - origin;
        Vector3 depth = _negativeSpace.LocalSurface.SurfaceBottomRight - origin;

        if (session == WhackAMoleSessionType.FOUR) _distribute2x2(origin, length, depth);
        else if (session == WhackAMoleSessionType.EIGHT) _distribute4x2(origin, length, depth);
        else if (session == WhackAMoleSessionType.SIXTEEN) _distribute4x4(origin, length, depth);
    }

    private void _cleanCubes()
    {
        if (_availableCubes != null) _availableCubes.RemoveAll(delegate (GameObject o) { Destroy(o); return o == null; });
    }

    internal void PointingEvent(Vector3 origin, Vector3 direction)
    {
        if (!_init) return;

        RaycastHit hit;
        Ray ray = new Ray(origin, direction);
        if (Physics.Raycast(ray, out hit, float.PositiveInfinity))
        {
            Debug.DrawLine(origin, hit.point, Color.red);

            float distance = float.PositiveInfinity;
            GameObject selectedCube = null;
            foreach (GameObject cube in _availableCubes)
            {
                float newDistance = Vector3.Distance(hit.point, cube.transform.position);
                if ( newDistance < distance)
                {
                    selectedCube = cube;
                    distance = newDistance;
                }
            }
            print(selectedCube.ToString());
        }   
    }

    public bool IAmPointing(Ray ray, bool click, out Vector3 hitPoint)
    {
        print("POINT_NIG YEAHHH");

        hitPoint = Vector3.zero;
        bool didHit = false;

        if (!_init) return didHit;

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.PositiveInfinity))
        {
            didHit = true;
            hitPoint = hit.point;

            Highlight h = hit.transform.gameObject.GetComponent<Highlight>();
            if (h != null)
            {
                print("I pointed to a " + hit.transform.gameObject.name + " and I liked it!!!");
            }
        }
        else
        {

        }

        if (click)
        {
            
        }
        return didHit;
    }

    private GameObject _instantiateNewCube(string cubeName)
    {
        GameObject newCube = Instantiate(cubePrefab, transform) as GameObject;
        newCube.name = cubeName;
        newCube.transform.localScale = CubesScale;
        newCube.transform.localPosition = new Vector3(0f, 0f + CubesScale.y / 2, 0f);
        newCube.transform.localRotation = Quaternion.identity;
        return newCube;
    }



    private void _upTheCubes()
    {
        foreach (GameObject c in _availableCubes)
        {
            c.transform.position += c.transform.up * (c.transform.lossyScale.y / 2);
        }
    }

    private void _distribute2x2(Vector3 o, Vector3 length, Vector3 depth)
    {
        _availableCubes[0].transform.position = o + (length / 4) + (depth / 4);
        _availableCubes[1].transform.position = o + (3 * length / 4) + (depth / 4);
        _availableCubes[2].transform.position = o + (length / 4) + (3 * depth / 4);
        _availableCubes[3].transform.position = o + (3 * length / 4) + (3 * depth / 4);
        _upTheCubes();
    }

    private void _distribute4x2(Vector3 o, Vector3 length, Vector3 depth)
    {
        _availableCubes[0].transform.position = o + (length / 5) + (depth / 4);
        _availableCubes[1].transform.position = o + (2 * length / 5) + (depth / 4);
        _availableCubes[2].transform.position = o + (3 * length / 5) + (depth / 4);
        _availableCubes[3].transform.position = o + (4 * length / 5) + (depth / 4);
        _availableCubes[4].transform.position = o + (length / 5) + (3 * depth / 4);
        _availableCubes[5].transform.position = o + (2 * length / 5) + (3 * depth / 4);
        _availableCubes[6].transform.position = o + (3 * length / 5) + (3 * depth / 4);
        _availableCubes[7].transform.position = o + (4 * length / 5) + (3 * depth / 4);
        _upTheCubes();
    }

    private void _distribute4x4(Vector3 o, Vector3 length, Vector3 depth)
    {
        _availableCubes[0].transform.position = o + (length / 5) + (depth / 5);
        _availableCubes[1].transform.position = o + (2 * length / 5) + (depth / 5);
        _availableCubes[2].transform.position = o + (3 * length / 5) + (depth / 5);
        _availableCubes[3].transform.position = o + (4 * length / 5) + (depth / 5);

        _availableCubes[4].transform.position = o + (length / 5) + (2 * depth / 5);
        _availableCubes[5].transform.position = o + (2 * length / 5) + (2 * depth / 5);
        _availableCubes[6].transform.position = o + (3 * length / 5) + (2 * depth / 5);
        _availableCubes[7].transform.position = o + (4 * length / 5) + (2 * depth / 5);

        _availableCubes[8].transform.position = o + (length / 5) + (3 * depth / 5);
        _availableCubes[9].transform.position = o + (2 * length / 5) + (3 * depth / 5);
        _availableCubes[10].transform.position = o + (3 * length / 5) + (3 * depth / 5);
        _availableCubes[11].transform.position = o + (4 * length / 5) + (3 * depth / 5);

        _availableCubes[12].transform.position = o + (length / 5) + (4 * depth / 5);
        _availableCubes[13].transform.position = o + (2 * length / 5) + (4 * depth / 5);
        _availableCubes[14].transform.position = o + (3 * length / 5) + (4 * depth / 5);
        _availableCubes[15].transform.position = o + (4 * length / 5) + (4 * depth / 5);
        _upTheCubes();
    }


}
