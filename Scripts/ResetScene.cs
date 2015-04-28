using UnityEngine;
using System.Collections;
/*Reset scene when there are obvious ghost objects in the room. 
 * Reset to be:
 * nobody in the room clear up all person info.
 * no machines in the room
 * no icons on the screen, except for an empty patient icon.
 * ambulation distance set to zero
 * no pop up window. 
 */

public class ResetScene : MonoBehaviour
{
    public WalkThrough walkThrough;
    public UserController userCont;
    public MachineController machineCont;
    public PeopleInRoomController peepsInRoom;
    
	// Use this for initialization
	void Start () {
        walkThrough = GameObject.Find("HeadsUp").GetComponent<HeadsUpController>().walkThroughCont;
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetKeyUp(KeyCode.U))
	    {      
            for (int i = 0; i < userCont.userMax; ++i )
            {
                userCont.persons[i].GetComponent<LinePath>().ResetLength();
                userCont.persons[i].GetComponent<PersonInfo>().Reset();
            }
	        machineCont.ResetPosition();
	        walkThrough.Reset();
            peepsInRoom.resetIcons();
        }
        
	}
}
