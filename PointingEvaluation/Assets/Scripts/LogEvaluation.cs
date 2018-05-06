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

	public char CSVSeparator = '#';

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

		filename = _dir + System.IO.Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyy MMMM d H-mm-ss.csv");

		if (!File.Exists (filename)) {
			File.Create (filename);
		}
			
		string header = "";

		header += "TIMESTAMP" + CSVSeparator;
		header += "TASK" + CSVSeparator;
		header += "CONDITION" + CSVSeparator;

		header += "LH_POINTING_HAND" + CSVSeparator; // LEFT HUMAN
		header += "LH_TARGET_POINT_X" + CSVSeparator;
		header += "LH_TARGET_POINT_Y" + CSVSeparator;
		header += "LH_TARGET_POINT_Z" + CSVSeparator;

		header += "LH_HEAD_X" + CSVSeparator;
		header += "LH_HEAD_Y" + CSVSeparator;
		header += "LH_HEAD_Z" + CSVSeparator;

		header += "LH_LEFT_SHOULDER_X" + CSVSeparator;
		header += "LH_LEFT_SHOULDER_Y" + CSVSeparator;
		header += "LH_LEFT_SHOULDER_Z" + CSVSeparator;

		header += "LH_LEFT_ELBOW_X" + CSVSeparator;
		header += "LH_LEFT_ELBOW_Y" + CSVSeparator;
		header += "LH_LEFT_ELBOW_Z" + CSVSeparator;

		header += "LH_LEFT_HAND_X" + CSVSeparator;
		header += "LH_LEFT_HAND_Y" + CSVSeparator;
		header += "LH_LEFT_HAND_Z" + CSVSeparator;

		header += "LH_LEFT_HAND_TIP_X" + CSVSeparator;
		header += "LH_LEFT_HAND_TIP_Y" + CSVSeparator;
		header += "LH_LEFT_HAND_TIP_Z" + CSVSeparator;

		header += "LH_RIGHT_SHOULDER_X" + CSVSeparator;
		header += "LH_RIGHT_SHOULDER_Y" + CSVSeparator;
		header += "LH_RIGHT_SHOULDER_Z" + CSVSeparator;

		header += "LH_RIGHT_ELBOW_X" + CSVSeparator;
		header += "LH_RIGHT_ELBOW_Y" + CSVSeparator;
		header += "LH_RIGHT_ELBOW_Z" + CSVSeparator;

		header += "LH_RIGHT_HAND_X" + CSVSeparator;
		header += "LH_RIGHT_HAND_Y" + CSVSeparator;
		header += "LH_RIGHT_HAND_Z" + CSVSeparator;

		header += "LH_RIGHT_HAND_TIP_X" + CSVSeparator;
		header += "LH_RIGHT_HAND_TIP_Y" + CSVSeparator;
		header += "LH_RIGHT_HAND_TIP_Z" + CSVSeparator;

		header += "RH_POINTING_HAND" + CSVSeparator; // RIGHT HUMAN
		header += "RH_TARGET_POINT_X" + CSVSeparator;
		header += "RH_TARGET_POINT_Y" + CSVSeparator;
		header += "RH_TARGET_POINT_Z" + CSVSeparator;

		header += "RH_HEAD_X" + CSVSeparator;
		header += "RH_HEAD_Y" + CSVSeparator;
		header += "RH_HEAD_Z" + CSVSeparator;

		header += "RH_LEFT_SHOULDER_X" + CSVSeparator;
		header += "RH_LEFT_SHOULDER_Y" + CSVSeparator;
		header += "RH_LEFT_SHOULDER_Z" + CSVSeparator;

		header += "RH_LEFT_ELBOW_X" + CSVSeparator;
		header += "RH_LEFT_ELBOW_Y" + CSVSeparator;
		header += "RH_LEFT_ELBOW_Z" + CSVSeparator;

		header += "RH_LEFT_HAND_X" + CSVSeparator;
		header += "RH_LEFT_HAND_Y" + CSVSeparator;
		header += "RH_LEFT_HAND_Z" + CSVSeparator;

		header += "RH_LEFT_HAND_TIP_X" + CSVSeparator;
		header += "RH_LEFT_HAND_TIP_Y" + CSVSeparator;
		header += "RH_LEFT_HAND_TIP_Z" + CSVSeparator;

		header += "RH_RIGHT_SHOULDER_X" + CSVSeparator;
		header += "RH_RIGHT_SHOULDER_Y" + CSVSeparator;
		header += "RH_RIGHT_SHOULDER_Z" + CSVSeparator;

		header += "RH_RIGHT_ELBOW_X" + CSVSeparator;
		header += "RH_RIGHT_ELBOW_Y" + CSVSeparator;
		header += "RH_RIGHT_ELBOW_Z" + CSVSeparator;

		header += "RH_RIGHT_HAND_X" + CSVSeparator;
		header += "RH_RIGHT_HAND_Y" + CSVSeparator;
		header += "RH_RIGHT_HAND_Z" + CSVSeparator;

		header += "RH_RIGHT_HAND_TIP_X" + CSVSeparator;
		header += "RH_RIGHT_HAND_TIP_Y" + CSVSeparator;
		header += "RH_RIGHT_HAND_TIP_Z" + CSVSeparator;


		_writeLine (header, filename);

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

	public void recordSnapshot(int task, int condition, Human leftHuman, bool leftHumanPointingHandisTheLeft, Vector3 leftHumanTargetOnTheWall,
														Human rightHuman, bool rightHumanPointingHandisTheLeft, Vector3 rightHumanTargetOnTheWall)
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
			line += condition + CSVSeparator;

			// LEFT HUMAN
			line += "" + (leftHumanPointingHandisTheLeft ? "LEFT" : "RIGHT") + CSVSeparator;
			line += _vector3ToLine (leftHumanTargetOnTheWall) + CSVSeparator;

			line += _vector3ToLine (leftHuman.body.Joints[BodyJointType.head]) + CSVSeparator;

			line += _vector3ToLine (leftHuman.body.Joints[BodyJointType.leftShoulder]) + CSVSeparator;
			line += _vector3ToLine (leftHuman.body.Joints[BodyJointType.leftElbow]) + CSVSeparator;
			line += _vector3ToLine (leftHuman.body.Joints[BodyJointType.leftHand]) + CSVSeparator;
			line += _vector3ToLine (leftHuman.body.Joints[BodyJointType.leftHandTip]) + CSVSeparator;

			line += _vector3ToLine (leftHuman.body.Joints[BodyJointType.rightShoulder]) + CSVSeparator;
			line += _vector3ToLine (leftHuman.body.Joints[BodyJointType.rightElbow]) + CSVSeparator;
			line += _vector3ToLine (leftHuman.body.Joints[BodyJointType.rightHand]) + CSVSeparator;
			line += _vector3ToLine (leftHuman.body.Joints[BodyJointType.rightHandTip]) + CSVSeparator;

			// RIGHT HUMAN
			line += "" + (rightHumanPointingHandisTheLeft ? "LEFT" : "RIGHT") + CSVSeparator;
			line += _vector3ToLine (rightHumanTargetOnTheWall) + CSVSeparator;

			line += _vector3ToLine (rightHuman.body.Joints[BodyJointType.head]) + CSVSeparator;

			line += _vector3ToLine (rightHuman.body.Joints[BodyJointType.leftShoulder]) + CSVSeparator;
			line += _vector3ToLine (rightHuman.body.Joints[BodyJointType.leftElbow]) + CSVSeparator;
			line += _vector3ToLine (rightHuman.body.Joints[BodyJointType.leftHand]) + CSVSeparator;
			line += _vector3ToLine (rightHuman.body.Joints[BodyJointType.leftHandTip]) + CSVSeparator;

			line += _vector3ToLine (rightHuman.body.Joints[BodyJointType.rightShoulder]) + CSVSeparator;
			line += _vector3ToLine (rightHuman.body.Joints[BodyJointType.rightElbow]) + CSVSeparator;
			line += _vector3ToLine (rightHuman.body.Joints[BodyJointType.rightHand]) + CSVSeparator;
			line += _vector3ToLine (rightHuman.body.Joints[BodyJointType.rightHandTip]) + CSVSeparator;

			_writeLine (line, filename);

			if (task == NumberOfTrials) {
				endSession ();
			}
		} else
			_console.writeLineRed ("Not in session. Start with Task = 1");
	}

	private void _writeLine (string line, string file)
	{
		using (StreamWriter w = File.AppendText(file))
		{
			w.WriteLine (line);
		}
	}

	private string _vector3ToLine(Vector3 v)
	{
		return "" + v.x + CSVSeparator + v.y + CSVSeparator + v.z;
	}
}
