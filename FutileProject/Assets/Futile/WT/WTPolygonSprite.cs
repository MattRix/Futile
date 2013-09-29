using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WTPolygonSprite : FSprite
{
 	public WTPolygonData polygonData {get; private set;}

	private int _triangleCount; 

	private Vector2 _uvTopLeft;
	private Vector2 _uvBottomLeft;
	private Vector2 _uvBottomRight;

	public WTPolygonSprite (WTPolygonData polygonData) {
		_element = Futile.atlasManager.GetElementWithName("Futile_White");
		_isAlphaDirty = true;

		UpdateWithData(polygonData);
	}

	// Call this method if you want to change the points of the polygon after it already exists.
	public void UpdateWithData(WTPolygonData newData) {
		this.polygonData = newData;

		if (polygonData != null) {
			RefreshVertices();

			_triangleCount = this.polygonData.polygonTriangles.Length / 3;

			Init(FFacetType.Triangle, _element, _triangleCount);

			_isMatrixDirty = true;
			_isAlphaDirty = true;
			_areLocalVerticesDirty = true;

			Redraw(true, false);
		}
	}

	override public void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		bool wasMatrixDirty = _isMatrixDirty;
		bool wasAlphaDirty = _isAlphaDirty;

		UpdateDepthMatrixAlpha(shouldForceDirty, shouldUpdateDepth);

		if(shouldUpdateDepth)
		{
			UpdateFacets();
		}

		if(wasMatrixDirty || shouldForceDirty || shouldUpdateDepth)
		{
			_isMeshDirty = true;
		}

		if(wasAlphaDirty || shouldForceDirty)
		{
			_isMeshDirty = true;
			_color.ApplyMultipliedAlpha(ref _alphaColor, _concatenatedAlpha);	
		}

		if(_areLocalVerticesDirty)
		{
			UpdateLocalVertices();
		}

		if(_isMeshDirty) 
		{
			PopulateRenderLayer();
		}
	}

	override public void HandleElementChanged()
	{
		_areLocalVerticesDirty = true;
	}

  // This is being overridden because if the vertice data gets changed for
  // some reason, it needs to update some things.
	override public void UpdateLocalVertices()
	{
		_areLocalVerticesDirty = false;

		_uvTopLeft = _element.uvTopLeft;
		_uvBottomLeft = _element.uvBottomLeft;
		_uvBottomRight = _element.uvBottomRight;

		int[] trianglePolygons = polygonData.polygonTriangles;

		_triangleCount = trianglePolygons.Length / 3;	

		if(_numberOfFacetsNeeded != _triangleCount)
		{
			_numberOfFacetsNeeded = _triangleCount;
			if(_isOnStage) _stage.HandleFacetsChanged();
		}

		_isMeshDirty = true;
	} 

  // This is where the polygon is actually "drawn"
	override public void PopulateRenderLayer()
	{
		if(_isOnStage && _firstFacetIndex != -1) 
		{
			_isMeshDirty = false;

			Vector3[] vertices = _renderLayer.vertices;
			Vector2[] uvs = _renderLayer.uvs;
			Color[] colors = _renderLayer.colors;

			Vector2[] polygonVertices = polygonData.polygonVertices;
			int[] trianglePolygons = polygonData.polygonTriangles;

			int nextTriangleIndex = _firstFacetIndex;

			int polyTriangleCount = trianglePolygons.Length / 3;

			for(int t = 0; t < polyTriangleCount; t++)
			{
				int vertexIndex0 = nextTriangleIndex*3;
				int vertexIndex1 = vertexIndex0 + 1;
				int vertexIndex2 = vertexIndex0 + 2;
				int threeT = t*3;

				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0], polygonVertices[trianglePolygons[threeT]],0);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex1], polygonVertices[trianglePolygons[threeT+1]],0);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex2], polygonVertices[trianglePolygons[threeT+2]],0);

				uvs[vertexIndex0] = _uvBottomLeft;
				uvs[vertexIndex1] = _uvTopLeft;
				uvs[vertexIndex2] = _uvBottomRight;

				colors[vertexIndex0] = _alphaColor;
				colors[vertexIndex1] = _alphaColor;
				colors[vertexIndex2] = _alphaColor;

				nextTriangleIndex++;
			}

			_renderLayer.HandleVertsChange();
		}
	}

  // If for some reason you change the WTPolygonData for this polygon sprite, make sure to call this so
  // it refreshes them and knows what to draw. There may be ways to make this more optimized for performance
  // that I need to look into.
	public void RefreshVertices() {
		if(_isOnStage && _firstFacetIndex != -1) {
			_renderLayer.Open();

      // Adding more facets than necessary because for some reason sometimes there aren't enough.
      // I need to look into this to figure out a better way to handle this.
			_renderLayer.GetNextFacetIndex(_triangleCount * 2); 

			Vector3[] vertices = _renderLayer.vertices;

			for (int i = 0; i < vertices.Length; i++) {
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[i], Vector2.zero, 0);
			}
		}
	}

  // This method returns the polygon data for the sprite after taking into account the sprite's
  // rotation, scale, and position. This is useful to get the actual vertice points that are
  // being shown on screen rather than the unaltered ones. CAUTION: this does not take any
  // parent container rotation, scale, or position into account. It just gets the data based
  // on the sprite's own properties. This method has NOT been optimized for performance yet.
  // There are ways to make it faster that I will update it with in the future.
	public WTPolygonData GetAdjustedPolygonData() {
		Vector2[] newPolygonVertices = new Vector2[polygonData.polygonVertices.Length];

		for (int i = 0; i < newPolygonVertices.Length; i++) {
			newPolygonVertices[i] = new Vector2(polygonData.polygonVertices[i].x, polygonData.polygonVertices[i].y);
		}

		for (int i = 0; i < newPolygonVertices.Length; i++) {
			Vector2 v = newPolygonVertices[i];

			v.x *= _scaleX;
			v.y *= _scaleY;

			Vector2 origV = v;

      // rotation needs to be negative because of the way rotation works in futile (clockwise rather than counter clockwise)
			float cosAngle = Mathf.Cos(-_rotation);
			float sinAngle = Mathf.Sin(-_rotation);

			v.x = origV.x * cosAngle - origV.y * sinAngle;
			v.y = origV.x * sinAngle + origV.y * cosAngle;

			v.x += this.x;
			v.y += this.y;

			newPolygonVertices[i] = v;
		}

		return new WTPolygonData(newPolygonVertices);
	}
}