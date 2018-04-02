using System;
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

    private CubeSTATE _state = CubeSTATE.NONE;
    public CubeSTATE State
    {
        get
        {
            return _state;
        }
        set
        {


            _state = value;
        }
    }


    public Material normalMaterial;
    public Material highlightedMaterial;
    public Material selectedMaterial_CORRECT;
    public Material selectedMaterial_WRONG;

    private DateTime _selectedTime;
    private double notificationTime = 500; //ms

    private Location _location;

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
        if (_state == CubeSTATE.NONE)
        {
            setMaterial(normalMaterial);
        }
        else if (_state == CubeSTATE.SELECT)
        {
            //setMaterial(_location == Location.Instructor ? highlightedMaterial : normalMaterial);
            setMaterial(highlightedMaterial);
        }
        else if (_state == CubeSTATE.SELECT_CORRECT)
        {
            setMaterial(selectedMaterial_CORRECT);
        }
        else if (_state == CubeSTATE.SELECT_WRONG)
        {
            if (_selectedTime.AddMilliseconds(notificationTime) > DateTime.Now)
            {
                print("RED");
                setMaterial(selectedMaterial_WRONG);
            }
            else
            {
                _state = CubeSTATE.NONE;
                setMaterial(normalMaterial);
            }
        }
    }

    internal void correctSelection()
    {
        _state = CubeSTATE.SELECT_CORRECT;
    }

    internal void wrongSelection()
    {
        _state = CubeSTATE.SELECT_WRONG;
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
