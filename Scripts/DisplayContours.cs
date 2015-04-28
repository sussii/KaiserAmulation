using UnityEngine;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;

/* Attach this script to Main Camera
 * This script is used to show contours on an texture (GameObject display) and show on Gui.
 * */
public class DisplayContours : MonoBehaviour {

	public InfraredSourceManager im;
	public CalculateContours CC;
	public FilterGrayImage FGI;
	public GameObject display;
	public DepthToGray d2g;
	
	Image<Gray, byte> infraredImage;
	List<List<Vector2>> contours;
	
	// Display filter step
	Byte[,,] grayData;
	Texture2D texture;
	Color32[] pixels;
	
	// Update is called once per frame
	void Update () 
	{
		if (infraredImage == null) 
		{
			infraredImage = new Image<Gray, byte>(im.GetInfraredTexture().width, im.GetInfraredTexture().height);
			//Debug.Log("Initialized live image of width: " + infraredImage.Width + " height: " + infraredImage.Height);
		}
		
		infraredImage.Bytes = im.GetDataByte();
		infraredImage._Flip(Emgu.CV.CvEnum.FLIP.VERTICAL);
		
		Image<Gray, byte> result = FGI.Do(infraredImage);
		contours = CC.Do(result);
		
		//DisplayImage(d2g.Get(), display);
		GetContourPos(0);
		GetContourPos(1);
	}
	
	void OnPostRender () 
	{
		if(contours == null) return;
		//Debug.Log("Contour count: " + contours.Count);
		foreach(List<Vector2> contour in contours) 
		{
			for(int i = 0; i < contour.Count; i++) 
			{
				Vector2 i1 = contour[i];
				Vector2 i2 = contour[(i+1) % contour.Count];
				
				//GUIHelper.DrawLine(i1, i2, UnityEngine.Color.blue);
			}
		}
	}
	
	public void DisplayImage(Image<Gray,Byte> image, GameObject obj)
	{
		if(image == null) return;
		
		grayData = image.Data;
		int w = image.Width;
		int h = image.Height;
		
		if(texture == null || texture.width != w) 
		{
			texture = new Texture2D (w, h, TextureFormat.ARGB32, false);
			pixels = new UnityEngine.Color32[w * h];
			obj.renderer.material.mainTexture = texture;
		}
		
		for(int y = 0, i = 0; y < h; y++)
		{
			for(int x = 0; x < w; x++, i++)
			{
				Byte g = grayData[y, x, 0];
				
				pixels[i].r = g;
				pixels[i].g = g;
				pixels[i].b = g;
				pixels[i].a = g;
			}
		}

		((Texture2D)texture).SetPixels32(pixels);
		((Texture2D)texture).Apply(false);
		obj.renderer.material.mainTexture = texture;
	}
	
	public Vector2 GetContourPos(int idx) // the position of which contour
	{
		if(contours == null) return new Vector2(-1,-1);
		if (idx >= contours.Count) return new Vector2(-1,-1);	
		float sumX = 0;
		float sumY = 0;
		Vector2 center;
		foreach(Vector2 c in contours[idx])
		{
			sumX+=c.x;
			sumY+=c.y;
		}
		int totalContourPoints = contours[idx].Count;
		center = new Vector2(sumX/totalContourPoints, sumY/totalContourPoints);
		//Debug.Log("contour id: " + idx + " center: " + center);
		return center;	
	}

    public int GetContourNum()
    {
        return contours.Count;
    }


}
