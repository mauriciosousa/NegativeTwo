using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSelection : MonoBehaviour {

    public Material normalMaterial;
    public Material highlightedMaterial;
    public Material selectedMaterial_CORRECT;
    public Material selectedMaterial_WRONG;

    public bool selected { get; private set; }
    private DateTime _selectedTime;
    public double notificationTime; //ms

    void Start()
    {
    }

    void Update()
    {
        if (this.name == "cube_1")
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                _select(true);
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                _select(false);
            }
        }

        //
        if (selected && (DateTime.Now >= _selectedTime.AddMilliseconds(notificationTime)))
        {
            print(selected);
            GetComponent<Renderer>().material = normalMaterial;
            selected = false;
            // RAISE END TASK
        }
    }

    private void _select(bool correct)
    {
        if (selected) return;
        selected = true;
        _selectedTime = DateTime.Now;
        GetComponent<Renderer>().material = correct ? selectedMaterial_CORRECT : selectedMaterial_WRONG;
    }
}
