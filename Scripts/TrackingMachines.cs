using UnityEngine;
using System.Collections;


/* Project: Kaiser
 * Date: 10/02/2014
 * Attach this script to GameObject "ObjectTracking_Contour"
 * This script is used to place two machines(PVM and Pump) in the 3D world.
 * */
public class TrackingMachines : MonoBehaviour {
	
	public DisplayContours dc;
	public DepthToGray d2g;
	public float depthScale = 0.00328084f * 6f;
	public GameObject M_PVM; // machine0
	public GameObject M_Pump; // machine1
	//optimize object detection
	short oldDepth_PVM = 0;
	float oldX_PVM = 0;
	float oldY_PVM = 0;
	
	short oldDepth_Pump = 0;
	float oldX_Pump = 0;
	float oldY_Pump = 0;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if (dc.GetContourNum() != 0)
	    {
            float x = dc.GetContourPos(0).x;
            float y = dc.GetContourPos(0).y;

            if (x <= 0 || y <= 0)
            {
                x = oldX_PVM;
                y = oldY_PVM;
            }
            oldX_PVM = x;
            oldY_PVM = y;

            Vector2 vpCenter = new Vector2(x, y);

            short depth = d2g.GetPixelDepth(vpCenter);
            if (depth == 0)
                depth = oldDepth_PVM;
            if (depth != 0 && oldDepth_PVM != 0 && Mathf.Abs(depth - oldDepth_PVM) > 100)
            {
                //Debug.Log("real depth = " + depth);
                depth = oldDepth_PVM;
            }
            oldDepth_PVM = depth;


            // Debug.Log ("depthPMV: "+depth + "   vpCetner.x:" + vpCenter.x);
            if (vpCenter.x > 0.95f)
            {
                depth = 0;
            } // if machine is out, set depth = 0, so that the machine module is out of room
            Vector3 worldPos = Camera.main.ViewportToWorldPoint(new Vector3(1 - vpCenter.x, 0.5f, depthScale * depth)); //depth* 0.00328084f*6f //1-vpCenter.x
            M_PVM.transform.position = worldPos;

            //Pump
            float xPump = dc.GetContourPos(1).x;
            float yPump = dc.GetContourPos(1).y;

            if (x <= 0 || y <= 0)
            {
                x = oldX_Pump;
                y = oldY_Pump;
            }
            oldX_Pump = xPump;
            oldY_Pump = yPump;

            Vector2 vpCenterPump = new Vector2(xPump, yPump);

            short depthPump = d2g.GetPixelDepth(vpCenterPump);
            if (depthPump == 0)
                depthPump = oldDepth_Pump;
            if (depthPump != 0 && oldDepth_Pump != 0 && Mathf.Abs(depthPump - oldDepth_Pump) > 100)
            {
                //Debug.Log("real depth = " + depth);
                depthPump = oldDepth_Pump;
            }
            oldDepth_Pump = depthPump;

            if (vpCenterPump.x > 0.95f) depthPump = 0;
            //Debug.Log ("depthPump: "+depthPump + "   vpCetner:" + vpCenterPump);
            M_Pump.transform.position = Camera.main.ViewportToWorldPoint(new Vector3(1 - vpCenterPump.x, 0.5f, depthScale * depthPump));
	    }
	    else // if contour counts == 0
	    {
	        M_PVM.transform.position = Camera.main.gameObject.transform.position;
            M_Pump.transform.position = Camera.main.gameObject.transform.position;

	    }
		
	}
}
