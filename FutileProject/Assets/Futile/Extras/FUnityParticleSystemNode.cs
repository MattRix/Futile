using System;
using UnityEngine;

public class FUnityParticleSystemNode : FGameObjectNode
{
	protected ParticleSystem _particleSystem;
	
	public FUnityParticleSystemNode (GameObject gameObject, bool shouldDuplicate)
	{
		if(shouldDuplicate) //make a copy
		{
			gameObject = UnityEngine.Object.Instantiate(gameObject) as GameObject;
		}
		
		_particleSystem = gameObject.GetComponent<ParticleSystem>();
		
		if(_particleSystem == null)
		{
			throw new FutileException("The FUnityParticleSystemNode was not passed a gameObject with a ParticleSystem component");	
		}
		
		Init(gameObject, true, false, false);
	}
	
	override public void HandleAddedToStage()
	{
		Futile.instance.SignalUpdate += HandleUpdate;
		base.HandleAddedToStage();	
	}
	
	override public void HandleRemovedFromStage()
	{
		Futile.instance.SignalUpdate -= HandleUpdate;
		base.HandleRemovedFromStage();	
	}
	
	protected void HandleUpdate()
	{
		if(!_particleSystem.IsAlive())
		{
			this.RemoveFromContainer();
		}
	}
	
	public ParticleSystem particleSystem
	{
		get { return _particleSystem;}	
	}
	
}

