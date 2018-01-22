# SLayout

SLayout is a Unity component that can be added alongside any RectTransform, and provides:

* **Easier, more convenient layout properties:** using a consistent coordinate system that works independently from a RectTransform’s anchoring, you can use properties like `x` to always refer to the distance from the parent rect’s left edge to the given rect’s edge (a bit like Unity’s IMGUI).
* **Animation:** a system inspired by iOS’s Core Animation allows easy animation/tweening when using the above properties.

![](https://github.com/inkle/slayout/blob/master/slayout.gif)

## Getting started

The core of SLayout is its set of [root properties](#root-animatable-properties) such as `x`, `height` and `color`. Partly, these are simply convenience properties that wrap calls to the RectTransform (and any Text/Image/CanvasGroup) on your object, and use a simplified rect-based [coordinate system](#coordinate-system) that doesn't depend on the RectTransform's anchoring.

For example:

    var layout = GetComponent<SLayout>();
    
    // Position RectTransform’s left edge at 50px from parent’s left edge
    layout.x = 50.0f;
    
    // Set the height to 150px, extending from the bottom edge upwards
    layout.height = 150.0f;

However, using these properties consistently (instead of direct calls to the RectTransform) also allows you use SLayout's [animation methods](#animation-reference). Each property does a quick check in the setter to see whether an animation is currently being defined, and then automatically lerps/tweens to the given values if so:

    
    // Animate x to 100px, height to 50px, easing with a duration 0.5 seconds
    layout.Animate(0.5f, () => {
    	layout.x = 100.0f;
    	layout.height = 50.0f;
    });

There are a number of advantages to this technique. You don't have to learn a new syntax to an Animate method - you simply provide a duration and then in the animation method set the properties using the same code that you would use if you weren't animating. It also allows you to re-use code that you use to lay out views dynamically, even if the method does other things besides simply setting properties. You can even include loops in your animation code, positioning a set of views in one single `Animate` call.

As mentioned above, the animation system was inspired by Apple's Core Animation system, and this is how iOS fluidly resizes views when the orientation of a device changes - it simply calls a standard layout method from within an animation block.

For example, you could make your animation call like this:

    layout.Animate(0.5f, RefreshLayout);
    
Which calls a standard `RefreshLayout()`, which may sometimes by called without animation (such as in `Start()`), and sometimes with animation as above:
    
    void RefreshLayout()
    {
    	layout.width = layout.parentWidth;
    	layout.height = 0.25f * layout.parentHeight;
    	layout.color = _selected ? Color.yellow : Color.white;
    }

You can also animate the values on another SLayout, such as a child:

    layout.Animate(0.5f, () => {
        childA.x = 100.0f;
        childB.color = Color.Red;
    });

The animation works by snapshotting the initial state of a property right before it's actually changed, and then snapshotting the state when the method that defines the animation is complete. It then automatically lerps (tweens) between them on subsequent frames.

### A slightly more complex example

This example is in the [SLayoutExamples project](https://github.com/inkle/slayout/tree/master/SLayoutExamples/SLayoutExamples) and is demonstrated in the gif:

![](https://github.com/inkle/slayout/blob/master/slayout.gif)

We define the static layout function which positions all the individual word view using word wrapping. It takes 3 parameters which define how we're going to animate the individual words:

	void LayoutWords(Color color, float offset, float rotation)
	{
		float x = margin;
		float y = margin;

		foreach(var wordLayout in _wordLayouts) {

			var nextX = x;
			var nextY = y;

			// Word wrap when we exceed our line length
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
	
Then, here's the setup to create the looping animation, which is initially kicked off from the `Start()` method:

	void Animate() {

		// Static, non-animated layout
		LayoutWords(Color.clear, offset:250, rotation:-5);

		// Animate into position with a new paragraph width
		_layout.Animate(0.5f, () => LayoutWords(Color.black, offset:150, rotation:0), 
			completeAction:() => {
			
				// Animate out again in completion callback after a 2 second pause
				_layout.Animate(0.5f, 2.0f, () => 
					LayoutWords(Color.clear, offset:50, rotation:+5)
				);
			}
		);

		// Repeat the whole sequence again
		_layout.After(6.0f, Animate);
	}

For a few more examples, take a look in the full [Animation reference](#animation-reference).

## Coordinate system

SLayout provides a choice of two different coordinate systems, but by default it simply matches the normal UI system, where the Y axis points up the screen. To toggle between them, use the `originTopLeft` field on the `SLayout`, either in code or in the inspector.

Importantly though, the coordinate system is consistent, no matter how the anchoring is set up on the RectTransform.

* **Origin bottom left (default)**: `(0,0)` is in the bottom left corner of the parent RectTransform (the parent doesn't need to be an SLayout), with positive y going up the screen.

    This matches the way that Unity's UI system works.

* **Origin top left**: Sometimes it's useful to flip the Y axis, so that the origin is in the top left of the parent rect, with positive Y values going down the screen.

    This matches Unity's legacy/Editor GUI system, and can make a lot more sense when you want to dynamically lay out a vertical flow that goes down the screen - for example paragraphs of text, or pretty much any other UI that "stacks".

# Reference

### Root animatable properties

All these properties get/set values on the RectTransform, CanvasGroup or Graphic directly - values aren't cached on the SLayout, except when animating.

* `x` the distance from the left edge of the rect to the left edge of the parent rect.
* `y` the distance from the bottom edge to the bottom edge of the parent rect, or when `originTopLeft` is true, `y` is the distance between he top edges.
* `width` and `height` give the size of the RectTransform's rect. 

* `rotation` - single float maps to the transform's z euler rotation.
* `scale` - single float maps to all three scale components on the transform.
* `groupAlpha` - Tries to get/set a CanvasGroup's alpha property. Ignored if no CanvasGroup exists, it does nothing when setting, and returns `1.0` when getting.
* `color` - Tries to get/set the color property of a Graphic component on the same object - for example, a Text or Image component. If no Graphic exists, it does nothing when setting, and returns `Color.white` when getting.

### Convenience animatable properties

To make your code shorter and easier to read - these properties are simple wrappers for the root properties.

All of these properties are animatable.

* `rightX` - distance from right edge to parent rect's left edge.
* `topY` - distance from top edge to parent's bottom edge. When using `originTopLeft`, it's the distance from the top edge to the parent's top edge.
* `bottomY` - distance from bottom edge to parent's bottom edge. When using `originTopLeft`, it's the distance from the bottom edge to the parent's top edge.
* `position` - `Vector2` of the root properties `x` and `y`.
* `size` - `Vector2` of the root properties `width` and `height`.
* `rect` - `Rect` of root properties `x`, `y`, `width` and `height`.
* `localRect` - `Rect` that uses `width` and `height`. When getting, position is always `(0, 0)`. When setting, the position is used as an offset from the existing position.
* `centerX` - `x + 0.5f * width` - doesn't use the RectTransform's pivot position.
* `centerY` - `y + 0.5f * height` - doesn't use the RectTransform's pivot position.
* `center` - `Vector2` of `centerX` and `centerY`.
* `originX` - x position of own pivot in parent's rect.
* `originY` - y position of own pivot in parent's rect.
* `origin` - `Vector2` of `originX` and `originY`.


### Cached/other properties

SLayout caches certain component references for easy/quick access and also provides a few other related properties:

* `graphic` - internally obtained using `GetComponent<Graphic>`
* `image` - attempts to cast the internal Graphic reference: `graphic as Image`.
* `text` - attempts to cast the internal Graphic reference: `graphic as Text`.
* `rectTransform` - calls `transform as RectTransform`.
* `parentRectTransform` - calls `transform.parent as RectTransform`.
* `parent` - calls `transform.parent.GetComponent<SLayout>()`
* `parentRect` - Even if parent doesn't have an SLayout, this will be correct (as long as it is a RectTransform).
* `canvas` - internally obtained using `transform.GetComponentInParent<Canvas>()`.
* `canvasWidth`, `canvasHeight`, `canvasSize` - convenience properties that access the size of the current canvas.
* `canvasGroup` - internally obtained using `GetComponent<CanvasGroup>`

### Target properties

The following properties are provided to get the final property value during an animation. When an animation isn't active, it just returns the current property value.

`targetX`, `targetY`, `targetWidth`, `targetHeight`, `targetPosition`, `targetSize`, `targetRect`, `targetRotation`, `targetScale`, `targetGroupAlpha`, `targetAlpha`  `targetColor`, `targetLocalRect`, `targetCenter`, `targetCenterX`, `targetCenterY`, `targetRightX`, `targetBottomY`, `targetTopY`

## Animation reference

### `Animate()`

All `Animate()` methods take a duration in seconds, and an animation definition Action that you use to perform animation property changes. Every `Animate()` method also takes an optional completion callback that will be called when the animation finishes.

For more information on how to use `Animate`, see [Getting Started](#getting-started).

---

`void Animate(float duration, Action animAction, Action completeAction = null)`

The standard `Animate` method.

---

`void Animate(float duration, float delay, System.Action animAction, System.Action completeAction = null)`

As above, except this overload of `Animate` also takes a `delay` parameter - a number of seconds to wait before starting the animation.

---

`void Animate(float duration, float delay, AnimationCurve customCurve, System.Action animAction, System.Action completeAction = null)`

As above, except this overload of `Animate` can also take an `AnimationCurve` which defines the easing function. The curve should be defined between 0.0 and 1.0 seconds. It can "overshoot" above and below 0.0 and 1.0 on the y axis, although some properties will clamp, such as colors.

---

### `AddDelay` and `AddDuration`

While an animation is being defined, add an extra delay/duration (in addition to any existing delay/duration that has already been defined) before animating any subsequent properties that are set after this call.

---

`void AddDelay(float extraDelay)`

Example:
    
    // After an initial delay of 1 second, start positioning
    // each word one after another. Each word has an delay of 0.1s
    // before it starts animating in, and each word will take 0.5s
    // to animate in.
    layout.Animate(0.5f, 1.0f, () => {
        float x = 0.0f;
        foreach(SLayout wordLayout in _wordLayouts) {
            wordLayout.x = x;
            x += wordLayout.width + 10.0f;
            AddDelay(0.1f);
        }
    });
    
---

`void AddDuration(float extraDuration)`

Example:
    
    // Over 0.3 seconds move layout to x=100, and over 1.0 seconds
    // animate its height to 150px.
    layout.Animate(0.3f, () => {
        layout.x = 100.0f;
        AddDuration(0.7f);
        layout.height = 150.0f;
    });
    
    // Effectively equivalent to the following, though
    // internally the animations are set up differently:
    layout.Animate(0.3f, () => layout.x = 100.0f);
    layout.Animate(1.0f, () => layout.height = 150.0f);
    
---

### `AnimateCustom`

`void AnimateCustom(float duration, System.Action<float> customAnimAction, System.Action completeAction = null)`

Allows something else to be animated besides the built in SLayout UI properties. This isn't as convenient to use as the built in properties since you have to handle the lerping/tweening yourself.

The `customAnimAction` method is called every frame, passing a normalised time `t` between 0 and 1 for the progress through the animation.

**See also:** [`Animatable`](#animatable), which depending on circumstance, could be more convenient to use than this "full manual" approach.

Example:
    
    // Move a GameObject from y=0 to y=100 over 0.5 seconds
    layout.AnimateCustom(0.5f, t => {
        var lerped = Mathf.Lerp(100.0f, 200.0f, t);
        transform.position = new Vector3(0, lerped, 0);
    });

---

`void AnimateCustom(float duration, float delay, System.Action<float> customAnimAction, System.Action completeAction = null)`

As above, but allows an optional extra `delay` in seconds before beginning the animation.

---

### `Animatable`

Allow a custom values to be animated as part of an animation definition. 

**See also:** [`AnimateCustom`](#animatecustom), which may be more useful for a fully custom animation. `Animatable` may be more useful when you already have an animation you're defining, and you also want to animate some other value that isn't a standard property.

---

`static void Animatable(Action<float> customAnim)`

If an animation is currently being defined using `Animate`, then the `customAnim` delegate will be called every frame that the animation is running, passing the normalised animation time `t`. If no animation is being defined, then it will simply call the callback immediately, passing `1.0` to ensure that it is set to its final value.

Example:

    layout.Animate(0.5f, () => {
    
        // Move as usual using an ordinary property
        layout.x = 100.0f;
        
        // Animate something as part of the definition that isn't animatable
        // using the usual properties.
        SLayout.Animatable(t => {
            someTransform.position = new Vector3(0, 100.0f * t, 0));
        });
    });
    
    
---

`static void Animatable(float initial, float target, Action<float> setter)`

Allow a custom float to be animated between an initial value and a target value, as part of an animation definition. The setter delegate method takes the actual value rather than the normalised value from the animation, meaning you don't have to do any lerping yourself.

Example:

    layout.Animate(0.5f, () => {
    
        // Move as usual using an ordinary property
        layout.x = 100.0f;
        
        // Animate the value of _health from its current value to 100 
        // over the given 0.5 seconds of the rest of the animation.
        SLayout.Animatable(_health, 100.0f, val => _health = val);
    });
    
---

`static void Animatable(Color initial, Color target, Action<Color> setter)`

Allow a custom Color to be animated between an initial value and a target value, as part of an animation definition. The setter delegate method takes the actual value rather than the normalised value from the animation, meaning you don't have to do any lerping yourself.

##### Example:

    layout.Animate(0.5f, () => {
    
        // Move as usual using an ordinary property
        layout.x = 100.0f;
        
        // Animate the color of a LineRenderer from white to yellow
        // over 0.5 seconds, as defined by the main animation
        SLayout.Animatable(Color.white, Color.yellow, c => lineRenderer.color = c);
    });

---

### `CancelAnimations` / `CompleteAnimations`

End any animations that were defined and active on the given `SLayout`. Note that an `SLayout` may animate the values of another `SLayout`, and these will only be cancelled when `CancelAnimations()` or `CompleteAnimations()` is called on the former, not the latter.

* `CancelAnimations()` leaves property values in their partially animated state.
* `CompleteAnimations()` leaves property values in their final state as defined by the animation, jumping the animation forward if necessary.

Example:

    // Color animation
    layoutA.Animate(0.5f, () => {
        layoutA.color = Color.red;
        layoutB.color = Color.red;
    });
    
    // Position animation
    layoutB.Animate(0.5f, () => {
        layoutA.x = 100.0f;
        layoutB.x = 100.0f;
    });
    
    // Cancels/completes the color animation on both but not the position 
    // animation on either.
    layoutA.CancelAnimations(); // OR: layoutA.CompleteAnimations();
    
