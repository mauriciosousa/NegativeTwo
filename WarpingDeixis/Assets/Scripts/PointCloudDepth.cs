
using UnityEngine;
using System.Collections.Generic;

public class PointCloudDepth : MonoBehaviour
{
    uint _id;
    Texture2D _colorTex;
    Texture2D _depthTex;
    List<GameObject> _objs;
    Material _mat;
    RVLDecoder _decoder;
    Decompressor _colorDecoder;
    byte[] _depthBytes;
    byte[] _colorBytes;
    int _texScale;
    int _width;
    int _height;
    void Start()
    {
        _width = 512;
        _height = 424;
        _texScale = 1;
        _objs = null;


        _mat = Resources.Load("Materials/cloudmatDepth") as Material;

        _decoder = new RVLDecoder();
        _colorDecoder = new Decompressor();

        initStructs();
    }

    int createSubmesh(int h, int submeshHeight, int id)
    {
        List<Vector3> points = new List<Vector3>();
      //  List<int> ind = new List<int>();
        List<int> tri = new List<int>();
        int n = 0;
        
        for (int k = 0; k < submeshHeight; k++,h++)
        {
            for (int w = 0; w < _width; w++)
            {
                Vector3 p = new Vector3(w / (float)_width, h / (float)_height, 0);
                points.Add(p);
                // ind.Add(n);

                // Skip the last row/col
                if (w != (_width - 1) && k != (submeshHeight - 1))
                {
                    int topLeft = n;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + _width;
                    int bottomRight = bottomLeft + 1;

                    tri.Add(topLeft);
                    tri.Add(topRight);
                    tri.Add(bottomLeft);
                    tri.Add(bottomLeft);
                    tri.Add(topRight);
                    tri.Add(bottomRight);
                }
                n++;
            }
        }

        GameObject a = new GameObject("cloud" + id);
        MeshFilter mf = a.AddComponent<MeshFilter>();
        MeshRenderer mr = a.AddComponent<MeshRenderer>();
        mr.material = _mat;
        mf.mesh = new Mesh();
        mf.mesh.vertices = points.ToArray();
      //  mf.mesh.SetIndices(ind.ToArray(), MeshTopology.Triangles, 0);
        mf.mesh.SetTriangles(tri.ToArray(), 0);
        mf.mesh.bounds = new Bounds(new Vector3(0, 0, 4.5f), new Vector3(5, 5, 5));
        a.transform.parent = this.gameObject.transform;
        a.transform.localPosition = Vector3.zero;
        a.transform.localRotation = Quaternion.identity;
        a.transform.localScale = new Vector3(1, 1, 1);
        n = 0;
        _objs.Add(a);
       
        return h;
    }

    void createStitchingMesh(int submeshHeight, int id)
    {
        List<Vector3> points = new List<Vector3>();
        //  List<int> ind = new List<int>();
        List<int> tri = new List<int>();
        int n = 0;

        for (int h = submeshHeight-1; h < _height; h+=submeshHeight)
        {
            for(int i = 0; i < 2; i++)
            {
                for (int w = 0; w < _width; w++)
                {
                    Vector3 p = new Vector3(w / (float)_width, (h+i)/ (float)_height, 0);

                    points.Add(p);
                    // ind.Add(n);

                    // Skip the last row/col
                    if (w != (_width - 1) && i ==0)
                    {
                        int topLeft = n;
                        int topRight = topLeft + 1;
                        int bottomLeft = topLeft + _width;
                        int bottomRight = bottomLeft + 1;

                        tri.Add(topLeft);
                        tri.Add(topRight);
                        tri.Add(bottomLeft);
                        tri.Add(bottomLeft);
                        tri.Add(topRight);
                        tri.Add(bottomRight);
                    }
                    n++;
                }
            }
        }

        GameObject a = new GameObject("cloud" + id);
        MeshFilter mf = a.AddComponent<MeshFilter>();
        MeshRenderer mr = a.AddComponent<MeshRenderer>();
        mr.material = _mat;
        mf.mesh = new Mesh();
        mf.mesh.vertices = points.ToArray();
        //  mf.mesh.SetIndices(ind.ToArray(), MeshTopology.Triangles, 0);
        mf.mesh.SetTriangles(tri.ToArray(), 0);
        mf.mesh.bounds = new Bounds(new Vector3(0, 0, 4.5f), new Vector3(5, 5, 5));
        a.transform.parent = this.gameObject.transform;
        a.transform.localPosition = Vector3.zero;
        a.transform.localRotation = Quaternion.identity;
        a.transform.localScale = new Vector3(1, 1, 1);
        n = 0;
        _objs.Add(a);
    }

    void initStructs()
    {
        _colorTex = new Texture2D(_width, _height, TextureFormat.BGRA32, false);
        _depthTex = new Texture2D(_width, _height, TextureFormat.BGRA32, false);
        _colorTex.filterMode = FilterMode.Point;
        _depthTex.filterMode = FilterMode.Point;
        _depthBytes = new byte[_width * _height * 4];
        if (_objs != null)
        {
            foreach (GameObject g in _objs)
            {
                GameObject.Destroy(g);
            }
        }
        _objs = new List<GameObject>();

        int w = 0;
        int h = 0;
        int submeshes;
        for(submeshes = 0; submeshes < 4; submeshes++)
        {
           h = createSubmesh(h, _height / 4, submeshes);
            
        }
        createStitchingMesh(_height / 4, submeshes);

    }

    public void hide()
    {
        foreach (GameObject a in _objs)
            a.SetActive(false);
    }

    public void show()
    {
        foreach (GameObject a in _objs)
            a.SetActive(true);
    }

    public void setPoints(byte[] colorBytes, byte[] depthBytes, bool compressed, int sizec, int scale)
    {
        if (scale != _texScale)
        {
            _texScale = scale;
            _width = Mathf.CeilToInt(512.0f / scale);
            _height = Mathf.CeilToInt(424.0f / scale);
            initStructs();
        }

        if (compressed)
        {
            bool ok = _decoder.DecompressRVL(depthBytes, _depthBytes, _width * _height);
            _colorDecoder.Decompress(colorBytes, colorBytes, sizec);
            if (ok)
            {
                _depthTex.LoadRawTextureData(_depthBytes);
                _colorTex.LoadRawTextureData(colorBytes);
            }
        }
        else
        {
            _depthTex.LoadRawTextureData(depthBytes);
            _colorTex.LoadRawTextureData(colorBytes);
        }


        _colorTex.Apply();
        _depthTex.Apply();

        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();

        BodyWarping m = GameObject.Find("DeixisEvaluation").GetComponent<BodyWarping>();

        for (int i = 0; i < renderers.Length; i++)
        {
            MeshRenderer mr = renderers[i];
            mr.material.SetInt("_TexScale", _texScale);
            mr.material.SetTexture("_ColorTex", _colorTex);
            mr.material.SetTexture("_DepthTex", _depthTex);

            mr.material.SetVector("_VRHeadPos", m.getLocalHead());
            mr.material.SetFloat("_HeadSize", m.headSize);
            // as shader adventures do mau e do dan (o resto é do rafinha)
            mr.material.SetMatrix("_LeftTransform", m.leftPointingInfo.matrix);
            mr.material.SetVector("_LeftMidPoint", m.leftPointingInfo.midPoint);
            mr.material.SetFloat("_LeftDistance", m.leftPointingInfo.distance);
            mr.material.SetVector("_LeftShoulder", m.leftPointingInfo.Shoulder);
            mr.material.SetVector("_LeftElbow", m.leftPointingInfo.Elbow);
            mr.material.SetVector("_LeftWrist", m.leftPointingInfo.Wrist);
            mr.material.SetVector("_LeftHand", m.leftPointingInfo.Hand);
            mr.material.SetVector("_LeftHandTip", m.leftPointingInfo.HandTip);
            mr.material.SetInt("_LeftPointing", m.leftPointingInfo.pointing ? 1 : 0);


            mr.material.SetMatrix("_RightTransform", m.rightPointingInfo.matrix);
            mr.material.SetVector("_RightMidPoint", m.rightPointingInfo.midPoint);
            mr.material.SetFloat("_RightDistance", m.rightPointingInfo.distance);
            mr.material.SetVector("_RightShoulder", m.rightPointingInfo.Shoulder);
            mr.material.SetVector("_RightElbow", m.rightPointingInfo.Elbow);
            mr.material.SetVector("_RightWrist", m.rightPointingInfo.Wrist);
            mr.material.SetVector("_RightHand", m.rightPointingInfo.Hand);
            mr.material.SetVector("_RightHandTip", m.rightPointingInfo.HandTip);
            mr.material.SetInt("_RightPointing", m.rightPointingInfo.pointing ? 1 : 0);

        }
    }

}
