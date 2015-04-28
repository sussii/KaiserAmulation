using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using SimpleJSON;

public class KaiserJSON : MonoBehaviour {

	public string Url = "http://192.168.51.18/updates";
	public int personCount = 0;
	public OutPutLog outPutLogCont;
	public GameObject headsUpDisplayCont;
	public GameObject userCont; 
    public MachineController machineCont;
	
	private JSONNode _jNode;
    WWW www;
	//private bool _init = false;

    private string _preData = "{\"saveData\":[{\"id\":\"20000039ec\",\"name\":\"Diana Lane\",\"type\":\"Person\",\"gender\":\"Female\",\"jobTitle\":\"Patient\",\"TasksList\":[\"\",\"\"],\"ItemsList\":[\"\",\"\"],\"typeOfEquipment\":\"\"},{\"id\":\"2000003aac\",\"name\":\"Infustion Pump\",\"type\":\"Equipment\",\"gender\":\"\",\"jobTitle\":\"ip\",\"TasksList\":[\"\",\"\"],\"ItemsList\":[\"\",\"\"],\"typeOfEquipment\":\"Infustion Pump\"},{\"id\":\"14eb0100013053\",\"name\":\"PVM\",\"type\":\"Equipment\",\"gender\":\"\",\"jobTitle\":\"PVM\",\"TasksList\":[\"\",\"\"],\"ItemsList\":[\"\",\"\"],\"typeOfEquipment\":\"PVM\"},{\"id\":\"2000003a4a\",\"name\":\"Eileen\",\"type\":\"Person\",\"gender\":\"Female\",\"jobTitle\":\"Doctor M.D.\",\"TasksList\":[\"\",\"\"],\"ItemsList\":[\"\",\"\"],\"typeOfEquipment\":\"\"},{\"id\":\"2000003a7f\",\"name\":\"Michael\",\"type\":\"Person\",\"gender\":\"Male\",\"jobTitle\":\"Medtech\",\"TasksList\":[\"Take vitals for Diana\",\"\"],  \"ItemsList\":[\"\",\"\"],\"typeOfEquipment\":\"\"},{\"id\":\"2000003b51\",\"name\":\"Dylan\",\"type\":\"Person\",\"gender\":\"Male\",\"jobTitle\":\"Nurse, R.N.\",\"TasksList\":[\"Take Meds for Diana\",\"Replace IV Bag\"], \"ItemsList\":[\"IV Bag with Medication\",\"IV Bag with Medication\"],\"typeOfEquipment\":\"\"},{\"id\":\"20000039c8\",\"name\":\"IV Bag1\",\"type\":\"Equipment\",\"gender\":\"\",\"jobTitle\":\"iv bag1\",\"TasksList\":[\"\",\"\"],\"ItemsList\":[\"\",\"\"],\"typeOfEquipment\":\"\"},{\"id\":\"2000003c0f\",\"name\":\"IV Bag2\",\"type\":\"Equipment\",\"gender\":\"\",\"jobTitle\":\"iv bag2\",\"TasksList\":[\"\",\"\"],\"ItemsList\":[\"\",\"\"],\"typeOfEquipment\":\"\"}]}";  
	private Dictionary<string,EntryData> _entryList = new Dictionary<string, EntryData>();
	private JSONNode _preJSONData;
	private Dictionary<string,int> _idToIndex = new Dictionary<string, int>(); 

	// Use this for initialization
	void Start () {
		_preJSONData = JSONNode.Parse(_preData);
        _idToIndex.Add("20000039ec", 0); //_idToIndex.Add("2000000b94",0);
		_idToIndex.Add("2000003aac",1);
		_idToIndex.Add("2000003a1c",2);
		_idToIndex.Add("14eb0100013053",2);
		_idToIndex.Add("2000003a4a",3);
		_idToIndex.Add("2000003a7f",4);
		_idToIndex.Add("2000003b51",5);
        _idToIndex.Add("20000039c8",6);
        _idToIndex.Add("2000003c0f",7);
        www = new WWW(Url);
	}
	
	// Update is called once per frame
	void Update () {
		//if(!_init){
			//_init = true;
			//if(withTablet){			
				StartCoroutine (WaitForRequest (www));
			//}
		//}
		personCount = _entryList.Count;
	}

	private IEnumerator WaitForRequest(WWW w){
        www = new WWW(Url);
		yield return w;
		
		// check for errors
		if (w.error == null){
			string data = w.text;
			data = data.Replace('+',' ');
			_jNode = JSON.Parse(data);
			//Debug.Log("Data= " + data);
			if(_jNode["updates"].AsArray.Count != 0){
				FetchDataScan2(-1);
			}	
		}else {
			Debug.Log("WWW Error: " + w.error);
		}
		yield return new WaitForSeconds (0.5f);
		//WWW www = new WWW (Url);
		//StartCoroutine (WaitForRequest(www));
	}

	private void FetchDataScan2(int bKey){
		//Debug.Log("FetchDataScan2");
		int checkInLength = _jNode["updates"].AsArray.Count;
		//Debug.Log("checkInLength= "+checkInLength);

		for(int i = 0; i < checkInLength ; i++){
			string deviceId = _jNode["updates"][i]["device"].Value;
			string roomName = _jNode["updates"][i]["roomName"].Value;
			bool isInroom = false;
			if(roomName == "160 Patient Room"){//Patient Room  160 Patient Room  Room 347
				isInroom = true;
			}

			if(deviceId != null){
				//Debug.Log("deviceId= " + deviceId+", roomName= "+roomName);
				EntryData tempEntry = new EntryData();
				if(_entryList.ContainsKey(deviceId)){
					tempEntry = _entryList[deviceId];
					tempEntry.InRoom = isInroom;
					if(isInroom){
						headsUpDisplayCont.SendMessage("EnteredRoom",tempEntry, SendMessageOptions.DontRequireReceiver);
						userCont.SendMessage("EnteredRoom",tempEntry, SendMessageOptions.DontRequireReceiver);
						outPutLogCont.Log("Awarepoint: "+tempEntry.Name+" has entered the room");
					}else{
					    if (machineCont.IsOut() && tempEntry.Type == "Equipment")
					    {
                            headsUpDisplayCont.SendMessage("LeftRoom", tempEntry, SendMessageOptions.DontRequireReceiver);
                            machineCont.ResetisOutStatus();
					    }	    
						//userCont.SendMessage("LeftRoom",tempEntry, SendMessageOptions.DontRequireReceiver);
						outPutLogCont.Log("Awarepoint: "+tempEntry.Name+" has left the room");
					}
				}else{
					tempEntry.ID = deviceId;
					tempEntry.Name = _preJSONData["saveData"][_idToIndex[deviceId]]["name"];
					tempEntry.Type = _preJSONData["saveData"][_idToIndex[deviceId]]["type"];
					if(tempEntry.Type == "Person"){
						tempEntry.Female = _preJSONData["saveData"][_idToIndex[deviceId]]["gender"].AsBool;
						tempEntry.JobTitle = _preJSONData["saveData"][_idToIndex[deviceId]]["jobTitle"];
						tempEntry.TasksList = _preJSONData["saveData"][_idToIndex[deviceId]]["TasksList"].AsArray;
						tempEntry.ItemsList = _preJSONData["saveData"][_idToIndex[deviceId]]["ItemsList"].AsArray;

					}else if(tempEntry.Type == "Equipment"){
						tempEntry.typeOfEquipment = _preJSONData["saveData"][_idToIndex[deviceId]]["typeOfEquipment"];
                        tempEntry.JobTitle = _preJSONData["saveData"][_idToIndex[deviceId]]["jobTitle"];
					}
					tempEntry.InRoom = isInroom;
					_entryList.Add(deviceId,tempEntry);
					if(isInroom){
						headsUpDisplayCont.SendMessage("EnteredRoom",tempEntry, SendMessageOptions.DontRequireReceiver);
						userCont.SendMessage("EnteredRoom",tempEntry, SendMessageOptions.DontRequireReceiver);
						outPutLogCont.Log("Awarepoint: "+tempEntry.Name+" has entered the room");
					}else{
						//headsUpDisplayCont.SendMessage("LeftRoom",tempEntry, SendMessageOptions.DontRequireReceiver);
						//userCont.SendMessage("LeftRoom",tempEntry, SendMessageOptions.DontRequireReceiver);
						outPutLogCont.Log("Awarepoint: "+tempEntry.Name+" has left the room");
					}
				}
			}
		}
	}
}
