using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SimpleExamples : MonoBehaviour {

	public SLayout moveRight;
	public SLayout scaleRotateColor;
	public SLayout separateAnimsSeparateProperties;
	public SLayout withCompletion;

	public AnimationCurve parabola;

	void Start() {
		_layout = GetComponent<SLayout>();

		var animateButton = GetComponentInChildren<Button>();
		animateButton.onClick.AddListener(Animate);
	}
				
	void Animate() {

		_forward = !_forward;

		// 1. Left/right animation
		_layout.Animate(0.5f, () => moveRight.x = _forward ? 100.0f : 500.0f);

		// 2. Spin, change colour
		_layout.Animate(1.0f, () => {
			if( _forward ) {
				scaleRotateColor.rotation = 120.0f;
				scaleRotateColor.scale = 1.5f;
				scaleRotateColor.color = Color.white;
			} else {
				scaleRotateColor.rotation = 0;
				scaleRotateColor.scale = 1.0f;
				scaleRotateColor.color = Color.black;
			}
		});

		// 3. Animating x and y separately, and use custom animation curve
		var s = separateAnimsSeparateProperties;
		if( !s.isAnimating ) {
			s.Animate(2.0f, () => {
				if( s.x < 0.5f * _layout.width )
					s.x = 0.75f * _layout.width;
				else
					s.x = 0.25f * _layout.width;
			});
			s.Animate(2.0f, 0.0f, parabola, () => {
				s.y += 200.0f;
			});
		}

		// 4. With completion callback
		if( !withCompletion.isAnimating ) {
			withCompletion.Animate(0.5f, () => {
				withCompletion.width = _forward ? 400.0f : 100.0f;
			}, () => {
				withCompletion.Animate(0.3f, 0.0f, parabola, () => withCompletion.scale = 1.2f);
			});
		}
	}
		
	bool _forward;
	SLayout _layout;
}
