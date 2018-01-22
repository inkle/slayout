using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Instance of an active animation that's created by SLayout's Animate method. The animations currently being defined are stored
/// in a static list so that as you run the animAction code, it can detects properties that have been changed and react accordingly.
/// </summary>
public class SLayoutAnimation {

	public SLayoutAnimation(float duration, float delay, Action animAction, Action completionAction, AnimationCurve customCurve, SLayout owner)
	{
		if( _animationsBeingDefined == null )
			_animationsBeingDefined = new List<SLayoutAnimation>();

		_animationsBeingDefined.Add(this);

		_time = 0.0f;
		_duration = _maxDuration = duration;
		_delay = _maxDelay = delay;
		_completionAction = completionAction;
		_customCurve = customCurve;
		_owner = owner;

		// animAction == null when using the After method, for example
		if( animAction != null ) {
			_properties = new List<SAnimatedProperty>();

			animAction();

			// Rewind animation back to beginning
			// But only if our duration > 0
			if( !isInstant ) {
				foreach(var property in _properties)
					property.Start();
			}

		}

		// Duration = zero? Done already
		if( isInstant ) Done();

		_animationsBeingDefined.RemoveAt(_animationsBeingDefined.Count-1);
	}

	public bool canAnimate {
		get {
			// If owner is removed/deleted then the animation is cancelled.
			// Likewise if all properties have been removed (e.g. by being overriden)
			return owner != null || _properties != null &&_properties.Count > 0;
		}
	}

	public static SLayoutAnimation AnimationUnderDefinition()
	{
		if( _animationsBeingDefined != null && _animationsBeingDefined.Count > 0 )
			return _animationsBeingDefined[_animationsBeingDefined.Count-1];
		else
			return null;
	}
		
	public void SetupPropertyAnim<T>(SLayoutProperty<T> layoutProperty)
	{
		SAnimatedProperty<T> animatedProperty = layoutProperty.animatedProperty;

		// Already being animated as part of a different animation?
		// Cancel the animation of this property in that animation, and instead
		// animate as part of a new animation.
		if( animatedProperty != null ) {
			var existingAnim = animatedProperty.animation;
			if( existingAnim != this ) {
				existingAnim.RemovePropertyAnim(animatedProperty);
				animatedProperty = null;
			}
		}

		// Create the animated property 
		// (But only if necessary: This property may have already been set 
		// as part of an animation, but the code may be overriding with a new value)
		if( animatedProperty == null ) {
			animatedProperty = SAnimatedProperty<T>.Create(_duration, _delay, layoutProperty, this);
			_properties.Add(animatedProperty);
		}

		animatedProperty.start = layoutProperty.getter();
	}

	public void Cancel()
	{
		RemoveAnimFromAllProperties();
	}

	void RemoveAnimFromAllProperties()
	{
		if( _properties == null ) return;

		foreach(var prop in _properties)
			prop.Remove();
		
		_properties.Clear();
	}

	void RemovePropertyAnim(SAnimatedProperty animProperty)
	{
		animProperty.Remove();

		// Could potentially make _properties a HashSet rather than List
		// to make this faster, but I think that the Remove is rare enough
		// compared to the Add that it's probably faster to keep it as a list.
		if( _properties != null )
			_properties.Remove(animProperty);
	}

	public bool isComplete {
		get {
			return _completed;
		}
	}

	public SLayout owner {
		get {
			return _owner;
		}
	}

	public void AddDelay(float extraDelay) {
		_delay += extraDelay;
		_maxDelay = Mathf.Max(_delay, _maxDelay);
	}

	public void AddDuration(float extraDuration) {
		_duration += extraDuration;
		_maxDuration = Mathf.Max(_duration, _maxDuration);
	}

	public void AddCustomAnim(Action<float> customAnim)
	{
		_properties.Add(new SAnimatedCustomProperty(customAnim, _duration, _delay));
	}

	bool timeIsUp {
		get {
			return _time >= _maxDelay + _maxDuration;
		}
	}
		
	bool isInstant {
		get {
			// Delay and duration may vary over the course of the animation definition,
			// so we use the maxDelay/maxDuration to detect if there has *ever* been
			// any length at all to the "animation".
			return _maxDelay == 0.0f && _maxDuration == 0.0f;
		}
	}

	public void Update() 
	{
		// Use unscaledDeltaTime so that we don't get affected by slow motion effects
		// etc, but on the other hand, don't allow it to be a large delta - unscaledDeltaTime
		// can get the absolutely true/real time between frames, which when the game gets
		// stalled/paused for some reason can give us unexpected results.
		float dt = Mathf.Min(Time.unscaledDeltaTime, 1.0f/15.0f);
		_time += dt;

		if( isComplete )
			return;

		if( _properties != null ) {
			foreach(var property in _properties) {

				float lerpValue = 0.0f;
				if( _time > property.delay ) {
					lerpValue =  Mathf.Clamp01((_time-property.delay) / property.duration);

					// TODO: Allow different curves?
					if( _customCurve != null ) {
						lerpValue = _customCurve.Evaluate(lerpValue);
					} else {
						lerpValue = Mathf.SmoothStep(0.0f, 1.0f, lerpValue);
					}

					property.Animate(lerpValue);
				}
			}
		}

		if( timeIsUp )
			Done();
	}

	public void CompleteImmediate()
	{
		if( _properties != null )
			foreach(var property in _properties) 
				property.Animate(1.0f);
		
		Done();
	}

	void Done() 
	{
		_completed = true;

		RemoveAnimFromAllProperties();

		if( _completionAction != null )
			_completionAction();
	}
		
	List<SAnimatedProperty> _properties;

	float _time;

	float _duration;
	float _delay;

	float _maxDuration;
	float _maxDelay;

	AnimationCurve _customCurve;
	Action _completionAction;
	bool _completed;
	SLayout _owner;

	static List<SLayoutAnimation> _animationsBeingDefined;
}