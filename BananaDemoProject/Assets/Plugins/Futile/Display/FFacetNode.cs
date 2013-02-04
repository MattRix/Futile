using UnityEngine;
using System;

public class FFacetNode : FNode
{
	protected FAtlasElement _element;
	
	protected FShader _shader = null;
	
	protected int _firstFacetIndex = -1;
	protected int _numberOfFacetsNeeded;
	
	protected FFacetRenderLayer _renderLayer;
	
	protected FFacetType _facetType;
	
	public FFacetNode ()
	{
		
	}
	
	protected void Init(FFacetType facetType, FAtlasElement element, int numberOfFacetsNeeded)
	{
		_facetType = facetType;
		
		_element = element;
		if(_shader == null) _shader = FShader.defaultShader;
		_numberOfFacetsNeeded = numberOfFacetsNeeded; 
		
		HandleElementChanged();
	}
	
	protected void UpdateFacets()
	{
		_stage.renderer.GetFacetRenderLayer(out _renderLayer, out _firstFacetIndex, _facetType, _element.atlas, _shader, _numberOfFacetsNeeded);
	}
	
	virtual public int firstFacetIndex
	{
		get {return _firstFacetIndex;}	
	}
	
	virtual public void HandleElementChanged()
	{
		//override by parent things
	}
	
	virtual public void PopulateRenderLayer()
	{
		//override in parent, this is when you set the quads of the render layer
	}
	
	override public void HandleAddedToStage()
	{
		if(!_isOnStage)
		{
			_isOnStage = true;
			_stage.HandleFacetsChanged();
		}
	}
	
	override public void HandleRemovedFromStage()
	{
		if(_isOnStage)
		{
			_isOnStage = false;
			_stage.HandleFacetsChanged();
		}
	}
	
	public void SetElementByName(string elementName)
	{
		this.element = Futile.atlasManager.GetElementWithName(elementName);
	}
	
	public FAtlasElement element
	{
		get { return _element;}
		set
		{
			if(_element != value)
			{
				bool isAtlasDifferent = (_element.atlasIndex != value.atlasIndex);
	
				_element = value;	
				
				if(isAtlasDifferent)
				{
					if(_isOnStage) _stage.HandleFacetsChanged();	
				}
				
				HandleElementChanged();
			}
		}
	}
	
	public FShader shader
	{
		get { return _shader;}
		set
		{
			if(_shader != value)
			{
				_shader = value;
				if(_isOnStage) _stage.HandleFacetsChanged();	
			}
		}
	}
	
	public FFacetType facetType
	{
		get {return _facetType;}	
	}
	
	
	
}


