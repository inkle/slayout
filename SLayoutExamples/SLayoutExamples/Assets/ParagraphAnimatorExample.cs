using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParagraphAnimatorExample : MonoBehaviour {

	[TextArea]
	public string paragraphText;
	public SLayout wordPrefab;

	const float spaceWidth = 10.0f;
	const float margin = 20;
	const float lineHeight = 50;

	void Start() {
		_layout = GetComponent<SLayout>();

		CreateWords();

		Animate();
	}

	void CreateWords() {
		var words = paragraphText.Split(' ');

		_wordLayouts = words.Select(word => {
			SLayout wordLayout = Instantiate(wordPrefab);
			wordLayout.transform.SetParent(transform, worldPositionStays:false);
			wordLayout.text.text = word;
			wordLayout.width = wordLayout.text.preferredWidth;
			return wordLayout;
		}).ToArray();
	}

	void Animate() {

		// Static, non-animated layout
		LayoutWords(paragraphWidth:200.0f, color:Color.clear);

		// Animate into position with a new paragraph width
		_layout.Animate(1.0f, 1.0f, () => {
			LayoutWords(paragraphWidth:500.0f, color:Color.black);
		}, () => {

			// Drift away and fade out
			_layout.Animate(1.0f, 1.0f, () => {
				for(int i=_wordLayouts.Length-1; i>=0; i--) {
					var wordLayout = _wordLayouts[i];
					wordLayout.alpha = 0.0f;
					wordLayout.x = 1.2f * wordLayout.x + 50.0f;
					wordLayout.rotation = -5.0f;
					_layout.AddDelay(0.05f);
				}
			});
		});

		// Repeat the whole sequence again
		_layout.After(8.0f, Animate);
	}

	void LayoutWords(float paragraphWidth, Color color)
	{
		float x = margin;
		float y = margin;

		foreach(var wordLayout in _wordLayouts) {

			var nextX = x;
			var nextY = y;

			if( nextX + wordLayout.width > paragraphWidth ) {
				nextX = margin;
				nextY += lineHeight;
			}

			wordLayout.x = nextX;
			wordLayout.y = nextY;
			wordLayout.color = color;
			wordLayout.rotation = 0;

			x = nextX + wordLayout.width + spaceWidth;
			y = nextY;

			// Delay before next word animates in
			// (When not animating, this does nothing)
			_layout.AddDelay(0.05f);
		}
	}

	SLayout _layout;
	SLayout[] _wordLayouts;
}
