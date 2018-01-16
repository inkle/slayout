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

(See Animation for an explanation of exactly how this works!)

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

`void Animate(float duration, Action animAction, Action completeAction = null)`

