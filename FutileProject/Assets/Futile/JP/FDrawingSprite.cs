using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Author @jpsarda
 * A drawing class.
 * 
 * Examples :
 * 
 
 		_draw=new FDrawingSprite("Blank");
		AddChild(_draw);
		
		_draw.SetLineThickness(10);
		_draw.SetLineColor(new Color(1,0,1,0.5f));
		_draw.SetLineCapStyle(FTDrawingCapStyle.ARROW);
		_draw.SetLineJointStyle(FTDrawingJointStyle.ROUND);
		
		_draw.MoveTo(50,50);
		_draw.LineTo(100,100);
		_draw.LineTo(120,80);
		_draw.LineTo(120,50);
		_draw.LineTo(170,50);
		_draw.Flush(); // Stop the line, add caps
		
		
		_draw.SetLineColor(new Color(0.5f,0.5f,1.0f,1.0f));
		_draw.SetLineCapStyle(FTDrawingCapStyle.ROUND);
		_draw.SetLineJointStyle(FTDrawingJointStyle.BEVEL);
		_draw.ClearBorders();
		_draw.MoveTo(-50,50);
		_draw.LineTo(-100,100);
		_draw.LineTo(-120,80);
		_draw.LineTo(-120,50);
		_draw.LineTo(-170,50);
		_draw.Flush(); 
		

		_draw.ClearBorders();
		_draw.PushBorder(5.0f,new Color(1.0f,0.5f,0.0f,1.0f),false);
		_draw.PushBorder(10.0f,new Color(0.0f,0.0f,0.0f,0.25f),true);
		_draw.SetLineColor(new Color(1,1,0,1.0f));
		_draw.MoveTo(40,0);
		for (int i=1;i<16;i++) {
			float angle=-2*Mathf.PI*(float)i/16.0f;
			_draw.LineTo (Mathf.Cos (angle)*40.0f,Mathf.Sin (angle)*40.0f);
		}
		_draw.Loop(); //Stop the line and loop with first line
 
 * 
 * 
 */

public enum FTDrawingCapStyle
{
	NONE,
	ROUND,
	SQUARE,
	TRIANGLE,
	ARROW
}

public enum FTDrawingJointStyle
{
	MITER,
	ROUND,
	BEVEL
}

public class FDrawingBorder
{
	public Color color;
	public float thickness;
	public bool gradient;
	public bool top,bottom;
	public FDrawingQuad topQuad,bottomQuad;
	
	public FDrawingBorder Clone() {
		return new FDrawingBorder(this);
	}
	
	public FDrawingBorder(float thickness,Color color, bool gradient, bool top, bool bottom):base()
	{
		this.thickness=thickness;
		this.gradient=gradient;
		this.color=color;
		this.top=top;
		this.bottom=bottom;
		topQuad=bottomQuad=null;
	}
	
	public FDrawingBorder(FDrawingBorder model):base()
	{
		Copy(model);
	}
	
	public void Copy(FDrawingBorder model) {
		thickness=model.thickness;
		gradient=model.gradient;
		color=model.color;
		top=model.top;
		bottom=model.bottom;
		topQuad=model.topQuad;
		bottomQuad=model.bottomQuad;
	}
}

public class FDrawingCursor
{
	public Vector2 position,lineFromPosition,direction;
	public bool lineFromValid;
	public Color color;
	public float thickness;
	public FTDrawingJointStyle jointType;
	public FTDrawingCapStyle capType;
	protected LinkedList<FDrawingQuad> _quads;
	//public FDrawingQuad topQuad,bottomQuad;
	public FDrawingQuad lineQuad;
	public List<FDrawingBorder> borders;
	
	public FDrawingCursor Clone() {
		return new FDrawingCursor(this);
	}
	
	public FDrawingCursor(LinkedList<FDrawingQuad> quads):base()
	{
		_quads=quads;
		lineFromValid=false;
		thickness=1.0f;
		jointType=FTDrawingJointStyle.BEVEL;
		capType=FTDrawingCapStyle.NONE;
		color=new Color(1,1,1,1);
		position=new Vector2(0,0);
		//topQuad=bottomQuad=null;
		lineQuad=null;
		borders=null;
	}
	
	public FDrawingCursor(FDrawingCursor model):base()
	{
		Copy(model);
	}
	
	public void Copy(FDrawingCursor model) {
		_quads=model.GetQuads();
		lineFromValid=model.lineFromValid;
		thickness=model.thickness;
		jointType=model.jointType;
		capType=model.capType;
		color=model.color;
		position=model.position;
		lineFromPosition=model.lineFromPosition;
		direction=model.direction;
		//topQuad=model.topQuad;
		//bottomQuad=model.bottomQuad;
		lineQuad=model.lineQuad;
		if (model.borders!=null) {
			borders=new List<FDrawingBorder>(model.borders.Count);
			int n=model.borders.Count;
			for (int i=0;i<n;i++) {
				FDrawingBorder border=model.borders[i];
				borders.Add(border.Clone());
			}
		} else {
			borders=null;
		}
	}
	
	public LinkedList<FDrawingQuad> GetQuads() {
		return _quads;
	}
	
	virtual public void MoveTo(float x,float y,FDrawingCursor firstCursor)
	{
		Flush (firstCursor);
		
		position.x=x;
		position.y=y;
	}
	
	virtual protected void AddLineSideBorders(FDrawingQuad topBaseQuad,FDrawingQuad bottomBaseQuad)
	{
		if (borders!=null) {
			Vector2 ortho=new Vector2(-direction.y,direction.x);
			FDrawingQuad quad;
			int n=borders.Count;
			for (int i=0;i<n;i++) {
				FDrawingBorder border = borders[i];
				//top
				if (border.top) {
					quad=new FDrawingQuad(border.color);
					quad.tlVertice=topBaseQuad.tlVertice+ortho*border.thickness;
					quad.trVertice=topBaseQuad.trVertice+ortho*border.thickness;
					quad.blVertice=topBaseQuad.tlVertice;
					quad.brVertice=topBaseQuad.trVertice;
					if (border.gradient) {
						quad.blColor=topBaseQuad.tlColor;
						quad.brColor=topBaseQuad.trColor;
					}
					_quads.AddLast(quad);
					border.topQuad=quad;
					topBaseQuad=quad;
				}
				
				//bottom
				if (border.bottom) {
					quad=new FDrawingQuad(border.color);
					quad.blVertice=bottomBaseQuad.blVertice-ortho*border.thickness;
					quad.brVertice=bottomBaseQuad.brVertice-ortho*border.thickness;
					quad.tlVertice=bottomBaseQuad.blVertice;
					quad.trVertice=bottomBaseQuad.brVertice;
					if (border.gradient) {
						quad.tlColor=bottomBaseQuad.blColor;
						quad.trColor=bottomBaseQuad.brColor;
					}
					_quads.AddLast(quad);
					border.bottomQuad=quad;
					bottomBaseQuad=quad;
				}
			}
		}
	}
	
	virtual public bool LineTo(float x,float y,FDrawingCursor previousCursor) {
		//Main line (no caps, caps are drawn on Flush, MoveTo)		
		FDrawingQuad quad;
		quad=new FDrawingQuad(color);
		if (!quad.SetLineVertices(position,new Vector2(x,y),thickness,this)) {
			return false;
		}
		AddLineSideBorders(quad,quad);
		_quads.AddLast(quad);
		lineQuad=quad;

		lineFromValid=true;
		lineFromPosition=position;
		position.x=x;
		position.y=y;

		if (previousCursor!=null) {
			DrawJoint(previousCursor);
		}
		return true;
	}
	
	virtual public void Flush(FDrawingCursor firstCursor)
	{
		if (lineFromValid) {
			DrawEndCap();
		}
		if (firstCursor!=null) {
			firstCursor.DrawStartCap();
		}
		lineFromValid=false;
	}
	
	virtual public void Loop(FDrawingCursor firstCursor,FDrawingCursor previousCursor)
	{
		if (firstCursor!=null) {
			LineTo (firstCursor.lineFromPosition.x,firstCursor.lineFromPosition.y,previousCursor);
			firstCursor.DrawJoint(this);
			lineFromValid=false;
		} else {
			Flush (firstCursor);
		}
	}
	
	protected static string supportedCapTypesWithBorders="Supported cap styles : FTDrawingCapStyle.NONE";
	protected static string supportedJointTypesWithBorders="Supported joint styles : FTDrawingJointStyle.BEVEL";
	
	virtual public void DrawEndCap() {
		//Draw ending cap
		if (capType==FTDrawingCapStyle.SQUARE) {
			FDrawingQuad quad=new FDrawingQuad(color);
			quad.SetLineVertices(position,position+direction*thickness*0.5f,thickness,null);
			_quads.AddLast(quad);
			if ((borders!=null)&&(borders.Count>0)) {
				throw new FutileException("Cap type "+capType+" not suported with borders. "+supportedCapTypesWithBorders);
			}
		} else if (capType==FTDrawingCapStyle.TRIANGLE) {
			FDrawingQuad quad=new FDrawingQuad(color);
			Vector2 ortho=new Vector2(-direction.y,direction.x);
			quad.tlVertice=position+ortho*thickness*0.5f;
			quad.blVertice=position-ortho*thickness*0.5f;
			quad.brVertice=position+direction*thickness*0.5f;
			quad.trVertice=quad.brVertice;
			_quads.AddLast(quad);
			if ((borders!=null)&&(borders.Count>0)) {
				throw new FutileException("Cap type "+capType+" not suported with borders. "+supportedCapTypesWithBorders);
			}
		} else if (capType==FTDrawingCapStyle.ARROW) {
			FDrawingQuad quad;
			Vector2 ortho=new Vector2(-direction.y,direction.x);
			if (color.a>=1) {
				quad=new FDrawingQuad(color);
				quad.tlVertice=position+ortho*thickness*1.0f-direction*thickness*0.5f;
				quad.blVertice=position-ortho*thickness*1.0f-direction*thickness*0.5f;
				quad.brVertice=position+direction*thickness*0.5f;
				quad.trVertice=quad.brVertice;
				_quads.AddLast(quad);
			} else {
				Vector2 A=position+direction*thickness*0.5f;

				quad=new FDrawingQuad(color);
				quad.tlVertice=position+ortho*thickness*0.5f;
				quad.blVertice=position+ortho*thickness*0.5f-direction*thickness*0.5f;
				quad.brVertice=position+ortho*thickness*1.0f-direction*thickness*0.5f;
				quad.trVertice=A;
				_quads.AddLast(quad);
				
				quad=new FDrawingQuad(color);
				quad.tlVertice=position-ortho*thickness*0.5f;
				quad.blVertice=position-ortho*thickness*0.5f-direction*thickness*0.5f;
				quad.brVertice=position-ortho*thickness*1.0f-direction*thickness*0.5f;
				quad.trVertice=A;
				_quads.AddLast(quad);
				
				quad=new FDrawingQuad(color);
				quad.tlVertice=position+ortho*thickness*0.5f;
				quad.blVertice=position-ortho*thickness*0.5f;
				quad.brVertice=A;
				quad.trVertice=A;
				_quads.AddLast(quad);
			}
			if ((borders!=null)&&(borders.Count>0)) {
				throw new FutileException("Cap type "+capType+" not suported with borders. "+supportedCapTypesWithBorders);
			}
		} else if (capType==FTDrawingCapStyle.ROUND) {
			Vector2 ortho=new Vector2(-direction.y,direction.x);
			int nbQuads=(int)(this.thickness*0.5f*Mathf.PI *0.5f *0.5f *0.5f)*2+2;
			float angle=0;
			//2 triangles by quads
			float deltaAngle=0.5f*Mathf.PI/nbQuads;
			for (int i=0;i<nbQuads;i++) {
				FDrawingQuad quad=new FDrawingQuad(color);
				quad.blVertice=position;
				quad.tlVertice=position+ortho*thickness*0.5f*Mathf.Cos(angle)+direction*thickness*0.5f*Mathf.Sin(angle);
				angle+=deltaAngle;
				quad.trVertice=position+ortho*thickness*0.5f*Mathf.Cos(angle)+direction*thickness*0.5f*Mathf.Sin(angle);
				angle+=deltaAngle;
				quad.brVertice=position+ortho*thickness*0.5f*Mathf.Cos(angle)+direction*thickness*0.5f*Mathf.Sin(angle);
				_quads.AddLast(quad);
			}
			if ((borders!=null)&&(borders.Count>0)) {
				throw new FutileException("Cap type "+capType+" not suported with borders. "+supportedCapTypesWithBorders);
			}
		}
	}
	
	virtual public void DrawStartCap() {
		//Draw starting cap
		if (capType==FTDrawingCapStyle.SQUARE) {
			FDrawingQuad quad=new FDrawingQuad(color);
			quad.SetLineVertices(lineFromPosition-direction*thickness*0.5f,lineFromPosition,thickness,null);
			_quads.AddLast(quad);
			
			if ((borders!=null)&&(borders.Count>0)) {
				throw new FutileException("Cap type "+capType+" not suported with borders. "+supportedCapTypesWithBorders);
			}
		} else if (capType==FTDrawingCapStyle.TRIANGLE) {
			FDrawingQuad quad=new FDrawingQuad(color);
			Vector2 ortho=new Vector2(-direction.y,direction.x);
			quad.trVertice=lineFromPosition+ortho*thickness*0.5f;
			quad.brVertice=lineFromPosition-ortho*thickness*0.5f;
			quad.blVertice=lineFromPosition-direction*thickness*0.5f;
			quad.tlVertice=lineFromPosition-direction*thickness*0.5f;
			_quads.AddLast(quad);
			if ((borders!=null)&&(borders.Count>0)) {
				throw new FutileException("Cap type "+capType+" not suported with borders. "+supportedCapTypesWithBorders);
			}
		} else if (capType==FTDrawingCapStyle.ARROW) {
			FDrawingQuad quad;
			Vector2 ortho=new Vector2(-direction.y,direction.x);
			if (color.a>=1) {
				quad=new FDrawingQuad(color);
				quad.trVertice=lineFromPosition+ortho*thickness*1.0f+direction*thickness*0.5f;
				quad.brVertice=lineFromPosition-ortho*thickness*1.0f+direction*thickness*0.5f;;
				quad.blVertice=lineFromPosition-direction*thickness*0.5f;
				quad.tlVertice=lineFromPosition-direction*thickness*0.5f;
				_quads.AddLast(quad);
			} else {
				Vector2 A=lineFromPosition-direction*thickness*0.5f;

				quad=new FDrawingQuad(color);
				quad.tlVertice=lineFromPosition+ortho*thickness*0.5f;
				quad.blVertice=lineFromPosition+ortho*thickness*0.5f+direction*thickness*0.5f;
				quad.brVertice=lineFromPosition+ortho*thickness*1.0f+direction*thickness*0.5f;
				quad.trVertice=A;
				_quads.AddLast(quad);
				
				quad=new FDrawingQuad(color);
				quad.tlVertice=lineFromPosition-ortho*thickness*0.5f;
				quad.blVertice=lineFromPosition-ortho*thickness*0.5f+direction*thickness*0.5f;
				quad.brVertice=lineFromPosition-ortho*thickness*1.0f+direction*thickness*0.5f;
				quad.trVertice=A;
				_quads.AddLast(quad);
				
				quad=new FDrawingQuad(color);
				quad.tlVertice=lineFromPosition+ortho*thickness*0.5f;
				quad.blVertice=lineFromPosition-ortho*thickness*0.5f;
				quad.brVertice=A;
				quad.trVertice=A;
				_quads.AddLast(quad);
			}
			if ((borders!=null)&&(borders.Count>0)) {
				throw new FutileException("Cap type "+capType+" not suported with borders. "+supportedCapTypesWithBorders);
			}
		} else if (capType==FTDrawingCapStyle.ROUND) {
			Vector2 ortho=new Vector2(-direction.y,direction.x);
			int nbQuads=(int)(this.thickness*0.5f*Mathf.PI *0.5f *0.5f *0.5f)*2+2;
			float angle=0;
			//2 triangles by quads
			float deltaAngle=0.5f*Mathf.PI/nbQuads;
			for (int i=0;i<nbQuads;i++) {
				FDrawingQuad quad=new FDrawingQuad(color);
				quad.trVertice=lineFromPosition;
				quad.brVertice=lineFromPosition-ortho*thickness*0.5f*Mathf.Cos(angle)-direction*thickness*0.5f*Mathf.Sin(angle);
				angle+=deltaAngle;
				quad.blVertice=lineFromPosition-ortho*thickness*0.5f*Mathf.Cos(angle)-direction*thickness*0.5f*Mathf.Sin(angle);
				angle+=deltaAngle;
				quad.tlVertice=lineFromPosition-ortho*thickness*0.5f*Mathf.Cos(angle)-direction*thickness*0.5f*Mathf.Sin(angle);
				_quads.AddLast(quad);
			}
			
			if ((borders!=null)&&(borders.Count>0)) {
				throw new FutileException("Cap type "+capType+" not suported with borders. "+supportedCapTypesWithBorders);
			}
		}
	}
	
	virtual protected void AddBorders (Vector2 leftPoint, Color leftColor, Vector2 leftDirectionNormalized, Vector2 rightPoint, Color rightColor, Vector2 rightDirectionNormalized, bool top) {
		if ((borders!=null)&&(borders.Count>0)) {
			int n=borders.Count;
			for (int i=0;i<n;i++) {
				FDrawingBorder border = borders[i];
				if ((border.top && top)||(border.bottom && !top)) {
					FDrawingQuad quad=new FDrawingQuad(border.color);
					quad.tlVertice=leftPoint+leftDirectionNormalized*border.thickness;
					quad.blVertice=leftPoint;
					quad.brVertice=rightPoint;
					quad.trVertice=rightPoint+rightDirectionNormalized*border.thickness;
					if (border.gradient) {
						quad.blColor=leftColor;
						quad.brColor=rightColor;
					}
					_quads.AddLast(quad);
					
					leftPoint=quad.tlVertice;
					leftColor=quad.tlColor;
					rightPoint=quad.trVertice;
					rightColor=quad.trColor;
				}
			}
		}
	}
	
	
	
	
	
	
	
	virtual public void DrawJoint(FDrawingCursor previousCursor) {
		Vector2 prevOrtho=new Vector2(-previousCursor.direction.y,previousCursor.direction.x);
		Vector2 ortho=new Vector2(-direction.y,direction.x);
		float dot=Vector2.Dot(prevOrtho,direction);
		
		
		//Cut inside of the turn
		bool medianDone=false;
		Vector2 median=Vector2.zero;
		if (borders!=null) {
			if (borders.Count>0) {
				median=prevOrtho+ortho;
				median.Normalize();
				medianDone=true;
				if (median==Vector2.zero) {
					//???
				} else {
					Vector2 A=lineFromPosition;
					Vector2 B=lineFromPosition+median;
					float S,T;
					bool ret;
					if (dot<0) { //bottomQuad
						int n=borders.Count;
						for (int i=0;i<n;i++) {
							FDrawingBorder border = borders[i];
							if (border.bottom) {
								FDrawingBorder previousBorder=previousCursor.borders[i];
	
								//Previous line
								//bottom line
								ret=VectorUtils.LinesIntersect(A,B,previousBorder.bottomQuad.blVertice,previousBorder.bottomQuad.brVertice,out S,out T);
								if ((ret)&&(T>=0)&&(T<=1)) {
									previousBorder.bottomQuad.brVertice = A + S * (B - A);
								} else {
									ret=VectorUtils.LinesIntersect(A,B,previousBorder.bottomQuad.tlVertice,previousBorder.bottomQuad.blVertice,out S,out T);
									if ((ret)&&(T>=0)&&(T<=1)) {
										previousBorder.bottomQuad.brVertice = A + S * (B - A);
										previousBorder.bottomQuad.blVertice = previousBorder.bottomQuad.brVertice;
									}
								}
								//topline
								ret=VectorUtils.LinesIntersect(A,B,previousBorder.bottomQuad.tlVertice,previousBorder.bottomQuad.trVertice,out S,out T);
								if ((ret)&&(T>=0)&&(T<=1)) {
									previousBorder.bottomQuad.trVertice = A + S * (B - A);
								}
								
								//Current line
								//bottom line
								ret=VectorUtils.LinesIntersect(A,B,border.bottomQuad.blVertice,border.bottomQuad.brVertice,out S,out T);
								if ((ret)&&(T>=0)&&(T<=1)) {
									border.bottomQuad.blVertice = A + S * (B - A);
								} else {
									ret=VectorUtils.LinesIntersect(A,B,border.bottomQuad.trVertice,border.bottomQuad.brVertice,out S,out T);
									if ((ret)&&(T>=0)&&(T<=1)) {
										border.bottomQuad.brVertice = A + S * (B - A);
										border.bottomQuad.blVertice = border.bottomQuad.brVertice;
									}
								}
								//topline
								ret=VectorUtils.LinesIntersect(A,B,border.bottomQuad.tlVertice,border.bottomQuad.trVertice,out S,out T);
								if ((ret)&&(T>=0)&&(T<=1)) {
									border.bottomQuad.tlVertice = A + S * (B - A);
								}
							}
							//i++;
						}
					} else if (dot>0) { //topQuad
						int n=borders.Count;
						for (int i=0;i<n;i++) {
							FDrawingBorder border = borders[i];
							if (border.top) {
								FDrawingBorder previousBorder=previousCursor.borders[i];
								//Previous line
								//top line
								ret=VectorUtils.LinesIntersect(A,B,previousBorder.topQuad.tlVertice,previousBorder.topQuad.trVertice,out S,out T);
								if ((ret)&&(T>=0)&&(T<=1)) {
									previousBorder.topQuad.trVertice = A + S * (B - A);
								} else {
									ret=VectorUtils.LinesIntersect(A,B,previousBorder.topQuad.tlVertice,previousBorder.topQuad.blVertice,out S,out T);
									if ((ret)&&(T>=0)&&(T<=1)) {
										previousBorder.topQuad.trVertice = A + S * (B - A);
										previousBorder.topQuad.tlVertice = previousBorder.topQuad.trVertice;
									}
								}
								//bottom line
								ret=VectorUtils.LinesIntersect(A,B,previousBorder.topQuad.blVertice,previousBorder.topQuad.brVertice,out S,out T);
								if ((ret)&&(T>=0)&&(T<=1)) {
									previousBorder.topQuad.brVertice = A + S * (B - A);
								}
								
								//Current line
								//top line
								ret=VectorUtils.LinesIntersect(A,B,border.topQuad.tlVertice,border.topQuad.trVertice,out S,out T);
								if ((ret)&&(T>=0)&&(T<=1)) {
									border.topQuad.tlVertice = A + S * (B - A);
								} else {
									ret=VectorUtils.LinesIntersect(A,B,border.topQuad.trVertice,border.topQuad.trVertice,out S,out T);
									if ((ret)&&(T>=0)&&(T<=1)) {
										border.topQuad.trVertice = A + S * (B - A);
										border.topQuad.tlVertice = border.topQuad.trVertice;
									}
								}
								//bottom line
								ret=VectorUtils.LinesIntersect(A,B,border.topQuad.blVertice,border.topQuad.brVertice,out S,out T);
								if ((ret)&&(T>=0)&&(T<=1)) {
									border.topQuad.blVertice = A + S * (B - A);
								}
							}
							//i++;
						}
					}
				}
			}
		}
		
		Vector2 center=lineFromPosition;
		
		//if ((color.a<1)&&(previousCursor.color.a<1)) { //not necessary for solid colors but removing overlapping mught help performances anyway
		if (!medianDone) {
			median=prevOrtho+ortho;
			median.Normalize();
			medianDone=true;
		}
		if (median==Vector2.zero) {
			//???
		} else {
			Vector2 A=lineFromPosition;
			Vector2 B=lineFromPosition+median;
			float S,T;
			bool ret;
			if (dot<0) { //bottomQuad
				//Previous line
				ret=VectorUtils.LinesIntersect(A,B,previousCursor.lineQuad.blVertice,previousCursor.lineQuad.brVertice,out S,out T);
				if ((ret)&&(T>=0)&&(T<=1)) {
					center=previousCursor.lineQuad.brVertice = A + S * (B - A);
				}
				//Current line
				ret=VectorUtils.LinesIntersect(A,B,lineQuad.blVertice,lineQuad.brVertice,out S,out T);
				if ((ret)&&(T>=0)&&(T<=1)) {
					center=lineQuad.blVertice = A + S * (B - A);
				}
			} else if (dot>0) { //topQuad
				//Previous line
				ret=VectorUtils.LinesIntersect(A,B,previousCursor.lineQuad.tlVertice,previousCursor.lineQuad.trVertice,out S,out T);
				if ((ret)&&(T>=0)&&(T<=1)) {
					center=previousCursor.lineQuad.trVertice = A + S * (B - A);
				}
				//Current line
				ret=VectorUtils.LinesIntersect(A,B,lineQuad.tlVertice,lineQuad.trVertice,out S,out T);
				if ((ret)&&(T>=0)&&(T<=1)) {
					center=lineQuad.tlVertice = A + S * (B - A);
				}
			}
		}
		//}

		//Draw joint
		if (jointType==FTDrawingJointStyle.BEVEL) {
			if (dot<0) {
				FDrawingQuad quad=new FDrawingQuad(this.color);
				quad.tlVertice=previousCursor.position+prevOrtho*previousCursor.thickness*0.5f;
				quad.blVertice=center;
				quad.brVertice=center;
				quad.trVertice=lineFromPosition+ortho*thickness*0.5f;
				_quads.AddLast(quad);
				
				AddBorders (quad.tlVertice,quad.tlColor,prevOrtho,quad.trVertice,quad.trColor,ortho,true);
			} else if (dot>0) {
				FDrawingQuad quad=new FDrawingQuad(this.color);
				quad.blVertice=previousCursor.position-prevOrtho*previousCursor.thickness*0.5f;
				quad.tlVertice=center;
				quad.trVertice=center;
				quad.brVertice=lineFromPosition-ortho*thickness*0.5f;
				_quads.AddLast(quad);
				
				AddBorders (quad.blVertice,quad.blColor,-prevOrtho,quad.brVertice,quad.brColor,-ortho,false);
			} else {
				// What else?
				// Parallel lines, no joint necessary
			}
		} else if (jointType==FTDrawingJointStyle.ROUND) {
			if (dot<0) {
				float angleDiff=Mathf.Abs(Mathf.Acos(Vector2.Dot(previousCursor.direction,direction)));
				int nbQuads=(int)(this.thickness*0.5f*angleDiff *0.5f *0.5f *0.5f)*2+2;
				
				float angle=0;
				//2 triangles by quads
				float deltaAngle=0.5f*angleDiff/nbQuads;
				for (int i=0;i<nbQuads;i++) {
					FDrawingQuad quad=new FDrawingQuad(color);
					//FDrawingQuad quad=new FDrawingQuad(new Color(1,(float)i/(float)nbQuads,(float)i/(float)nbQuads,1));
					quad.trVertice=center;
					quad.brVertice=previousCursor.position+prevOrtho*thickness*0.5f*Mathf.Cos(angle)+previousCursor.direction*thickness*0.5f*Mathf.Sin(angle);
					angle+=deltaAngle;
					quad.blVertice=previousCursor.position+prevOrtho*thickness*0.5f*Mathf.Cos(angle)+previousCursor.direction*thickness*0.5f*Mathf.Sin(angle);
					angle+=deltaAngle;
					quad.tlVertice=previousCursor.position+prevOrtho*thickness*0.5f*Mathf.Cos(angle)+previousCursor.direction*thickness*0.5f*Mathf.Sin(angle);
					_quads.AddLast(quad);
				}
			} else if (dot>0) {
				float angleDiff=Mathf.Abs(Mathf.Acos(Vector2.Dot(previousCursor.direction,direction)));
				int nbQuads=(int)(this.thickness*0.5f*angleDiff *0.5f *0.5f *0.5f)*2+2;
				
				float angle=0;
				//2 triangles by quads
				float deltaAngle=0.5f*angleDiff/nbQuads;
				for (int i=0;i<nbQuads;i++) {
					FDrawingQuad quad=new FDrawingQuad(color);
					//FDrawingQuad quad=new FDrawingQuad(new Color(1,(float)i/(float)nbQuads,(float)i/(float)nbQuads,1));
					quad.trVertice=center;
					quad.brVertice=previousCursor.position-prevOrtho*thickness*0.5f*Mathf.Cos(-angle)+previousCursor.direction*thickness*0.5f*Mathf.Sin(-angle);
					angle-=deltaAngle;
					quad.blVertice=previousCursor.position-prevOrtho*thickness*0.5f*Mathf.Cos(-angle)+previousCursor.direction*thickness*0.5f*Mathf.Sin(-angle);
					angle-=deltaAngle;
					quad.tlVertice=previousCursor.position-prevOrtho*thickness*0.5f*Mathf.Cos(-angle)+previousCursor.direction*thickness*0.5f*Mathf.Sin(-angle);
					_quads.AddLast(quad);
				}
			} else {
				// What else?
				// Parallel lines, no joint necessary
			}
			if ((borders!=null)&&(borders.Count>0)) {
				throw new FutileException("Joint type "+jointType+" not suported with borders. "+supportedJointTypesWithBorders);
			}
		} else if (jointType==FTDrawingJointStyle.MITER) {
			if (dot<0) {
				FDrawingQuad quad=new FDrawingQuad(this.color);
				quad.tlVertice=previousCursor.position+prevOrtho*previousCursor.thickness*0.5f;
				quad.blVertice=center;
				quad.brVertice=lineFromPosition+ortho*thickness*0.5f;
				bool valid;
				quad.trVertice=VectorUtils.LinesIntersectPoint(quad.tlVertice, quad.tlVertice+previousCursor.direction, quad.brVertice, quad.brVertice-direction, out valid);
				if (!valid) {
					quad.trVertice=quad.brVertice;
				}
				_quads.AddLast(quad);
			} else if (dot>0) {
				FDrawingQuad quad=new FDrawingQuad(this.color);
				quad.blVertice=previousCursor.position-prevOrtho*previousCursor.thickness*0.5f;
				quad.tlVertice=center;
				quad.trVertice=lineFromPosition-ortho*thickness*0.5f;
				bool valid;
				quad.brVertice=VectorUtils.LinesIntersectPoint(quad.blVertice, quad.blVertice-previousCursor.direction, quad.trVertice, quad.trVertice+direction, out valid);
				if (!valid) {
					quad.brVertice=quad.trVertice;
				}
				_quads.AddLast(quad);
				
			} else {
				// What else?
				// Parallel lines, no joint necessary
			}
			if ((borders!=null)&&(borders.Count>0)) {
				throw new FutileException("Joint type "+jointType+" not suported with borders. "+supportedJointTypesWithBorders);
			}
		}
	}
	
	virtual public void PushBorder(float thickness, Color color, bool gradient, bool top, bool bottom) {
		if (borders==null) {
			borders=new List<FDrawingBorder>();
		}
		borders.Add (new FDrawingBorder(thickness, color, gradient, top, bottom));
	}
	
	virtual public bool PopBorder() {
		if (borders!=null) {
			if (borders.Count>0) {
				borders.RemoveAt(borders.Count-1);
				return true;
			}
		}
		return false;
	}
	
	virtual public bool ClearBorders() {
		if (borders!=null) {
			if (borders.Count>0) {
				borders.Clear();
				return true;
			}
		}
		return false;
	}
}

public class FDrawingQuad
{
	public Vector2 tlVertice,blVertice,brVertice,trVertice;
	public Color tlColor,blColor,brColor,trColor;

	public FDrawingQuad(Color color):base()
	{
		tlColor=blColor=brColor=trColor=color;
	}
	
	public bool SetLineVertices(Vector2 fromPosition,Vector2 toPosition,float thickness, FDrawingCursor cursor)
	{
		Vector2 direction=toPosition-fromPosition;
		float dist=Mathf.Sqrt(Vector2.SqrMagnitude(direction));
		if (dist<0.5f) {
			return false;
		}
		direction/=dist;
		Vector2 ortho=new Vector2(-direction.y,direction.x);
		tlVertice=fromPosition+ortho*thickness*0.5f;
		blVertice=fromPosition-ortho*thickness*0.5f;
		
		trVertice=toPosition+ortho*thickness*0.5f;
		brVertice=toPosition-ortho*thickness*0.5f;
		
		if (cursor!=null) cursor.direction=direction;
		return true;
	}
}

public class FDrawingSprite : FSprite
{
	protected FDrawingCursor _cursor,_firstLineCursor,_previousLineCursor;
	protected LinkedList<FDrawingQuad> _quads;
	
	public FDrawingSprite() : base(Futile.whiteElement)
	{
	}

	public FDrawingSprite (string elementName) : this(Futile.atlasManager.GetElementWithName(elementName))
	{
	}
	
	public FDrawingSprite (FAtlasElement element) : base()
	{
		_quads=new LinkedList<FDrawingQuad>();
		_cursor=new FDrawingCursor(_quads);
		_firstLineCursor=null;
		_previousLineCursor=null;
		
		Init(FFacetType.Quad, element,0); //this will call HandleElementChanged(), which will call Setup();
		
		_isAlphaDirty = true;
		
		UpdateLocalVertices();
	}
	
	override public void HandleElementChanged()
	{
		Setup();
	}

	virtual public void Setup ()
	{	
		_areLocalVerticesDirty = true;
		
		_numberOfFacetsNeeded=16;
		if(_isOnStage) _stage.HandleFacetsChanged();
	}
	
	private void CheckNeedForMoreFacets() {
		while (_quads.Count>_numberOfFacetsNeeded) {
			_numberOfFacetsNeeded*=2;
			if(_isOnStage) _stage.HandleFacetsChanged();
		}
	}
	
	override public void UpdateLocalVertices()
	{
		_areLocalVerticesDirty = false;
	} 
	
	override public void PopulateRenderLayer()
	{
		if(_isOnStage && _firstFacetIndex != -1) 
		{
			_isMeshDirty = false;
			
			int vertexIndex0=_firstFacetIndex*4;
			foreach (FDrawingQuad quad in _quads) {
				int vertexIndex1 = vertexIndex0 + 1;
				int vertexIndex2 = vertexIndex0 + 2;
				int vertexIndex3 = vertexIndex0 + 3;
				
				Vector3[] vertices = _renderLayer.vertices;
				Vector2[] uvs = _renderLayer.uvs;
				Color[] colors = _renderLayer.colors;
				
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0], quad.tlVertice,0);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex1], quad.blVertice,0);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex2], quad.brVertice,0);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex3], quad.trVertice,0);
				
				uvs[vertexIndex0] = _element.uvTopLeft;
				uvs[vertexIndex1] = _element.uvTopRight;
				uvs[vertexIndex2] = _element.uvBottomRight;
				uvs[vertexIndex3] = _element.uvBottomLeft;
				
				if (_concatenatedAlpha<1f) {
					colors[vertexIndex0] = quad.tlColor.CloneWithMultipliedAlpha(_concatenatedAlpha);
					colors[vertexIndex1] = quad.blColor.CloneWithMultipliedAlpha(_concatenatedAlpha);
					colors[vertexIndex2] = quad.brColor.CloneWithMultipliedAlpha(_concatenatedAlpha);
					colors[vertexIndex3] = quad.trColor.CloneWithMultipliedAlpha(_concatenatedAlpha);
				} else {
					colors[vertexIndex0] = quad.tlColor;
					colors[vertexIndex1] = quad.blColor;
					colors[vertexIndex2] = quad.brColor;
					colors[vertexIndex3] = quad.trColor;
				}
				
				vertexIndex0+=4;
				
				_renderLayer.HandleVertsChange();
			}

			Vector3 dummyVector3=new Vector3(1000000,100000,100000);
			Color dummyColor=new Color(0,0,0,0);
			for (int i=_quads.Count;i<_numberOfFacetsNeeded;i++) {
				int vertexIndex1 = vertexIndex0 + 1;
				int vertexIndex2 = vertexIndex0 + 2;
				int vertexIndex3 = vertexIndex0 + 3;
				
				Vector3[] vertices = _renderLayer.vertices;
				Vector2[] uvs = _renderLayer.uvs;
				Color[] colors = _renderLayer.colors;

				vertices[vertexIndex0]=vertices[vertexIndex1]=vertices[vertexIndex2]=vertices[vertexIndex3]=dummyVector3;

				uvs[vertexIndex0] = uvs[vertexIndex1] = uvs[vertexIndex2] = uvs[vertexIndex3] = _element.uvBottomLeft;
				
				colors[vertexIndex0] = colors[vertexIndex1] = colors[vertexIndex2] = colors[vertexIndex3] = dummyColor;

				vertexIndex0+=4;
				
				_renderLayer.HandleVertsChange();
			}
		}
	}
	
	virtual public void Clear()
	{
		_quads=new LinkedList<FDrawingQuad>();
		_cursor=new FDrawingCursor(_quads);
		_firstLineCursor=null;
		_previousLineCursor=null;
		
		//Init(FFacetType.Quad, element,0); //this will call HandleElementChanged(), which will call Setup();
		
		_isAlphaDirty = true;
		
		UpdateLocalVertices();
		Setup();
	}

	virtual public void MoveTo(float x,float y)
	{
		_cursor.MoveTo(x,y,_firstLineCursor);
		_firstLineCursor=null;
		_previousLineCursor=null;
		CheckNeedForMoreFacets();
		_isMeshDirty=true;
	}
	
	virtual public void LineTo(float x,float y)
	{
		if (_cursor.LineTo(x,y,_previousLineCursor)) {
			CheckNeedForMoreFacets();
			if (_previousLineCursor==null) {
				_previousLineCursor=_cursor.Clone();
			} else {
				_previousLineCursor.Copy(_cursor);
			}
			if (_firstLineCursor==null) {
				_firstLineCursor=_cursor.Clone();
			}
			_isMeshDirty=true;
		}
	}
	
	
	virtual public void Loop()
	{
		_cursor.Loop(_firstLineCursor,_previousLineCursor);
		_firstLineCursor=null;
		_previousLineCursor=null;
		CheckNeedForMoreFacets();
		_isMeshDirty=true;
	}
	
	virtual public void Flush()
	{
		_cursor.Flush(_firstLineCursor);
		_firstLineCursor=null;
		_previousLineCursor=null;
		CheckNeedForMoreFacets();
		_isMeshDirty=true;
	}
	
	
	virtual public void SetLineColor(Color color,float thickness,FTDrawingJointStyle jointType,FTDrawingCapStyle capType)
	{
		Flush();
		_cursor.color=color;
		_cursor.thickness=thickness;
		_cursor.jointType=jointType;
		_cursor.capType=capType;
	}
	
	virtual public void SetLineColor(Color color)
	{
		Flush();
		_cursor.color=color;
	}
	
	virtual public void SetLineThickness(float thickness)
	{
		Flush();
		_cursor.thickness=thickness;
	}
	
	virtual public void SetLineJointStyle(FTDrawingJointStyle jointType)
	{
		Flush();
		_cursor.jointType=jointType;
	}
	
	virtual public void SetLineCapStyle(FTDrawingCapStyle capType)
	{
		Flush();
		_cursor.capType=capType;
	}
	
	
	virtual public void PushBorder(float thickness, Color color, bool gradient)
	{
		Flush ();
		_cursor.PushBorder(thickness, color, gradient,true, true);
	}
	virtual public void PushTopBorder(float thickness, Color color, bool gradient) {
		Flush ();
		_cursor.PushBorder(thickness,color,gradient,true,false);
	}
	
	virtual public void PushBottomBorder(float thickness, Color color, bool gradient) {
		Flush ();
		_cursor.PushBorder(thickness,color,gradient,false,true);
	}
	
	virtual public void ClearBorders()
	{
		if (_cursor.ClearBorders()) {
			Flush ();
		}
	}
	
	virtual public void PopBorder()
	{
		if (_cursor.PopBorder()) {
			Flush ();
		}
	}
	
	
	public Vector2 GetCursorPosition() {
		return _cursor.position;
	}
}