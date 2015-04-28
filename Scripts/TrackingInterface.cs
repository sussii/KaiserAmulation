using UnityEngine;
using System.Collections;


/* Project: Kaiser
 * Date: Last modified at 10/22/2014
 * Note: Attach this script to GameObject "TrackingInterface"
 * The script is to show distance between people and machines as well as alert on Gui. 
 * */

public class TrackingInterface : MonoBehaviour {
	public float alarmDistance = 6f;

	public GUIText[] guiText;
	public GameObject M_PVM;
	public GameObject M_Pump;
	public GameObject[] persons = null;
	public PopupsController popUpsCont;
	public OutPutLog outPutLogger;
	private float[] distancePVM = new float[6];
	private float[] distancePump = new float[6];
	private Transform person;
	private Transform machine;

    public bool pendingAlarm; // in case someone come in but the awarepoint hasn't identify the person.
    private bool showAlarm;
    private bool prevAlarm;

    public void Start()
    {
        showAlarm = false;
        prevAlarm = false;
    }

    void Update () {
        //Debug.Log( "Pending Alarm: " + pendingAlarm);
        if (!pendingAlarm)
        {
            //Debug.Log("PVM: " + (persons[0].transform.position - M_PVM.transform.position).magnitude);
            //Debug.Log("Pump:" + (persons[0].transform.position - M_Pump.transform.position).magnitude);
            //Debug.DrawLine(persons[0].transform.position, M_PVM.transform.position, Color.red);
            //Debug.DrawLine(persons[0].transform.position, M_Pump.transform.position, Color.blue);
            //TODO: Need to check if it is an anonamous getting close to machine. Only Anonamous people trigger the alarm.
            for (int i = 1; i < 7; i++)
            {
                if (persons[i - 1].GetComponent<PersonInfo>().GetPersonName() == "")
                {
                    distancePVM[i - 1] = (persons[i - 1].transform.position - M_PVM.transform.position).magnitude;
                    distancePump[i - 1] = (persons[i - 1].transform.position - M_Pump.transform.position).magnitude;
                }
                else
                {
                    distancePVM[i - 1] = 100;
                    distancePump[i - 1] = 100;
                }
                //guiText [i].text = "userID:" + i + "  dist:" + distance[i-1];
            }

            for (int i = 1; i < 7; i++)
            {
                if (distancePVM[i - 1] < alarmDistance || distancePump[i - 1] < alarmDistance) showAlarm = true;
                // too close
            }


            if (showAlarm && prevAlarm == false)
            {
                guiText[0].text = "Go Away!!";
                //Debug.Log("TOO CLOSE!!!!");
                outPutLogger.Log("Anonymous person too close to a machine");
                popUpsCont.showPopup(1, "Please do not tamper with\nmedical equipment.", "Unauthorised people are not allowed to use the\nPMV machine on their own.");
            }
            else if (showAlarm == false && prevAlarm)
            {
                guiText[0].text = "Come back";
                //Debug.Log("Come back");
                outPutLogger.Log("Anonymous person has moved away from the machines");
                popUpsCont.hideAllPopups();
            }
            else
            {
                guiText[0].text = "nothing";
            }
            prevAlarm = showAlarm;
            showAlarm = false;
        }
        

    }
}
