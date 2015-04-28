using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;

public class CalculateContours : MonoBehaviour {

	public enum TYPE {
		INTERNAL_ONLY,
		EXTERNAL_ONLY,
		ALL
	};
	
	public double thresh = 170;
	public double threshLinking = 160;
	
	//public float imageBinaryThreshold = 0f;
	
	public TYPE type = TYPE.INTERNAL_ONLY;
	
	public float smoothingFactor = 0.5f;
	
	public float accuracy = 0f;
	public float minPerimeter = 0f;
	
	public bool showContour = false;
	
	private MemStorage storage = new MemStorage();
	
	// Update is called once per frame
	public List<List<Vector2>> Do (Image<Gray, Byte> input) 
	{
		if(input == null) return null;
		
		Image<Gray, Byte> img = input.Copy();
		
		Byte[,,] grayData = img.Data;
		
		int dw = img.Width;
		int dh = img.Height;
		
		// Add border
		if(type == TYPE.ALL)
		{
			grayData = input.Data;
			
			for(int y = dh - 1, x = dw - 1; y >= 0; y--)	grayData[y, x, 0] = 0;
			for(int y = dh - 1, x = 0; y >= 0; y--)			grayData[y, x, 0] = 0;
			for(int x = dw - 1, y = dh - 1; x >= 0; x--)	grayData[y, x, 0] = 0;
			for(int x = dw - 1, y = 0; x >= 0; x--)			grayData[y, x, 0] = 0;
			
			for(int y = dh - 1, x = dw - 2; y >= 0; y--)	grayData[y, x, 0] = 0;
			for(int y = dh - 1, x = 1; y >= 0; y--)			grayData[y, x, 0] = 0;
			for(int x = dw - 1, y = dh - 2; x >= 0; x--)	grayData[y, x, 0] = 0;
			for(int x = dw - 1, y = 1; x >= 0; x--)			grayData[y, x, 0] = 0;
		}
		
		// Detect edges
		Image<Gray, Byte> cannyEdges = img.Canny(thresh, threshLinking);
		
		List<Contour<Point>> contours = new List<Contour<Point>>();
		
		using (storage)
		{
			Contour<Point> c = cannyEdges.FindContours(
				Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
				type == TYPE.EXTERNAL_ONLY ? Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL : Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_TREE);
			
			if(c != null)
			{
				Contour<Point> currentContour = c.ApproxPoly(c.Perimeter * smoothingFactor, 1, storage);
				AddContourToList(ref contours, currentContour ,true);
			}
		}
		
		// Approximate contour
		List<List<Vector2>> contourPoints = new List<List<Vector2>> ();
		
		foreach(Contour<Point> contour in contours)
		{
			Point[] cpts = contour.ToArray();
			
			if(cpts.Length > 1 && contour.Perimeter > minPerimeter)
			{
				List<Vector2> cnt = new List<Vector2>();
				
				for(int i = 0; i < cpts.Length; i++)
				{
					Point cp0  = cpts[i];
					cnt.Add( new Vector2(cp0.X / (float)dw, cp0.Y / (float)dh));
					//cnt.Add(silhouette.unmap(new Vector2(cp0.X / (float)dw, cp0.Y / (float)dh)));
				}
				
				contourPoints.Add(cnt);
			}
		}
		
		return contourPoints;
	}
	
	private void AddContourToList(ref List<Contour<Point>> contours, Contour<Point> c, bool isoutside) 
	{
		if (c == null)
			return;
		
		if(type == TYPE.ALL || (type == TYPE.EXTERNAL_ONLY && isoutside) || (type == TYPE.INTERNAL_ONLY && !isoutside))
		{
			if (accuracy > 0f)
				contours.Add (c.ApproxPoly (c.Perimeter * SilhouetteGUI.accuracy));
			else 				
				contours.Add (c);
		}
		
		AddContourToList (ref contours, c.HNext, true);
		AddContourToList (ref contours, c.VNext, false);
	}
}
