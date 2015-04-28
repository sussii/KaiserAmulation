using UnityEngine;
using System.Collections;

public class PositionInfo : MonoBehaviour
{
    public GameObject testCube;

    public Transform door;
    public Transform[] wallCorner;
    public Transform[] bedCorner;
    public float closedistance = 3;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
       // Debug.Log("In Room: " + InRoom(testCube));
        //Debug.Log("In Bed: " + InBed(testCube));
	}

    public bool InBed(GameObject obj)
    {
        Vector3 objPos = obj.transform.position;
        if (objPos.z < bedCorner[0].position.z && objPos.z > bedCorner[1].position.z &&
            objPos.x < bedCorner[2].position.x && objPos.x > bedCorner[3].position.x)        
            return true;
        return false;
    }

    public bool InRoom(GameObject obj)
    {
        Vector3 objPos= obj.transform.position;
        if (objPos.z < wallCorner[0].position.z && objPos.z > wallCorner[1].position.z &&
            objPos.x < wallCorner[2].position.x && objPos.x > wallCorner[3].position.x)
            return true; 
        return false;
    }

    public bool CloseEnough(GameObject obj0 , GameObject obj1) //use case: doctor and patient are close to eachother
    {
        if (obj0 == null || obj1 == null) return false; 
        float actualDistance = (obj0.transform.position - obj1.transform.position).magnitude;
        if (actualDistance<= closedistance)
            return true;
        return false;
    }

}
