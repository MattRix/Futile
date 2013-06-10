using System;
using UnityEngine;

public class FParticleSystem : FFacetNode
{
	private int _maxParticleCount;
	private FParticle[] _particles;
	private FParticle[] _availableParticles;
	private int _availableParticleCount;
	private int _unavailableParticleIndex;
	private bool _isMeshDirty;
	
	public float accelX = 0.0f;
	public float accelY = 0.0f;
	
	private bool _hasInited = false;

	public bool shouldNewParticlesOverwriteExistingParticles = true;
	
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
			if(stage != null) stage.HandleFacetsChanged();
		}
		
		FParticle particle;
		
		if(_availableParticleCount == 0) 
		{
			if(shouldNewParticlesOverwriteExistingParticles)
			{
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
				return; //there are no particles available, so don't create a new one
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

		float lifetimeInverse = 1.0f / lifetime; //we'll use this a few times, so invert it because multiplication is faster
		
		particle.scaleDeltaPerSecond = (particleDefinition.endScale - particleDefinition.startScale) * lifetimeInverse;
		
		Color startColor = particleDefinition.startColor;
		Color endColor = particleDefinition.endColor;
		
		particle.color = startColor;
		
		particle.redDeltaPerSecond = (endColor.r - startColor.r) * lifetimeInverse;
		particle.greenDeltaPerSecond = (endColor.g - startColor.g) * lifetimeInverse;
		particle.blueDeltaPerSecond = (endColor.b - startColor.b) * lifetimeInverse;
		particle.alphaDeltaPerSecond = (endColor.a - startColor.a) * lifetimeInverse;
		
		particle.elementHalfWidth = element.sourceSize.x * 0.5f;
		particle.elementHalfHeight = element.sourceSize.y * 0.5f;
		
		particle.uvTopLeft = element.uvTopLeft;
		particle.uvTopRight = element.uvTopRight;
		particle.uvBottomRight = element.uvBottomRight;
		particle.uvBottomLeft = element.uvBottomLeft;

		particle.initialTopLeft = new Vector2(-particle.elementHalfWidth,particle.elementHalfHeight);
		particle.initialTopRight = new Vector2(particle.elementHalfWidth,particle.elementHalfHeight);
		particle.initialBottomRight = new Vector2(particle.elementHalfWidth,-particle.elementHalfHeight);
		particle.initialBottomLeft = new Vector2(-particle.elementHalfWidth,-particle.elementHalfHeight);

		//notice how these are both multiplied by -1, this is to account for the flipped vertical coordinates in unity/futile
		particle.rotation = particleDefinition.startRotation * RXMath.DTOR * -1.0f;
		particle.rotationDeltaPerSecond = (particleDefinition.endRotation - particleDefinition.startRotation) * lifetimeInverse * RXMath.DTOR * -1.0f;

		if(particle.rotationDeltaPerSecond == 0) //no rotation
		{
			particle.doesNeedRotationUpdates = false;

			if(particle.rotation == 0)
			{
				particle.resultTopLeftX = particle.initialTopLeft.x;
				particle.resultTopLeftY = particle.initialTopLeft.y;
				particle.resultTopRightX = particle.initialTopRight.x;
				particle.resultTopRightY = particle.initialTopRight.y;
				particle.resultBottomRightX = particle.initialBottomRight.x;
				particle.resultBottomRightY = particle.initialBottomRight.y;
				particle.resultBottomLeftX = particle.initialBottomLeft.x;
				particle.resultBottomLeftY = particle.initialBottomLeft.y;
			}
			else //bake the rotation once
			{
				float sin = (float)Math.Sin(particle.rotation);
				float cos = (float)Math.Cos(particle.rotation);

				float ix, iy;

				ix = particle.initialTopLeft.x;
				iy = particle.initialTopLeft.y;
				particle.resultTopLeftX = ix * cos - iy * sin; 
				particle.resultTopLeftY = ix * sin + iy * cos; 

				ix = particle.initialTopRight.x;
				iy = particle.initialTopRight.y;
				particle.resultTopRightX = ix * cos - iy * sin; 
				particle.resultTopRightY = ix * sin + iy * cos; 

				ix = particle.initialBottomRight.x;
				iy = particle.initialBottomRight.y;
				particle.resultBottomRightX = ix * cos - iy * sin; 
				particle.resultBottomRightY = ix * sin + iy * cos; 

				ix = particle.initialBottomLeft.x;
				iy = particle.initialBottomLeft.y;
				particle.resultBottomLeftX = ix * cos - iy * sin; 
				particle.resultBottomLeftY = ix * sin + iy * cos; 
			}

		}
		else
		{
			//the rotation will be updated on the update
			particle.doesNeedRotationUpdates = true;
		}
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

				if(particle.doesNeedRotationUpdates)
				{
					particle.rotation += particle.rotationDeltaPerSecond * (double)deltaTime;

					float sin = (float)Math.Sin(particle.rotation);
					float cos = (float)Math.Cos(particle.rotation);

					float ix, iy;

					ix = particle.initialTopLeft.x;
					iy = particle.initialTopLeft.y;
					particle.resultTopLeftX = ix * cos - iy * sin; 
					particle.resultTopLeftY = ix * sin + iy * cos; 

					ix = particle.initialTopRight.x;
					iy = particle.initialTopRight.y;
					particle.resultTopRightX = ix * cos - iy * sin; 
					particle.resultTopRightY = ix * sin + iy * cos; 

					ix = particle.initialBottomRight.x;
					iy = particle.initialBottomRight.y;
					particle.resultBottomRightX = ix * cos - iy * sin; 
					particle.resultBottomRightY = ix * sin + iy * cos; 

					ix = particle.initialBottomLeft.x;
					iy = particle.initialBottomLeft.y;
					particle.resultBottomLeftX = ix * cos - iy * sin; 
					particle.resultBottomLeftY = ix * sin + iy * cos; 
				}
			}
		}

		_isMeshDirty = true; //needs redraw!
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
					float scale = particle.scale;
					float px = particle.x * a + particle.y * b + tx;
					float py = particle.x * c + particle.y * d + ty;
					
					vertices[vertexIndex0] = new Vector3
					(
						px + particle.resultTopLeftX * scale,
						py + particle.resultTopLeftY * scale,
						0
					);
					
					vertices[vertexIndex1] = new Vector3
					(
						px + particle.resultTopRightX * scale,
						py + particle.resultTopRightY * scale,
						0
					);

					vertices[vertexIndex2] = new Vector3
					(
						px + particle.resultBottomRightX * scale,
						py + particle.resultBottomRightY * scale,
						0
					);

					vertices[vertexIndex3] = new Vector3
						(
						px + particle.resultBottomLeftX * scale,
						py + particle.resultBottomLeftY * scale,
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

	public float startRotation = 0;
	public float endRotation = 0;
	
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

	public Vector2 initialTopLeft;
	public Vector2 initialTopRight;
	public Vector2 initialBottomRight;
	public Vector2 initialBottomLeft;

	//storing as straight floats rather than vectors for extra speed
	public float resultTopLeftX;
	public float resultTopLeftY;
	public float resultTopRightX;
	public float resultTopRightY;
	public float resultBottomRightX;
	public float resultBottomRightY;
	public float resultBottomLeftX;
	public float resultBottomLeftY;

	public double rotation; //in radians
	public double rotationDeltaPerSecond; //in radians

	public bool doesNeedRotationUpdates;
}




