using UnityEngine;
using System;
using System.Collections.Generic;

public class FxUtils {
	public FxUtils () {
	}
	
	static public void Flash (FSprite sprite) {
		FlashUtil util=new FlashUtil();
		util.go(sprite);
	}
	static public void Oscil (FNode node, float period, float ampX, float ampY) {
		OscilUtil util=new OscilUtil();
		util.go(node,period,ampX,ampY);
	}
	static public void OscilScale (FNode node, float period, float scaleX, float scaleY) {
		OscilScaleUtil util=new OscilScaleUtil();
		util.go(node,period,scaleX,scaleY);
	}
	static public void OscilColor (FSprite sprite, float period, Color toColor) {
		OscilColorUtil util=new OscilColorUtil();
		util.go(sprite,period,toColor);
	}
	static public void OscilAlpha (FSprite sprite, float period, float toAlpha) {
		OscilAlphaUtil util=new OscilAlphaUtil();
		util.go(sprite,period,toAlpha);
	}
	static public void Bump (FNode node, float period, float scaleRatio) {
		BumpUtil util=new BumpUtil();
		util.go(node,period,scaleRatio);
	}
	static public void Bump (FNode node) {
		Bump (node,0.1f,1.1f);
	}
	
	static public void CancelActions(FSprite node) {
		FlashUtil.Cancel(node);
		OscilScaleUtil.Cancel(node);
		BumpUtil.Cancel(node);
		
		OscilColorUtil.Cancel(node);
		OscilAlphaUtil.Cancel(node);
		OscilUtil.Cancel(node);
	}
	static public void CancelActions(FNode node) {
		OscilUtil.Cancel(node);
		OscilScaleUtil.Cancel(node);
		BumpUtil.Cancel(node);
	}
}

public class FlashUtil {
	static Dictionary<FNode, Tween> tweens = new Dictionary<FNode, Tween>();
	
	public FlashUtil () {
	}

	public void go(FSprite sprite) {
		Cancel (sprite);
		sprite.shader=FShader.AdditiveColor;
		sprite.color=Futile.white;
		TweenConfig config0=new TweenConfig().colorProp("color",new Color(0f,0f,0f)).onComplete(HandleFlashDone);
		config0.setEaseType(EaseType.ExpoOut);
		tweens.Add(sprite,Go.to (sprite, 0.5f, config0));
	}
	protected void HandleFlashDone(AbstractTween tween) {
		FSprite sprite=(FSprite)(((Tween)tween).target);
		tweens.Remove(sprite);
		sprite.shader=FShader.Basic;
		sprite.color=Futile.white;
	}
	static public void Cancel (FSprite sprite) {
		Tween tween=null;
		tweens.TryGetValue(sprite, out tween);
		if (tween!=null) {
			tween.destroy();
			tweens.Remove(sprite);
		}
	}
}



public class OscilUtil {
	static Dictionary<FNode, OscilUtil> pendings = new Dictionary<FNode, OscilUtil>();
	
	public Vector2 memoPos;
	public TweenChain chain=null;

	public OscilUtil () {
	}

	public void go(FNode node, float period, float ampX, float ampY) {
		Cancel (node);
		
		chain = new TweenChain();
		chain.setIterations( -1, LoopType.PingPong );
		TweenConfig config0=new TweenConfig();
		
		if (ampX!=0) config0.floatProp( "x", node.x+ampX );
		if (ampY!=0) config0.floatProp( "y", node.y+ampY );
		
		config0.setEaseType(EaseType.SineInOut);
		
		chain.append( new Tween( node, period, config0 ) );
		chain.play();
		
		pendings.Add(node,this);
	}
	static public void Cancel (FNode node) {
		OscilUtil instance=null;
		pendings.TryGetValue(node, out instance);
		if (instance!=null) {
			instance.chain.destroy();
			node.SetPosition(instance.memoPos);
			pendings.Remove(node);
		}
	}
}




public class OscilScaleUtil {
	static Dictionary<FNode, OscilScaleUtil> pendings = new Dictionary<FNode, OscilScaleUtil>();
	
	public float memoScaleX,memoScaleY;
	public TweenChain chain=null;

	public OscilScaleUtil () {
	}

	public void go(FNode node, float period, float scaleX, float scaleY) {
		Cancel (node);
		
		chain = new TweenChain();
		chain.setIterations( -1, LoopType.PingPong );
		TweenConfig config0=new TweenConfig();
		
		if (scaleX!=node.scaleX) config0.floatProp( "scaleX", scaleX );
		if (scaleY!=node.scaleY) config0.floatProp( "scaleY", scaleY );
		
		config0.setEaseType(EaseType.SineInOut);
		
		chain.append( new Tween( node, period, config0 ) );
		chain.play();
		
		memoScaleX=node.scaleX;
		memoScaleY=node.scaleY;
		pendings.Add(node,this);
	}
	static public void Cancel (FNode node) {
		OscilScaleUtil instance=null;
		pendings.TryGetValue(node, out instance);
		if (instance!=null) {
			instance.chain.destroy();
			node.scaleX=instance.memoScaleX;
			node.scaleY=instance.memoScaleX;
			pendings.Remove(node);
		}
	}
}




public class OscilColorUtil {
	static Dictionary<FNode, OscilColorUtil> pendings = new Dictionary<FNode, OscilColorUtil>();
	
	public Color memoColor;
	public TweenChain chain=null;

	public OscilColorUtil () {
	}

	public void go(FSprite node, float period, Color toColor  ) {
		Cancel (node);
		
		//Debug.Log ("OscilColorUtil node="+node);
		chain = new TweenChain();
		chain.setIterations( -1, LoopType.PingPong );
		TweenConfig config0=new TweenConfig();
		config0.colorProp("color",toColor);
		config0.setEaseType(EaseType.SineInOut);
		
		chain.append( new Tween( node, period, config0 ) );
		chain.play();
		
		memoColor=node.color;
		pendings.Add(node,this);
	}
	static public void Cancel (FSprite node) {
		OscilColorUtil instance=null;
		pendings.TryGetValue(node, out instance);
		if (instance!=null) {
			instance.chain.destroy();
			node.color=instance.memoColor;
			pendings.Remove(node);
		}
	}
}



public class OscilAlphaUtil {
	static Dictionary<FNode, OscilAlphaUtil> pendings = new Dictionary<FNode, OscilAlphaUtil>();
	
	public float memoAlpha;
	public TweenChain chain=null;
	
	public OscilAlphaUtil () {
	}

	public void go(FSprite node, float period, float toAlpha  ) {
		Cancel (node);
		
		chain = new TweenChain();
		chain.setIterations( -1, LoopType.PingPong );
		TweenConfig config0=new TweenConfig();
		config0.floatProp("alpha",toAlpha);
		config0.setEaseType(EaseType.SineInOut);
		
		chain.append( new Tween( node, period, config0 ) );
		chain.play();
		
		memoAlpha=node.alpha;
		pendings.Add(node,this);
	}
	static public void Cancel (FSprite node) {
		OscilAlphaUtil instance=null;
		pendings.TryGetValue(node, out instance);
		if (instance!=null) {
			instance.chain.destroy();
			node.alpha=instance.memoAlpha;
			pendings.Remove(node);
		}
	}
}



public class BumpUtil {
	static Dictionary<FNode, BumpUtil> pendings = new Dictionary<FNode, BumpUtil>();
	
	public float memoScale;
	public Tween tween=null;
	
	public BumpUtil () {
	}

	public void go(FNode node,float duration,float scaleRatio) {
		Cancel (node);
		
		memoScale=node.scale;
		pendings.Add(node,this);
		
		TweenConfig config0=new TweenConfig().floatProp("scale",node.scale).onComplete(HandleDone);
		node.scale*=scaleRatio;
		config0.setEaseType(EaseType.Linear);
		tween=Go.to (node, duration, config0);
	}
	protected void HandleDone(AbstractTween tween) {
		FNode node=(FNode)(((Tween)tween).target);
		//BumpUtil instance=null;
		//pendings.TryGetValue(node, out instance);
		pendings.Remove(node);
	}
	static public void Cancel (FNode node) {
		BumpUtil instance=null;
		pendings.TryGetValue(node, out instance);
		if (instance!=null) {
			instance.tween.destroy();
			node.scale=instance.memoScale;
			pendings.Remove(node);
		}
	}
}




public class ShakeUtil {
	static Dictionary<FNode, ShakeUtil> pendings = new Dictionary<FNode, ShakeUtil>();
	
	public Vector2 oPosition;
	public float duration,amplitude;
	public FNode node=null;
	
	public float curDuration,curAmplitude;
	
	public ShakeUtil () {
	}
	public void go(FNode node_,float duration_,float amplitude_) {
		Cancel(node_);

		oPosition=node_.GetPosition();
		//Debug.Log("oPosition="+oPosition);
		curDuration=duration=duration_;
		curAmplitude=amplitude=amplitude_;
		pendings.Add(node_,this);
		if (node==null) {
			Futile.instance.SignalUpdate+=HandleUpdate;
		}
		node=node_;
	}
	protected void HandleUpdate() {
		curDuration-=Time.deltaTime;
		if (curDuration<0) {
			Stop();
		} else {
			curAmplitude=amplitude*curDuration/duration;
			node.x=oPosition.x+RXRandom.Range(-curAmplitude,curAmplitude);
			node.y=oPosition.y+RXRandom.Range(-curAmplitude,curAmplitude);
		}
	}
	protected void Stop() {
		if (node!=null) {
			Futile.instance.SignalUpdate-=HandleUpdate;
			node.SetPosition(oPosition);
			pendings.Remove(node);
			node=null;
			curDuration=-1f;
		}
	}
	static public void Go(FNode node_,float duration_,float amplitude_) {
		(new ShakeUtil()).go (node_,duration_,amplitude_);
	}
	static public void Cancel(FNode node) {
		ShakeUtil obj=null;
		pendings.TryGetValue(node, out obj);
		if (obj!=null) {
			obj.Stop();
		}
	}
}



public class CenteredOscilUtil {
	static Dictionary<FNode, CenteredOscilUtil> pendings = new Dictionary<FNode, CenteredOscilUtil>();
	
	public Vector2 oPosition;
	public float duration,amplitudeX,amplitudeY,periodX,periodY;
	public FNode node=null;
	
	public float curDuration,curAmplitudeX,curAmplitudeY,curDelay;
	
	public CenteredOscilUtil () {
	}
	public void go(FNode node_,float duration_,float amplitudeX_,float periodX_,float amplitudeY_,float periodY_) {
		go (node_,duration_,amplitudeX_,periodX_,amplitudeY_,periodY_,-1f);
	}
	public void go(FNode node_,float duration_,float amplitudeX_,float periodX_,float amplitudeY_,float periodY_,float delay_) {
		Cancel(node_);

		oPosition=node_.GetPosition();
		//Debug.Log("oPosition="+oPosition);
		curDuration=duration=duration_;
		curAmplitudeX=amplitudeX=amplitudeX_;
		curAmplitudeY=amplitudeY=amplitudeY_;
		periodX=periodX_;
		periodY=periodY_;
		curDelay=delay_;
		pendings.Add(node_,this);
		if (node==null) {
			Futile.instance.SignalUpdate+=HandleUpdate;
		}
		node=node_;
	}
	protected void HandleUpdate() {
		if (curDelay>0f) {
			curDelay-=Time.deltaTime;
			return;
		}
		curDuration-=Time.deltaTime;
		if (curDuration<0) {
			Stop();
		} else {
			curAmplitudeX=amplitudeX*curDuration/duration;
			curAmplitudeY=amplitudeY*curDuration/duration;
			node.x=oPosition.x+curAmplitudeX*(float)Math.Sin((duration-curDuration)*2*Math.PI/periodX);
			node.y=oPosition.y+curAmplitudeY*(float)Math.Sin((duration-curDuration)*2*Math.PI/periodY);
		}
	}
	protected void Stop() {
		if (node!=null) {
			Futile.instance.SignalUpdate-=HandleUpdate;
			node.SetPosition(oPosition);
			pendings.Remove(node);
			node=null;
			curDuration=-1f;
		}
	}
	static public void Go(FNode node_,float duration_,float amplitudeX_,float periodX_,float amplitudeY_,float periodY_) {
		(new CenteredOscilUtil()).go (node_,duration_,amplitudeX_,periodX_,amplitudeY_,periodY_);
	}
	static public void Go(FNode node_,float duration_,float amplitudeX_,float periodX_,float amplitudeY_,float periodY_,float delay_) {
		(new CenteredOscilUtil()).go (node_,duration_,amplitudeX_,periodX_,amplitudeY_,periodY_,delay_);
	}
	static public void Cancel(FNode node) {
		CenteredOscilUtil obj=null;
		pendings.TryGetValue(node, out obj);
		if (obj!=null) {
			obj.Stop();
		}
	}
}



public class FxHelper {
	static readonly FxHelper instance=new FxHelper();
	
	static FxHelper () {
	}

	FxHelper() {
	}

	public static FxHelper Instance { get { return instance; } }
	
	public void RemoveFromContainer(AbstractTween tween) {
		//Debug.Log ("RemoveFromContainer tween="+tween);
		FNode node =(FNode)(((Tween)tween).target);
		node.RemoveFromContainer();
	}
}


