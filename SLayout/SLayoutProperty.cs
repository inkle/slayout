using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

/// <summary>
/// Wraps an accessor to a particular property on the SLayout (e.g. x, width, color, etc) that can be 
/// animated, so that the animation can detect when properties are changed, and cause those properties to be animated.
/// </summary>
public abstract class SLayoutProperty<T> 
{
	public Func<T> getter;
	public Action<T> setter;

	public T value {
		get {
			return getter();
		}
		set {

			var currentAnim = SLayoutAnimation.AnimationUnderDefinition();
			if( currentAnim != null )
				currentAnim.SetupPropertyAnim<T>(this);

			setter(value);
		}
	}

	public abstract T Lerp(T v0, T v1, float t);
		
	// When this property is being animated, it receives an instance
	// of a SAnimatedProperty, which contains the start value,
	// target value, the delay and the duration.
	public SAnimatedProperty<T> animatedProperty;
}

public class SLayoutFloatProperty : SLayoutProperty<float> {
	public override float Lerp(float v0, float v1, float t) {
		// Don't use Mathf.Lerp since we want to allow extrapolation, not clamped
		return v0 + t * (v1-v0);
	}
}

public class SLayoutAngleProperty : SLayoutProperty<float> {
	public override float Lerp(float v0, float v1, float t) {
		return Mathf.LerpAngle(v0, v1, t);
	}
}

public class SLayoutColorProperty : SLayoutProperty<Color> {
	public override Color Lerp(Color v0, Color v1, float t) {
		return Color.Lerp(v0, v1, t);
	}
}