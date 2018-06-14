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

        header += "LEFT_HUMAN_POINTING_ARM" + CSVSeparator;
        header += "RIGHT_HUMAN_POINTING_ARM" + CSVSeparator;

        header += "LEFT_HUMAN_ROLE" + CSVSeparator;
        header += "RIGHT_HUMAN_ROLE" + CSVSeparator;

        header += "TARGET_X" + CSVSeparator;
        header += "TARGET_Y" + CSVSeparator;
        header += "TARGET_Z" + CSVSeparator;

        header += "Phead_X" + CSVSeparator;
        header += "Phead_Y" + CSVSeparator;
        header += "Phead_Z" + CSVSeparator;

        header += "PElbow_X" + CSVSeparator;
        header += "PElbow_Y" + CSVSeparator;
        header += "PElbow_Z" + CSVSeparator;

        header += "PHandTip_X" + CSVSeparator;
        header += "PHandTip_Y" + CSVSeparator;
        header += "PHandTip_Z" + CSVSeparator;

        header += "PhHead_X" + CSVSeparator;
        header += "PhHead_Y" + CSVSeparator;
        header += "PhHead_Z" + CSVSeparator;

        header += "PhElbow_X" + CSVSeparator;
        header += "PhElbow_Y" + CSVSeparator;
        header += "PhElbow_Z" + CSVSeparator;

        header += "Mhead_X" + CSVSeparator;
        header += "Mhead_Y" + CSVSeparator;
        header += "Mhead_Z" + CSVSeparator;

        header += "MElbow_X" + CSVSeparator;
        header += "MElbow_Y" + CSVSeparator;
        header += "MElbow_Z" + CSVSeparator;

        header += "MHandTip_X" + CSVSeparator;
        header += "MHandTip_Y" + CSVSeparator;
        header += "MHandTip_Z" + CSVSeparator;

        header += "MhHead_X" + CSVSeparator;
        header += "MhHead_Y" + CSVSeparator;
        header += "MhHead_Z" + CSVSeparator;

        header += "MhElbow_X" + CSVSeparator;
        header += "MhElbow_Y" + CSVSeparator;
        header += "MhElbow_Z" + CSVSeparator;

        header += "d_PhHead_Target" + CSVSeparator;
        header += "d_MhHead_Target" + CSVSeparator;
        header += "d_PhHead_MhHead" + CSVSeparator;

        header += "d_PhElbow_Target" + CSVSeparator;
        header += "d_MhElbow_Target" + CSVSeparator;
        header += "d_PhElbow_MhElbow" + CSVSeparator;

        header += "d_PhHead_MhElbow" + CSVSeparator;
        header += "d_MhHead_PhElbow";

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

                               PointingArm leftHuman_PointingArm,
                               PointingArm rightHuman_PointingArm,

                               Role leftHuman_Role,
                               Role rightHuman_Role,

                               Vector3 target,

                               Vector3 Phead,
                               Vector3 PElbow,
                               Vector3 PHandTip,
                               Vector3 PhHead,
                               Vector3 PhElbow,

                               Vector3 Mhead,
                               Vector3 MElbow,
                               Vector3 MHandTip,
                               Vector3 MhHead,
                               Vector3 MhElbow,

                               float d_PhHead_Target,
                               float d_MhHead_Target,
                               float d_PhHead_MhHead,

                               float d_PhElbow_Target,
                               float d_MhElbow_Target,
                               float d_PhElbow_MhElbow,

                               float d_PhHead_MhElbow,
                               float d_MhHead_PhElbow
                               )
	{
		if (!__init__)
			return;

		if (task == 2) {
			startSession ();
		}

		if (__inSession__) {

			string line = "";
			line += DateTime.Now.ToString ("H:mm:ss:ff") + CSVSeparator;
			line += task + CSVSeparator;
			line += condition.ToString() + CSVSeparator;

            line += leftHuman_PointingArm.ToString() + CSVSeparator;
            line += rightHuman_PointingArm.ToString() + CSVSeparator;

            line += leftHuman_Role.ToString() + CSVSeparator;
            line += rightHuman_Role.ToString() + CSVSeparator;

            line += target.x + CSVSeparator;
            line += target.y + CSVSeparator;
            line += target.z + CSVSeparator;

            line += Phead.x + CSVSeparator;
            line += Phead.y + CSVSeparator;
            line += Phead.z + CSVSeparator;

            line += PElbow.x + CSVSeparator;
            line += PElbow.y + CSVSeparator;
            line += PElbow.z + CSVSeparator;

            line += PHandTip.x + CSVSeparator;
            line += PHandTip.y + CSVSeparator;
            line += PHandTip.z + CSVSeparator;

            line += PhHead.x + CSVSeparator;
            line += PhHead.y + CSVSeparator;
            line += PhHead.z + CSVSeparator;

            line += PhElbow.x + CSVSeparator;
            line += PhElbow.y + CSVSeparator;
            line += PhElbow.z + CSVSeparator;

            line += Mhead.x + CSVSeparator  ;
            line += Mhead.y + CSVSeparator;
            line += Mhead.z + CSVSeparator;

            line += MElbow.x + CSVSeparator;
            line += MElbow.y + CSVSeparator;
            line += MElbow.z + CSVSeparator;

            line += MHandTip.x + CSVSeparator;
            line += MHandTip.y + CSVSeparator;
            line += MHandTip.z + CSVSeparator;

            line += MhHead.x + CSVSeparator;
            line += MhHead.y + CSVSeparator;
            line += MhHead.z + CSVSeparator;

            line += MhElbow.x + CSVSeparator;
            line += MhElbow.y + CSVSeparator;
            line += MhElbow.z + CSVSeparator;

            line += d_PhHead_Target + CSVSeparator;
            line += d_MhHead_Target + CSVSeparator;
            line += d_PhHead_MhHead + CSVSeparator;

            line += d_PhElbow_Target + CSVSeparator;
            line += d_MhElbow_Target + CSVSeparator;
            line += d_PhElbow_MhElbow + CSVSeparator;

            line += d_PhHead_MhElbow + CSVSeparator;
            line += d_MhHead_PhElbow;

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
