using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleProjector : MonoBehaviour {

    public Texture oquequiseres;
    public Transform screenCenter = null;
    public int textureSize = 10;
    public bool didHit = false;

    //private Plane surface;
    //private Vector3 hitPoint;
    public Vector2 pixel;

    /*public Vector3 HitPoint
    {
        get
        {
            return hitPoint;
        }

        set
        {
            hitPoint = value;
            pixel = Camera.main.WorldToScreenPoint(hitPoint);
        }
    }*/

    void Start () {
		
	}
	
	void Update ()
    {
        /*if (screenCenter != null)
        {
            /*if (!didHit)
            {
                Ray ray = new Ray(transform.position, transform.forward);

                float distance;

                if (surface.Raycast(ray, out distance))
                {
                    hitPoint = ray.GetPoint(distance);
                    pixel = Camera.main.WorldToScreenPoint(hitPoint);
                }
            }* /
        }*/
    }

    /*internal void init(Transform transform)
    {
        screenCenter = transform;
        surface = new Plane(screenCenter.forward, screenCenter.position);
    }*/

    void OnGUI()
    {
        if(/*screenCenter != null &&*/ didHit)
        {
            GUI.DrawTexture(new Rect(pixel.x - textureSize * 0.5f, Screen.height - pixel.y - textureSize * 0.5f, textureSize, textureSize), oquequiseres);
        }
    }
}
