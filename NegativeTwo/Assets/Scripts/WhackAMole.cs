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

public class WhackAMole : MonoBehaviour {

    private bool _init = false;

    private Transform _workspace;
    private NegativeSpace _negativeSpace;

    public GameObject cubePrefab;
    public Vector3 CubesScale;

    private List<GameObject> _availableCubes; 


    void Start()
    {
        _availableCubes = null;
    }

    internal void Init()
    {
        _negativeSpace = GameObject.Find("Main").GetComponent<NegativeSpace>();
        transform.position = (_negativeSpace.LocalSurface.SurfaceBottomLeft + _negativeSpace.RemoteSurface.SurfaceBottomRight) * 0.5f;
        transform.rotation = _negativeSpace.NegativeSpaceCenter.transform.rotation;

        _setupWorkspace(WhackAMoleSessionType.SIXTEEN);

        _init = true;
    }

    void Update ()
    {
        if (!_init) return;


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

        Vector3 O = _negativeSpace.RemoteSurface.SurfaceBottomRight;
        Vector3 length = _negativeSpace.RemoteSurface.SurfaceBottomLeft - O;
        Vector3 depth = _negativeSpace.LocalSurface.SurfaceBottomRight - O;

        if (session == WhackAMoleSessionType.FOUR) _distribute2x2(O, length, depth);
        else if (session == WhackAMoleSessionType.EIGHT) _distribute4x2(O, length, depth);
        else if (session == WhackAMoleSessionType.SIXTEEN) _distribute4x4(O, length, depth);
    }

    private void _cleanCubes()
    {
        if (_availableCubes != null) _availableCubes.RemoveAll(delegate (GameObject o) { Destroy(o); return o == null; });
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
