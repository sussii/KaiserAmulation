using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

public class LinePath : MonoBehaviour {

	public int id;
	public float length;
	public Shader shader;
	public Color32 color;

	public float minMoveDist;
    public float minMoveAngle = 20; //Degree
	public TextMesh fakeDistance;
	public HeadsUpController headsUpCont;
    public PositionInfo positionInfo;
    public bool startWalk;

	private Material mat;
	private Vector3[] points;// = new Vector3[3];
	private VectorLine line;
	private List<Vector3> allPoints = new List<Vector3>();
	private Vector3 oldPosition;

    
    private float tempDist;
    private float oldDistance;
    private float currentAngle;
    private float oldAngle;

	// Use this for initialization
	void Start () {
		points = new Vector3[3000];
		points[0] = transform.position;
		allPoints.Add(points[0]);
		
		mat = new Material(shader);
		mat.shader = Shader.Find ("Solid Color");
		Color32 col = new Color32((byte)Random.Range(0,255),(byte)Random.Range(0,255),(byte)Random.Range(0,255),255);
		mat.color =col;
		
		line = new VectorLine("Path", points, col, mat, 6f, LineType.Continuous, Joins.Fill);

        startWalk = false;
        length = 0;
		//line.vectorObject.transform.parent = transform;
		//line.Draw3DAuto();
	}
	
	// Update is called once per frame
	void Update () {
        //if (!startWalk) return; //only show on the ACT III
		if(Input.GetKeyUp(KeyCode.I)){
			length = 0;
		}
        if (!positionInfo.InBed(gameObject))
        { 
		    if(!allPoints.Contains(transform.position)){
			    tempDist = Vector3.Distance(transform.position, oldPosition);
		        currentAngle = Vector3.Angle(transform.position, oldPosition);
			    if(oldDistance > minMoveDist && (currentAngle-oldAngle) < Mathf.Deg2Rad * minMoveAngle){
				    length += oldDistance;
                    //Debug.Log("Adding distance, angle:" + (currentAngle - oldAngle));
			    }
			    allPoints.Add(transform.position);
			    Vector3[] tempPoints = new Vector3[allPoints.Count];
			    //allPoints.CopyTo(points);
			    //line.points3 = points;
			    //line.Draw3DAuto();
			    //line.material.color = color;
		    }
	        oldAngle = currentAngle;
		    oldPosition = transform.position;
	        oldDistance = tempDist;
		    //if(fakeDistance != null){
		    //	fakeDistance.text = length.ToString().Split('.')[0] + "ft";
			    headsUpCont.updateStepCounter(Mathf.FloorToInt(length));
		    //}
        }
	}

	public void SetColor(UnityEngine.Color c){
		color = (Color32)c;
	}
	public void ResetLength()
	{
		length = 0;
        startWalk = false;
	}
}
