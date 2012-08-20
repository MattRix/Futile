using UnityEngine;
using System;


public class FQuadNode : FNode
{
	protected FAtlasElement _element;
	
	protected FShader _shader;
	
	protected int _firstQuadIndex = -1;
	protected int _numberOfQuadsNeeded;
	
	protected FRenderLayer _renderLayer;
	
	public FQuadNode ()
	{
		
	}
	
	protected void Init(FAtlasElement element, int numberOfQuadsNeeded)
	{
		_element = element;
		_shader = FShader.defaultShader;
		_numberOfQuadsNeeded = numberOfQuadsNeeded; 
		
		HandleElementChanged();
	}
	
	protected void UpdateQuads()
	{
		_stage.renderer.GetRenderLayer(out _renderLayer, out _firstQuadIndex, _element.atlas, _shader, _numberOfQuadsNeeded);
	}
	
	virtual public int firstQuadIndex
	{
		get {return _firstQuadIndex;}	
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
			_stage.HandleQuadsChanged();
		}
	}
	
	override public void HandleRemovedFromStage()
	{
		if(_isOnStage)
		{
			_isOnStage = false;
			_stage.HandleQuadsChanged();
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
					if(_isOnStage) _stage.HandleQuadsChanged();	
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
				if(_isOnStage) _stage.HandleQuadsChanged();	
			}
		}
	}
	
	
	
}


