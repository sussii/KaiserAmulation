using UnityEngine;
using System.Collections;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;

public class FilterGrayImage : MonoBehaviour 
{
	public int blurIterations = 0;
	public int postBlurIterations = 0;
	public int erode = 0;
	public int dilate = 0;
	public int erodeDilateIterations = 0;
	public float binaryThreshold = 0f;
	public bool thresholdImage = false;
	public float resize = 1f;

	public Image<Gray, byte> Do(Image<Gray, byte> input) 
	{
		if (input == null) return null;
		
		Image<Gray, byte> img = input.Copy();
		
		if (thresholdImage) img._ThresholdBinary(new Gray(binaryThreshold), new Gray(255f));
		if (blurIterations > 0) img._SmoothGaussian(blurIterations);
		
		for (int i = 0; i < erodeDilateIterations; i++) 
		{
			if (erode > 0) img._Erode(erode);
			if (dilate > 0) img._Dilate(dilate);
		}
		
		if (thresholdImage) img._ThresholdBinary(new Gray(binaryThreshold), new Gray(255f));
		if (postBlurIterations > 0) img._SmoothGaussian(postBlurIterations);
		if (resize != 1f) img = img.Resize(resize, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
		
		return img;
	}
}
