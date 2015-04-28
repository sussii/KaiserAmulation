using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {
	public bool isOut;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnCollisionEnter(Collision col){
		if(col.gameObject.name == "Door")
			isOut = true;
		Debug.Log(isOut);
	}
	/*void OnColliderExit(Collider col)
	{
		if(col.gameObject.name == "Door")
			isOut = false;
		Debug.Log( isOut);
	}*/

}
