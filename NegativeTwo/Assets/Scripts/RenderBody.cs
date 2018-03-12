using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderBody : MonoBehaviour {

    private bool _enabled = false;
    public bool Enabled
    {
        get
        {
            return _enabled;
        }

        set
        {
            if (_enabled != value)
            {
                _enabled = value;
                _setRender();
            }
        }
    }

    private void _setRender()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = _enabled;
        }
    }

    void Start()
    {
        _setRender();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _enabled = !_enabled;
            _setRender();
        }
    }
}
