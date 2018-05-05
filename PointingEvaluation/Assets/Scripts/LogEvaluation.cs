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

	public void recordSnapshot(int task, int condition, Human leftHuman, Human rightHuman)
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
			line += task + CSVSeparator;

			// TODO Left Human Values
			// TODO Right Human Values

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
}
