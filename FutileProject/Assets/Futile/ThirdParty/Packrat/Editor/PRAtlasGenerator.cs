using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

//consider using https://github.com/kwnetzwelt/mono_scaling/tree/master/unity_example_project/Assets/scripts/Scaling

public class PRAtlasGenerator
{
	public PRAtlasLink link;
	public List<PRAtlasElement> elements;
	public int atlasWidth;
	public int atlasHeight;
	public Texture2D atlasTexture;
	public IEnumerator<string> generateEnumerator;
	public string progressMessage = "";
	public string pngSuffix;

	public int atlasScaleIndex = -1;

	public PRAtlasGenerator(PRAtlasLink link)
	{
		this.link = link;

		string atlasName = Path.GetFileNameWithoutExtension(link.atlasFilePath);

		atlasScaleIndex = GetScaleIndexFromName(atlasName);

		generateEnumerator = Generate();
	}

	public int GetScaleIndexFromName(string fileName)
	{
		int scaleIndex = -1;
		int stringIndex = fileName.IndexOf("_Scale");
		
		if(stringIndex != -1)
		{
			if(!int.TryParse(fileName.Substring(stringIndex+6), out scaleIndex))
			{
				scaleIndex = -1;
			}
		}
		return scaleIndex;
	}

	public bool Advance()
	{
		bool didMove = generateEnumerator.MoveNext();

		if(didMove)
		{
			progressMessage = generateEnumerator.Current;
		}

		return didMove;
	}
	 
	public IEnumerator<string> Generate()
	{
		if(link.shouldUseBytes)
		{
			pngSuffix = "_png.bytes";
		}
		else 
		{
			pngSuffix = ".png";
		}

		yield return "Creating Elements";
		CreateElementsFromImages();

		yield return "Trimming Elements";
		TrimElements();

		yield return "Packing Elements";
		CalculateElementPacking();


		IEnumerator<string> enumerator = CreateAtlasTexture();

		while(enumerator.MoveNext())
		{
			yield return enumerator.Current;
		}

		yield return "Generating Data File";
		CreateAtlasDataFile();

		yield return "Cleaning Up";
		CleanUp();
		
		Debug.Log ("Packrat: Packed "+Path.GetFileNameWithoutExtension(link.atlasFilePath)+" into a " + atlasWidth + "x"+atlasHeight +" atlas");
	}

	private void CreateElementsFromImages ()
	{
		SearchOption searchOption = link.shouldAddSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

		string[] filePaths = Directory.GetFiles(link.sourceFolderPath,"*.png",searchOption);
		
		elements = new List<PRAtlasElement>(filePaths.Length);
		
		for(int s = 0; s<filePaths.Length; s++)
		{
			bool shouldAddElement = true;

			string filePath = filePaths[s];

			string fileName = filePath.Remove(0,link.sourceFolderPath.Length+1);
			fileName = fileName.Remove(fileName.Length-4,4); //remove extension
			fileName = fileName.Replace("\\","/"); //replace slashes

			PRAtlasElement element = new PRAtlasElement(this, fileName);
			element.filePath = filePath;

			Texture2D texture = new Texture2D(0,0,TextureFormat.ARGB32,false,false);
			texture.wrapMode = TextureWrapMode.Clamp; //so we don't get pixels from the other edge when scaling
			texture.filterMode = FilterMode.Trilinear;
			texture.LoadImage(File.ReadAllBytes(filePath));

			if(link.shouldTrim)
			{
				if(element.name.EndsWith("_notrim"))
				{
					element.shouldTrim = false;
				}
				else 
				{
					element.shouldTrim = true;
				}
			}
			else
			{
				if(element.name.EndsWith("_trim"))
				{
					element.shouldTrim = true;
				}
				else 
				{
					element.shouldTrim = false;
				}
			}

			if(atlasScaleIndex != -1)
			{
				int elementScaleIndex = GetScaleIndexFromName(element.name);

				if(elementScaleIndex != -1) //the element has a scale index
				{
					if(elementScaleIndex == atlasScaleIndex) //don't scale an element if it has the same suffix
					{
						element.shouldScale = false;
					}
					else 
					{
						shouldAddElement = false; //this element is for a different scale so don't add it
					}
				}
			}

			element.padding = link.padding;
			element.extrude = link.extrude;

			element.texture = texture;

			element.sourceFullWidth = texture.width;
			element.sourceFullHeight = texture.height;

			if(element.shouldScale)
			{
				element.scaledFullWidth = Mathf.CeilToInt((float)element.sourceFullWidth * link.scale);
				element.scaledFullHeight = Mathf.CeilToInt((float)element.sourceFullHeight * link.scale);
			}
			else 
			{
				element.scaledFullWidth = element.sourceFullWidth;
				element.scaledFullHeight = element.sourceFullHeight;
			}

			if(element.texture == null)
			{
				shouldAddElement = false;
			}

			if(shouldAddElement)
			{
				elements.Add(element);
			}
			else 
			{
				if(element.texture != null)
				{
					UnityEngine.Object.DestroyImmediate(element.texture,true);
					element.texture = null;
				}
			}
		}
	}

	private void TrimElements()
	{
		int trimPadding = link.trimPadding;
		int elementCount = elements.Count;

		for(int e = 0; e<elementCount; e++)
		{
			PRAtlasElement element = elements[e];

			if(element.shouldTrim)
			{
				Color[] sourcePixels = element.texture.GetPixels(0);

				int cols = element.sourceFullWidth;
				//int rows = element.sourceFullHeight;

				int left = 0;
				int right = element.sourceFullWidth-1;
				int top = element.sourceFullHeight-1;
				int bottom = 0;

				//note: y=0 is at the bottom, not the top.

				//try to find the first solid pixel on the LEFT
				for(int c = left; c<=right; c++) //left to right
				{
					bool didFindSolid = false;

					for(int r = bottom; r<=top; r++) //bottom to top
					{
						if(sourcePixels[r*cols + c].a != 0.0f)
						{
							didFindSolid = true;
							left = c;
							break;
						}
					}

					if(didFindSolid) break;
				}

				//try to find the first solid pixel on the RIGHT
				for(int c = right; c>=left; c--) //right to left
				{
					bool didFindSolid = false;
					
					for(int r = bottom; r<=top; r++) //bottom to top
					{
						if(sourcePixels[r*cols + c].a != 0.0f)
						{
							didFindSolid = true;
							right = c;
							break;
						}
					}
					
					if(didFindSolid) break;
				}

				//try to find the first solid pixel on the TOP
				for(int r = top; r>=bottom; r--) //top to bottom
				{
					bool didFindSolid = false;

					for(int c = left; c<=right; c++) //left to right
					{
						if(sourcePixels[r*cols + c].a != 0.0f)
						{
							didFindSolid = true;
							top = r;
							break;
						}
					}
					
					if(didFindSolid) break;
				}

				//try to find the first solid pixel on the BOTTOM
				for(int r = bottom; r<=top; r++) //bottom to top
				{
					bool didFindSolid = false;
					
					for(int c = left; c<=right; c++) //left to right
					{
						if(sourcePixels[r*cols + c].a != 0.0f)
						{
							didFindSolid = true;
							bottom = r;
							break;
						}
					}
					
					if(didFindSolid) break;
				}

				//Debug.Log(element.name + "  left:" + left + " right:"+ right +" top:" + top + " bottom:" + bottom);

				//apply trim padding
				if(trimPadding != 0)
				{
					left = Mathf.Max(0, left-trimPadding);
					right = Mathf.Min(element.sourceFullWidth-1, right+trimPadding);
					bottom = Mathf.Max(0, bottom-trimPadding);
					top = Mathf.Min(element.sourceFullHeight-1, top+trimPadding);
				}

				element.sourceTrimX = left;
				element.sourceTrimY = bottom; //todo: figure out if this should be bottom or top
				element.sourceTrimWidth = right-left+1; //the +1 is because the values are INCLUSIVE
				element.sourceTrimHeight = top-bottom+1; //the +1 is because the values are INCLUSIVE
			}
			else 
			{
				element.sourceTrimX = 0;
				element.sourceTrimY = 0;
				element.sourceTrimWidth = element.sourceFullWidth;
				element.sourceTrimHeight = element.sourceFullHeight;
			} 

			if(element.shouldScale)
			{
				element.scaledTrimX = Mathf.FloorToInt((float)element.sourceTrimX * link.scale);
				element.scaledTrimY =  Mathf.FloorToInt((float)element.sourceTrimY * link.scale);
				element.scaledTrimWidth = Mathf.CeilToInt((float)element.sourceTrimWidth * link.scale);
				element.scaledTrimHeight = Mathf.CeilToInt((float)element.sourceTrimHeight * link.scale);
			}
			else 
			{
				element.scaledTrimX = element.sourceTrimX;
				element.scaledTrimY =  element.sourceTrimY;
				element.scaledTrimWidth = element.sourceTrimWidth;
				element.scaledTrimHeight = element.sourceTrimHeight;
			}

			//padding is only on 2 sides (top and right)
			//extrude is on all 4 sides
			element.expandedWidth = element.scaledTrimWidth + element.extrude * 2 + element.padding;
			element.expandedHeight = element.scaledTrimHeight + element.extrude * 2 + element.padding;

			//Debug.Log("expanded width " +element.scaledTrimWidth + ", " + element.scaledTrimHeight);

			//Debug.Log("Turned " + element.name + " " + element.sourceFullWidth+","+element.sourceFullHeight + " into " + element.sourceTrimWidth +"," + element.sourceTrimHeight);
			//Debug.Log("Turned " + element.name + " " + element.scaledFullWidth+","+element.scaledFullHeight + " into " + element.scaledTrimWidth +"," + element.scaledTrimHeight);
		}
	}

	private void CalculateElementPacking()
	{
		int elementCount = elements.Count;
		
		//use the min area of all the elements to start with a sensible estimated size for the atlas to be
		int minArea = 0;
		
		for(int e = 0; e<elementCount; e++)
		{
			minArea += elements[e].expandedWidth * elements[e].expandedHeight;
		}
		
		int tryWidth = 16;
		int tryHeight = 16;
		
		int tries = 0;
		
		while((tryWidth-link.padding)*(tryHeight-link.padding) < minArea && tries++ < 100) 
		{
			if(tries % 2 == 0) //alternate increasing width and height until we find a size that fits everything
			{
				tryWidth *= 2;
			}
			else 
			{
				tryHeight *= 2;
			}
		}
		
		tries = 0;

		//subtract padding because we can't pack right up to the padded border
		//but don't subtract padding*2 because the element sizes already account for padding on one side
		PRPacker packer = new PRPacker(tryWidth-link.padding,tryHeight-link.padding); 

		while(tries++ < 100) //tries is to prevent infinite loops
		{
			bool didFail = false;
			for(int e = 0; e<elementCount; e++)
			{
				PRAtlasElement element = elements[e];

				//Debug.Log("Try fitting " + element.expandedWidth + ", " + element.expandedHeight + " into " + tryWidth+","+tryHeight);
				//TODO update
				PRRect rect = packer.Insert(element.expandedWidth,element.expandedHeight, PRPacker.ChoiceHeuristic.ShortSideFit);

				if(rect.width == 0 && rect.height == 0) //both at 0 means it failed
				{
					didFail = true;
					
					if(tryWidth <= tryHeight) //alternate increasing width and height until we find a size that fits everything
					{
						tryWidth *= 2;
					}
					else 
					{
						tryHeight *= 2;
					}
					
					packer.Init(tryWidth-link.padding,tryHeight-link.padding);
					break;
				}
				else 
				{
					element.packedRect = rect;
					element.packedRect.x += element.padding; //push the rect off the wall (note, y doesn't need this for the reason below)
					//flip packing y coord. This is because the algorithm tries to pack everything around 0,0
					//and we want 0,0 to be top left instead of bottom left (which it would be with Unity's coord system)
					//there's no real reason for it to be top left, except that it's what people are used to.
					element.packedRect.y = (tryHeight - element.packedRect.y) - element.packedRect.height; 
				}
			}
			
			if(!didFail)
			{
				atlasWidth = tryWidth;
				atlasHeight = tryHeight;
				break; //we're done!
			}
		}
	}

	private IEnumerator<string> CreateAtlasTexture ()
	{
		atlasTexture = new Texture2D(atlasWidth,atlasHeight, TextureFormat.ARGB32,false);
		atlasTexture.filterMode = FilterMode.Bilinear;
		atlasTexture.SetPixels32(new Color32[atlasWidth*atlasHeight]); //clear it out (using color32 for speed)

		int elementCount = elements.Count;
		
		for(int e = 0; e<elementCount; e++)
		{
			PRAtlasElement element = elements[e];

			yield return "Packing " + element.name;

			PRRect packedRect = element.packedRect;

			int extrude = element.extrude;

			int outputX = packedRect.x + extrude;
			int outputY = packedRect.y + extrude; 
			int outputWidth = packedRect.width - extrude*2 - element.padding;
			int outputHeight = packedRect.height - extrude*2 - element.padding;

			if(link.scale == 1.0f)
			{
				Color[] elementPixels = element.texture.GetPixels(element.sourceTrimX,element.sourceTrimY,element.sourceTrimWidth,element.sourceTrimHeight,0);

				atlasTexture.SetPixels(outputX, outputY, outputWidth, outputHeight, elementPixels);
				//atlasTexture.SetPixels(outputX,outputY,outputWidth,outputHeight,elementPixels);

				if(extrude != 0) //do extruding by pulling pixels from each edge
				{
					//left
					elementPixels = element.texture.GetPixels(element.sourceTrimX,element.sourceTrimY,1,element.sourceTrimHeight,0);
					for(int c = 0; c<extrude; c++)
					{
						atlasTexture.SetPixels(outputX-(c+1),outputY,1,outputHeight,elementPixels);
					}

					//right
					elementPixels = element.texture.GetPixels(element.sourceTrimX+element.sourceTrimWidth-1,element.sourceTrimY,1,element.sourceTrimHeight,0);
					for(int c = 0; c<extrude; c++)
					{
						atlasTexture.SetPixels(outputX+outputWidth+c,outputY,1,outputHeight,elementPixels);
					}

					//bottom
					elementPixels = element.texture.GetPixels(element.sourceTrimX,element.sourceTrimY,element.sourceTrimWidth,1,0);
					for(int c = 0; c<extrude; c++)
					{
						atlasTexture.SetPixels(outputX,outputY-(c+1),outputWidth,1,elementPixels);
					}

					//top
					elementPixels = element.texture.GetPixels(element.sourceTrimX,element.sourceTrimY+element.sourceTrimHeight-1,element.sourceTrimWidth,1,0);
					for(int c = 0; c<extrude; c++)
					{
						atlasTexture.SetPixels(outputX,outputY+outputHeight+c,outputWidth,1,elementPixels);
					}

					//bottom left
					Color[] cornerPixels = RXArrayUtil.CreateArrayFilledWithItem<Color>(element.texture.GetPixel(element.sourceTrimX,element.sourceTrimY), extrude*extrude);
					atlasTexture.SetPixels(outputX-extrude,outputY-extrude,extrude,extrude,cornerPixels); 

					//top left
					cornerPixels = RXArrayUtil.CreateArrayFilledWithItem<Color>(element.texture.GetPixel(element.sourceTrimX,element.sourceTrimY+element.sourceTrimHeight-1), extrude*extrude);
					atlasTexture.SetPixels(outputX-extrude,outputY+outputHeight,extrude,extrude,cornerPixels); 

					//top right
					cornerPixels = RXArrayUtil.CreateArrayFilledWithItem<Color>(element.texture.GetPixel(element.sourceTrimX+element.sourceTrimWidth-1,element.sourceTrimY+element.sourceTrimHeight-1), extrude*extrude);
					atlasTexture.SetPixels(outputX+outputWidth,outputY+outputHeight,extrude,extrude,cornerPixels); 

					//bottom right
					cornerPixels = RXArrayUtil.CreateArrayFilledWithItem<Color>(element.texture.GetPixel(element.sourceTrimX+element.sourceTrimWidth-1,element.sourceTrimY), extrude*extrude);
					atlasTexture.SetPixels(outputX+outputWidth,outputY-extrude,extrude,extrude,cornerPixels); 
				}
			}
			else //scale isn't 1, so do a bilinear resample of the texture into the atlas
			{
				/*
				Texture2D sourceTexture = element.texture;

				float ratioX = (1.0f / (float)element.sourceFullWidth);
				float ratioY = (1.0f / (float)element.sourceFullHeight); 

				float trimX = ratioX * element.sourceTrimX;
				float trimY = ratioY * element.sourceTrimY;
				*/

				Texture2D sourceTexture = element.texture;

				Color[] outputPixels = new Color[outputWidth*outputHeight];

				float sourceRatioX = 1.0f / (float)element.sourceFullWidth; //the width of one pixel in uv coords
				float sourceRatioY = 1.0f / (float)element.sourceFullHeight; //the height of one pixel in uv coords

				float trimPercentX = sourceRatioX * (float)element.sourceTrimX;
				float trimPercentY = sourceRatioY * (float)element.sourceTrimY;
				float trimPercentWidth = sourceRatioX * (float)element.sourceTrimWidth;
				float trimPercentHeight = sourceRatioY * (float)element.sourceTrimHeight;

				trimPercentX += sourceRatioX * 0.5f;
				trimPercentY += sourceRatioY * 0.5f;

				trimPercentWidth -= sourceRatioX * 1.0f;
				trimPercentHeight -= sourceRatioX * 1.0f;

				float outputRatioX = 1.0f/(float)(outputWidth-1);
				float outputRatioY = 1.0f/(float)(outputHeight-1);

				//Debug.Log(string.Format("{0} tpx:{1} tpy:{2} tpw:{3} tph:{4}",element.name,trimPercentX,trimPercentY,trimPercentWidth,trimPercentHeight));

				for(int r = 0; r<outputHeight; r++)
				{ 
					for(int c = 0; c<outputWidth; c++)
					{
						float colPercent = (float)c * outputRatioX;
						float rowPercent = (float)r * outputRatioY;

						outputPixels[r*outputWidth + c] = sourceTexture.GetPixelBilinear(trimPercentX + colPercent*trimPercentWidth, trimPercentY + rowPercent*trimPercentHeight); 

//						
						//multi sample with a kernel
// 						float bx = trimPercentX + colPercent*trimPercentWidth;
//						float by = trimPercentY + rowPercent*trimPercentHeight;
//
//						Color outputColor;
//
//						//outputColor = sourceTexture.GetPixelBilinear(baseX, baseY) * 0.7f; 
//
//						float sx = sourceRatioX*(1.0f-link.scale);
//						float sy = sourceRatioY*(1.0f-link.scale);
//
//						float dsx = sx*2;
//						float dsy = sy*2;
//
//						float one = 1.0f/64.0f;
//						float three = 3.0f/64.0f;
//						float nine = 9.0f/64.0f;
//
//						//from https://groups.google.com/forum/#!topic/comp.graphics.algorithms/XpcOL2xiKO4
//
//						outputColor = sourceTexture.GetPixelBilinear(bx-dsx, by+dsy) * one; 
//						outputColor += sourceTexture.GetPixelBilinear(bx-sx, by+dsy) * three; 
//						outputColor += sourceTexture.GetPixelBilinear(bx+sx, by+dsy) * three; 
//						outputColor += sourceTexture.GetPixelBilinear(bx+dsx, by+dsy) * one; 
//
//						outputColor += sourceTexture.GetPixelBilinear(bx-dsx, by+sy) * three; 
//						outputColor += sourceTexture.GetPixelBilinear(bx-sx, by+sy) * nine; 
//						outputColor += sourceTexture.GetPixelBilinear(bx+sx, by+sy) * nine; 
//						outputColor += sourceTexture.GetPixelBilinear(bx+dsx, by+sy) * three; 
//
//						outputColor += sourceTexture.GetPixelBilinear(bx-dsx, by-sy) * three; 
//						outputColor += sourceTexture.GetPixelBilinear(bx-sx, by-sy) * nine; 
//						outputColor += sourceTexture.GetPixelBilinear(bx+sx, by-sy) * nine; 
//						outputColor += sourceTexture.GetPixelBilinear(bx+dsx, by-sy) * three; 
//
//						outputColor += sourceTexture.GetPixelBilinear(bx-dsx, by-dsy) * one; 
//						outputColor += sourceTexture.GetPixelBilinear(bx-sx, by-dsy) * three; 
//						outputColor += sourceTexture.GetPixelBilinear(bx+sx, by-dsy) * three; 
//						outputColor += sourceTexture.GetPixelBilinear(bx+dsx, by-dsy) * one; 

//						outputColor = sourceTexture.GetPixelBilinear(bx, by) * 0.7f; 
//
//						outputColor += sourceTexture.GetPixelBilinear(bx+sourceRatioX, by) * 0.05f; 
//						outputColor += sourceTexture.GetPixelBilinear(bx-sourceRatioX, by) * 0.05f; 
//						outputColor += sourceTexture.GetPixelBilinear(bx, by+sourceRatioY) * 0.05f; 
//						outputColor += sourceTexture.GetPixelBilinear(bx, by-sourceRatioY) * 0.05f; 
//
//						outputColor += sourceTexture.GetPixelBilinear(bx+sourceRatioX, by+sourceRatioY) * 0.0025f; 
//						outputColor += sourceTexture.GetPixelBilinear(bx+sourceRatioX, by-sourceRatioY) * 0.0025f; 
//						outputColor += sourceTexture.GetPixelBilinear(bx-sourceRatioX, by+sourceRatioY) * 0.0025f; 
//						outputColor += sourceTexture.GetPixelBilinear(bx-sourceRatioX, by-sourceRatioY) * 0.0025f; 
//
//						outputPixels[r*outputWidth + c] = outputColor;

						//nearest neighbor
//						int pixelX = Mathf.RoundToInt((trimPercentX + colPercent*trimPercentWidth) * (float)sourceTexture.width);
//						int pixelY = Mathf.RoundToInt((trimPercentY + rowPercent*trimPercentHeight) * (float)sourceTexture.height);
//						outputPixels[r*outputWidth + c] = sourceTexture.GetPixel(pixelX,pixelY);
					}
				} 

				atlasTexture.SetPixels(outputX,outputY,outputWidth,outputHeight,outputPixels);

				if(element.extrude != 0)
				{
					//yield return "Extruding " + element.name;

					int heightExtruded = outputHeight + extrude*2;

					Color[] sidePixels = new Color[extrude*heightExtruded]; //used for the left and right sides

					//LEFT SIDE
					for(int r = 0; r<heightExtruded; r++) //figure out row colour and then fill that row
					{
						int outputRow = Mathf.Max(0, Mathf.Min(outputHeight-1,r-extrude));
						Color color = outputPixels[outputRow*outputWidth];
						for(int c = 0; c<extrude; c++)
						{
							sidePixels[r*extrude + c] = color;
						}
						atlasTexture.SetPixels(outputX-extrude,outputY-extrude,extrude,heightExtruded,sidePixels);
					}

					//RIGHT SIDE
					for(int r = 0; r<heightExtruded; r++) //figure out row colour and then fill that row
					{
						int outputRow = Mathf.Max(0, Mathf.Min(outputHeight-1,r-extrude));
						Color color = outputPixels[outputRow*outputWidth + outputWidth-1];
						for(int c = 0; c<extrude; c++)
						{
							sidePixels[r*extrude + c] = color;
						}
						atlasTexture.SetPixels(outputX+outputWidth,outputY-extrude,extrude,heightExtruded,sidePixels);
					}

					sidePixels = new Color[extrude*outputWidth]; //used for the top and bottom sides

					//BOTTOM SIDE
					for(int c = 0; c<outputWidth; c++) //figure out row colour and then fill that row
					{
						Color color = outputPixels[c];
						for(int r = 0; r<extrude; r++)
						{
							sidePixels[r*outputWidth + c] = color;
						}
						atlasTexture.SetPixels(outputX,outputY-extrude,outputWidth,extrude,sidePixels);
					}

					//TOP SIDE
					int topOffset = (outputHeight-1)*outputWidth;//start at the top row
					for(int c = 0; c<outputWidth; c++) //figure out row colour and then fill that row
					{
						Color color = outputPixels[topOffset+c];
						for(int r = 0; r<extrude; r++)
						{
							sidePixels[r*outputWidth + c] = color;
						}
						atlasTexture.SetPixels(outputX,outputY+outputHeight,outputWidth,extrude,sidePixels);
					}
				}
			}
		}
		
//		int count = 5;
//		for(int i = 0; i < count; i++)
//		{
//			float percent = (float)i/(float)(count-1);
//			Debug.Log(i + " percent " + percent);
//		}

		yield return "Writing PNG to disk";

		atlasTexture.Apply(false,false);

		File.WriteAllBytes(link.atlasFilePath + pngSuffix,atlasTexture.EncodeToPNG());

		if(PRViewAtlasWindow.instance != null && PRViewAtlasWindow.instance.link == link)
		{
			PRViewAtlasWindow.instance.UpdateAtlas();
		}

		//in theory this should make it unreadable, 
		//but it doesn't matter because we're about to delete it anyway
		//atlasTexture.Apply(false,true); 



		//This stuff would be handy, but it's SO SLOW and unreliable that it's not worth it
//		TextureImporter importer = TextureImporter.GetAtPath(link.atlasFilePath+pngSuffix) as TextureImporter;
//
//		if(importer != null)
//		{
//			importer.maxTextureSize = Mathf.Max(importer.maxTextureSize,packedWidth,packedHeight);
//			importer.alphaIsTransparency = true;
//			AssetDatabase.ImportAsset(importer.assetPath);
//		}
//
//		yield return "Importing Atlas";
	}

	static void SetPixels(Color[] destPixels, int destWidth, int drawX, int drawY, int drawWidth, int drawHeight, Color[] sourcePixels)
	{
		int destIndex = drawY*destWidth + drawX;
		int sourceIndex = 0;
		int widthJump = destWidth-drawWidth;

		for(int r = 0; r<drawHeight; r++)
		{
			for(int c = 0; c<drawWidth; c++)
			{
				destPixels[destIndex++] = sourcePixels[sourceIndex++];
			}
			destIndex += widthJump;
		}
	}

	private void CreateAtlasDataFile ()
	{
		StringBuilder stringBuilder = new StringBuilder("{\"frames\": {\n\n");

		int elementCount = elements.Count;
		string[] elementStrings = new string[elementCount];

		for(int e = 0; e<elementCount; e++)
		{
			PRAtlasElement element = elements[e];
			elementStrings[e] = element.GetJSONString();
		}

		stringBuilder.Append(string.Join(",\n",elementStrings));

		stringBuilder.Append("},\n");

		stringBuilder.Append("\"meta\":{\n");

		stringBuilder.Append("\t\"app\": \"Packrat Atlas Generator github.com/MattRix/Packrat\",\n");
		stringBuilder.Append("\t\"version\": \"1.0\",\n");
		stringBuilder.Append("\t\"image\": \""+link.atlasFilePath+".png\",\n");
		stringBuilder.Append("\t\"format\": \"RGBA8888\",\n");
		stringBuilder.Append("\t\"size\": {\"w\":"+atlasWidth+",\"h\":"+atlasHeight+"},\n");
		stringBuilder.Append("\t\"scale\": \""+link.scale.ToString()+"\"\n");

		stringBuilder.Append("}\n}\n");

		File.WriteAllText(link.atlasFilePath +".txt", stringBuilder.ToString());
	}

	private void CleanUp ()
	{
		Object.DestroyImmediate(atlasTexture); //we've already written it to a file, so destroy it

		int elementCount = elements.Count;
		for(int e = 0; e<elementCount; e++)
		{
			PRAtlasElement element = elements[e];
			Object.DestroyImmediate(element.texture);
		}

		Resources.UnloadUnusedAssets();
	}
}








