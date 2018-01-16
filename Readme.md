# SLayout

SLayout is a Unity component that can be added alongside any RectTransform, and provides:

* **Easier, more convenient layout properties:** using a consistent coordinate system that works independently from a RectTransform’s anchoring, you can use properties like `x` to always refer to the distance from the parent rect’s left edge to the given rect’s edge (a bit like Unity’s IMGUI).
* **Animation:** a system inspired by iOS’s Core Animation allows easy animation/tweening when using the above properties.

## Examples

**Simple positioning and animation:**

    var layout = GetComponent<SLayout>();
    
    // Position RectTransform’s left edge at 50px from parent’s left edge
    layout.x = 50.0f;
    
    // Animate x to 100px, height to 50px, easing with a duration 0.5 seconds
    layout.Animate(0.5f, () => {
    	layout.x = 100.0f;
    	layout.height = 50.0f;
    });

If you’re wondering how that Animate method works, see [How animation works](#How-animation-works) below.

**Using a method when refreshing layout**

    layout.Animate(0.5, RefreshLayout);
    
    ...
    
    void RefreshLayout()
    {
    	layout.width = layout.parentWidth;
    	layout.height = 0.25f * layout.parentHeight;
    	layout.color = _selected ? Color.yellow : Color.white;
    }


2-3 animated examples

For more examples, see the full [Animation reference](#animation-reference);

## Coordinate system

SLayout provides a choice of two different coordinate systems, but by default it simply matches the normal UI system. To toggle between them, use the `originTopLeft` field on the `SLayout`, either in code or in the inspector.

Importantly though, the coordinate system is consistent, no matter how the anchoring is set up on the RectTransform.

* **Origin bottom left (default)**: `(0,0)` is in the bottom left corner of the parent RectTransform (the parent doesn't need to be an SLayout), with positive y going up the screen.

    This matches the way that Unity's UI system works.

* **Origin top left**: Sometimes it's useful to flip the Y axis, so that the origin is in the top left of the parent rect, with positive Y values going down the screen.

    This matches Unity's legacy/Editor GUI system, and can make a lot more sense when you want to dynamically lay out a vertical flow that goes down the screen - for example paragraphs of text, or pretty much any other UI that "stacks".


## How animation works

SLayout's animation system is inspired by Apple's Core Animation, which "instruments" a number of properties from a view, allowing them to be animated when they're changed from within the `Animate` callback. It does  this by snapshotting the initial state of a property right before it's actually changed, and then snapshotting the state when the method that defines the animation is complete. It then automatically lerps (tweens) between them on subsequent frames.

There are a number of advantages to this technique. You don't have to learn a new syntax to an Animate method - in simple cases you simply provide a duration and then in an anonymous method set the properties using the same code you would use if you weren't animating. It also allows you to re-use code that you use to lay out views dynamically. For example, this is how iOS fluidly resizes views when the orientation of a device changes - it simply calls a standard layout method from within an animation block.

# Reference

### Root properties

All these properties get/set values on the RectTransform, CanvasGroup or Graphic directly - values aren't cached on the SLayout, except when animating.

* `x` the distance from the left edge of the rect to the left edge of the parent rect.
* `y` the distance from the bottom edge to the bottom edge of the parent rect, or when `originTopLeft` is true, `y` is the distance between he top edges.
* `width` and `height` give the size of the RectTransform's rect. 

* `rotation` - single float maps to the transform's z euler rotation.
* `scale` - single float maps to all three scale components on the transform.
* `groupAlpha` - Tries to get/set a CanvasGroup's alpha property. Ignored if no CanvasGroup exists, it does nothing when setting, and returns `1.0` when getting.
* `color` - Tries to get/set the color property of a Graphic component on the same object - for example, a Text or Image component. If no Graphic exists, it does nothing when setting, and returns `Color.white` when getting.

### Convenience properties

To make your code shorter and easier to read!

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


### Target properties

A number of properties such as `targetX`, `targetY`, `targetColor` etc are provided to get the final property value during an animation. When an animation isn't active, it just returns the current property value.

### Cached/Other properties

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


## Animation reference

#### `void Animate(float duration, Action animAction, Action completeAction = null)`

The standard `Animate` method, taking a duration in seconds, an animation action definition action that contains animation property changes, and an optional callback that will be called when the animation finishes.

---

#### `public void Animate(float duration, float delay, System.Action animAction, System.Action completeAction = null)`

As above, except this overload of `Animate` also takes a `delay` parameter - a number of seconds to wait before starting the animation.

---

#### `public void Animate(float duration, float delay, AnimationCurve customCurve, System.Action animAction, System.Action completeAction = null)`

As above, except this overload of `Animate` can also take an `AnimationCurve` which defines the easing function. The curve should be defined between 0.0 and 1.0 seconds. It can "overshoot" above and below 0.0 and 1.0 on the y axis, although some properties will clamp, such as colors.

---

#### `public void AnimateCustom(float duration, System.Action<float> customAnimAction, System.Action completeAction = null)`

Allows something else to be animated besides the built in SLayout UI properties. This isn't quite as convenient to use as the built in properties but may nonetheless be helpful.

The `customAnimAction` method is called every frame, passing a normalised time `t` between 0 and 1 for the progress through the animation.

**See also:** `Animatable`

##### Example:
    
    // Move a GameObject from y=0 to y=100 over 0.5 seconds
    layout.AnimateCustom(0.5f, t => {
        transform.position = new Vector3(0, 100.0f * t, 0);
    });
    
---

#### `public void AnimateCustom(float duration, float delay, System.Action<float> customAnimAction, System.Action completeAction = null)`

As above, but allows an optional extra `delay` in seconds before beginning the animation.

---

#### `public void AddDelay(float extraDelay)`

While an animation is being defined, add a delay (in addition to any existing delay that has already been defined) before animating any further properties that are changed.

##### Example:
    
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

#### `public void AddDuration(float extraDuration)`

While an animation is being defined, add an extra duration (in addition to the initial duration that was defined) for any further properties that are changed.

##### Example:
    
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

#### `public static void Animatable(Action<float> customAnim)`

Allow a custom value to be animated as part of an animation definition. If an animation is currently being defined using `Animate`, then it will used the callback every frame that the animation is running, passing the normalised animation time `t`. If no animation is being defined, then it will simply call the callback immediately, passing `1.0` to ensure that it is set to its final value.

**See also:** `AnimateCustom`, which may be more useful for a fully custom animation. `Animatable` may be more useful when you already have an animation you're defining, and you want to animate some other value that isn't a standard property.

##### Example:

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

#### `public static void Animatable(float initial, float target, Action<float> setter)`

Allow a custom float to be animated between an initial value and a target value, as part of an animation definition. If an animation is currently being defined using `Animate`, then it will used the callback every frame that the animation is running, passing the normalised animation time `t`. If no animation is being defined, then it will simplycall the callback immediately, passing `1.0` to ensure that it is set to its final value.

**See also:** `AnimateCustom`, which may be more useful for a fully custom animation. `Animatable` may be more useful when you already have an animation you're defining, and you want to animate some other value that isn't a standard property.

##### Example:

    layout.Animate(0.5f, () => {
    
        // Move as usual using an ordinary property
        layout.x = 100.0f;
        
        // Animate the value of _health from 80 to 100 over the given
        // 0.5 seconds of the rest of the animation
        SLayout.Animatable(80.0f, 100.0f, val => _health = val);
    });
    
---

#### `public static void Animatable(Color initial, Color target, Action<Color> setter)`

Allow a custom color to be animated between an initial value and a target value, as part of an animation definition. If an animation is currently being defined using `Animate`, then it will used the callback every frame that the animation is running, passing the normalised animation time `t`. If no animation is being defined, then it will simplycall the callback immediately, passing `1.0` to ensure that it is set to its final value.

**See also:** `AnimateCustom`, which may be more useful for a fully custom animation. `Animatable` may be more useful when you already have an animation you're defining, and you want to animate some other value that isn't a standard property.

##### Example:

    layout.Animate(0.5f, () => {
    
        // Move as usual using an ordinary property
        layout.x = 100.0f;
        
        // Animate the color of a LineRenderer from white to yellow
        // over 0.5 seconds, as defined by the main animation
        SLayout.Animatable(Color.white, Color.yellow, c => lineRenderer.color = c);
    });

---

#### `public void CancelAnimations()`

End any animations that were defined and active on the given `SLayout`. Note that an `SLayout` may animate the values of another `SLayout`, and these will only be cancelled when `CancelAnimations()` is called on the former, not the latter.

The properties will be left in their partially animated state. To fully complete an animation early, use `CompleteAnimations()`.

##### Example:

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
    
    // Cancels the color animation on both but not the position 
    // animation on either.
    layoutA.CancelAnimations();
    
---

#### `public void CompleteAnimations()`

Complete any animations that were defined and active on the given `SLayout`, setting them in their final state as defined by the animation. Note that an `SLayout` may animate the values of another `SLayout`, and these will only be completed when `CompleteAnimations()` is called on the former, not the latter.

To cancel an animation leaving its properties in a partial state rather than setting them in their final state, use `CancelAnimations()`.

##### Example:

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
    
    // Completes the color animation on both but not the position 
    // animation on either.
    layoutA.CompleteAnimations();
