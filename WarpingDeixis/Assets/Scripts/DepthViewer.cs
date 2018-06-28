using UnityEngine;

public class SubMesh
{
    public GameObject gameObject;

    private Mesh _Mesh;
    public Vector3[] _Vertices;
    private Vector2[] _UV;
    private int[] _Triangles;

    private Texture2D _texture;
    private int _width;
    private int _height;
    public Color[] colors;

    public SubMesh(int width, int height)
    {
        _width = width;
        _height = height;

        gameObject = new GameObject();
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>().material = Resources.Load("Materials/MaterialLouco") as Material;

        CreateMesh(width, height);

        _texture = new Texture2D(_width, _height);
        colors = new Color[_width * _height];
    }

    void CreateMesh(int width, int height)
    {
        _Mesh = new Mesh();
        gameObject.GetComponent<MeshFilter>().mesh = _Mesh;

        _Vertices = new Vector3[width * height];
        _UV = new Vector2[width * height];
        _Triangles = new int[6 * ((width - 1) * (height - 1))];

        int triangleIndex = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width) + x;

                _Vertices[index] = new Vector3(x, y, 0);
                _UV[index] = new Vector2(((float)x / (float)width), ((float)y / (float)height));

                // Skip the last row/col
                if (x != (width - 1) && y != (height - 1))
                {
                    int topLeft = index;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + width;
                    int bottomRight = bottomLeft + 1;

                    _Triangles[triangleIndex++] = topLeft;
                    _Triangles[triangleIndex++] = topRight;
                    _Triangles[triangleIndex++] = bottomLeft;
                    _Triangles[triangleIndex++] = bottomLeft;
                    _Triangles[triangleIndex++] = topRight;
                    _Triangles[triangleIndex++] = bottomRight;
                }
            }
        }

        _Mesh.vertices = _Vertices;
        _Mesh.uv = _UV;
        _Mesh.triangles = _Triangles;
        _Mesh.RecalculateNormals();
    }

    public void Apply()
    {
        _texture.SetPixels(colors);
        _texture.Apply();

        gameObject.GetComponent<Renderer>().material.mainTexture = _texture;

        _Mesh.vertices = _Vertices;
        //_Mesh.uv
        //_Mesh.triangles = _Triangles;
        //_Mesh.RecalculateNormals();
    }
}

public class DepthViewer : MonoBehaviour
{

    private SubMesh[] subMeshes;

    public Color[] colors;
    public ushort[] depth;

    //[Range(0,1)]
    private float depthScale = 0.3f;

    private int width = 512;
    private int height = 424;

    private GameObject meshes;

    private Main _main;

    private Vector3 originalLocalScale = new Vector3(0.01f, 0.01f, 0.008f);

    void Start()
    {
        meshes = new GameObject("Meshes");
        

        subMeshes = new SubMesh[4];
        for (int i = 0; i < subMeshes.Length; i++)
        {
            SubMesh subMesh = new SubMesh(width, height / subMeshes.Length);
            Vector3 position = subMesh.gameObject.transform.position;
            position.y = -(i * (height / (float)subMeshes.Length - 1.0f));
            subMesh.gameObject.name = "SubMesh" + i;
            subMesh.gameObject.transform.localPosition = position + new Vector3(- width / 2, height / 2, 0);
            //subMesh.gameObject.transform.parent = gameObject.transform;
            subMesh.gameObject.transform.parent = meshes.transform;
            subMeshes[i] = subMesh;
        }


        _main = GameObject.Find("Main").GetComponent<Main>();



        //gameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        meshes.transform.localScale = originalLocalScale;
        meshes.transform.parent = transform;
        meshes.transform.localPosition = Vector3.zero;
        meshes.transform.localRotation = Quaternion.identity;
    }

    void Update()
    {

        meshes.transform.localScale = originalLocalScale;
        if (_main.mirrorPessoa)
        {
            meshes.transform.localScale = new Vector3(originalLocalScale.x * -1, originalLocalScale.y, originalLocalScale.z);
        }
        

        int submeshLength = colors.Length / subMeshes.Length;
        int a, b;
        for (int i = 0; i < colors.Length; i++)
        {
            a = i / submeshLength;
            b = i % submeshLength;

            subMeshes[a].colors[b] = colors[i];

            int x = i % width;
            int y = i / width;

            //subMeshes[a]._Vertices[b].z = depth[i];
            subMeshes[a]._Vertices[b].z = (depth[i] == 0 || depth[1] > 1500) ? 1000000 : depth[i] * (float)depthScale;


            //if (depth[i] != 0) { 
            //    int babababa = 1;
            //    babababa++;
            //}

            subMeshes[a]._Vertices[b].x = subMeshes[a]._Vertices[b].z * (x - 255.5f) / 351.001462f;
            subMeshes[a]._Vertices[b].y = subMeshes[a]._Vertices[b].z * (y - 211.5f) / 351.001462f;
            // bckp subMeshes[a]._Vertices[b].z = (depth[i] == 0 || depth[1] > 1500) ? 1000000 : depth[i] * (float)depthScale; // AQUI
        }

        foreach(SubMesh m in subMeshes)
        {
            m.Apply();
            m.gameObject.GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();
        }


        // correct mesh position
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            meshes.transform.position += meshes.transform.up * 0.1f;
            Debug.Log("mesh up");
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            meshes.transform.position -= meshes.transform.up * 0.1f;
            Debug.Log("mesh down");
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            meshes.transform.position += meshes.transform.right * 0.1f;
            Debug.Log("mesh right");
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            meshes.transform.position -= meshes.transform.right * 0.1f;
            Debug.Log("mesh left");
        }
    }
}
