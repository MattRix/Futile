using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Reflection;




/*

Pseudo Html renderer. Can render text, fsprites, fbuttons, and can be extended.
 
Example : 


FPseudoHtmlText text=new FPseudoHtmlText(Config.fontFile,"<style text-scale='1.25'>FPseudoHtmlText</style><br/><style text-scale='0.75'>With FPseudoHtmlText you get a PSeudo HTML renderer for Futile. The pseudo html code is rendered within a limited width (word cut), can be centered, left or right aligned. You can change <style text-color='#FF99FF'>text colors or <style text-alpha='0.5'>alpha or <style text-scale='1.25'>size with nested styles</style></style><br/>But that's not all, you can also display FSprites <fsprite src='diamant'/> and also <fbutton src='yes' label='FButtons' scale='0.5' label-scale='0.5' color-down='#FF0000' action='MyMethodNameWithData' data='mybutttonid'/>. It's can be used for popups or credits pages for example. It's easy to add your own tags to instantiate your favorite Futile nodes (FSliceButton, FSliceSprite...).</style>",Config.textParams,400f,PseudoHtmlTextAlign.left,1f,this);
AddChild(text);

Which gives this (sorry for the bad english and typos) : http://s21.postimg.org/dygtaqe53/Screen_Shot_2013_08_20_at_8_29_07_PM.png

*/





public enum PseudoHtmlTextAlign : int
{
	left = 0,
	center = 1,
	right = 2,
}


public class LinePiece {
	public FNode node;
	public float height,width;
	public LinePiece(FNode node_,float width_,float height_) {
		node=node_;
		width=width_;
		height=height_;
	}
}


public class FPseudoHtmlText : FContainer
{
	protected string _fontName;
	protected string _text;
	protected FTextParams _textParams;
	protected PseudoHtmlTextAlign _align;
	protected float _maxWidth, _width, _height;
	protected FContainer _contentContainer;
	protected float _lineOffset;
	protected object _actionsDelegate;
	protected Dictionary<FButton,string> _buttonActions;
	
	
	
	//styles and tags
	protected List<Dictionary<string,string>> _stylesStack;
	
	
	
	public FPseudoHtmlText (string fontName, string text, FTextParams textParams, float maxWidth, PseudoHtmlTextAlign align, float lineOffset,object actionsDelegate)
	{
		_fontName = fontName;
		_text = text.Replace("\n"," ");
		_textParams = textParams;
		_maxWidth = maxWidth;
		_align=align;
		_lineOffset=lineOffset;
		_actionsDelegate=actionsDelegate;
		
		_stylesStack=new List<Dictionary<string, string>>();
		PushDefaultStyles();
		
		_buttonActions=new Dictionary<FButton, string>();
		
		_contentContainer = new FContainer ();
		AddChild (_contentContainer);
		
		Update ();
	}
	
	public float maxWidth { get { return _maxWidth; } }
	public float width { get { return _width; } }
	public float height { get { return _height; } }

	
	public void SetText (string text)
	{
		_text = text.Replace("\n"," ");
		Update ();
	}
	
	public void SetTextAndWidth (string text,float maxWidth)
	{
		_text = text.Replace("\n"," ");
		_maxWidth=maxWidth;
		Update ();
	}
	
	virtual protected void PushDefaultStyles() {
		Dictionary<string, string> defaultStyles=new Dictionary<string, string>()
		{
    		{ "text-color", "#FFFFFF" }, 
    		{ "text-alpha", "1" },
			{ "text-scale", "1" },
		};
		_stylesStack.Add (defaultStyles);
	}
	
	protected int _pos;
	protected List<LinePiece> _line;
	protected string _textNotRendered;
	protected float _parsingY;
	
	protected void Update ()
	{
		_buttonActions.Clear();
		_contentContainer.RemoveAllChildren ();
		
		_pos=0;
		_line=null;
		_textNotRendered=null;
		_parsingY=0;
		
		bool lineSplit=false; //set to true when, for example, a style is changed and requires the line to be split in several pieces
		HtmlTag tag;
	    HtmlParser parse = new HtmlParser(_text);
	    while (parse.ParseNext("*", true, out tag)) {
			//Debug.Log ("found tag = ["+_text.Substring (tag.startPos, tag.endPos - tag.startPos)+"] tag.TrailingSlash="+tag.TrailingSlash+" tag.Name="+tag.Name);

			Dictionary<string, string> styleToPush=null;
			bool styleToPop=false;

			if (!tag.Closing) {
				if (!tag.TrailingSlash) {
					if (tag.Name.Equals("style")) {
						//Push new style, break line
						styleToPush=new Dictionary<string, string>(_stylesStack.Last());
						
						//_stylesStack.Add(style);

						foreach (KeyValuePair<string, string> pair in tag.Attributes)
						{
						    string defaultValue;
							styleToPush.TryGetValue(pair.Key,out defaultValue);
							if (defaultValue!=null) {
								if (!defaultValue.Equals(pair.Value)) {
									styleToPush.Remove(pair.Key);
									styleToPush.Add(pair.Key,pair.Value);
									lineSplit=true;
								}
							} else {
								styleToPush.Add(pair.Key,pair.Value);
								lineSplit=true;
							}
						}
					}
				}
			} else {
				if (tag.Name.Equals("style")) {
					styleToPop=true;
					//_stylesStack.Pop();
					lineSplit=true;
				}
			}
			

			
			Render(tag.startPos,lineSplit);
			lineSplit=false;
			
			if (styleToPush!=null) {
				_stylesStack.Add(styleToPush);
			} else {
				if (styleToPop) {
					_stylesStack.Pop();
				}
			}
			
			if (tag.TrailingSlash) {
				if (tag.Name.Equals("br")) {
					//Debug.Log ("br");
					if (_textNotRendered==null) _textNotRendered="";
					RenderPiece(_textNotRendered); _textNotRendered=null;
					RenderLine();
				} else if (tag.Name.Equals("fsprite")) {
					//Create a FSprite
					string val=null;
					StringAttributeParam("src","fsprite",tag.Attributes,ref val);
					if (val!=null) {
						FSprite sprite=new FSprite(val);
						ApplyStyles(sprite,tag.Attributes);
						RenderPiece(sprite);
					}
				} else if (tag.Name.Equals("fbutton")) {
					//Create a FButton
					string up=null;
					StringAttributeParam("src","fbutton",tag.Attributes,ref up);
					if (up!=null) {
						string down=up;
						StringAttributeParam("down","fbutton",tag.Attributes,ref down);

						string over=null;
						StringAttributeParam("over","fbutton",tag.Attributes,ref over);
						
						string sound=null;
						StringAttributeParam("sound","fbutton",tag.Attributes,ref sound);
							
						FButton button=new FButton(up,down,over,sound);
						
						ApplyStyles(button,tag.Attributes);
						RenderPiece(button);
					}
				}
			}
			
			
			_pos=tag.endPos; //skipping tag
	    }
		Render(_text.Length,lineSplit);
		lineSplit=false; //useless, but that's how I do things
		
		//Last line?
		RenderPiece(_textNotRendered); _textNotRendered=null;
		RenderLine();
		
		//center the text (positio 0,0 is in the moddile of the text box)
		_contentContainer.y=-_parsingY*0.5f;
		_width=_maxWidth;
		_height=-_parsingY;
	}

	protected float LineWidth() {
		float ret=0f;
		if (_line!=null) {
			foreach (LinePiece piece in _line) {
				ret+=piece.width;
			}
		}
		return ret;
	}
	protected float LineHeight() {
		float ret=0f;
		if (_line!=null) {
			foreach (LinePiece piece in _line) {
				if (piece.height>ret) ret=piece.height;
			}
		}
		return ret;
	}
	
	protected void RenderLine() {
		RenderLine(_line);
		_line=null;
	}
	
	protected void RenderLine(List<LinePiece> line) {
		if (line!=null) {
			
			float totWidth=LineWidth();
			float maxHeight=LineHeight();
			
			float parseX=0;
			
			if (_align==PseudoHtmlTextAlign.left) {
				parseX=-_maxWidth*0.5f;
			} else if (_align==PseudoHtmlTextAlign.center) {
				parseX=-totWidth*0.5f;
			} else if (_align==PseudoHtmlTextAlign.right) {
				parseX=_maxWidth*0.5f-totWidth;
			}
			
			_parsingY-=(maxHeight+_lineOffset)*0.5f;
			
			foreach (LinePiece piece in line) {
				_contentContainer.AddChild(piece.node);
				parseX+=piece.width*0.5f;
				piece.node.x=(float)Math.Round(parseX);
				piece.node.y=(float)Math.Round(_parsingY);
				
				parseX+=piece.width*0.5f;
			}
			
			_parsingY-=(maxHeight+_lineOffset)*0.5f;
		}
	}
	
	
	
	
	
	
	//Convertions from string to XXX
	protected float SizeAttribute(string val) {
		//TODO : allow "px"
		return float.Parse(val);
	}
	protected float WidthAttribute(string val) {
		if (val.EndsWith("%")) {
			return _maxWidth*float.Parse(val.Substring(0,val.Length-1))/100f;
		} else {
			return SizeAttribute(val);
		}
	}
	protected Color ColorAttribute(string val) {
		int idxPrefix=val.IndexOf("#");
		if (idxPrefix>=0) {
			return RXUtils.GetColorFromHex(val.Substring(idxPrefix+1));
		}
		return Futile.white;
	}
	
	
	
	
	
	
	
	
	protected void SetNodeScale(FNode node, Vector2 originalSize , Dictionary<string,string> attributes) {
		node.scale=1;
		
		float width=-1f,height=-1f,maxWidth=-1f,maxHeight=-1f;
		string val;
		
		attributes.TryGetValue("max-width",out val);
		if (val!=null) {
			maxWidth=WidthAttribute(val);
		}
		attributes.TryGetValue("width",out val);
		if (val!=null) {
			width=WidthAttribute(val);
		}
		
		attributes.TryGetValue("max-height",out val);
		if (val!=null) {
			maxHeight=SizeAttribute(val);
		}
		attributes.TryGetValue("height",out val);
		if (val!=null) {
			height=SizeAttribute(val);
		}
		
		//Debug.Log ("width="+width+" maxWidth="+maxWidth+" height="+height+" maxHeight="+maxHeight);
		
		if ((width>maxWidth)&&(maxWidth>=0)) width=maxWidth;
		if ((height>maxHeight)&&(maxHeight>=0)) height=maxHeight;
		
		//Debug.Log ("width="+width+" maxWidth="+maxWidth+" height="+height+" maxHeight="+maxHeight);
		
		if (width>=0) {
			if (height<0) {
				height=width*originalSize.y/originalSize.x;
			}
		} else {
			if (height>=0) {
				width=height*originalSize.x/originalSize.y;
			}
		}
		
		//Debug.Log ("width="+width+" maxWidth="+maxWidth+" height="+height+" maxHeight="+maxHeight);
		
		if (width>=0) {
			node.scaleX=width/originalSize.x;
		}
		if (height>=0) {
			node.scaleY=height/originalSize.y;
		}
	}
	
	
	
	protected void StringAttributeParam(string paramName,string stylePrefix,Dictionary<string,string> attributes, ref string retVal) {
		string val;
		attributes.TryGetValue(paramName,out val);
		if (val!=null) {
			retVal=val;
		} else {
			_stylesStack.Last().TryGetValue(stylePrefix+"-"+paramName,out val);
			if (val!=null) {
				retVal=val;
			}
		}
	}
	protected void FloatAttributeParam(string paramName,string stylePrefix,Dictionary<string,string> attributes, ref float retVal) {
		string val;
		attributes.TryGetValue(paramName,out val);
		if (val!=null) {
			retVal=float.Parse(val);
		} else {
			_stylesStack.Last().TryGetValue(stylePrefix+"-"+paramName,out val);
			if (val!=null) {
				retVal=float.Parse(val);
			}
		}
	}
	protected void ColorAttributeParam(string paramName,string stylePrefix,Dictionary<string,string> attributes, ref Color retVal) {
		string val;
		attributes.TryGetValue(paramName,out val);
		if (val!=null) {
			retVal=ColorAttribute(val);
		} else {
			_stylesStack.Last().TryGetValue(stylePrefix+"-"+paramName,out val);
			if (val!=null) {
				retVal=ColorAttribute(val);
			}
		}
	}

	
	protected void ApplyStyles(FButton button,Dictionary<string,string> attributes) {
		string val;
		float fVal;

		SetNodeScale(button,new Vector2(button.hitRect.width,button.hitRect.height),attributes);
		
		fVal=1f;
		FloatAttributeParam("scale","fbutton",attributes,ref fVal);
		button.scaleX*=fVal;
		button.scaleY*=fVal;
		
		fVal=1f;
		FloatAttributeParam("alpha","fbutton",attributes,ref fVal);
		button.alpha=1f;

		Color upColor=Futile.white;
		Color downColor=Futile.white;
		ColorAttributeParam("color-up","fbutton",attributes,ref upColor);
		ColorAttributeParam("color-down","fbutton",attributes,ref downColor);
		button.SetColors(upColor,downColor);
		
		val=null;
		StringAttributeParam("label","fbutton",attributes,ref val);
		if (val!=null) {
			Color labelColor=Futile.white;
			ColorAttributeParam("label-color","fbutton",attributes,ref labelColor);
			button.AddLabel(_fontName,val,labelColor);
			
			fVal=-1f;
			FloatAttributeParam("label-scale","fbutton",attributes,ref fVal);
			if (fVal>=0f) {
				button.label.scaleX*=fVal/button.scaleX;
				button.label.scaleY*=fVal/button.scaleY;
			}
			button.label.x=(float)(Math.Round(button.label.x));
			button.label.y=(float)(Math.Round(button.label.y));
		}
		
		val=null;
		StringAttributeParam("data","fbutton",attributes,ref val);
		button.data=val;

		
		//action
		string action=null;
		StringAttributeParam("action","fbutton",attributes,ref action);
		if (action!=null) {
			if (_actionsDelegate!=null) {
				_buttonActions.Add (button,action);
				button.SignalRelease+=HandleButtonAction;
			} else {
				Debug.LogWarning("FPseudoHtmlText : fbutton created with an \"action\" attribute but actionsDelegate is null.");
			}
		}
	}
	
	protected void HandleButtonAction(FButton button) {
		if (_actionsDelegate!=null) {
			string action;
			_buttonActions.TryGetValue(button,out action);
			if (action!=null) {
				Type delegateType = _actionsDelegate.GetType();
				MethodInfo theMethod = delegateType.GetMethod(action);
				object[] methodParams = new object[]{button.data};
				
				//Debug.Log ("button.data="+button.data+" theMethod="+theMethod);
				/*
				if (button.data!=null) {
					methodParams = new object[]{button.data};
				} else {
					Debug.Log ("button.data=null");
					methodParams = new object[]{};
				}
				*/
				theMethod.Invoke(_actionsDelegate, methodParams);
			}
		}
	}

	protected void ApplyStyles(FSprite sprite,Dictionary<string,string> attributes) {
		string val;
		float fVal;
		Color cVal;

		SetNodeScale(sprite,new Vector2(sprite.textureRect.width,sprite.textureRect.height),attributes);
		
		fVal=1f;
		FloatAttributeParam("scale","fsprite",attributes,ref fVal);
		sprite.scaleX*=fVal;
		sprite.scaleY*=fVal;
		
		fVal=1f;
		FloatAttributeParam("alpha","fsprite",attributes,ref fVal);
		sprite.alpha=1f;
		
		cVal=Futile.white;
		ColorAttributeParam("color","fsprite",attributes,ref cVal);
		sprite.color=cVal;
		
		val=null;
		StringAttributeParam("data","fsprite",attributes,ref val);
		sprite.data=val;
	}
	
	protected void ApplyStyles(FLabel label) {
		Dictionary<string, string> lastStyle=_stylesStack.Last();
		
		string val;
		
		//scale
		lastStyle.TryGetValue("text-scale",out val);
		if (val!=null) {
			float fVal=float.Parse(val);
			label.scale=fVal;
		}
		
		//alpha
		lastStyle.TryGetValue("text-alpha",out val);
		if (val!=null) {
			float fVal=float.Parse(val);
			label.alpha=fVal;
		}
		
		//color
		lastStyle.TryGetValue("text-color",out val);
		if (val!=null) {
			label.color=ColorAttribute(val);
		}
		
		//Debug.Log ("ApplyStyes ["+label.text+"] scale="+label.scale);
	}
	
	protected void RenderPiece(FButton button) {
		LinePiece piece=new LinePiece(button,button.hitRect.width*button.scaleX,button.hitRect.height*button.scaleY);
		RenderPiece (piece);
	}
	
	protected void RenderPiece(FSprite sprite) {
		LinePiece piece=new LinePiece(sprite,sprite.textureRect.width*sprite.scaleX,sprite.textureRect.height*sprite.scaleY);
		RenderPiece (piece);
	}
	
	protected void RenderPiece(LinePiece piece) {
		RenderPiece(_textNotRendered); _textNotRendered=null;
		if (_line==null) {
			_line=new List<LinePiece>();
		}
		float lineWidth=LineWidth();
		if (piece.width+lineWidth>_maxWidth) {
			RenderLine();
		}
		if (_line==null) {
			_line=new List<LinePiece>();
		}
		_line.Add(piece);
	}
	
	protected void RenderPiece(string text) {
		if (text==null) return;
		//if (text.Length==0) return;
		
		FLabel label=new FLabel(_fontName,text+" "); //because FLabel rip the last space only, here we want the last space to take some real space

		ApplyStyles(label);
		
		if (_line==null) {
			_line=new List<LinePiece>();
		}
		_line.Add(new LinePiece(label,label.textRect.width*label.scaleX,(label.textRect.height+_textParams.lineHeightOffset)*label.scaleY));
	}
	
	protected void Render(int toPos, bool split) {
		if (toPos>_pos) {
			//Debug.Log ("Render toPos="+toPos+" split="+split);
			
			string toRender=_text.Substring (_pos, toPos - _pos);
			
			if (_textNotRendered==null) _textNotRendered=toRender;
			else _textNotRendered+=toRender;
			
			float lineWidth=LineWidth();

			while (true) {
				int bestPos=0;
				int bestBadPos=_textNotRendered.Length;
				int endPos=bestBadPos;
				
				FLabel label=new FLabel(_fontName,_textNotRendered);
				ApplyStyles(label);

				//dichotomy
				while (true) {
					//Debug.Log("dicho label.textRect.width="+label.textRect.width+" bestPos="+bestPos+" endPos="+endPos+" bestBadPos="+bestBadPos);
					if (lineWidth+label.textRect.width*label.scaleX>_maxWidth) {
						if (bestBadPos>endPos) {
							bestBadPos=endPos;
						}
						if (bestBadPos<=bestPos+1) {
							break;
						}
						endPos=(int)((bestPos+bestBadPos)/2);
					} else {
						if (bestPos<endPos) {
							bestPos=endPos;
						}
						if (bestPos>=bestBadPos-1) {
							break;
						}
						endPos=(int)((bestPos+bestBadPos)/2);
					}
					label.text=_textNotRendered.Substring (0, endPos);
				}
				
				//Debug.Log ("bestPos="+bestPos+" _textNotRendered.Length="+_textNotRendered.Length+" _textNotRendered=["+_textNotRendered+"]");
				
				if ((bestPos==0)&&(lineWidth==0)) {
					if (_textNotRendered.Length>=1) {
						bestPos=1;
					}
				}
				
				//We got our bestPos
				if (bestPos>=_textNotRendered.Length) {
					//wait for new characters to render
					break;
				} else {
					//word cut
					bool spacefound=true;
					int cutPos=_textNotRendered.LastIndexOf(" ", bestPos/*+1*/); //in case the following char is a space
					if (cutPos<0) {
						//already some pieces in the line? if so don't render this big word
						if (lineWidth<=0) {
							//no pieces in the line
							//cut in the middle of the (too big) word
							cutPos=bestPos; 
							spacefound=false;
						}
					}
					
					if (cutPos>=0) {
						RenderPiece(_textNotRendered.Substring (0, cutPos));
						
						if (spacefound) cutPos++;
						if (_textNotRendered.Length>cutPos) {
							_textNotRendered=_textNotRendered.Substring (cutPos, _textNotRendered.Length-(cutPos));
						} else {
							_textNotRendered=null; //empty new line
							break;
						}
					}
					
					RenderLine();
					lineWidth=LineWidth();
				}
			}
			
			if (split) {
				//render remaining piece because style will change
				RenderPiece(_textNotRendered); _textNotRendered=null;
			}

			_pos=toPos;
		}
	}
}



//HTML generic Parser. Could be used with other things than FPseudoHtmlText
// a slightly modified version of this excellent parser http://www.codeproject.com/Articles/57176/Parsing-HTML-Tags-in-C
// mainly modified to allow returning closing tags



public class HtmlTag
{
	// Name of this tag
	public string Name { get; set; }

	// Collection of attribute names and values for this tag
	public Dictionary<string, string> Attributes { get; set; }

	// True if this tag contained a trailing forward slash
	public bool TrailingSlash { get; set; }
	
	// True is it's a closing tag (example </tag>)
	public bool Closing { get; set; }
	
	// start and end pos of the tag in the string
	public int startPos { get; set; }
	public int endPos { get; set; }
};

public class HtmlParser
{
	protected string _html;
	protected int _pos;
	//protected bool _scriptBegin;

	public HtmlParser (string html)
	{
		Reset (html);
	}
		
	public int Pos { get { return _pos;} }

	// Resets the current position to the start of the current document
	public void Reset ()
	{
		_pos = 0;
	}

	// Sets the current document and resets the current position to the start of it
	public void Reset (string html)
	{
		_html = html;
		_pos = 0;
	}

	// Indicates if the current position is at the end of the current document
	public bool EOF {
		get { return (_pos >= _html.Length); }
	}

	// Parses the next tag that matches the specified tag name
	// name=Name of the tags to parse ("*" = parse all tags)
	// if closingTagsToo is true, it will return the matching closing tags (exemple </tag>)
	// returns true if a tag was parsed or false if the end of the document was reached
	public bool ParseNext (string name, bool closingTagsToo, out HtmlTag tag)
	{
		tag = null;

		// Nothing to do if no tag specified
		if (String.IsNullOrEmpty (name))
			return false;

		// Loop until match is found or there are no more tags
		while (MoveToNextTag()) {
			// Skip opening '<'
			Move ();

			// Examine first tag character
			char c = Peek ();
			if (c == '!' && Peek (1) == '-' && Peek (2) == '-') {
				// Skip over comments
				const string endComment = "-->";
				_pos = _html.IndexOf (endComment, _pos);
				NormalizePosition ();
				Move (endComment.Length);
			} else if (c == '/') {
				if (closingTagsToo) {
					Move ();
					bool result = ParseTag (name, true, ref tag);
					// Return true if requested tag was found
					if (result)
						return true;
				} else {
					// Skip over closing tags
					_pos = _html.IndexOf ('>', _pos);
					NormalizePosition ();
					Move ();
				}
			} else {
				// Parse tag
				bool result = ParseTag(name, false, ref tag);

				// Because scripts may contain tag characters,
				// we need special handling to skip over
				// script contents
				/*
				if (_scriptBegin) {
					const string endScript = "</script";
					_pos = _html.IndexOf (endScript, _pos, StringComparison.OrdinalIgnoreCase);
					NormalizePosition ();
					Move (endScript.Length);
					SkipWhitespace ();
					if (Peek () == '>')
						Move ();
				}
				*/

				// Return true if requested tag was found
				if (result)
					return true;
			}
		}
		return false;
	}

	// Parses the contents of an HTML tag. The current position should be at the first character following the tag's opening less-than character.
	protected bool ParseTag (string name, bool closing, ref HtmlTag tag)
	{
		int startPos=_pos-1; // "<X...."
		if (closing) {
			startPos--; // "</X...."
		}
		
		// Get name of this tag
		string s = ParseTagName();

		// Special handling
		bool doctype = false;
		if (String.Compare (s, "!DOCTYPE", true) == 0)
			doctype = true;
		/*
		bool doctype = _scriptBegin = false;
		if (String.Compare (s, "!DOCTYPE", true) == 0)
			doctype = true;
		else if (String.Compare (s, "script", true) == 0)
			_scriptBegin = true;
		*/

		// Is this a tag requested by caller?
		bool requested = false;
		if (name == "*" || String.Compare (s, name, true) == 0) {
			// Yes, create new tag object
			tag = new HtmlTag ();
			tag.Name = s;
			tag.Attributes = new Dictionary<string, string> ();
			tag.Closing=closing;
			tag.startPos=startPos;
			requested = true;
		}

		// Parse attributes
		SkipWhitespace ();
		while (Peek() != '>') {
			if (Peek () == '/') {
				// Handle trailing forward slash
				if (requested)
					tag.TrailingSlash = true;
				Move ();
				SkipWhitespace ();
				// If this is a script tag, it was closed
				//_scriptBegin = false;
			} else {
				// Parse attribute name
				s = (!doctype) ? ParseAttributeName () : ParseAttributeValue ();
				SkipWhitespace ();
				// Parse attribute value
				string value = String.Empty;
				if (Peek () == '=') {
					Move ();
					SkipWhitespace ();
					value = ParseAttributeValue ();
					SkipWhitespace ();
				}
				// Add attribute to collection if requested tag
				if (requested) {
					// This tag replaces existing tags with same name
					if (tag.Attributes.Keys.Contains (s))
						tag.Attributes.Remove (s);
					tag.Attributes.Add (s, value);
				}
			}
		}
		// Skip over closing '>'
		Move ();
		tag.endPos=_pos;

		return requested;
	}

	// Parses a tag name. The current position should be the first character of the name
	protected string ParseTagName()
	{
		int start = _pos;
		while (!EOF && !Char.IsWhiteSpace(Peek()) && Peek() != '>' && Peek() != '/')
			Move ();
		return _html.Substring (start, _pos - start);
	}

	// Parses an attribute name. The current position should be the first character of the name
	protected string ParseAttributeName ()
	{
		int start = _pos;
		while (!EOF && !Char.IsWhiteSpace(Peek()) && Peek() != '>'
        && Peek() != '=')
			Move ();
		return _html.Substring (start, _pos - start);
	}

	// Parses an attribute value. The current position should be the first non-whitespace character following the equal sign.
	// Note: We terminate the name or value if we encounter a new line.
	// This seems to be the best way of handling errors such as values missing closing quotes, etc.
	protected string ParseAttributeValue ()
	{
		int start, end;
		char c = Peek ();
		if (c == '"' || c == '\'') {
			// Move past opening quote
			Move ();
			// Parse quoted value
			start = _pos;
			_pos = _html.IndexOfAny (new char[] { c, '\r', '\n' }, start);
			NormalizePosition ();
			end = _pos;
			// Move past closing quote
			if (Peek () == c)
				Move ();
		} else {
			// Parse unquoted value
			start = _pos;
			while (!EOF && !Char.IsWhiteSpace(c) && c != '>') {
				Move ();
				c = Peek ();
			}
			end = _pos;
		}
		return _html.Substring (start, end - start);
	}

	// Moves to the start of the next tag
	// returns true if another tag was found, false otherwise

	protected bool MoveToNextTag ()
	{
		_pos = _html.IndexOf ('<', _pos);
		NormalizePosition ();
		return !EOF;
	}

	// Returns the character at the current position, or a null character if we're at the end of the document.
	public char Peek ()
	{
		return Peek (0);
	}

	// Returns the character at the specified number of characters beyond the current position, or a null character if the specified position is at the end of the document
	public char Peek (int ahead)
	{
		int pos = (_pos + ahead);
		if (pos < _html.Length)
			return _html [pos];
		return (char)0;
	}

	// Moves the current position ahead one character
	protected void Move ()
	{
		Move (1);
	}

	// Moves the current position ahead the specified number of characters
	protected void Move (int ahead)
	{
		_pos = Math.Min (_pos + ahead, _html.Length);
	}

	// Moves the current position to the next character that is not whitespace
	protected void SkipWhitespace ()
	{
		while (!EOF && Char.IsWhiteSpace(Peek()))
			Move ();
	}

	// Normalizes the current position. This is primarily for handling conditions where IndexOf(), etc. return negative values when the item being sought was not found
	protected void NormalizePosition ()
	{
		if (_pos < 0)
			_pos = _html.Length;
	}
}