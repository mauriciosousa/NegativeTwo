using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hide : MonoBehaviour {

    private Renderer _renderer;

    public bool Enabled { get { return GetComponent<Renderer>().enabled; } private set { } }

    void Start()
    {
        _renderer = this.GetComponent<Renderer>();
        _renderer.enabled = false;
    }

    public void setEnabled(bool enabled)
    {
        _renderer.enabled = enabled;
    }
}
