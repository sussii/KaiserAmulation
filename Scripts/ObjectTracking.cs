using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

using Windows.Kinect;

public class ObjectTracking : MonoBehaviour {

	public TrackingSURF surf;
	public ColorSourceManager cm;
	public InfraredSourceManager im;
	public DepthToGray d2g;
	public Texture2D templateImg2D;

	public float depthScale = 0.00328084f * 6f;
	//to display images for testing;
	public bool isTracking = true;

	public GameObject liveDisplay;
	public GameObject templateDisplay;

	public Texture2D liveImg2D;
	Image <Gray,Byte> templateImg = null;
	Image <Gray,Byte> liveImg = null;

	//Convert Texture2D to Image <Gray,Byte>

	Texture2D templateTex = null;
	Color32 [] templateImageColors = null;
	Texture2D liveTex = null;
	Color32 [] liveImageColors = null;

	byte[,,] grayData = null;
	byte[, ,] imgData = null;

	// Color Image from Kinect
	public bool displayColorImage = true;
	Image <Bgra, Byte> colorImgRaw = null;
	Image <Bgra, Byte> colorImg = null;
	int colorWidth;
	int colorHeight;


	public float colorImageScale = 1f;

	//optimize object detection
	short oldDepth = 0;
	float oldX = 0;
	float oldY = 0;
	//test
	public GameObject testBall;


	// Use this for initialization
	void Start () {
		surf = new TrackingSURF ();
		string templateImgPath = Application.dataPath + "/tuio0.jpeg"; //+ UnityEditor.AssetDatabase.GetAssetPath(templateImg2D);
		templateImg = new Image <Gray, byte> (templateImgPath);
		//string liveImgPath =  Application.dataPath + "/../" + UnityEditor.AssetDatabase.GetAssetPath(liveImg2D);
		//liveImg = new Image<Gray, byte> (liveImgPath);

		//template image
		templateTex = new Texture2D (templateImg.Width, templateImg.Height, TextureFormat.ARGB32, false);
		templateImageColors = new UnityEngine.Color32[templateImg.Width * templateImg.Height];

		/*liveTex = new Texture2D (liveImg.Width, liveImg.Height, TextureFormat.ARGB32, false);
		liveImageColors = new UnityEngine.Color32[liveImg.Width * liveImg.Height];	
		*/
		//Debug.Log("start():    w: " + liveImg.Width + "h: " + liveImg.Height );
		 //live image

		/*liveImg2D = cm.GetColorTexture ();
		colorImg = new Image<Bgr, byte> (liveImg2D.width, liveImg2D.height);
		imgData = new byte [liveImg2D.width, liveImg2D.height, 3];
		UnityTextureToOpenCVImage(liveImg2D.GetPixels32(), liveImg2D.width, liveImg2D.height);
*/

		//liveImg = colorImg.Convert<Gray,byte> ();
		//liveTex = new Texture2D (liveImg.Width, liveImg.Height, TextureFormat.ARGB32, false);
		//liveImageColors = new UnityEngine.Color32[liveImg.Width * liveImg.Height];	
		//liveTex = new Texture2D (cm.ColorWidth, cm.ColorHeight, TextureFormat.ARGB32, false);
		//liveImageColors = new UnityEngine.Color32[cm.ColorWidth * cm.ColorHeight];	
		//Display live image
		/*if (liveImg != null) {
			this.DisplayImage (liveImg, liveDisplay);
		}*/
	}


	//Infared Update
	/*void Update () {
		if(isTracking)
		{
			// Initialize color image if not initialized yet
			if (liveImg == null) 
			{
				colorWidth = im.GetInfraredTexture().width;
				colorHeight = im.GetInfraredTexture().height;
				liveImg = new Image<Gray, byte>(colorWidth, colorHeight);

				Debug.Log("Initialized live image of width: " + colorWidth + " height: " + colorHeight);
			}
			liveImg.Bytes = im.GetDataByte();
			liveImg._Flip(FLIP.VERTICAL);

			liveImg = surf.ObjectTrackingSurf(liveImg,templateImg,true);
			Debug.Log(surf.GetObjectCenter());
			
			float x = surf.GetObjectCenter().X;
			float y = surf.GetObjectCenter().Y;
			
			if(x <= 0 || y <= 0) {
				x = oldX;
				y = oldY;
			}
			oldX = x;
			oldY = y;
			
			float vpCenterX = x/(float)liveImg.Width;
			float vpCenterY = y/(float)liveImg.Height;
			Vector2 vpCenter = new Vector2 (vpCenterX,vpCenterY);
			

			Vector3 worldPos = Camera.main.ViewportToWorldPoint(new Vector3(1-vpCenter.x,0.5f, 20 )); //depth* 0.00328084f*6f
			testBall.transform.position = worldPos;

			//Kinect Example Scene
			if(displayColorImage)
				this.DisplayImage (liveImg, liveDisplay);
		}
	}*/

	// Update is called once per frame
	void Update () {

		//if (Input.GetKeyDown (KeyCode.M)) 
		if(isTracking)
		{
			// Initialize color image if not initialized yet
			if (colorImgRaw == null) 
			{
				colorWidth = cm.GetColorTexture().width;
				colorHeight = cm.GetColorTexture().height;
				colorImgRaw = new Image<Bgra, byte>(colorWidth, colorHeight);

				Debug.Log("Initialized color image of width: " + colorWidth + " height: " + colorHeight);
			}
			// Copy color data into color image
			colorImgRaw.Bytes = cm.Data();

			colorImgRaw._Flip(FLIP.VERTICAL);
			//colorImgRaw._Flip(FLIP.HORIZONTAL);

			if (colorImageScale != 1f)
				colorImg = colorImgRaw.Resize(colorImageScale, INTER.CV_INTER_NN);
			else
				colorImg = colorImgRaw;

			liveImg = colorImgRaw.Convert<Gray, byte>();
		    liveImg = surf.ObjectTrackingSurf(liveImg,templateImg,true);
			//Debug.Log("w: " + liveImg.Width + "  h: " + liveImg.Height);
			//Debug.Log(surf.GetObjectCenter());

			float x = surf.GetObjectCenter().X;
			float y = surf.GetObjectCenter().Y;

			if(x <= 0 || y <= 0) {
				x = oldX;
				y = oldY;
			}
			oldX = x;
			oldY = y;

			float vpCenterX = x/(float)liveImg.Width;
			float vpCenterY = y/(float)liveImg.Height;
			Vector2 vpCenter = new Vector2 (vpCenterX,vpCenterY);

			short depth = d2g.GetPixelDepth(vpCenter);
			if(depth == 0)
				depth = oldDepth;
			if(depth !=0 && oldDepth != 0 && Mathf.Abs(depth-oldDepth) > 100)
			{
				//Debug.Log("real depth = " + depth);
				depth = oldDepth;
			}
			oldDepth = depth;

			//Debug.Log ("depth: "+depth + "   vpCetner:" + vpCenter + "  x: " + x + " y: " + y);

			Vector3 worldPos = Camera.main.ViewportToWorldPoint(new Vector3(1-vpCenter.x,0.5f, depthScale * depth )); //depth* 0.00328084f*6f
			testBall.transform.position = worldPos;
		}
	}
	
/*	void DisplayImage(Image<Gray,Byte> image, GameObject obj)
	{
		if(image != null)
		{
			//image = image.Resize(0.3f , Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
			grayData = image.Data;
			int w = image.Width;
			int h = image.Height;

			if(liveTex == null || liveTex.width != w) 
			{
				liveTex = new Texture2D (w, h, TextureFormat.ARGB32, false);
				liveImageColors = new UnityEngine.Color32[w * h];
				obj.renderer.material.mainTexture = liveTex;
			}
				

			for(int y = 0, i = 0; y < h; y++)
			{
				for(int x = 0; x < w; x++, i++)
				{
					Byte g = grayData[y, x, 0];
					//float gv = (float)g/255f;
					if(obj.name == "LiveDisplay")
					{
						liveImageColors[i].r = g;// = new UnityEngine.Color(gv, gv, gv, gv);
						liveImageColors[i].g = g;
						liveImageColors[i].b = g;
						liveImageColors[i].a = g;
					}
					else
					{
						templateImageColors[i].r = g;// = new UnityEngine.Color(gv, gv, gv, gv);
						templateImageColors[i].g = g;
						templateImageColors[i].b = g;
						templateImageColors[i].a = g;
					}
				}
			}
			if(obj.name == "LiveDisplay")
			{
				((Texture2D)liveTex).SetPixels32(liveImageColors);
				((Texture2D)liveTex).Apply(false);
				//obj.renderer.material.mainTexture = liveTex;
			}
			else
			{
				((Texture2D)templateTex).SetPixels32(templateImageColors);
				((Texture2D)templateTex).Apply(false);
				//obj.renderer.material.mainTexture = templateTex;
			}
		}

	}*/

	public void UnityTextureToOpenCVImage(Color32[] data, int width, int height){
		int index = 0;
		//imgData = colorImg.Data;

		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				imgData[x,y,0] = data[index].b;
				imgData[x,y,1] = data[index].g;
				imgData[x,y,2] = data[index].r;
				index++;
			}
		}
		colorImg.Data = imgData;
		//image.Data = imageData; 
		//return new Image<Bgr, byte>(width, height);
	}


	public Bitmap Bytes2Bitmap(Texture2D tex)
	{
		byte[] data = tex.EncodeToPNG ();
		MemoryStream ms = new MemoryStream(data);
		ms.Seek (0, SeekOrigin.Begin);
		Bitmap bmpImage = new Bitmap(ms);
		ms.Close ();
		ms = null;
		return bmpImage;
	}


	public Bitmap MakeGrayscale3(Bitmap original) // 62ms
	{
		//create a blank bitmap the same size as original
		Bitmap newBitmap = new Bitmap(original.Width, original.Height);
		//get a graphics object from the new image
		System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newBitmap);
		
		//create the grayscale ColorMatrix
		ColorMatrix colorMatrix = new ColorMatrix(
			new float[][] 
			{
			new float[] {.3f, .3f, .3f, 0, 0},
			new float[] {.59f, .59f, .59f, 0, 0},
			new float[] {.11f, .11f, .11f, 0, 0},
			new float[] {0, 0, 0, 1, 0},
			new float[] {0, 0, 0, 0, 1}
		});
		
		//create some image attributes
		ImageAttributes attributes = new ImageAttributes();
		
		//set the color matrix attribute
		attributes.SetColorMatrix(colorMatrix);
		
		//draw the original image on the new image
		//using the grayscale color matrix
		g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
		            0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
		
		//dispose the Graphics object
		g.Dispose();
		return newBitmap;

	}


	public Vector2 GetObjectHorizonalPosition()
	{
		return new Vector2 (testBall.transform.position.x, testBall.transform.position.z);
	}
	public void SetTracking(bool b)
	{
		isTracking = b;
	}
	
	
}
