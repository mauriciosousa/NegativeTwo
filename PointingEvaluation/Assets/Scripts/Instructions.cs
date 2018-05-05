using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionsText
{
	private static string targetMessage = "";
	private static string replicateMessage = "";
	private static string holdOnMessage = "";

	public static string getInstruction(int task, EvaluationPosition human)
	{
		if (human == EvaluationPosition.ON_THE_LEFT) {
			if ((new List<int> (new int[]{ 1, 3, 5, 7, 9 })).Contains (task)) {
				return replicateMessage;
			}

			if ((new List<int> (new int[]{ 2, 4, 6, 8, 10 })).Contains (task)) {
				return holdOnMessage;
			}
		}

		if (human == EvaluationPosition.ON_THE_RIGHT) {
			if (task == 1)
				return targetMessage;
			
			if ((new List<int> (new int[]{ 2, 4, 6, 8, 10 })).Contains (task)) {
				return replicateMessage;
			}

			if ((new List<int> (new int[]{ 3, 5, 7, 9 })).Contains (task)) {
					return replicateMessage;
			}
		}

		return "";
	}
}

public class Instructions : MonoBehaviour {

	private Evaluation _eval;
	private NetworkCommunication _network;

	private bool __shouldExist = false;

	void Start () {
		_eval = GameObject.Find ("PointingEvaluation").GetComponent<Evaluation> ();
		_network = GameObject.Find ("PointingEvaluation").GetComponent<NetworkCommunication> ();

		if (_network.evaluationPeerType == EvaluationPeertype.CLIENT)
			__shouldExist = true;

		if ((this.gameObject.name.Contains ("Left") && _eval.evaluationPosition == EvaluationPosition.ON_THE_LEFT)
		    || (this.gameObject.name.Contains ("Right") && _eval.evaluationPosition == EvaluationPosition.ON_THE_RIGHT)) {
			__shouldExist = true;
		} else {
			__shouldExist = false;
		}

		if (!__shouldExist)
			this.gameObject.SetActive (false);
	}

	void Update () {
		if (!__shouldExist)
			return;

		if (!_eval.taskInProgress) {
			// TODO: hide render do texto
		} else {
		
			string instruction = InstructionsText.getInstruction (_eval.Task, _eval.evaluationPosition);
			// TODO: change text and display
		}
	}
}
