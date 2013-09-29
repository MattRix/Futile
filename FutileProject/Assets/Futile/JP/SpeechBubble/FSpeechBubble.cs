using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Reflection;

public class FSpeechBubble : FContainer
{
	protected float _width,_height,_cornerRadius=10f;
	protected int _cornerSegmentsCount;
	protected float _pointerBaseSize=10f;
	protected float _minSize,_bigCornerRadius;
	protected Vector2 _point;
	protected float _pointerMargin=10f;
	
	//draw
	//protected Color _borderColor=Color.white;
	protected WTPolygonSprite _polygonSprite;
	protected FDrawingSprite _borderSprite=null;
	
	public FSpeechBubble ()
	{
		UpdateIntermediateValues();
		_polygonSprite=new WTPolygonSprite(null);
		AddChild(_polygonSprite);
		_borderSprite=new FDrawingSprite("Futile_White");
		AddChild(_borderSprite);
	}
	
	public void SetSizeAndPointer(float width,float height,Vector2 point,float pointerMargin) {
		_width=width;
		_height=height;
		if (_width<_minSize) _width=_minSize;
		if (_height<_minSize) _height=_minSize;
		_point=point;
		_pointerMargin=pointerMargin;
		Update();
	}
	
	public Color backgroundColor {
		get { return _polygonSprite.color;}
		set {
			if (value!=_polygonSprite.color) {
				_polygonSprite.color=value;
			}
		}
	}
	
	public float width { get { return _width; } }
	public float height { get { return _height; } }
	
	protected void Update() {
		float halfWidth=_width*0.5f;
		float halfHeight=_height*0.5f;
		//Is point outside of the rectangle?
		bool outside=true;
		if ((_point.x-_pointerMargin<=halfWidth)&&(_point.x+_pointerMargin>=-halfWidth)) {
			if ((_point.y-_pointerMargin<=halfHeight)&&(_point.y+_pointerMargin>=-halfHeight)) {
				Debug.LogWarning("FSpeechBubble, the point is inisde the bubble, not supported.");
				outside=false;
				//return;
			}
		}
		
		
		//All clockwise		
		
		Vector2[] segmentTop = new Vector2[] {new Vector2(-halfWidth+_cornerRadius, halfHeight),new Vector2(halfWidth-_cornerRadius, halfHeight)};
		Vector2[] segmentBottom = new Vector2[] {new Vector2(halfWidth-_cornerRadius, -halfHeight),new Vector2(-halfWidth+_cornerRadius, -halfHeight)};
		Vector2[] segmentLeft = new Vector2[] {new Vector2(-halfWidth, -halfHeight+_cornerRadius),new Vector2(-halfWidth, halfHeight-_cornerRadius)};
		Vector2[] segmentRight = new Vector2[] {new Vector2(halfWidth, halfHeight-_cornerRadius),new Vector2(halfWidth, -halfHeight+_cornerRadius)};
		
		
		Vector2[] cornerTopLeft=new Vector2[_cornerSegmentsCount+1],cornerTopRight=new Vector2[_cornerSegmentsCount+1],cornerBottomRight=new Vector2[_cornerSegmentsCount+1],cornerBottomLeft=new Vector2[_cornerSegmentsCount+1];
		
		
		for (int i=0;i<=_cornerSegmentsCount;i++) {
			float angle=(float)Math.PI*0.5f*(float)i/(float)_cornerSegmentsCount;
			cornerTopLeft[i]=new Vector2(-_cornerRadius*(float)Math.Cos(angle),_cornerRadius*(float)Math.Sin(angle));
			cornerBottomRight[i]=cornerTopLeft[i]*(-1);
		}
		
		for (int i=0;i<=_cornerSegmentsCount;i++) {
			cornerTopRight[i]=new Vector2(-cornerTopLeft[_cornerSegmentsCount-i].x,cornerTopLeft[_cornerSegmentsCount-i].y);
			cornerBottomLeft[i]=cornerTopRight[i]*(-1);
		}
		
		Vector2 centerCornerTopLeft=new Vector2(-halfWidth+_cornerRadius,halfHeight-_cornerRadius);
		Vector2 centerCornerTopRight=new Vector2(halfWidth-_cornerRadius,halfHeight-_cornerRadius);
		Vector2 centerCornerBottomRight=new Vector2(halfWidth-_cornerRadius,-halfHeight+_cornerRadius);
		Vector2 centerCornerBottomLeft=new Vector2(-halfWidth+_cornerRadius,-halfHeight+_cornerRadius);
		for (int i=0;i<=_cornerSegmentsCount;i++) {
			cornerTopLeft[i]+=centerCornerTopLeft;
			cornerTopRight[i]+=centerCornerTopRight;
			cornerBottomRight[i]+=centerCornerBottomRight;
			cornerBottomLeft[i]+=centerCornerBottomLeft;
		}
		
		if (outside) {

			//Which part of the rectangle is the point? (8 zones)
			if ((_point.x>=-halfWidth+_bigCornerRadius) && (_point.x<=halfWidth-_bigCornerRadius)) {
				//top and bottom zones
				if (_point.y<0) {
					//bottom
					segmentBottom=new Vector2[] {segmentBottom[0],new Vector2(_point.x+_pointerBaseSize*0.5f,segmentBottom[0].y),_point+new Vector2(0,_pointerMargin),new Vector2(_point.x-_pointerBaseSize*0.5f,segmentBottom[0].y),segmentBottom[1]};
				} else {
					//top
					segmentTop=new Vector2[] {segmentTop[0],new Vector2(_point.x-_pointerBaseSize*0.5f,segmentTop[0].y),_point-new Vector2(0,_pointerMargin),new Vector2(_point.x+_pointerBaseSize*0.5f,segmentTop[0].y),segmentTop[1]};
				}
			} else if ((_point.y>=-halfHeight+_bigCornerRadius) && (_point.y<=halfHeight-_bigCornerRadius)) {
				//left and right zones
				if (_point.x<0) {
					//left
					segmentLeft=new Vector2[] {segmentLeft[0],new Vector2(segmentLeft[0].x,_point.y-_pointerBaseSize*0.5f),_point+new Vector2(_pointerMargin,0),new Vector2(segmentLeft[0].x,_point.y+_pointerBaseSize*0.5f),segmentLeft[1]};
				} else {
					//right
					segmentRight=new Vector2[] {segmentRight[0],new Vector2(segmentRight[0].x,_point.y+_pointerBaseSize*0.5f),_point-new Vector2(_pointerMargin,0),new Vector2(segmentRight[0].x,_point.y-_pointerBaseSize*0.5f),segmentRight[1]};
				}
			} else {
				//corners
				Vector2 aim;
				if (_point.x<0) {
					if (_point.y<0) {
						//bottom-left corner
						aim=new Vector2(-halfWidth+_bigCornerRadius,-halfHeight+_bigCornerRadius);
						InsertPointer(aim,ref segmentBottom,ref cornerBottomLeft,ref segmentLeft);
					} else {
						//top-left corner
						aim=new Vector2(-halfWidth+_bigCornerRadius,halfHeight-_bigCornerRadius);
						InsertPointer(aim,ref segmentLeft,ref cornerTopLeft,ref segmentTop);
					}
				} else {
					if (_point.y<0) {
						//bottom-right corner
						aim=new Vector2(halfWidth-_bigCornerRadius,-halfHeight+_bigCornerRadius);
						InsertPointer(aim,ref segmentRight,ref cornerBottomRight,ref segmentBottom);
					} else {
						//top-right corner
						aim=new Vector2(halfWidth-_bigCornerRadius,halfHeight-_bigCornerRadius);
						InsertPointer(aim,ref segmentTop,ref cornerTopRight,ref segmentRight);
					}
				}
				//find intersection point-aim with the bubble
			}
		
		}
		
		//Link all paths together
		Vector2[][] paths=new Vector2[][] { segmentTop,cornerTopRight,segmentRight,cornerBottomRight,segmentBottom,cornerBottomLeft,segmentLeft,cornerTopLeft };
		
		int verticesCount=0;
		foreach (Vector2[] path in paths) {
			verticesCount+=path.Length-1;
		}
		
		Vector2[] vertices = new Vector2[verticesCount];
		int j=0;
		//int l=0;
		foreach (Vector2[] path in paths) {
			for (int k=1;k<path.Length;k++) { //skip first point that is redundant with last point of previous path (could also skip last point, same result)
				vertices[j]=path[k]; j++;
			}
			
			/*
			Debug.Log("Path >>> "+l); l++;
			for (int i = 0; i < path.Length; i++) {
				Vector2 v = path[i];
				Debug.Log("Vertex " + i + ": " + v);
			}
			*/
		}
		
		_polygonSprite.UpdateWithData(new WTPolygonData(vertices));
		
		//log
		/*
		Debug.Log("Total path >>> "+l);
		Vector2[] originalVertices = _polygonSprite.polygonData.polygonVertices;
		for (int i = 0; i < originalVertices.Length; i++) {
			Vector2 v = originalVertices[i];
			Debug.Log("Vertex " + i + ": " + v);
		}
		*/
		
		_borderSprite.Clear();
		_borderSprite.SetLineJointStyle(FTDrawingJointStyle.BEVEL);
		_borderSprite.SetLineThickness(1.5f);
		_borderSprite.SetLineColor(new Color(0,0,0,1.0f));
		_borderSprite.PushTopBorder(1.5f,new Color(1.0f,1.0f,1.0f,0.0f),true);
		_borderSprite.PushBottomBorder(1.5f,new Color(0.0f,0.0f,0.0f,0.0f),true);
		Vector2[] originalVertices = _polygonSprite.polygonData.polygonVertices;
		_borderSprite.MoveTo(originalVertices[0].x,originalVertices[0].y);
		for (int i = 1; i < originalVertices.Length; i++) {
			Vector2 v = originalVertices[i];
			_borderSprite.LineTo(v.x,v.y);
		}
		_borderSprite.Loop();
	}
	
	protected bool FindClosestIntersection(Vector2 aim, Vector2[] path, out Vector2 intersection, out int lowerIdx) {
		Vector2 diff=aim-_point;
		
		intersection=Vector2.zero;
		lowerIdx=-1;
		float minDist=-1f;
		for (int i=1;i<path.Length;i++) {
			Vector2 seg0=path[i-1];
			Vector2 seg1=path[i];

			float S, T;
	
			if ( VectorUtils.LinesIntersect(seg0, seg1, _point, aim, out S, out T ) ) {
				if (S >= 0.0f && S <= 1.0f && T >= 0.0f /*&& T <= 1.0f*/) { //don't check T<=1 because the intersection go on after aim
					Vector2 foundIntersection=_point+diff*T;
					float dist=(foundIntersection-_point).magnitude;
					if ((lowerIdx<0)||(dist<minDist)) {
						minDist=dist;
						lowerIdx=i-1;
						intersection=foundIntersection;
					}
				}
			}
		}
		
		return (lowerIdx>=0);
	}
	
	protected bool FindClosestIntersection(Vector2 aim, Vector2[][] paths, out Vector2 intersection, out int pathIdx, out int segmentLowerIdx) {
		//Vector2 diff=aim-_point;
		
		intersection=Vector2.zero;
		pathIdx=-1;
		segmentLowerIdx=-1;
		float minDist=-1f;
		
		for (int i=0;i<paths.Length;i++) {
			Vector2[] path = paths[i];
			Vector2 foundIntersection;
			int foundSegmentLowerIdx;
			bool ret=FindClosestIntersection(aim,path,out foundIntersection,out foundSegmentLowerIdx);
			if (ret) {
				float dist=(foundIntersection-_point).magnitude;
				if ((pathIdx<0)||(dist<minDist)) {
					minDist=dist;
					pathIdx=i;
					segmentLowerIdx=foundSegmentLowerIdx;
					intersection=foundIntersection;
				}
			}
		}
		return (pathIdx>=0);
	}
	
	protected bool InsertPointer(Vector2 aim, ref Vector2[] path0, ref Vector2[] path1, ref Vector2[] path2) {
		Vector2 diff=aim-_point;
		
		Vector2[][] paths=new Vector2[][] {path0,path1,path2};
		Vector2 foundIntersection;
		int foundPathIdx;
		int foundSegmentLowerIdx;
		bool ret=FindClosestIntersection(aim,paths,out foundIntersection,out foundPathIdx, out foundSegmentLowerIdx);
		if (ret) {
			Vector2 dir=diff.normalized;
			Vector2 orth=new Vector2(-dir.y,dir.x);
			
			Vector2 point0=foundIntersection-orth*_pointerBaseSize*0.5f;
			Vector2 point1=foundIntersection+orth*_pointerBaseSize*0.5f;
			/*
			Vector2 diff0=point0-_point;
			point0=_point+diff0+(diff0.normalized*_pointerBaseSize*2);
			Vector2 diff1=point1-_point;
			point1=_point+diff1+(diff1.normalized*_pointerBaseSize*2);
			*/
			
			//Take into acount the pointerMargin
			Vector2 correctedPoint=_point+dir*_pointerMargin;
			

			//find intersections with point0 and point1
			
			Vector2 foundIntersection0;
			int foundPathIdx0;
			int foundSegmentLowerIdx0;
			bool ret0=FindClosestIntersection(point0,paths,out foundIntersection0,out foundPathIdx0, out foundSegmentLowerIdx0);
			if (ret0) {
				Vector2 foundIntersection1;
				int foundPathIdx1;
				int foundSegmentLowerIdx1;
				bool ret1=FindClosestIntersection(point1,paths,out foundIntersection1,out foundPathIdx1, out foundSegmentLowerIdx1);
				if (ret1) {
					if (foundPathIdx0<foundPathIdx1) {
						int i;
						Vector2[] path;
						
						path=paths[foundPathIdx0];
						int pathPointsCount0=foundSegmentLowerIdx0+3;
						Vector2[] newPath0=new Vector2[pathPointsCount0];
						i=0;
						for (;i<pathPointsCount0-2;i++) {
							newPath0[i]=path[i];
						}
						newPath0[i]=foundIntersection0; i++;
						newPath0[i]=correctedPoint; i++;
						
						path=paths[foundPathIdx1];
						int pathPointsCount1=path.Length-foundSegmentLowerIdx1+1;
						Vector2[] newPath1=new Vector2[pathPointsCount1];
						i=0;
						newPath1[i]=correctedPoint; i++;
						newPath1[i]=foundIntersection1; i++;
						for (int j=foundSegmentLowerIdx1+1;j<path.Length;j++) {
							newPath1[i]=path[j]; i++;
						}
						
						if (foundPathIdx0==0) {
							path0=newPath0;
							path1=newPath1;
						} else { //foundPathIdx0==1
							path1=newPath0;
							path2=newPath1;
						}
						return true;
					} else if (foundPathIdx0==foundPathIdx1) {
						if (foundSegmentLowerIdx0<=foundSegmentLowerIdx1) {
							int i;
							Vector2[] path=paths[foundPathIdx0];
							int pathPointsCount=path.Length-(foundSegmentLowerIdx1-foundSegmentLowerIdx0)+3;
							
							Vector2[] newPath=new Vector2[pathPointsCount];
							i=0;
							for (int j=0;j<=foundSegmentLowerIdx0;j++) {
								newPath[i]=path[j]; i++;
							}
							newPath[i]=foundIntersection0; i++;
							newPath[i]=correctedPoint; i++;
							newPath[i]=foundIntersection1; i++;
							for (int j=foundSegmentLowerIdx1+1;j<path.Length;j++) {
								newPath[i]=path[j]; i++;
							}
							if (foundPathIdx0==0) {
								path0=newPath;
							} else if (foundPathIdx0==1) {
								path1=newPath;
							} else { //foundPathIdx0==2
								path2=newPath;
							}
						} 
						//else if (foundSegmentLowerIdx0==foundSegmentLowerIdx1) {
						//}
					}
				}
			}
		}
		return false;
	}
	
	//To call when _cornerRadius, or _pointerBaseSize changed
	protected void UpdateIntermediateValues() {
		_minSize=2*_cornerRadius+_pointerBaseSize;
		_bigCornerRadius=_cornerRadius+_pointerBaseSize*0.5f;
		_cornerSegmentsCount=(int)(_cornerRadius/2); if (_cornerSegmentsCount<4) _cornerSegmentsCount=4;
	}
}


