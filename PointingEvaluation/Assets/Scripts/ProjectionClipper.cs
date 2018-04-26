using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ProjectionClipper: MonoBehaviour {

    private Projector _projector;
    public float NearProjectionDistanceTolerance = 1f;
    public float FarProjectionDistanceTolerance = 0.5f;

    private float _origNearClipPlane;
    private float _origFarClipPlane;

    public GameObject HitObject = null;

    public bool drawRay = false;

	void Start ()
    {
        if (_projector == null)
        {
            _projector = GetComponent<Projector>() as Projector;
        }

        _origNearClipPlane = _projector.nearClipPlane;
        _origFarClipPlane = _projector.farClipPlane;
	}

    void Update()
    {
        Ray ray = new Ray(_projector.transform.position, _projector.transform.forward);

        RaycastHit hit = new RaycastHit(); ;
        if (Physics.Raycast(ray, out hit))
        {
            float distance = hit.distance;
            _projector.nearClipPlane = Mathf.Max(distance - NearProjectionDistanceTolerance, 0);
            _projector.farClipPlane = distance + FarProjectionDistanceTolerance;

            if (drawRay) Debug.DrawLine(ray.origin, hit.point);

            HitObject = hit.collider.gameObject;
        }
        else
        {
            HitObject = null;
        }
    }
}
