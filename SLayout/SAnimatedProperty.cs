using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Base abstract class for the animation settings for a single property (e.g. X coordinate, width, or color)
/// for a single property. When an SLayoutProperty is animated, it receives an instances of this class.
/// </summary>
public abstract class SAnimatedProperty
{
	public float delay;
	public float duration;

	public abstract void Start();
	public abstract void Remove();
	public abstract void Animate(float lerpValue);
}
	
public class SAnimatedProperty<T> : SAnimatedProperty
{
	public SLayoutProperty<T> property;
	public SLayoutAnimation animation;

	public T start;
	public T end;

	public override void Start()
	{
		// StartAnimation is called immediately after calling the animAction,
		// so will be in its final state, ready to be reset. First though,
		// we need to record this final state.
		end = property.getter();

		// Now we reset, ready to go to that final state.
		property.setter(start);
	}

	public override void Remove()
	{
		property.animatedProperty = null;
		property = null;
		animation = null;
		start = default(T);
		end = default(T);
		_reusePool.Push(this);
	}

	public override void Animate(float lerpValue)
	{
		property.value = property.Lerp(start, end, lerpValue);
	}

	public static SAnimatedProperty<T> Create(float duration, float delay, SLayoutProperty<T> layoutProperty, SLayoutAnimation anim)
	{
		SAnimatedProperty<T> animProperty = null;

		if( _reusePool.Count > 0 ) 
			animProperty = _reusePool.Pop();
		else
			animProperty = new SAnimatedProperty<T>();

		animProperty.duration = duration;
		animProperty.delay = delay;

		animProperty.animation = anim;

		// Link and back-link
		animProperty.property = layoutProperty;
		layoutProperty.animatedProperty = animProperty;

		return animProperty;
	}

	static Stack<SAnimatedProperty<T>> _reusePool = new Stack<SAnimatedProperty<T>>();
}


public class SAnimatedCustomProperty : SAnimatedProperty
{
	public SAnimatedCustomProperty(Action<float> customAnim, float duration, float delay) {
		this._customAnim = customAnim;
		this.duration = duration;
		this.delay = delay;
	}

	public override void Start() {}
	public override void Remove() {}
	public override void Animate(float lerpValue)
	{
		_customAnim(lerpValue);
	}

	Action<float> _customAnim;
}