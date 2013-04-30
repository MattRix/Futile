using System;
using UnityEngine;

public class FParticleSystem : FFacetNodeBase
{
	private int _maxParticleCount;
	private FParticle[] _particles;
	private FParticle[] _availableParticles;
	private int _availableParticleCount;
	private int _unavailableParticleIndex;
	private bool _isMeshDirty;
	
	public float accelX = 0.0f;
	public float accelY = 0.0f; //-625.0f;
	
	private bool _hasInited = false;
	
	public FParticleSystem (int maxParticleCount)
	{
		_maxParticleCount = maxParticleCount;
		_particles = new FParticle[_maxParticleCount];
		
		_availableParticles = new FParticle[_maxParticleCount];
		_availableParticleCount = _maxParticleCount;
		_unavailableParticleIndex = _maxParticleCount-1;
		
		for(int p = 0; p<_maxParticleCount; p++)
		{
			_particles[p] = _availableParticles[p] = new FParticle();	
		}
		
		ListenForUpdate(HandleUpdate);
	}
	
	private void HandleUpdate()
	{
		float deltaTime = Time.deltaTime;
		
		for(int p = 0; p<_maxParticleCount; p++)
		{
			FParticle particle = _particles[p];

			if(particle.timeRemaining <= 0) 
			{
				//this particle isn't alive, go to the next one
			}
			else if(particle.timeRemaining <= deltaTime) //is it going to end during this update?
			{
				//add it back to the available particles
				_availableParticles[_availableParticleCount] = particle;
				_availableParticleCount++;
				particle.timeRemaining = 0;

				//don't bother updating it because it won't be rendered anyway
			}
			else //do the update!
			{
				particle.timeRemaining -= deltaTime;
				
				particle.color.r += particle.redDeltaPerSecond * deltaTime;
				particle.color.g += particle.greenDeltaPerSecond * deltaTime;
				particle.color.b += particle.blueDeltaPerSecond * deltaTime;
				particle.color.a += particle.alphaDeltaPerSecond * deltaTime;
				
				particle.scale += particle.scaleDeltaPerSecond * deltaTime;
				
				particle.speedX += accelX * deltaTime;
				particle.speedY += accelY * deltaTime;
				
				particle.x += particle.speedX * deltaTime;
				particle.y += particle.speedY * deltaTime;
			}
		}

		_isMeshDirty = true; //needs redraw!
	}
	
	public void AddParticle(FParticleDefinition particleDefinition)
	{
		FAtlasElement element = particleDefinition.element;
		
		if(_hasInited)
		{
			if(element.atlas != _atlas)
			{
				throw new FutileException("All elements added to a particle system must be from the same atlas");
			}
		}
		else 
		{
			_hasInited = true;
			Init(FFacetType.Quad, element.atlas, _maxParticleCount);
		}
		
		FParticle particle;
		
		if(_availableParticleCount == 0) 
		{
			//return; //there are no particles available, just don't create a new one! (later on we could just reuse an existing one)
			
			//get one of the currently running particles and overwrite it
			//but make sure we don't keep overwriting the same particle!
			particle = _availableParticles[_unavailableParticleIndex--];
			if(_unavailableParticleIndex < 0)
			{
				_unavailableParticleIndex = _maxParticleCount-1;	
			}
		}
		else 
		{
			_availableParticleCount--;
			particle = _availableParticles[_availableParticleCount]; 
		}
		
		float lifetime = particleDefinition.lifetime;
		
		particle.timeRemaining = lifetime;
	
		particle.x = particleDefinition.x;
		particle.y = particleDefinition.y;
		particle.speedX = particleDefinition.speedX;
		particle.speedY = particleDefinition.speedY;
		
		particle.scale = particleDefinition.startScale;
		
		particle.scaleDeltaPerSecond = (particleDefinition.endScale - particleDefinition.startScale) / lifetime;
		
		Color startColor = particleDefinition.startColor;
		Color endColor = particleDefinition.endColor;
		
		particle.color = startColor;
		
		particle.redDeltaPerSecond = (endColor.r - startColor.r) / lifetime;
		particle.greenDeltaPerSecond = (endColor.g - startColor.g) / lifetime;
		particle.blueDeltaPerSecond = (endColor.b - startColor.b) / lifetime;
		particle.alphaDeltaPerSecond = (endColor.a - startColor.a) / lifetime;
		
		particle.elementHalfWidth = element.sourceSize.x * 0.5f;
		particle.elementHalfHeight = element.sourceSize.y * 0.5f;
		
		particle.uvTopLeft = element.uvTopLeft;
		particle.uvTopRight = element.uvTopRight;
		particle.uvBottomRight = element.uvBottomRight;
		particle.uvBottomLeft = element.uvBottomLeft;
	}
	
	override public void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		bool wasMatrixDirty = _isMatrixDirty;
		//bool wasAlphaDirty = _isAlphaDirty;
		
		UpdateDepthMatrixAlpha(shouldForceDirty, shouldUpdateDepth);
		
		if(shouldUpdateDepth)
		{
			UpdateFacets();
		}
		
		if(wasMatrixDirty || shouldForceDirty || shouldUpdateDepth)
		{
			_isMeshDirty = true;
		}
		
		//not using color or alpha at the moment
//		if(wasAlphaDirty || shouldForceDirty)
//		{
//			_isMeshDirty = true;
//			_color.ApplyMultipliedAlpha(ref _alphaColor, _concatenatedAlpha);	
//		}
		
		if(_isMeshDirty) 
		{
			PopulateRenderLayer();
		}
	}

	
	override public void PopulateRenderLayer()
	{
		if(_isOnStage && _firstFacetIndex != -1) 
		{
			_isMeshDirty = false;
			
			Vector3[] vertices = _renderLayer.vertices;
			Vector2[] uvs = _renderLayer.uvs;
			Color[] colors = _renderLayer.colors;
		
			float a = _concatenatedMatrix.a;
			float b = _concatenatedMatrix.b;
			float c = _concatenatedMatrix.c;
			float d = _concatenatedMatrix.d;
			float tx = _concatenatedMatrix.tx;
			float ty = _concatenatedMatrix.ty;
				
//			Vector2 unitVector = _concatenatedMatrix.GetTransformedUnitVector();
//			float ux = unitVector.x;
//			float uy = unitVector.y;
			
			int vertexIndex0 = _firstFacetIndex*4;
			int vertexIndex1 = vertexIndex0 + 1;
			int vertexIndex2 = vertexIndex0 + 2;
			int vertexIndex3 = vertexIndex0 + 3;

			for(int p = 0; p<_maxParticleCount; p++)
			{
				FParticle particle = _particles[p];
				
				if(particle.timeRemaining > 0)
				{		
					float ew = particle.elementHalfWidth * particle.scale;
					float eh = particle.elementHalfHeight * particle.scale;
					float px = particle.x * a + particle.y * b + tx;
					float py = particle.x * c + particle.y * d + ty;
					
					vertices[vertexIndex0] = new Vector3
					(
						px - ew,
						py + eh,
						0
					);
					
					vertices[vertexIndex1] = new Vector3
					(
						px + ew,
						py + eh,
						0
					);
					
					vertices[vertexIndex2] = new Vector3
					(
						px + ew,
						py - eh,
						0
					);
					
					vertices[vertexIndex3] = new Vector3
					(
						px - ew,
						py - eh,
						0
					);
					
					uvs[vertexIndex0] = particle.uvTopLeft;
					uvs[vertexIndex1] = particle.uvTopRight;
					uvs[vertexIndex2] = particle.uvBottomRight;
					uvs[vertexIndex3] = particle.uvBottomLeft;
							
					colors[vertexIndex0] = particle.color;
					colors[vertexIndex1] = particle.color;
					colors[vertexIndex2] = particle.color;
					colors[vertexIndex3] = particle.color;
				}
				else //it's dead so put zeroes in
				{
					vertices[vertexIndex0].Set(50,0,1000000);	
					vertices[vertexIndex1].Set(50,0,1000000);	
					vertices[vertexIndex2].Set(50,0,1000000);	
					vertices[vertexIndex3].Set(50,0,1000000);	
				}
				
				vertexIndex0 += 4;
				vertexIndex1 += 4;
				vertexIndex2 += 4;
				vertexIndex3 += 4;
			}
			
			_renderLayer.HandleVertsChange();
		}
	}
	
}

public class FParticleDefinition
{
	public FAtlasElement element;
	
	public float lifetime = 1.0f;
	
	public float x = 0.0f;
	public float y = 0.0f;
	
	public float speedX = 0.0f;
	public float speedY = 0.0f;
	
	public float startScale = 1.0f;
	public float endScale = 1.0f;
	
	public Color startColor = Futile.white;
	public Color endColor = Futile.white;
	
	public FParticleDefinition(string elementName)
	{
		this.element = Futile.atlasManager.GetElementWithName(elementName);	
	}
	
	public FParticleDefinition(FAtlasElement element)
	{
		this.element = element;
	}
	
	public void SetElementByName(string elementName)
	{
		this.element = Futile.atlasManager.GetElementWithName(elementName);
	}
}

public class FParticle
{
	public float timeRemaining;
	
	public float x;
	public float y;
	
	public float speedX;
	public float speedY;
	
	public float scale;
	public float scaleDeltaPerSecond;
	
	public Color color;
	
	public float redDeltaPerSecond;
	public float greenDeltaPerSecond;
	public float blueDeltaPerSecond;
	public float alphaDeltaPerSecond;
	
	public float elementHalfWidth;
	public float elementHalfHeight;
	
	public Vector2 uvTopLeft;
	public Vector2 uvTopRight;
	public Vector2 uvBottomRight;
	public Vector2 uvBottomLeft;
}