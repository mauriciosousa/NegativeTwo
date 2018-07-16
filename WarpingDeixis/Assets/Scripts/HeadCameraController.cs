using UnityEngine;
using UnityEngine.VR;
using System.Collections;

/// <summary>
/// This class is responsible for updating the caracter head 
/// rotation according to the HMD (Head Mounted Display) rotation.
/// </summary>
public class HeadCameraController : MonoBehaviour
{

    public BodiesManager _bodies;

    public EvaluationConfigProperties config;

    void Start()
    {

    }

    void Update()
    {
        if (config.Peer == EvaluationPeer.SERVER) return;

            //this.transform.position = new Vector3(0, 1, 0); return;

        bool canApplyHeadPosition;
        Vector3 headposition = _bodies.getHeadPosition(out canApplyHeadPosition);
        if (canApplyHeadPosition)
        {
            this.transform.position = headposition;
        }
        else
        {
            this.transform.position = new Vector3(0, 1.8f, 0);
        }

    }
}