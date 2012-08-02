using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//parts of this were inspired by https://github.com/prime31/UIToolkit/blob/master/Assets/Plugins/UIToolkit/UIElements/UIText.cs

public struct FCharInfo
{
	public int charID;
	public float x;
	public float y;
	public float width;
	public float height;
	public Rect uvRect;
	public Vector2 uvTopLeft;
	public Vector2 uvTopRight;
	public Vector2 uvBottomRight;
	public Vector2 uvBottomLeft;
	public float offsetX;
	public float offsetY;
	public float xadvance;
	public int page;
}

public struct FKerningInfo
{
	public int first;
	public int second;
	public int amount;
}

public class FLetterQuad
{
	public FCharInfo charInfo;
	public Rect rect;
	public Vector2 topLeft;
	public Vector2 topRight;
	public Vector2 bottomRight;
	public Vector2 bottomLeft;
	
	public void CalculateVectors()
	{
		topLeft.Set(rect.xMin,rect.yMax);
		topRight.Set(rect.xMax,rect.yMax);
		bottomRight.Set(rect.xMax,rect.yMin);
		bottomLeft.Set(rect.xMin,rect.yMin);
	}
	
	public void CalculateVectors(float offsetX, float offsetY)
	{
		topLeft.Set(rect.xMin+offsetX,rect.yMax+offsetY);
		topRight.Set(rect.xMax+offsetX,rect.yMax+offsetY);
		bottomRight.Set(rect.xMax+offsetX,rect.yMin+offsetY);
		bottomLeft.Set(rect.xMin+offsetX,rect.yMin+offsetY);
	}
}

public class FLetterQuadLine
{
	public Rect bounds;
	public int letterCount;
	public FLetterQuad[] quads;
}

public class FFont
{
	public const int ASCII_NEWLINE = 10;
	public const int ASCII_SPACE = 32;
	public const int ASCII_HYPHEN_MINUS = 45;
	
	public const int ASCII_LINEHEIGHT_REFERENCE = 77; //77 is the letter M
	
	private string _name;
	private FAtlasElement _element;
	private string _configPath;
	
	private FCharInfo[] _charInfos;
	private FCharInfo[] _charInfosByID; //chars with the index of array being the char id
	private FKerningInfo[] _kerningInfos;
	
	private FKerningInfo _nullKerning = new FKerningInfo();
	
	private float _lineHeight;
	private int _lineBase;
	private int _configWidth;
	//private int _configHeight;
	private float _configRatio;
	
	private float _defaultLineHeight;
	private float _defaultLetterSpacing;
	
	public FFont (string name, FAtlasElement element, string configPath, float defaultLineHeight, float defaultLetterSpacing)
	{
		_name = name;
		_element = element;
		_configPath = configPath;
		_defaultLineHeight = defaultLineHeight;
		_defaultLetterSpacing = defaultLetterSpacing;
					
		LoadAndParseConfigFile();
	}
	
	private void LoadAndParseConfigFile()
	{
		TextAsset asset = (TextAsset) Resources.Load(_configPath, typeof(TextAsset));
		
		if(asset == null)
		{
			throw new Exception("Couldn't find font config file " + _configPath);	
		}
		
		string[] separators = {"\r\n"};
		string[] lines = asset.text.Split(separators,StringSplitOptions.RemoveEmptyEntries);
		int wordCount = 0;
		int c = 0;
		int k = 0;
		
		_charInfosByID = new FCharInfo[127];
		
		Vector2 textureSize = _element.atlas.textureSize;

// 		commented out because we shouldn't need offsets because the source element will already have them
//
//		float uvOffsetX;
//		float uvOffsetY;
//		
//		if(FEngine.isOpenGL)
//		{
//			uvOffsetX = 0.0f/textureSize.x;
//			uvOffsetY = 0.0f/textureSize.y;
//		}
//		else
//		{
//			uvOffsetX = 0.5f/textureSize.x;
//			uvOffsetY = -0.5f/textureSize.y; 
//		}
		
		bool wasKerningFound = false;
	
		foreach(string line in lines)
		{
			//Debug.Log ("LINE! " + line);
			string [] words = line.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
			
			/* we don't care about these, or else they could be in the elseif
			if(words[0] == "info") //info face="Franchise Bold" size=160 bold=0 italic=0 charset="" unicode=0 stretchH=100 smooth=1 aa=1 padding=0,0,0,0 spacing=1,1
			{
				//do nothing
			}
			else if(words[0] == "page") //page id=0 file="FranchiseLarge.png"
			{
				//do nothing
			}
			*/
			
			 if(words[0] == "common") //common lineHeight=168 base=26 scaleW=1024 scaleH=1024 pages=1 packed=0
			{
				//these are the height and width of the original atlas built by Hiero
				_configWidth = int.Parse(words[3].Split('=')[1]);
				//_configHeight = int.Parse(words[4].Split('=')[1]);
				
				//this is the ratio of the config vs what we're actually working with
				_configRatio = _element.sourceRect.width/_configWidth;
				
				_lineHeight = int.Parse(words[1].Split('=')[1]) * _configRatio;		
			}
			else if(words[0] == "chars") //chars count=92
			{
				int charCount = int.Parse(words[1].Split('=')[1]);
				_charInfos = new FCharInfo[charCount+1]; //gotta add 1 because the charCount seems to be off by 1
			}
			else if(words[0] == "char") //char id=32   x=0     y=0     width=0     height=0     xoffset=0     yoffset=120    xadvance=29     page=0  chnl=0 letter=a 
			{
				FCharInfo charInfo = new FCharInfo();
				
				wordCount = words.Length;
				
				for(int w = 1; w<wordCount; w++)
				{
					string[] parts = words[w].Split('=');	
					string partName = parts[0];
					if(partName != "letter")
					{
						int partValue = int.Parse(parts[1]);
					
						if(partName == "id")
						{
							charInfo.charID = partValue;
						}
						else if(partName == "x")
						{
							charInfo.x = partValue*_configRatio;
						}
						else if(partName == "y")
						{
							charInfo.y = partValue*_configRatio;
						}
						else if(partName == "width")
						{
							charInfo.width = partValue*_configRatio;
						}
						else if(partName == "height")
						{
							charInfo.height = partValue*_configRatio;
						}
						else if(partName == "xoffset")
						{
							charInfo.offsetX = partValue*_configRatio;
						}
						else if(partName == "yoffset")
						{
							charInfo.offsetY = partValue*_configRatio;
						}
						else if(partName == "xadvance")
						{
							charInfo.xadvance = partValue*_configRatio;
						}
						else if(partName == "page")
						{
							charInfo.page = partValue;
						}
						
						Rect uvRect = new Rect 	
						(
							_element.uvRect.x + charInfo.x/textureSize.x*FEngine.scale,
							(textureSize.y-charInfo.y-charInfo.height)/textureSize.y*FEngine.scale - (1.0f - _element.uvRect.yMax),
							charInfo.width/textureSize.x*FEngine.scale,
							charInfo.height/textureSize.y*FEngine.scale
						);
					
						//commented out because we shouldn't need offsets because the original element will already have them!
						//uvRect.x += uvOffsetX;
						//uvRect.y += uvOffsetY;
						
						charInfo.uvRect = uvRect;
						
						charInfo.uvTopLeft.Set(uvRect.xMin,uvRect.yMax);
						charInfo.uvTopRight.Set(uvRect.xMax,uvRect.yMax);
						charInfo.uvBottomRight.Set(uvRect.xMax,uvRect.yMin);
						charInfo.uvBottomLeft.Set(uvRect.xMin,uvRect.yMin);
					}
					
					
					
				}
				
				_charInfosByID[charInfo.charID] = charInfo;
				_charInfos[c] = charInfo;
				
				c++;
			}
			else if(words[0] == "kernings") //kernings count=169
			{
				wasKerningFound = true;
				int kerningCount = int.Parse(words[1].Split('=')[1]);
				_kerningInfos = new FKerningInfo[kerningCount+1]; //gotta add 1 because it's off by 1
			}
			else if(words[0] == "kerning") //kerning first=56  second=57  amount=-1
			{
				FKerningInfo kerningInfo = new FKerningInfo();
				
				wordCount = words.Length;
				
				for(int w = 1; w<wordCount; w++)
				{
					string[] parts = words[w].Split('=');	
					string partName = parts[0];
					int partValue = int.Parse(parts[1]);
					
					if(partName == "first")
					{
						kerningInfo.first = partValue;
					}
					else if(partName == "second")
					{
						kerningInfo.second = partValue;
					}
					else if(partName == "amount")
					{
						kerningInfo.amount = partValue;
					}
				}
				
				_kerningInfos[k] = kerningInfo;
				
				k++;
			}
			
		}
		
		if(!wasKerningFound) //if there are no kernings at all (like in a pixel font), then make an empty kerning array
		{
			_kerningInfos = new FKerningInfo[0];	
		}
		
	}
	
	public FLetterQuadLine[] GetQuadInfoForText(string text, float lineHeightDelta, float letterSpacingDelta)
	{
		int lineCount = 0;
		int letterCount = 0;
		
		char[] letters = text.ToCharArray();
		
		FLetterQuadLine[] lines = new FLetterQuadLine[10];
		
		for(int c = 0; c<letters.Length; c++)
		{
			char letter = letters[c];
			
			if(letter == ASCII_NEWLINE)
			{
				lines[lineCount] = new FLetterQuadLine();
				lines[lineCount].letterCount = letterCount;
				lines[lineCount].quads = new FLetterQuad[letterCount];
				
				lineCount++;
				letterCount = 0;
			}
			else 
			{
				letterCount++;	
			}
		}
		
		lines[lineCount] = new FLetterQuadLine();
		lines[lineCount].letterCount = letterCount;
		lines[lineCount].quads = new FLetterQuad[letterCount];
		
		FLetterQuadLine[] oldLines = lines;
		lines = new FLetterQuadLine[lineCount+1];
		 
		for(int c = 0; c<lineCount+1; c++)
		{
			lines[c] = oldLines[c];	
		}
		
		lineCount = 0;
		letterCount = 0;
		
		float nextX = 0;
		float nextY = 0;
		
		FCharInfo charInfo;
		
		char previousLetter = '\0';
		
		float minX = 100000;
		float maxX = -100000;
		float minY = 100000;
		float maxY = -100000;
		
		for(int c = 0; c<letters.Length; c++)
		{
			char letter = letters[c];
			
			if(letter == ASCII_NEWLINE)
			{	
				lines[lineCount].bounds = new Rect(minX,minY,maxX-minX,maxY-minY);
				
				minX = 100000;
				maxX = -100000;
				minY = 100000;
				maxY = -100000;
				
				nextX = 0;
				nextY -= _lineHeight * lineHeightDelta;
				
				lineCount++;
				letterCount = 0;
			}
			else 
			{
				FKerningInfo foundKerning = _nullKerning;
				
				foreach(FKerningInfo kerningInfo in _kerningInfos)
				{
					if(kerningInfo.first == previousLetter && kerningInfo.second == letter)
					{
						foundKerning = kerningInfo;
					}
				}
				
				nextX += foundKerning.amount * letterSpacingDelta; 
				
				//Debug.Log ("found kerning " + foundKerning.amount + " between " + (char)foundKerning.first + " and " + (char) foundKerning.second);	
				//Debug.Log ("found kerning " + foundKerning.amount + " between " + foundKerning.first + " and " + foundKerning.second);	

				FLetterQuad letterQuad = new FLetterQuad();
				
				charInfo = _charInfosByID[letter];
				letterQuad.charInfo = charInfo;
				letterQuad.rect = new Rect(nextX + charInfo.offsetX, nextY - charInfo.offsetY - charInfo.height, charInfo.width, charInfo.height);
				
				lines[lineCount].quads[letterCount] = letterQuad;	
				
				minX = Math.Min (minX, letterQuad.rect.xMin);
				maxX = Math.Max (maxX, letterQuad.rect.xMax);
				minY = Math.Min (minY, letterQuad.rect.yMin);
				maxY = Math.Max (maxY, letterQuad.rect.yMax);
				
				nextX += charInfo.xadvance * letterSpacingDelta;
				
				letterCount++;
			}
						
			previousLetter = letter; 
		}
		
		lines[lineCount].bounds = new Rect(minX,minY,maxX-minX,maxY-minY);
	
		return lines;
	}
	
	public string name
	{
		get { return _name;}	
	}
	
	public FAtlasElement element
	{
		get { return _element;}	
	}
	
	public float defaultLineHeight
	{
		get {return _defaultLineHeight;}	
	}
	
	public float defaultLetterSpacing
	{
		get {return _defaultLetterSpacing;}	
	}

//  Not gonna deal with this stuff unless it's actually needed
//
//	private int forceLowAsciiChar(int charID)
//	{
//		if(charID < 8200) return charID; //short circuit so we don't branch 6 times
//		
//		if(charID == 8211) return 150;
//		if(charID == 8212) return 151;
//		if(charID == 8216) return 145;
//		if(charID == 8217) return 146;
//		if(charID == 8220) return 147;
//		if(charID == 8221) return 148;
//			
//		return charID;	
//	}
	
	
	
}


