using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {

    public GameObject wall;
    public GameObject wallCollider;
    public GameObject cursor;
    public GameObject target;

    public Transform center;
    public Transform topLeft;
    public Transform topRight;
    public Transform bottomLeft;
    public Transform bottomRight;

    public DeixisNetwork network;
    public DeixisEvaluation deixisEvaluation;
    private EvaluationConfigProperties _config;
    private EvaluationPeer peer;

    private bool _inSession = false;
    private bool IAmObserver = false;

    private int trial = 0;

    private float targetZ = 0f;

    void Start () {
        hideWall();

        float distanceToAdd = GameObject.Find("leftHumanPosition").transform.position.z;
        this.transform.position += Vector3.forward * distanceToAdd;

        cursor.GetComponent<MeshRenderer>().enabled = false;
        target.GetComponent<MeshRenderer>().enabled = false;

        _config = deixisEvaluation.GetComponent<EvaluationConfigProperties>();
        peer = _config.Peer;

        targetZ = target.transform.position.z;
    }


    public void createWall(int trial, bool observer, EvaluationPeer evaluationPeer, WarpingCondition condition)
    {
        this.trial = trial;

        wall.GetComponent<MeshRenderer>().enabled = true;

        //if (peer == EvaluationPeer.SERVER) observer = _config.serverIsObserver;

        if (peer == EvaluationPeer.SERVER)
        {
            cursor.GetComponent<MeshRenderer>().enabled = true;
            target.transform.position = _getTarget(trial, evaluationPeer, condition);
            target.GetComponent<MeshRenderer>().enabled = true;
        }
        else
        {
            if (observer)
            {
                cursor.GetComponent<MeshRenderer>().enabled = true;
            }
            else
            {
                target.transform.position = _getTarget(trial, evaluationPeer, condition);
                target.transform.position = new Vector3(target.transform.position.x, target.transform.position.y, targetZ);
                target.GetComponent<MeshRenderer>().enabled = true;
            }
        }

        cursor.transform.localPosition = new Vector3(0, -2f, cursor.transform.localPosition.z);
        IAmObserver = observer;
        _inSession = true;
    }

    public void hideWall()
    {
        wall.GetComponent<MeshRenderer>().enabled = false;
        cursor.GetComponent<MeshRenderer>().enabled = false;
        target.GetComponent<MeshRenderer>().enabled = false;

        IAmObserver = false;
        _inSession = false;
    }

    void Update () {

        bool joyClick = false;
        if (Input.GetKey(KeyCode.Joystick1Button0)) joyClick = true;
        if (Input.GetKey(KeyCode.Joystick1Button1)) joyClick = true;
        if (Input.GetKey(KeyCode.Joystick2Button0)) joyClick = true;
        if (Input.GetKey(KeyCode.Joystick2Button1)) joyClick = true;

        if (joyClick)
        {
            //Debug.Log("joyclick " + IAmObserver);
            if (_inSession && IAmObserver)
            {

                Debug.Log("Cursor click");

                EvaluationPeer peer = deixisEvaluation.getObserver(trial);
                Vector3 hitpoint;
                PointingArm pointingArm = deixisEvaluation.getPointingArm(peer, out hitpoint);

                if (pointingArm == PointingArm.LEFT || pointingArm == PointingArm.RIGHT)
                {
                    cursor.transform.position = hitpoint;
                    //cursor.transform.localPosition = new Vector3(cursor.transform.localPosition.x, cursor.transform.localPosition.y, -0.06f);

                    network.UpdateWallCursor(cursor.transform.localPosition.x, cursor.transform.localPosition.y);
                }

                Debug.Log("" + peer + " used " + pointingArm + " hit " + hitpoint);

            }
        }

        /*  */

        if (Input.GetKeyDown(KeyCode.W))
        {
            List<GameObject> targets = new List<GameObject>();

            targets.Add(_generateTargetAround(center, "center_w1"));
            targets.Add(_generateTargetAround(center, "center_w2"));
            targets.Add(_generateTargetAround(center, "center_w3"));
            targets.Add(_generateTargetAround(center, "center_w4"));

            targets.Add(_generateTargetAround(topLeft, "topLeft_w1"));
            targets.Add(_generateTargetAround(topLeft, "topLeft_w2"));
            targets.Add(_generateTargetAround(topLeft, "topLeft_w3"));
            targets.Add(_generateTargetAround(topLeft, "topLeft_w4"));

            targets.Add(_generateTargetAround(topRight, "topRight_w1"));
            targets.Add(_generateTargetAround(topRight, "topRight_w2"));
            targets.Add(_generateTargetAround(topRight, "topRight_w3"));
            targets.Add(_generateTargetAround(topRight, "topRight_w4"));

            targets.Add(_generateTargetAround(bottomLeft, "bottomLeft_w1"));
            targets.Add(_generateTargetAround(bottomLeft, "bottomLeft_w2"));
            targets.Add(_generateTargetAround(bottomLeft, "bottomLeft_w3"));
            targets.Add(_generateTargetAround(bottomLeft, "bottomLeft_w4"));

            targets.Add(_generateTargetAround(bottomRight, "bottomRight_w1"));
            targets.Add(_generateTargetAround(bottomRight, "bottomRight_w2"));
            targets.Add(_generateTargetAround(bottomRight, "bottomRight_w3"));
            targets.Add(_generateTargetAround(bottomRight, "bottomRight_w4"));

            string filep = Application.dataPath + "/wallTargets.txt";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filep, false))
            {
                string l = "";

                foreach (GameObject t in targets)
                {
                    l = "" + t.name + " " + t.transform.position;

                    file.WriteLine(l);
                }

                file.WriteLine("");

                foreach (GameObject t in targets)
                {
                    Vector3 p = t.transform.position;

                    l = "public Vector3 " + t.name + " = new Vector3(" + p.x + "f, " + p.y + "f, " + p.z + "f);";

                    file.WriteLine(l);
                }
            }
        }
	}

    internal void updateWallCursor(float x, float y)
    {
        Debug.Log("Receiving Cursor Updates");
        if (peer == EvaluationPeer.SERVER)
        {
            cursor.transform.localPosition = new Vector3(x, y, cursor.transform.localPosition.z);
        }
    }

    private GameObject _generateTargetAround(Transform g, string name)
    {
        float r = 0.5f;
        float Rx = UnityEngine.Random.Range(0.0f, 1.0f) * r;
        float Ry = UnityEngine.Random.Range(0.0f, 1.0f) * r;
        float Ra = UnityEngine.Random.Range(0.0f, 1.0f) * 360f;

        GameObject target = new GameObject(name);
        target.transform.parent = g;
        target.transform.localPosition = Vector3.zero;
        target.transform.localPosition = new Vector3(Rx * Mathf.Cos(Ra), Ry * Mathf.Sin(Ra), 0f);

        return target;
    }

    internal void destroyCurrent()
    {
        hideWall();
    }

    private Vector3 _getTarget(int trial, EvaluationPeer evaluationPeer, WarpingCondition condition)
    {
        if (condition == WarpingCondition.BASELINE)
        {
            // w1
            if (trial == 10) return topLeft_w1;
            if (trial == 11) return topRight_w1;
            if (trial == 12) return center_w1;
            if (trial == 13) return bottomRight_w1;
            if (trial == 14) return bottomLeft_w1;

            // w2
            if (trial == 24) return topRight_w2;
            if (trial == 25) return topLeft_w2;
            if (trial == 26) return bottomRight_w2;
            if (trial == 27) return bottomLeft_w2;
            if (trial == 28) return center_w2;
        }

        if (condition == WarpingCondition.WARPING)
        {
            // w3
            if (trial == 10) return center_w3;
            if (trial == 11) return bottomLeft_w3;
            if (trial == 12) return topLeft_w3;
            if (trial == 13) return topRight_w3;
            if (trial == 14) return bottomRight_w3;

            // w4
            if (trial == 24) return bottomRight_w4;
            if (trial == 25) return center_w4;
            if (trial == 26) return bottomLeft_w4;
            if (trial == 27) return topLeft_w4;
            if (trial == 28) return topRight_w4;
        }




        return Vector3.zero;
    }

    public Vector3 center_w1 = new Vector3(-0.1798496f, 1.388596f, 2.575f);
    public Vector3 center_w2 = new Vector3(0.2393779f, 1.569755f, 2.575f);
    public Vector3 center_w3 = new Vector3(0.07586742f, 1.552207f, 2.575f);
    public Vector3 center_w4 = new Vector3(0.00348684f, 1.032613f, 2.575f);

    public Vector3 topLeft_w1 = new Vector3(-0.8982405f, 1.653912f, 2.575f);
    public Vector3 topLeft_w2 = new Vector3(-0.7911618f, 1.884994f, 2.575f);
    public Vector3 topLeft_w3 = new Vector3(-0.6061193f, 1.960249f, 2.575f);
    public Vector3 topLeft_w4 = new Vector3(-0.9442142f, 2.017196f, 2.575f);

    public Vector3 topRight_w1 = new Vector3(0.7185954f, 2.232494f, 2.575f);
    public Vector3 topRight_w2 = new Vector3(0.9918004f, 2.305739f, 2.575f);
    public Vector3 topRight_w3 = new Vector3(0.7897538f, 1.855848f, 2.575f);
    public Vector3 topRight_w4 = new Vector3(0.7402495f, 1.877652f, 2.575f);

    public Vector3 bottomLeft_w1 = new Vector3(-0.9857493f, 1.462909f, 2.575f);
    public Vector3 bottomLeft_w2 = new Vector3(-1.051283f, 0.7611822f, 2.575f);
    public Vector3 bottomLeft_w3 = new Vector3(-0.9214287f, 1.128704f, 2.575f);
    public Vector3 bottomLeft_w4 = new Vector3(-0.6065137f, 1.198214f, 2.575f);
     
    public Vector3 bottomRight_w1 = new Vector3(0.9459625f, 1.022236f, 2.575f);
    public Vector3 bottomRight_w2 = new Vector3(0.5777413f, 1.131206f, 2.575f);
    public Vector3 bottomRight_w3 = new Vector3(1.34024f, 1.356322f, 2.575f);
    public Vector3 bottomRight_w4 = new Vector3(1.084203f, 0.945923f, 2.575f);
}
