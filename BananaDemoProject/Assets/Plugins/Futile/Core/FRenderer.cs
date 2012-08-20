using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FRenderer
{	
	private List<FRenderLayer> _allLayers = new List<FRenderLayer>();
	private List<FRenderLayer> _liveLayers = new List<FRenderLayer>();
	private List<FRenderLayer> _previousLiveLayers = new List<FRenderLayer>();
	private List<FRenderLayer> _cachedLayers = new List<FRenderLayer>();
	
	private FRenderLayer _topLayer;
	
	private int _depthToUse;
	
	private FStage _stage;
	
	public FRenderer (FStage stage)
	{
		_stage = stage;
	}

	public void Clear () //wipe the renderlayers and remove their gameobjects
	{
		int allLayerCount = _allLayers.Count;
		for(int a = 0; a<allLayerCount; ++a)
		{
			_allLayers[a].Destroy();
		}
		
		_allLayers.Clear();
		_liveLayers.Clear();
		_previousLiveLayers.Clear();
		_cachedLayers.Clear();
	}

	public void UpdateLayerTransforms()
	{
		int allLayerCount = _allLayers.Count;
		for(int a = 0; a<allLayerCount; ++a)
		{
			_allLayers[a].UpdateTransform();
		}
	}
	
	public void StartRender()
	{
		//make the livelayers empty, put those layers in _previousLiveLayers
		List<FRenderLayer> swapLayers = _liveLayers;
		_liveLayers = _previousLiveLayers;
		_previousLiveLayers = swapLayers;
		
		_topLayer = null;
		
		_depthToUse = 0;
	}
	
	public void EndRender()
	{
		int previousLiveLayerCount = _previousLiveLayers.Count;
		for(int p = 0; p<previousLiveLayerCount; ++p)
		{
			_previousLiveLayers[p].RemoveFromWorld();
			_cachedLayers.Add(_previousLiveLayers[p]);
		}
		
		_previousLiveLayers.Clear();
		
		if(_topLayer != null) _topLayer.Close();
		
	}
	
	protected FRenderLayer CreateRenderLayer(int batchIndex, FAtlas atlas, FShader shader)
	{
		//first, check and see if we already have a layer that matches the batchIndex
		int previousLiveLayerCount = _previousLiveLayers.Count;
		for(int p = 0; p<previousLiveLayerCount; ++p)
		{
			FRenderLayer previousLiveLayer = _previousLiveLayers[p];
			if(previousLiveLayer.batchIndex == batchIndex)
			{
				_previousLiveLayers.RemoveAt(p);
				_liveLayers.Add (previousLiveLayer);
				previousLiveLayer.depth = _depthToUse++;
				return previousLiveLayer;
			}
		}
		
		//now see if we have a cached (old, now unused layer) that matches the batchIndex
		int cachedLayerCount = _cachedLayers.Count;
		for(int c = 0; c<cachedLayerCount; ++c)
		{
			FRenderLayer cachedLayer = _cachedLayers[c];
			if(cachedLayer.batchIndex == batchIndex)
			{
				_cachedLayers.RemoveAt(c);
				cachedLayer.AddToWorld();
				_liveLayers.Add (cachedLayer);
				cachedLayer.depth = _depthToUse++;
				return cachedLayer;
			}
		}
		
		//still no layer found? create a new one!
		FRenderLayer newLayer = new FRenderLayer(_stage, atlas,shader);
		_liveLayers.Add(newLayer);
		_allLayers.Add(newLayer);
		newLayer.AddToWorld();
		newLayer.depth = _depthToUse++;
		
		return newLayer;
	}
	
	public void GetRenderLayer (out FRenderLayer renderLayer, out int firstQuadIndex, FAtlas atlas, FShader shader, int numberOfQuadsNeeded)
	{
		int batchIndex = atlas.index*10000 + shader.index;
		
		if(_topLayer == null)
		{
			_topLayer = CreateRenderLayer(batchIndex, atlas, shader);
			_topLayer.Open();
		}
		else 
		{
			if(_topLayer.batchIndex != batchIndex) //we're changing layers!
			{
				_topLayer.Close(); //close the old layer
				
				_topLayer = CreateRenderLayer(batchIndex, atlas, shader);
				_topLayer.Open(); //open the new layer
			}
		}
		
		renderLayer = _topLayer;
		firstQuadIndex = _topLayer.GetNextQuadIndex(numberOfQuadsNeeded);
	}
	
	public FShader GetDefaultShader()
	{
		return FShader.Normal;
	}
	
	public void Update()
	{
		int liveLayerCount = _liveLayers.Count;
		for(int v = 0; v<liveLayerCount; ++v)
		{
			_liveLayers[v].depth = Futile.nextRenderLayerDepth++;
			_liveLayers[v].Update();	
		} 

	}
}


