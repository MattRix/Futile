using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//parts of this were inspired by https://github.com/prime31/UIToolkit/blob/master/Assets/Plugins/UIToolkit/UIElements/UIText.cs

//how to interpret BMFont files: http://www.gamedev.net/topic/284560-bmfont-and-how-to-interpret-the-fnt-file/

public class FCharInfo
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
	public string letter;
}

public class FKerningInfo
{
	public int first;
	public int second;
	public float amount;
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
		topLeft.Set(rect.xMin + offsetX,rect.yMax + offsetY);
		topRight.Set(rect.xMax + offsetX,rect.yMax + offsetY);
		bottomRight.Set(rect.xMax + offsetX,rect.yMin + offsetY);
		bottomLeft.Set(rect.xMin + offsetX,rect.yMin + offsetY);
	}
	
	//this moves the quads by a certain offset
	public void CalculateVectorsToWholePixels(float offsetX, float offsetY)
	{
		float scaleInverse = Futile.displayScaleInverse;
		
		//the stuff is used to make sure the quad is resting on a whole pixel
		float xMod = (rect.xMin+offsetX) % scaleInverse;
		float yMod = (rect.yMin+offsetY) % scaleInverse;
		
		offsetX -= xMod;
		offsetY -= yMod;
		
		float roundedLeft = rect.xMin+offsetX;
		float roundedRight = rect.xMax+offsetX;
		float roundedTop = rect.yMax+offsetY;
		float roundedBottom = rect.yMin+offsetY;
		
		topLeft.x = roundedLeft;
		topLeft.y = roundedTop;
		
		topRight.x = roundedRight;
		topRight.y = roundedTop;
		
		bottomRight.x = roundedRight;
		bottomRight.y = roundedBottom;
		
		bottomLeft.x = roundedLeft;
		bottomLeft.y = roundedBottom;
	}
	
	
}

public class FTextParams
{
	public float scaledLineHeightOffset = 0;
	public float scaledKerningOffset = 0;
	
	private float _lineHeightOffset = 0;
	private float _kerningOffset = 0;
	
	public float kerningOffset
	{
		get {return _kerningOffset;}
		set 
		{
			_kerningOffset = value; 
			scaledKerningOffset = value;
		}
	}
	
	public float lineHeightOffset
	{
		get {return _lineHeightOffset;}
		set 
		{
			_lineHeightOffset = value; 
			scaledLineHeightOffset = value;
		}
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
	private Dictionary<uint,FCharInfo> _charInfosByID; //chars with the index of array being the char id
	private FKerningInfo[] _kerningInfos;
	private int _kerningCount;
	
	private FKerningInfo _nullKerning = new FKerningInfo();
	
	private float _lineHeight;
	//private float _lineBase;
	private int _configWidth;
	//private int _configHeight;
	private float _configRatio;
	
	private FTextParams _textParams;
	
	private float _offsetX;
	private float _offsetY;
	
	public FFont (string name, FAtlasElement element, string configPath, float offsetX, float offsetY, FTextParams textParams)
	{
		_name = name;
		_element = element;
		_configPath = configPath;
		_textParams = textParams;
		_offsetX = offsetX;
		_offsetY = offsetY;
		
		LoadAndParseConfigFile();
	}
	
	private void LoadAndParseConfigFile()
	{
		TextAsset asset = (TextAsset) Resources.Load(_configPath, typeof(TextAsset));
		
		if(asset == null)
		{
			throw new FutileException("Couldn't find font config file " + _configPath);	
		}
		
		string[] separators = new string[1]; 
		
		separators[0] = "\n"; //osx
		string[] lines = asset.text.Split(separators,StringSplitOptions.RemoveEmptyEntries);
		
		if(lines.Length <= 1) //osx line endings didn't work, try windows
		{
			separators[0] = "\r\n";
			lines = asset.text.Split(separators,StringSplitOptions.RemoveEmptyEntries);
		}
		
		if(lines.Length <= 1) //those line endings didn't work, so we're on a magical OS
		{
			separators[0] = "\r";
			lines = asset.text.Split(separators,StringSplitOptions.RemoveEmptyEntries);
		}
		
		if(lines.Length <= 1) //WHAT
		{
			throw new FutileException("Your font file is messed up");
		}
		
		int wordCount = 0;
		int c = 0;
		int k = 0;
		
		_charInfosByID = new Dictionary<uint, FCharInfo>(127);
		
		//insert an empty char to be used when a character isn't in the font data file
		FCharInfo emptyChar = new FCharInfo();
		_charInfosByID[0] = emptyChar;
		
		float resourceScale = Futile.resourceScale;
		
		Vector2 textureSize = _element.atlas.textureSize;
		
		bool wasKerningFound = false;
		
		int lineCount = lines.Length;
		
		for(int n = 0; n<lineCount; ++n)
		{
			string line = lines[n];
			
			string [] words = line.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
			
			/* we don't care about these, or else they could be in the elseif
			if(words[0] == "info") //info face="Franchise Bold" size=160 bold=0 italic=0 charset="" unicode=0 stretchH=100 smooth=1 aa=1 padding=0,0,0,0 spacing=1,1
			{
				//do nothing
			}
			else if(words[0] == "page") //page id=0 file="FranchiseLarge"
			{
				//do nothing
			}
			*/
			
			if(words[0] == "common") //common lineHeight=168 base=26 scaleW=1024 scaleH=1024 pages=1 packed=0
			{
				//these are the height and width of the original atlas built by Hiero
				_configWidth = int.Parse(words[3].Split('=')[1]);
				//_configHeight = int.Parse(words[4].Split('=')[1]);
				
				//this is the ratio of the config vs the size of the actual texture element
				_configRatio = _element.sourceSize.x/_configWidth;
				
				_lineHeight = int.Parse(words[1].Split('=')[1]) * _configRatio;	
				//_lineBase = int.Parse(words[2].Split('=')[1]) * _configRatio;	
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
				
				for(int w = 1; w<wordCount; ++w)
				{
					string[] parts = words[w].Split('=');	
					string partName = parts[0];
					
					if(partName == "letter") 
					{
						if(parts[1].Length >= 3)
						{
							charInfo.letter = parts[1].Substring(1,1);
						} 
						continue; //we don't care about the letter	
					}
					
					if(partName == "\r") continue; //something weird happened with linebreaks, meh!
					
					int partValue = int.Parse(parts[1]);
						
					if(partName == "id")
					{
						charInfo.charID = partValue;
					}
					else if(partName == "x")
					{
						charInfo.x = partValue*_configRatio - _element.sourceRect.x; //offset to account for the trimmed atlas
					}
					else if(partName == "y")
					{
						charInfo.y = partValue*_configRatio - _element.sourceRect.y; //offset to account for the trimmed atlas
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
				}

				Rect uvRect = new Rect 	
				(
					_element.uvRect.x + charInfo.x/textureSize.x*resourceScale,
					(textureSize.y-charInfo.y-charInfo.height)/textureSize.y*resourceScale - (1.0f - _element.uvRect.yMax),
					charInfo.width/textureSize.x*resourceScale,
					charInfo.height/textureSize.y*resourceScale
				);
			
				charInfo.uvRect = uvRect;
				
				charInfo.uvTopLeft.Set(uvRect.xMin,uvRect.yMax);
				charInfo.uvTopRight.Set(uvRect.xMax,uvRect.yMax);
				charInfo.uvBottomRight.Set(uvRect.xMax,uvRect.yMin);
				charInfo.uvBottomLeft.Set(uvRect.xMin,uvRect.yMin);

				_charInfosByID[(uint)charInfo.charID] = charInfo;
				_charInfos[c] = charInfo;
				
				c++;
			}
			else if(words[0] == "kernings") //kernings count=169
			{
				wasKerningFound = true;
				int kerningCount = int.Parse(words[1].Split('=')[1]);
				_kerningInfos = new FKerningInfo[kerningCount+100]; //kerning count can be wrong so just add 100 items of potential fudge factor
			}
			else if(words[0] == "kerning") //kerning first=56  second=57  amount=-1
			{
				FKerningInfo kerningInfo = new FKerningInfo();
				
				kerningInfo.first = -1;
				
				wordCount = words.Length;
				
				for(int w = 1; w<wordCount; w++)
				{
					string[] parts = words[w].Split('=');	
					if(parts.Length >= 2)
					{
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
							kerningInfo.amount = partValue * _configRatio;
						}
					}
				}
				
				if(kerningInfo.first != -1)
				{
					_kerningInfos[k] = kerningInfo;
				}
				
				k++;
			}
			
		}
		
		_kerningCount = k;
		
		
		if(!wasKerningFound) //if there are no kernings at all (like in a pixel font), then make an empty kerning array
		{
			_kerningInfos = new FKerningInfo[0];	
		}
		
		//make sure the space character doesn't have offsetY and offsetX
		if(_charInfosByID[32] != null)
		{
			_charInfosByID[32].offsetX = 0;
			_charInfosByID[32].offsetY = 0;
		}
		
	}
	
	public FLetterQuadLine[] GetQuadInfoForText(string text, FTextParams textParams)
	{
		int lineCount = 0;
		int letterCount = 0;
		
		char[] letters = text.ToCharArray();
		
		//at some point these should probably be pooled and reused so we're not allocing new ones all the time
		FLetterQuadLine[] lines = new FLetterQuadLine[10];
		
		int lettersLength = letters.Length;
		for(int c = 0; c<lettersLength; ++c)
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
		 
		for(int c = 0; c<lineCount+1; ++c)
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
		
		float usableLineHeight = _lineHeight + textParams.scaledLineHeightOffset + _textParams.scaledLineHeightOffset;
		
		for(int c = 0; c<lettersLength; ++c)
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
				nextY -= usableLineHeight;
				
				lineCount++;
				letterCount = 0;
			}
			else 
			{
				FKerningInfo foundKerning = _nullKerning;
				
				for(int k = 0; k<_kerningCount; k++)
				{
					FKerningInfo kerningInfo = _kerningInfos[k];
					if(kerningInfo.first == previousLetter && kerningInfo.second == letter)
					{
						foundKerning = kerningInfo;
					}
				}
				
				//TODO: Reuse letterquads with pooling!
				FLetterQuad letterQuad = new FLetterQuad();
				
				if(_charInfosByID.ContainsKey(letter))
				{
					charInfo = _charInfosByID[letter];
				}
				else //we don't have that character in the font
				{
					//blank,  character (could consider using the "char not found square")
					charInfo = _charInfosByID[0];
				}
				
				float totalKern = foundKerning.amount + textParams.scaledKerningOffset + _textParams.scaledKerningOffset;

				nextX += totalKern; 
				
				letterQuad.charInfo = charInfo;
				
				Rect quadRect = new Rect(nextX + charInfo.offsetX, nextY - charInfo.offsetY - charInfo.height, charInfo.width, charInfo.height);
			
				letterQuad.rect = quadRect;
				
				lines[lineCount].quads[letterCount] = letterQuad;	
				
				
				
				minX = Math.Min (minX, quadRect.xMin);
				maxX = Math.Max (maxX, quadRect.xMax);
				maxY = Math.Max (maxY, nextY);
				
				minY = Math.Min (minY, nextY - usableLineHeight);

//				minY = Math.Min (minY, quadRect.yMin);
//				maxY = Math.Max (maxY, quadRect.yMax);
				
				nextX += charInfo.xadvance;

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
	
	public FTextParams textParams
	{
		get { return _textParams;}	
	}
	
	public float offsetX
	{
		get { return _offsetX;}	
	}
	
	public float offsetY
	{
		get { return _offsetY;}	
	}

//  Not gonna deal with this stuff unless it's actually needed
//  In theory it'll handle nice quotes
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


