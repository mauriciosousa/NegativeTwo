using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CubeSTATE
{
    NONE = 0,
    SELECT = 1,
    SELECT_WRONG = 2,
    SELECT_CORRECT = 3,
    RIGHT_ANSWER = 4
}

public class CubeSelection : MonoBehaviour {

    public CubeSTATE state = CubeSTATE.NONE;

    public Material normalMaterial;
    public Material targetMaterial;
    public Material selectedMaterial_CORRECT;
    public Material selectedMaterial_WRONG;
    public Material selectedMaterial_ANSWER;

    private bool _highlighted = false;

    private Location _location;

    public bool Highlighted
    {
        get
        {
            return _highlighted;
        }

        set
        {
            _highlighted = value;

            transform.localScale = Vector3.one * (_highlighted? 0.1f : 0.05f);
        }
    }

    void Start()
    {
        _location = GameObject.Find("Main").GetComponent<Main>().location;
    }

    void setMaterial(Material material)
    {
        GetComponent<Renderer>().material = material;
    }

    void Update()
    {
        if (state == CubeSTATE.NONE)
        {
            setMaterial(normalMaterial);
        }
        else if (state == CubeSTATE.SELECT)
        {
            setMaterial(targetMaterial);
        }
        else if (state == CubeSTATE.SELECT_CORRECT)
        {
            setMaterial(selectedMaterial_CORRECT);
        }
        else if (state == CubeSTATE.SELECT_WRONG)
        {
            setMaterial(selectedMaterial_WRONG);
        }
        else if(state == CubeSTATE.RIGHT_ANSWER)
        {
            setMaterial(selectedMaterial_ANSWER);
        }
    }

    internal void correctSelection()
    {
        state = CubeSTATE.SELECT_CORRECT;
        //_selectedTime = DateTime.Now;
    }

    internal void wrongSelection()
    {
        state = CubeSTATE.SELECT_WRONG;
        //_selectedTime = DateTime.Now;
    }
}
