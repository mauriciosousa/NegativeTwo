using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderJoints : MonoBehaviour {

    void Start()
    {

        EvaluationPeer peer = GameObject.Find("DeixisEvaluation").GetComponent<EvaluationConfigProperties>().Peer;

        bool render = peer == EvaluationPeer.SERVER;

        MeshRenderer[] children = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer r in children)
        {
            r.enabled = render;
        }
    }
}
