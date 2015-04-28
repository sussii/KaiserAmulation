using System;
using System.Collections;
using System.Drawing;
using System.Security.Cryptography;
using UnityEngine;
using System.Collections.Generic;
using Color = UnityEngine.Color;


/* Project: Kaiser
 * Date: Last modified at 10/22/2014
 * Note: Attach this script to GameObject "UserControler"
 * This is a main script controlling people in the room. Basically, compare kinect people to current people in the room. Mainly about hidden situation, come in and out situation. 
 * The script is also an interface for wirepoint to pass data.
 * */
public class UserController : MonoBehaviour
{
    public BodyIndexSourceManager kinect2BodyIndex;
    public int userMax = 6;
    public int PendingPersonIdx; //persons[pendingPersonIde] who is waiting for being assign data from Awarepoint
    public DepthToGray d2g;
    public TrackingInterface trackintInterface;
    //Debug
    public GUIText guiText_kList;
    public GUIText guiText_pList;
    public GUIText guiText_tmpList;

    public GameObject[] persons;
    public GameObject fakePerson;
    public float isSamePersonThresh;
    public float anonamousTimeOut;
    public HeadsUpController headsupCont;
    public OutPutLog outPutLogger;


    private List<int> _kList; // get user idx from kinect
    private List<int> _pList; // store index read from kinect
    private List<int> tmpList; // store hidden people
    private List<float> tmpDistance;
    //private PersonInfo newInfo = null;
    private BadgeInfo newInfo = null;

    private bool lostPersonChecked = true;

    // Use this for initialization
    void Start()
    {
        _pList = new List<int>(userMax);
        _kList = new List<int>(userMax);
        tmpList = new List<int>(userMax);
        tmpDistance = new List<float>(userMax);
        newInfo = new BadgeInfo(); // latest badge info from wirepoint
        if (!trackintInterface) Debug.Log("No tracking interface attached to user controller");
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("!!!!!~ person[0].isOut: " + persons[0].GetComponent<PersonInfo>().IsOut());
    	if(Input.GetMouseButtonUp(2)){
			foreach(GameObject pers in persons){
				pers.GetComponent<LinePath>().length = 0;
			}
		}
        //Debug.Log ("fake person out status:" + fakePerson.GetComponent<PersonInfo> ().IsOut ());
        _kList.Clear();
        for (var i = 1; i <= userMax; i++)
        {
            if (d2g.GetUserDepth(i) > 0f)
            {
                _kList.Add(i);
                //Debug.Log("i: " + i + " depth: " + d2g.GetUserDepth(i));
            }
        }

        var gui = "";

        guiText_pList.text = "";
        foreach (int t in _pList)
        {
            gui = guiText_pList.text + "," + t;
            guiText_pList.text = gui;
        }
        //guiText_pList.text = "pList size: " + pList.Count;

        string guik = "";
        guiText_kList.text = "";
        foreach (int t in _kList)
        {
            guik = guiText_kList.text + "," + t;
            guiText_kList.text = guik;
        }

        string guit = "";
        guiText_tmpList.text = "";
        foreach (int t in tmpList)
        {
            guit = guiText_tmpList.text + "," + t;
            guiText_tmpList.text = guit;
            //guiText_tmpList.text = guit;
        }

        if (_kList.Count > _pList.Count) // ToDo: Double check with WIREPOINT if someone just came in: Put new person(detected by kinect) in queue, wait for his info from WIREPOINT.
        {
            foreach (int t in _kList)
            {
                if (!_pList.Contains(t) )
                {   
                        AddNewPerson(t);
                }
            }
        }
        if (_kList.Count <= _pList.Count) // a person left or hide
        {
            foreach (int t in _pList.ToArray())
            {
				int personIdx = _pList.IndexOf(t);           
                if (!_kList.Contains(t) && t != -1)
                {
                    if (persons[personIdx].GetComponent<PersonInfo>().IsOut())	// ToDO: If necessery, double check with WIREPOINT if some one just left.
                    {            
                        SendLeftPersonName(persons[personIdx].GetComponent<PersonInfo>().GetPersonName());
                        Debug.Log(persons[personIdx].name + " has left, reset it");
                        string tempPersonName = persons[personIdx].GetComponent<PersonInfo>().GetJobTitle();
                        if (tempPersonName.Equals(""))
                        {
                            tempPersonName = "Unknown";
                        }
                        EntryData tempentrydata = new EntryData();
                        tempentrydata.typeOfEquipment = "";
                        tempentrydata.JobTitle = tempPersonName;
                        headsupCont.LeftRoom(tempentrydata);

                        outPutLogger.Log(tempPersonName + " has left the room");

                        persons[personIdx].GetComponent<PersonInfo>().Reset();
                        LeftShiftPersons(personIdx);
                        if (tmpList.Count != 0)
                        {
                            for (var i = 0; i < tmpList.Count; ++i)
                                if (tmpList[i] >= personIdx) tmpList[i]--;
                        }
                        _pList.Remove(t);
                    }
                    else //hide	
                    {
                        Debug.Log("personIdx:" + personIdx + "  " + persons[personIdx].name + " is hiding");
                        //outPutLogger.Log(persons[personIdx].name + " is hiding");
                        _pList[personIdx] = -1;
                        tmpList.Add(personIdx);
                        //fakePerson.GetComponent<PersonInfo>().UnFreeze();
                        Debug.Log("tmpList.Count: " + tmpList.Count);
                        persons[personIdx].GetComponent<PersonInfo>().Freeze();
                    }
                }
            }
            //kList.Count < pList.Count  
            foreach (int t in _kList)
            {
                if (!_pList.Contains(t)) // new person that comes out from hidden
                {
                    if (tmpList.Count != 0)
                    {
                        int personLost;
                        //personLost = -2 --not decide yet
                        //personLost = -1 -- no person feet distance check, especially for new person came in
                        //personLost = k  --reassign id to lost person

                        personLost = IsPersonLost(t);
                        Debug.Log("Person Lost: " + personLost);
                        if (personLost == -2) continue;
                        else if (personLost == -1) { AddNewPerson(t); } //ToDo: Check from WIREPOINT if new person comes in. personLost will never be -1 then.
                        else
                        {
                            Debug.Log("tmpList is not Empty, Re-assign " + personLost + "th in pList ");
                            tmpList.Remove(personLost);
                            _pList[personLost] = t;
                            persons[personLost].GetComponent<PersonInfo>().UserID = t;
                            persons[personLost].GetComponent<PersonInfo>().UnFreeze();
                        }
                    }
                }
            }
        }

    }

    public void EnteredRoom(EntryData data) // ToDo: Get info from AWAREPOINT, assign it to viarable "newInfo"
    {
        if (data.Type == "Person")
        {
            bool existing = false;

            foreach (GameObject p in persons)
            {
                if (p.GetComponent<PersonInfo>().GetPersonName().Equals(data.Name))
                    existing = true;
            }
            bool pending = persons[PendingPersonIdx].GetComponent<PersonInfo>().GetPersonName().Equals("");
            //Debug.Log("pending idx:" + PendingPersonIdx + "equals to null:" + pending);
            if (!existing && pending)
            {
                
                newInfo.ID = data.ID;
                newInfo.name = data.Name;
                newInfo.title = data.JobTitle;
                persons[PendingPersonIdx].GetComponent<PersonInfo>().SetBadgeInfo(newInfo);
                //Debug.Log(persons[PendingPersonIdx].name + "has been assigned Name: " + data.Name);
                if (persons[PendingPersonIdx].GetComponent<PersonInfo>().GetJobTitle().Equals("Patient"))
                {
                    //Debug.Log("patient name:" + data.Name + ", set line path to 0.");
                    LinePath linepath = persons[PendingPersonIdx].GetComponent<LinePath>();
                    linepath.enabled = true;
                    linepath.ResetLength();
                }
                trackintInterface.pendingAlarm = false;
            }


        }

        /*if (Input.GetKeyDown (KeyCode.Alpha8)) {
			
		}
		if (Input.GetKeyDown (KeyCode.Alpha9)) {
			newInfo.name = "Nancy S";
			newInfo.title = "Nurse";
			return true;
		}
		if (Input.GetKeyDown (KeyCode.P)) {
			newInfo.name = "Peter Parker";
			newInfo.title = "Patient";
			return true;
		}*/
    }

    public void AddNewPerson(int t)
    {
        _pList.Add(t);
        int personIdx = _pList.IndexOf(t);
        PendingPersonIdx = personIdx;
        //Debug.Log("Someone Coming:" + persons[personIdx].name);

        newInfo.Clear();
        persons[personIdx].GetComponent<PersonInfo>().UserID = t;
        persons[personIdx].GetComponent<PersonInfo>().SetBadgeInfo(newInfo);
        //Debug.Log("Anonamous person just came in, waiting for identification info from Awarepoint");
        trackintInterface.pendingAlarm = true;
        StartCoroutine("WaitAnonTimeOut", anonamousTimeOut);
        headsupCont.EnterRoomAnon(); //
    }


    private int IsPersonLost(int t)
    {
        fakePerson.transform.position = new Vector3(10, 10, 10);
        fakePerson.GetComponent<PersonInfo>().UserID = t;
        bool fakePersonValid = fakePerson.GetComponent<PersonInfo>().CheckValidPosition();
        Vector3 fakePos = fakePerson.GetComponent<PersonInfo>().GetPosition(t);
        Vector3 originPos = fakePerson.GetComponent<PersonInfo>().GetOriginPos();
        if (fakePos.Equals(originPos) || (!fakePersonValid))
            return -2;

        //position of ID t should be less then thresh. -> existing person908
        foreach (int k in tmpList)
        {
            Vector3 lostPersonPos = persons[k].GetComponent<PersonInfo>().GetCurrentPosition();
            Vector3 posDistance = fakePos - lostPersonPos;
            //Debug.Log("Lost Person Position: " + lostPersonPos);
           // Debug.Log("fakePerson Position:" + fakePos);
            // fakePerson.GetComponent<PersonInfo>().Freeze();

            float distance = posDistance.magnitude;
            if (distance >= isSamePersonThresh)
                Debug.DrawLine(fakePos, lostPersonPos, Color.red);
            else
                Debug.DrawLine(fakePos, lostPersonPos, Color.green);
            tmpDistance.Add(distance);
        }
        //check who is the closest person who also appear within threshold-distance
        float min = isSamePersonThresh;
        int minIndex = -1;
        for (int i = 0; i < tmpDistance.Count; ++i)
        {
            //Debug.Log("tmpDistance[" + i + "]" + ": " + tmpDistance[i] + "   isSamePersonThresh:" + isSamePersonThresh);
            if (tmpDistance[i] < min)
            {
                min = tmpDistance[i];
                minIndex = i;
            }
        }
        tmpDistance.Clear();

        return ((minIndex == -1) ? -1 : tmpList[minIndex]);
    }

    void LeftShiftPersons(int personleft) // left shift the person objs, so that there is no empty spot in plist. plist index will be matched with persons[] obj
    {
        GameObject tmpObj = persons[personleft];
        for (int i = personleft; i < (persons.Length - 1); i++)
        {
            persons[i] = null;
            persons[i] = persons[i + 1];
            persons[i + 1] = null;
        }
        int last = persons.Length - 1;
        persons[last] = tmpObj;
        persons[last].GetComponent<PersonInfo>().Reset();
    }


    /* private int IsPersonLost(int t)
    {
        fakePerson.GetComponent<PersonInfo> ().UserID = t;
        Vector3 fakePos = fakePerson.GetComponent<PersonInfo>().GetCurrentPosition();
        Vector3 originPos = fakePerson.GetComponent<PersonInfo>().GetOriginPos();
        if (fakePos.Equals (originPos))
            return -2;
        Debug.Log("fakePerson Position:" + fakePos);
        //position of ID t should be less then thresh. -> existing person
        foreach (int k in tmpList) {
            Vector3 lostPersonPos = persons[k].GetComponent<PersonInfo>().GetCurrentPosition();
            Vector3 posDistance = fakePos - lostPersonPos;
            Debug.DrawLine(fakePos,lostPersonPos, UnityEngine.Color.green);
            float distance = posDistance.magnitude; 
            if(distance < isSamePersonThresh)
                return  k;
        }
        return -1; //the person was not in the room
		
		
    }*/

    private IEnumerator WaitAnonTimeOut(float time)
    {
        yield return new WaitForSeconds(time);
        if (persons[PendingPersonIdx].GetComponent<PersonInfo>().GetPersonName().Equals(""))
        {         
            outPutLogger.Log("Anonymous person has entered the room");
            headsupCont.WelcomeAnon();
            yield return new WaitForSeconds(5f); // time = WelcomAnon() delay hide all
            trackintInterface.pendingAlarm = false;   
            newInfo.Clear();
        }     
    }

    public void SendLeftPersonName(string name)
    {
        GameObject.Find("HeadsUp").GetComponent<HeadsUpController>().walkThroughCont.ReceiveLeftPersonName(name);    
    }
}