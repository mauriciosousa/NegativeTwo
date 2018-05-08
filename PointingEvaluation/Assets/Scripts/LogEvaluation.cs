using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class LogEvaluation : MonoBehaviour {

	private ServerConsole _console;
	private EvaluationConfigProperties _config;

	public string Folder = "Recordings";
	private string _dir;

	private string CSVSeparator = "#";

	private bool __init__ = false; 

	private bool __inSession__ = false;

	private string filename;

	private int NumberOfTrials;

	public void Init (int numberOfTrials) {
		_console = GetComponent<ServerConsole> ();
		_config = GetComponent<EvaluationConfigProperties> ();	
		__init__ = true;
		NumberOfTrials = numberOfTrials;
	}

	private void startSession()
	{
		if (!__init__)
			return;

		_dir = Application.dataPath + System.IO.Path.DirectorySeparatorChar + Folder;
		if (!Directory.Exists(_dir)) {
			Directory.CreateDirectory (_dir);
		}

		filename = _dir + System.IO.Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyy MMMM d H-mm-ss") + ".txt";

		if (!File.Exists (filename)) {
			File.Create (filename).Dispose();
		}
			
		string header = "";

		header += "TIMESTAMP" + CSVSeparator;
		header += "TASK" + CSVSeparator;
		header += "CONDITION" + CSVSeparator;

        header += "LEFT_HUMAN_ROLE" + CSVSeparator;
        header += "RIGHT_HUMAN_ROLE" + CSVSeparator;

        header += "LEFT_HUMAN_POINTING_ARM" + CSVSeparator;
        header += "RIGHT_HUMAN_POINTING_ARM" + CSVSeparator;

        header += "ERROR_ANGLE" + CSVSeparator;
        header += "ERROR_DISTANCE" + CSVSeparator;

        header += "POINTER_HITPOINT_X" + CSVSeparator;
        header += "POINTER_HITPOINT_Y" + CSVSeparator;
        header += "POINTER_HITPOINT_Z" + CSVSeparator;

        header += "POINTER_HEAD_X" + CSVSeparator;
        header += "POINTER_HEAD_Y" + CSVSeparator;
        header += "POINTER_HEAD_Z" + CSVSeparator;

        header += "MIMIC_HITPOINT_X" + CSVSeparator;
        header += "MIMIC_HITPOINT_Y" + CSVSeparator;
        header += "MIMIC_HITPOINT_Z" + CSVSeparator;

        header += "MIMIC_HEAD_X" + CSVSeparator;
        header += "MIMIC_HEAD_Y" + CSVSeparator;
        header += "MIMIC_HEAD_Z" + CSVSeparator;

        _writeLine(header, filename);

		__inSession__ = true;
		_console.writeLine ("[LOG] session started");
	}

	private void endSession()
	{
		if (!__init__)
			return;

		__inSession__ = false;
		_console.writeLine ("[LOG] session ended");
	}

	public void recordSnapshot(int task, 
                               EvaluationCondition condition,
                               
                               Human leftHuman,
                               Role leftHuman_Role,
                               PointingArm leftHuman_PointingArm,
                               
                               Human rightHuman,
                               Role rightHuman_Role,
                               PointingArm rightHuman_PointingArm,
                               
                               float errorAngle,
                               float errorDistance,

                               Vector3 pointer_hitpoint,
                               Vector3 pointer_head,

                               Vector3 mimic_hitpoint,
                               Vector3 mimic_head
                               )
	{
		if (!__init__)
			return;

		if (task == 1) {
			startSession ();
		}

		if (__inSession__) {

			string line = "";
			line += DateTime.Now.ToString ("H:mm:ss:ff") + CSVSeparator;
			line += task + CSVSeparator;
			line += condition.ToString() + CSVSeparator;

            line += leftHuman_Role.ToString() + CSVSeparator;
            line += rightHuman_Role.ToString() + CSVSeparator;

            line += leftHuman_PointingArm.ToString() + CSVSeparator;
            line += rightHuman_PointingArm.ToString() + CSVSeparator;

            line += errorAngle + CSVSeparator;
            line += errorDistance + CSVSeparator;

            line += pointer_hitpoint.x + CSVSeparator;
            line += pointer_hitpoint.y + CSVSeparator;
            line += pointer_hitpoint.z + CSVSeparator;

            line += pointer_head.x + CSVSeparator;
            line += pointer_head.y + CSVSeparator;
            line += pointer_head.z + CSVSeparator;

            line += mimic_hitpoint.x + CSVSeparator;
            line += mimic_hitpoint.y + CSVSeparator;
            line += mimic_hitpoint.z + CSVSeparator;

            line += mimic_head.x + CSVSeparator;
            line += mimic_head.y + CSVSeparator;
            line += mimic_head.z + CSVSeparator;

            _writeLine(line, filename);

			if (task == NumberOfTrials) {
				endSession ();
			}
		} else
			_console.writeLineRed ("Not in session. Start with Task = 0");
	}

	private void _writeLine (string line, string file)
	{
        using (StreamWriter w = File.AppendText(file))
		{
			w.WriteLine (line);
            w.Close();
		}
	}

	private string _vector3ToLine(Vector3 v)
	{
		return "" + v.x + CSVSeparator + v.y + CSVSeparator + v.z;
	}
}
