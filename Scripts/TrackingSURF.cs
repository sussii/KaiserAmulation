using UnityEngine;
using System.Collections;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.Features2D;

using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

public class TrackingSURF {

	Image<Gray, byte> liveImgCopy;
	Image<Gray, byte> templateImgCopy;

	//SURF
	SURFDetector surfDetector = new SURFDetector(500, false);
	
	VectorOfKeyPoint vkpLiveKeyPoint;
	VectorOfKeyPoint vkpTemplateKeyPoint;
	
	Matrix<Single> mtxLiveDescriptors;
	Matrix<Single> mtxTemplateDescriptors;
	Matrix<int> mtxMatchIndices;
	Matrix<Single> mtxDistance;
	Matrix<Byte> mtxMask;
	BruteForceMatcher<Single> bruteForceMatcher;
	HomographyMatrix homographyMatrix;
	
	int KNumNearestNeighbors = 2;
	float UniquenessThreshold= 0.8f;
	int NumNonZeroElements;
	float ScaleIncrement = 1.5f;
	int RotationBins = 20;
	float RansacReprojectionThreshold = 2.0f;

	//Display
	Bgr pointColor = new Bgr(System.Drawing.Color.Red);
	Bgr boxColor = new Bgr(System.Drawing.Color.Blue);
	Gray boxGray = new Gray (0);
	Gray pointGray = new Gray (0);
	Rectangle rect;
	PointF[] pointsF = new PointF[4];
	Point[] points  = new Point[4];

	PointF centerPointF = new PointF();
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public Image<Gray,byte> ObjectTrackingSurf(Image<Gray, byte> liveImg, Image<Gray,byte> templateImg, bool showOnLiveImg)
	{
		vkpLiveKeyPoint = surfDetector.DetectKeyPointsRaw (liveImg, null);
		mtxLiveDescriptors = surfDetector.ComputeDescriptorsRaw (liveImg, null,vkpLiveKeyPoint);
		
		vkpTemplateKeyPoint = surfDetector.DetectKeyPointsRaw (templateImg, null);
		mtxTemplateDescriptors = surfDetector.ComputeDescriptorsRaw (templateImg, null,vkpTemplateKeyPoint);
		
		bruteForceMatcher = new BruteForceMatcher<Single> (DistanceType.L2);
		bruteForceMatcher.Add (mtxTemplateDescriptors);
		
		mtxMatchIndices = new Matrix<int> (mtxLiveDescriptors.Rows, KNumNearestNeighbors);
		mtxDistance = new Matrix<Single> (mtxLiveDescriptors.Rows, KNumNearestNeighbors);
		
		bruteForceMatcher.KnnMatch (mtxLiveDescriptors, mtxMatchIndices, mtxDistance, KNumNearestNeighbors, null);
		
		mtxMask = new Matrix<Byte> (mtxDistance.Rows, 1);
		mtxMask.SetValue (255);
		Features2DToolbox.VoteForUniqueness(mtxDistance,UniquenessThreshold, mtxMask);
		
		NumNonZeroElements = CvInvoke.cvCountNonZero(mtxMask);
		if (NumNonZeroElements >= 4) {
			NumNonZeroElements = Features2DToolbox.VoteForSizeAndOrientation (vkpTemplateKeyPoint,
	                                                                  vkpLiveKeyPoint,
	                                                                  mtxMatchIndices,
	                                                                  mtxMask,
	                                                                  ScaleIncrement,
	                                                                  RotationBins);
			if (NumNonZeroElements >= 4)
				homographyMatrix = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures (vkpTemplateKeyPoint,
		                                                                             vkpLiveKeyPoint,
		                                                                             mtxMatchIndices,
		                                                                             mtxMask,
		                                                                             RansacReprojectionThreshold);
		}
		//templateImgCopy = templateImg.Copy ();
		//templateImgCopy.Draw (new Rectangle (1, 1, templateImgCopy.Width - 3, templateImgCopy.Height - 3), boxGray, 2);
		liveImgCopy = liveImg.Copy ();//.ConcateHorizontal(templateImgCopy);

		if (homographyMatrix != null) {
			rect.X = 0;										
			rect.Y = 0;				
			rect.Width = templateImg.Width;
			rect.Height = templateImg.Height;	
			pointsF[0].X = rect.Left; pointsF[0].Y = rect.Top;
			pointsF[1].X = rect.Right; pointsF[1].Y = rect.Top; 
			pointsF[2].X = rect.Right; pointsF[2].Y = rect.Bottom; 
			pointsF[3].X = rect.Left; pointsF[3].Y = rect.Bottom;

			homographyMatrix.ProjectPoints(pointsF);	
			//Debug.Log("live w: "+ liveImgCopy.Width + "live h: " + liveImgCopy.Height);
			//Debug.Log ("pf0: " + pointsF[0] + "pf1: "+ pointsF[1] + " pf2: " + pointsF[2] + " pf3: " + pointsF[3]);

			centerPointF.X = 0;
			centerPointF.Y = 0;
			for(int i = 0; i < pointsF.Length; ++i )
			{
				centerPointF.X += pointsF[i].X;
				centerPointF.Y += pointsF[i].Y;
			}
			centerPointF.X = centerPointF.X/4f;
			centerPointF.Y = centerPointF.Y/4f;
			//Debug.Log("centerF: " + centerPointF);
			points[0] = Point.Round(pointsF[0]);
			points[1] = Point.Round(pointsF[1]);
			points[2] = Point.Round(pointsF[2]);
			points[3] = Point.Round(pointsF[3]);

			liveImgCopy.DrawPolyline(points,true,boxGray,4);

		}
		if (showOnLiveImg)
			return liveImgCopy;
		else
			return templateImgCopy;
	}


	public PointF GetObjectCenter()
	{
		return centerPointF;
	}
}
