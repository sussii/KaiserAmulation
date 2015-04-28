using System;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using System.Collections;

[System.Serializable]
public class PersonInfo : MonoBehaviour {
	
	public float depthScale = 0.00328084f*7;
	private int userID = 0; // from kinect's perspective
	public 	Transform door;
	public DepthToGray d2g;
	public QuadSilhouette[] qs;
	/*public UnityEngine.Color[] userColors =new UnityEngine.Color[7]{ UnityEngine.Color.red, 
												UnityEngine.Color.yellow, 
												UnityEngine.Color.green, 
												UnityEngine.Color.blue, 
												UnityEngine.Color.cyan, 
												UnityEngine.Color.white, 
												UnityEngine.Color.magenta,																
	};*/
    public string personName;
    //person hospital info
	private BadgeInfo badgeInfo;

	//person position info
	private Vector2 personCenter;
	private float depth = 0;
	
	//person status info
	private bool isFreezing;
	private bool isOut = true;
	private Vector3 oldPos;
	private Vector3 currentPos;
	private Vector3 originPos;

    private int wholeTextureOnce; // when kinect detect a person, there will be one frame that person center is (0.5f,0.5f) at very beginning. so get rid of the first few frames
    private int ignoredFrames = 3;


	// Use this for initialization
	void Start () {
		originPos = new Vector3 (11, 5, 0);
		currentPos = originPos;
		badgeInfo = new BadgeInfo ("","","","");
		//userObj.GetComponent <LinePath>().SetColor(userColors [userID]);
	}
	
	// Update is called once per frame
	void Update ()
	{
	    personName = GetPersonName();
		//Debug.Log ("user id: " + userID);
        //Debug.Log("person: " + gameObject.name + " isOut: " + isOut);
		if(!isFreezing)
		{
			if(qs!=null && userID!= 0)
			{
                if(userID<=0 || userID > 6) Debug.Log("UserID:" + userID);
				personCenter = qs[userID].GetPersonCenter();
				depth = qs[userID].GetDepth();
                //Debug.Log("id:" + userID + "  person center: " + personCenter.x);// + "  depth: " + depth
			}
			if( personCenter.x >=0 && personCenter.x <=1 && personCenter.y >=0 && personCenter.y <= 1 && depth>0.01f)
			{
				currentPos = Camera.main.ViewportToWorldPoint(new Vector3(1-personCenter.x,personCenter.y,depth * depthScale));
				gameObject.transform.position = currentPos;
				/*if(personIdx == 0)
					persons[personIdx].GetComponent<LinePath>().ResetLength();*/
				
			}
			else
			{
				currentPos = originPos;
				gameObject.transform.position = currentPos;
			}
		}
		
		//check if the person is out of room, otherwise he is hidden.
		if (currentPos.z > door.position.z && currentPos.x > door.position.x){ // Took out: personCenter.x >= 0.9f &&
			isOut = true;
		}else{
			isOut = false;
		}
	}
	public int UserID
	{
		get {return userID;}
		set {userID = value;}
	}
	public void Freeze()
	{
		Debug.Log ("Freeze: " + gameObject.name);
		isFreezing = true;
	    wholeTextureOnce = 0;
		gameObject.transform.position = currentPos;
	}
	public Vector3 GetCurrentPosition()
	{
		return currentPos;
	}

    public bool CheckValidPosition()
    {
        if (wholeTextureOnce < ignoredFrames) wholeTextureOnce ++;
        //Debug.Log("wholeTextureOnce: " + wholeTextureOnce + "depth:" + depth + " center: " + personCenter);
        if (wholeTextureOnce != ignoredFrames) return false;
        return (personCenter != Vector2.zero && depth > 0.01f);
    }

    public Vector3 GetPosition(int t)
	{
		if(qs!=null && t!= 0)
		{
			personCenter = qs[t].GetPersonCenter();
			depth = qs[t].GetDepth();
			//Debug.Log("t:" + t + "depth:" + depth + " center: " + personCenter);
		}
		if (personCenter.x >= 0 && personCenter.x <= 1 && personCenter.y >= 0 && personCenter.y <= 1 && depth >0.01f) {
			return Camera.main.ViewportToWorldPoint (new Vector3 (1 - personCenter.x, personCenter.y, depth * depthScale));		
		} 
	    return originPos;
	}

    public String GetPersonName()
    {
        return badgeInfo.name;
    }

    public String GetJobTitle()
    {
        return badgeInfo.title;
    }

    public void UnFreeze()
	{
		//TODO: assign userID
		isFreezing = false;
	}
	public bool IsOut(){
		return isOut;
	}
	public void Reset()
	{
		//Debug.Log ("Reset: " + gameObject.name);
		personCenter = Vector2.zero;
		depth = 0;
		//person status info
		isFreezing = false;
		isOut = true;
		oldPos = originPos;
		currentPos = originPos;
		gameObject.transform.position = originPos;   
		//person hospital info
		badgeInfo.Clear ();
        userID = 0;
	    wholeTextureOnce = 0;
	}
	public Vector3 GetOriginPos()
	{
		return originPos;
	}

	public Vector2 GetPersonHorizonalPosition()
	{
		return new Vector2 (gameObject.transform.position.x, gameObject.transform.position.z);
	}
	public void SetBadgeInfo(BadgeInfo bf)
	{
		badgeInfo.Set(bf);
        //Debug.Log(gameObject.name + "has been set to name: " + bf.name); 
	}
	public static void Equal(PersonInfo p)
	{
		//UserID = p.UserID;
		//badgeInfo.Set(p.badgeInfo);
		
	} 

}
