using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionsText
{
	private static string targetMessage = "Aponte para a bola vermelha na parede.";
	private static string replicateMessage = "Aponte pra onde o seu colega está a apontar.";
	private static string holdOnMessage = "Mantenha essa posição.";

	public static string getInstruction(int task, EvaluationPosition human)
	{
        if (task == 1) return targetMessage;

        if (human == EvaluationPosition.ON_THE_LEFT)
        {
            if ((new List<int>(new int[] { 2, 4, 6, 8, 10, 12, 14 })).Contains(task))
            {
                return holdOnMessage;
            }

            if ((new List<int>(new int[] { 3, 5, 7, 9, 11, 13 })).Contains(task))
            {
                return replicateMessage;
            }
        }

        if (human == EvaluationPosition.ON_THE_RIGHT)
        {
            if (task == 8) return targetMessage;

            if ((new List<int>(new int[] { 2, 4, 6, 10, 12, 14 })).Contains(task))
            {
                return replicateMessage;
            }

            if ((new List<int>(new int[] { 3, 5, 7, 9, 11, 13 })).Contains(task))
            {
                return holdOnMessage;
            }
        }


        return "";
	}
}

public class Instructions : MonoBehaviour {

	private Evaluation _eval;
	private NetworkCommunication _network;

    public TextMesh textMesh;

    public Transform LeftHumanPosition;
    public Transform RightHumanPosition;

    void Start () {
		_eval = GameObject.Find ("PointingEvaluation").GetComponent<Evaluation> ();
		_network = GameObject.Find ("PointingEvaluation").GetComponent<NetworkCommunication> ();


        if (_network.evaluationPeerType == EvaluationPeertype.CLIENT)
        {
            
            //this.transform.position = new Vector3(_eval.clientPosition == EvaluationPosition.ON_THE_LEFT ? -LeftHumanPosition.position.x : -RightHumanPosition.transform.position.x, transform.position.y, transform.position.z); 
        }
    }

    void Update () {
        if (_network.evaluationPeerType == EvaluationPeertype.SERVER)
            return;

		if (_eval.taskInProgress) {            
            Role myRole = _eval.getMyRole();    
            textMesh.text = InstructionsText.getInstruction(_eval.Task, _eval.clientPosition);
        }
    }
}
