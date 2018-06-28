using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRCameraController : MonoBehaviour {

    public GameObject childGO;

    public Vector3 threshold;

    void LateUpdate()
    {

        this.transform.localPosition = -childGO.transform.localPosition + threshold; /*new Vector3((-UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye).x),
                 (-UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye).y),
                 (-UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye).z));*/

        Quaternion rot = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head);
        Vector3 eulerAnglesDoRift = rot.eulerAngles;
    }
}
