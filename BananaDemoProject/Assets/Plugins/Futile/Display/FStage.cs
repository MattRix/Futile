using UnityEngine;
using System.Collections;

public class FStage : FContainer
{
	public int nextNodeDepth;
	
	private bool _needsDepthUpdate = false;
	
	private FRenderer _renderer;
	
	public FStage()
	{
		_stage = this;
		_renderer = new FRenderer();
		
		HandleAddedToStage(); //add it to itself!
	}

	public void HandleQuadsChanged ()
	{
		_needsDepthUpdate = true;
	}
	
	override public void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		bool didNeedDepthUpdate = _needsDepthUpdate;
		
		_needsDepthUpdate = false;
		
		if(didNeedDepthUpdate)
		{
			shouldForceDirty = true;
			shouldUpdateDepth = true;
			nextNodeDepth = 0;
			_renderer.StartRender();
		}
		
		base.Redraw(shouldForceDirty, shouldUpdateDepth);
		
		if(didNeedDepthUpdate)
		{
			_renderer.EndRender();
			Futile.touchManager.UpdatePrioritySorting();
		}
	}
	
	public void LateUpdate() //called by the engine
	{
		_renderer.Update();
	}
	
	public FRenderer renderer
	{
		get {return _renderer;}	
	}
	
}
