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

public class TrackingShape{
	Image<Gray, byte> liveImgCopy = null;
	Image<Gray, byte> smoothedImg = null;
	// Use this for initialization
	void Start () {
	
	}
	void Update () {
	
	}

	public Image<Gray,byte> ObjectTrackingSurf(Image<Gray, byte> liveImg, Image<Gray,byte> templateImg, bool showOnLiveImg)
	{
		smoothedImg = liveImgCopy.PyrDown ().PyrUp ();
		smoothedImg._SmoothGaussian (3);
		return null;
	}
}
