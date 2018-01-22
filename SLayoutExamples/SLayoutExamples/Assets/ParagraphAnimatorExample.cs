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
	const float lineWidth = 500;

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
		LayoutWords(Color.clear, offset:250, rotation:-5);

		// Animate into position with a new paragraph width
		_layout.Animate(0.5f, () => LayoutWords(Color.black, offset:150, rotation:0), completeAction:() => {

			// Animate out again in completion callback after a 2 second pause
			_layout.Animate(0.5f, 2.0f, () => LayoutWords(Color.clear, offset:50, rotation:+5));
		});

		// Repeat the whole sequence again
		_layout.After(6.0f, Animate);
	}

	void LayoutWords(Color color, float offset, float rotation)
	{
		float x = margin;
		float y = margin;

		foreach(var wordLayout in _wordLayouts) {

			var nextX = x;
			var nextY = y;

			if( nextX + wordLayout.width > lineWidth ) {
				nextX = margin;
				nextY += lineHeight;
			}

			wordLayout.x = nextX + offset;
			wordLayout.y = nextY;
			wordLayout.color = color;
			wordLayout.rotation = rotation;

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
