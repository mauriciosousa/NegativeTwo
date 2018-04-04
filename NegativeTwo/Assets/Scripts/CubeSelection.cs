﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CubeSTATE
{
    NONE = 0,
    SELECT = 1,
    SELECT_WRONG = 2,
    SELECT_CORRECT = 3
}

public class CubeSelection : MonoBehaviour {

    public CubeSTATE state = CubeSTATE.NONE;

    public Material normalMaterial;
    public Material targetMaterial;
    public Material selectedMaterial_CORRECT;
    public Material selectedMaterial_WRONG;

    private bool _highlighted = false;

    private DateTime _selectedTime;
    private double notificationTime = 500; //ms

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
            //setMaterial(_location == Location.Instructor ? highlightedMaterial : normalMaterial);
            setMaterial(targetMaterial);
        }
        else if (state == CubeSTATE.SELECT_CORRECT)
        {
            setMaterial(selectedMaterial_CORRECT);
        }
        else if (state == CubeSTATE.SELECT_WRONG)
        {
            if (_selectedTime.AddMilliseconds(notificationTime) > DateTime.Now)
            {
                print("RED");
                setMaterial(selectedMaterial_WRONG);
            }
            else
            {
                state = CubeSTATE.NONE;
                setMaterial(normalMaterial);
            }
        }
    }

    internal void correctSelection()
    {
        state = CubeSTATE.SELECT_CORRECT;
    }

    internal void wrongSelection()
    {
        state = CubeSTATE.SELECT_WRONG;
        _selectedTime = DateTime.Now;
    }



    /*
    private void _select(bool correct)
    {
        if (selected) return;
        selected = true;
        _selectedTime = DateTime.Now;
        GetComponent<Renderer>().material = correct ? selectedMaterial_CORRECT : selectedMaterial_WRONG;
    }
    */
}
