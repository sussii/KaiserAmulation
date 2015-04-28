using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using SimpleJSON;

public class EntryData : MonoBehaviour {


	public string ID = "";
	public string Type = "";
	public string Name = "";
	public bool Female = true;
	public string JobTitle = "";
	public string typeOfEquipment = "";
	//public string[] MedicationNames = new string[5];
	public JSONArray TasksList = new JSONArray();
	public JSONArray ItemsList = new JSONArray();

	public bool InRoom;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
