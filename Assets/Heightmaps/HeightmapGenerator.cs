//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using UnityEngine;
using System.Drawing;
public class HeightmapGenerator
{

	protected Texture2D heightMap;
	protected double minX, minZ, maxX, maxZ;

	protected double lowerBound, upperBound;
	protected double maxRadius;
	protected String filename;
	private double bWidth, bHeight;

	public static int HILLS = 0; 
	public static int PEAKS = 1;

	// Variables Holder
	Variables Vars;

	/// <summary>
	/// Initializes a new instance of the <see cref="HeightmapGenerator"/> class.
	/// </summary>
	/// <param name="Vars">Variables instance.</param>
	/// <param name="filename">Data source filename.</param>
	/// <param name="maxRadius">Max radius.</param>
	/// <param name="minX">Minimum X coord.</param>
	/// <param name="minZ">Minimum Z coord.</param>
	/// <param name="maxX">Max X coord.</param>
	/// <param name="maxZ">Max Z coord.</param>
	public HeightmapGenerator (Variables Vars, String filename, double maxRadius,
	                           double minX, double minZ, double maxX, double maxZ)
	{
		this.Vars = Vars;
		this.filename = filename;
		this.maxRadius = maxRadius;
		this.minX = minX;
		this.minZ = minZ;
		this.maxX = maxX;
		this.maxZ = maxZ;
//		this.minX = minZ;
//		this.minZ = maxX;
//		this.maxX = maxZ;
//		this.maxZ = minX;
	}

	/// <summary>
	/// Gets the heightmap Texture2D of the data.
	/// </summary>
	/// <returns>The heightmap texture.</returns>
	/// <param name="bWidth">Bitmap width.</param>
	/// <param name="bHeight">Bitmap height.</param>
	/// <param name="sexData">If set to <c>true</c>, 
	/// data is sex data (used for absolute value of data).</param>
	public virtual Texture2D GetHeightmap(double bWidth, double bHeight, bool sexData){
		DataHandler dataHandler = new DataHandler(filename, Vars.COLUMN_X, Vars.COLUMN_Y, Vars.COLUMN_Z, minX, minZ, maxX, maxZ);
		
		this.bWidth = bWidth;
		this.bHeight = bHeight;
		heightMap = new Texture2D((int)bWidth, (int)bHeight);
		FillInColor(UnityEngine.Color.black);

		double[] bnds= dataHandler.GetBounds();
		lowerBound = sexData ? 0f : 0f;
		upperBound = bnds[1];
		 
		CreateHeightMap(dataHandler.GetData(), sexData);
		SaveToDisk();
		return heightMap;
	}

	/// <summary>
	/// Gets the heightmap Texture2D generated with 
	/// cylinders for data points.
	/// </summary>
	/// <returns>The heightmap Texture2D.</returns>
	/// <param name="bWidth">Bitmap width.</param>
	/// <param name="bHeight">Bitmap height.</param>
	/// <param name="sexData">If set to <c>true</c>, 
	/// data is sex data (used for absolute value of data).</param>
	public virtual Texture2D GetHeightmapCylinder(double bWidth, double bHeight, bool sexData){
		DataHandler dataHandler = new DataHandler(filename, Vars.COLUMN_X, Vars.COLUMN_Y, Vars.COLUMN_Z, minX, minZ, maxX, maxZ);
		
		this.bWidth = bWidth;
		this.bHeight = bHeight;
		heightMap = new Texture2D((int)bWidth, (int)bHeight);
		FillInColor(UnityEngine.Color.black);
		
		double[] bnds= dataHandler.GetBounds();
		lowerBound = sexData ? 0f : 0f;
		upperBound = bnds[1];
		
		CreateHeightMapCyl(dataHandler.GetData(), sexData);
		SaveToDisk();
		return heightMap;
	}

	/// <summary>
	/// Fills the entire heightmap texture with a given color.
	/// </summary>
	/// <param name="c">Color with which to fill the texture..</param>
	protected void FillInColor(UnityEngine.Color c){
		for(int i = 0; i < heightMap.width; i++) {
			for(int j = 0; j < heightMap.height; j++) {
				heightMap.SetPixel(i,j, c);
			}
		}
	}  

	/// <summary>
	/// Saves texture to disk memory.
	/// </summary>
	public virtual void SaveToDisk() {
		byte[] bytes = heightMap.EncodeToPNG();
		File.WriteAllBytes(Application.dataPath + "/Heightmaps/Images/heightmap_" + Vars.TERRAIN_NAME + ".png", bytes);
	}

	/// <summary>
	/// Creates the height map. 
	/// </summary>
	/// <param name="data">Data with each object containing the following structure:    
	/// [ peak_height, peak_X, peak_Y ]
	/// </param> 
	/// <param name="sexData">If the data should be taken as the absolute
	/// value.</param>
	protected void CreateHeightMap(List<double[]> data, bool sexData){
		double it = (upperBound-lowerBound)/Vars.ITERATION_NUMBER;
		if(it == 0)
			it = 1f;
		double h = lowerBound;
		while(data.Count != 0){
			h += it;
			List<int> toRemove = new List<int>();
			for(int i=0; i<data.Count; i++){
				if(data[i][1] < h){ //If middle of the values is less than the lower bounds, then add it "toRemove" List
					toRemove.Add(i);
				}
				double height = sexData ? Math.Abs(data[i][1]) : data[i][1];
				DrawHillRadius(height, data[i][0] * bWidth, data[i][2] * bHeight, h, maxRadius * bWidth, HILLS);
			}
			for(int i=0; i<toRemove.Count; i++){
				data.RemoveAt(toRemove[i]-i);
			}
		} 
		heightMap.Apply();
	}

	/// <summary>
	/// Creates the height map with cylinders representing data points.
	/// </summary>
	/// <param name="data">Data with each object containing the following structure:    
	/// [ peak_height, peak_X, peak_Y ]
	/// </param> 
	/// <param name="sexData">If the data should be taken as the absolute
	/// value.</param>
	protected void CreateHeightMapCyl(List<double[]> data, bool sexData){
		double it = (upperBound-lowerBound)/Vars.ITERATION_NUMBER;
		if(it == 0){
			it = 1f;
		}
		System.Drawing.Bitmap bit = new System.Drawing.Bitmap(heightMap.width, heightMap.height);
		System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bit);
		double h = lowerBound;
		while(data.Count != 0){
			h += it;
			List<int> toRemove = new List<int>();
			for(int i=0; i<data.Count; i++){
				if(data[i][1] < h){ //If middle of the values is less than the lower bounds, then add it "toRemove" List
					toRemove.Add(i);
					double height = sexData ? Math.Abs(data[i][1]) : data[i][1];
					System.Drawing.Color col = ColourConvert(height);
					Brush brush = new SolidBrush(col);
					Rectangle rect = new Rectangle((int)(data[i][0] * bWidth) - 5, (int)(data[i][2] * bHeight) - 5, 10 , 10);
					g.FillEllipse(brush, rect);
				}
			}
			for(int i=0; i<toRemove.Count; i++){
				data.RemoveAt(toRemove[i]-i);
			}
		}
		bit.RotateFlip(RotateFlipType.Rotate180FlipX);
		ImageConverter imgconv = new ImageConverter();
		byte[] hmapbytes = (byte[])imgconv.ConvertTo(bit, typeof(byte[]));
		heightMap.LoadImage(hmapbytes);
		heightMap.Apply();
	}

	/// <summary>
	/// Performs the same function as <see cref="ColorFromHeight"/> 
	/// however returns a System.Drawing.Color instead of a Unity
	/// Color object.
	/// </summary>
	/// <returns>The color associated with the height from 
	/// the designated ColorSpectrumObj.</returns>
	/// <param name="val">Height value.</param>
	public System.Drawing.Color ColourConvert(double val)
	{
		UnityEngine.Color unityC = ColorFromHeight(val, upperBound, lowerBound);
		UnityEngine.Color32 unicolour = new UnityEngine.Color(unityC.r, unityC.g, unityC.b, unityC.a);
		System.Drawing.Color wincolour = new System.Drawing.Color();
		wincolour = System.Drawing.Color.FromArgb(unicolour.a, unicolour.r, unicolour.g, unicolour.b);
		return wincolour;
	}

	/// <summary>
	/// Draws a circle on the bitmap with given x and y coords
	/// as well as height to be input to <see cref="ColorFromHeight"/>
	/// and compared to maxRadius using <see cref="RadiusFunc"/>
	/// </summary>
	/// <param name="peak">Peak.</param>
	/// <param name="peakX">Peak x coord.</param>
	/// <param name="peakY">Peak y coord.</param>
	/// <param name="height">Height.</param>
	/// <param name="maxRadius">Max radius.</param>
	/// <param name="HillsOrPeaks">Hills or peaks REDUNDANT.</param>
	public void DrawHillRadius(double peak, double peakX, double peakY, double height, double maxRadius, int HillsOrPeaks)
	{
		Circle(ref heightMap, (int)peakX, (int)peakY, (int)RadiusFunc(peak, height, maxRadius), ColorFromHeight(height, upperBound, lowerBound));
	}

	/// <summary>
	/// Returns a 32 bit color given a height within the range
	/// of the max and min possible height values.
	/// </summary>
	/// <returns>The color associated with the height.</returns>
	/// <param name="height">Height.</param>
	/// <param name="maxHeight">Max height.</param>
	/// <param name="minHeight">Minimum height.</param>
	protected virtual UnityEngine.Color ColorFromHeight(double height, double maxHeight, double minHeight) 
	{ 
		UnityEngine.Color color = new UnityEngine.Color(0,0,0,1);
		double scalar = (height-minHeight) / (maxHeight-minHeight);
		Int32 mColor = (Int32)Math.Floor(scalar * Math.Pow(2, 24));
		Int32 b = mColor%256;
		Int32 g = mColor%(256*256);
		g /= 256;
		Int32 r = mColor;
		r /= 256*256;
		color.r = (float)((float)r/256f);
		color.g = (float)((float)g/256f);
		color.b = (float)((float)b/256f);
		return color;
	}

	/// <summary>
	/// Returns the radius of the hill at the given height
	/// given the peak height of the hill (the Y value of the
	/// data point)
	/// </summary>
	/// <returns>The the radius of the hill at height given
	/// max height of data point is peak.</returns>
	/// <param name="peak">Peak height of data point.</param>
	/// <param name="height">Height at which to find radius.</param>
	/// <param name="maxRadius">Max radius.</param>
	private double RadiusFunc(double peak, double height, double maxRadius)
	{
		double cosVal = height/peak;
		double z = 2 * Math.Acos((2*cosVal) - 1);
		double radius = (z * maxRadius) / (Math.PI * 2);
		return radius;
	}

	/// <summary>
	/// Draws a circle on the specified Texture2D
	/// </summary>
	/// <param name="tex">Texture on which to draw</param>
	/// <param name="cx">Centre of circle X coord.</param>
	/// <param name="cy">Centre of circle Y coord.</param>
	/// <param name="r">The radius of the circle.</param>
	/// <param name="col">Color of circle to draw.</param>
	public void Circle(ref Texture2D tex, int cx, int cy, int r, UnityEngine.Color col)
	{
		int x, y, px, nx, py, ny, d;
		
		for (x = 0; x <= r; x++)
		{
			d = (int)Mathf.Ceil(Mathf.Sqrt(r * r - x * x));
			for (y = 0; y <= d; y++)
			{
				px = cx + x;
				nx = cx - x;
				py = cy + y;
				ny = cy - y;
				
				if(px >= 0 && py >= 0 && px < tex.width && py < tex.height)
					tex.SetPixel(px, py, col);
				if(nx >= 0 && py >= 0 && nx < tex.width && py < tex.height)
					tex.SetPixel(nx, py, col);

				if(px >= 0 && ny >= 0 && px < tex.width && ny < tex.height)
					tex.SetPixel(px, ny, col);
				if(nx >= 0 && ny >= 0 && nx < tex.width && ny < tex.height)
					tex.SetPixel(nx, ny, col);
				
			}
		}
	}
}