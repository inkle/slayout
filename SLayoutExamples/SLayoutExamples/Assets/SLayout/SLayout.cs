#if DEBUG
//#define DEBUG_SLAYOUT
#endif

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;


/// <summary>
/// SLayout does two things:
///  - Provides an easy-to-use interface for dynamic UI Layout that uses a consistent coordinate space for positioning
///    UI rather than being dependent on Unity's anchoring etc.
///  - Allows iOS-style "implicit animation" by wrapping calls to change the layout in an animation function, causing
///    the elements to animate to the target setup.
///  - Partial so you can compile in your own shortcut properties (e.g. for TextMeshPro), but without having a full
///    dependency on 3rd party APIs.
/// </summary>
[ExecuteInEditMode]
public partial class SLayout : UIBehaviour {

	/// <summary>
	/// Normally, the Unity origin of bottom left is used, but sometimes it's useful to put
	/// the origin in the top left with the Y axis increasing as it does downwards, similar to
	/// on the web or on iOS. It's useful for laying out content progressively from the top of,
	/// the canvas for example for text layout or other document content, since at the end you
	/// know the expected height of the parent layout to fit that content.
	/// </summary>
	public bool originTopLeft;

	/// <summary>
	/// Useful to detect changes due to the auto layout system changing the size and position of
	/// a top level RectTransform due to non-SLayout related things like screen size changing or
	/// simply something else controlling the (top level) view.
	/// </summary>
	public event Action<SLayout> onRectChange;

	public bool isAnimating {
		get {
			return SLayoutAnimator.instance.IsAnimating(this);
		}
	}

	protected override void OnRectTransformDimensionsChange() {
		base.OnRectTransformDimensionsChange();
		if( onRectChange != null ) onRectChange(this);
	}

	public void Animate(float duration, System.Action animAction, System.Action completeAction = null)
	{
		Animate(duration, 0.0f, animAction, completeAction);
	}

	public void Animate(float duration, float delay, System.Action animAction, System.Action completeAction = null)
	{
		Animate(duration, delay, null, animAction, completeAction);
	}

	public void After(float delay, System.Action completeAction)
	{
		Animate(0.0f, delay, null, null, completeAction);
	}

	public void Animate(float duration, float delay, AnimationCurve customCurve, System.Action animAction, System.Action completeAction = null)
	{
		SLayoutAnimator.instance.Animate(duration, delay, customCurve, animAction, completeAction, this);
	}

	public void AnimateCustom(float duration, System.Action<float> customAnimAction, System.Action completeAction = null)
	{
		SLayoutAnimator.instance.Animate(duration, 0.0f, null, () => Animatable(customAnimAction), completeAction, this);
	}

	public void AnimateCustom(float duration, float delay, System.Action<float> customAnimAction, System.Action completeAction = null)
	{
		SLayoutAnimator.instance.Animate(duration, delay, null, () => Animatable(customAnimAction), completeAction, this);
	}

	/// <summary>
	/// While an animation is currently being defined, insert an extra delay, so that any animated values
	/// that are set after this call begin a bit later than previously defined elements.
	/// </summary>
	public void AddDelay(float extraDelay)
	{
		SLayoutAnimator.instance.AddDelay(extraDelay);
	}

	/// <summary>
	/// While an animation is currently being defined, insert an extra duration, so that any animated values
	/// that are set after this call have a longer duration than those defined before it.
	/// </summary>
	public void AddDuration(float extraDuration)
	{
		SLayoutAnimator.instance.AddDuration(extraDuration);
	}

	/// <summary>
	/// Allow a custom value to be animated.
	/// If an animation is currently being defined using layout.Animate, then it 
	/// will used the callback every frame that the animation is running, passing 
	/// the normalised time. If no animation is being defined, then it will simply 
	/// call the callback immediately, passing 1.0 to ensure that it is set to its 
	/// final value.
	/// </summary>
	public static void Animatable(Action<float> customAnim)
	{
		SLayoutAnimator.instance.Animatable(customAnim);
	}

	/// <summary>
	/// Allow a custom float value to be animated, starting at initial, and tweening
	/// to the target. The callback "setter" must be passed which sets the float to
	/// its intermediate value during animation.
	/// If no animation is currently being defined, then the value will immediately
	/// be set to its target value.
	/// </summary>
	public static void Animatable(float initial, float target, Action<float> setter)
	{
		SLayoutAnimator.instance.Animatable(t => setter(Mathf.LerpUnclamped(initial, target, t)));
	}

	/// <summary>
	/// Allow a custom color value to be animated, starting at initial, and tweening
	/// to the target. The callback "setter" must be passed which sets the color to
	/// its intermediate value during animation.
	/// If no animation is currently being defined, then the color will immediately
	/// be set to its target value.
	/// </summary>
	public static void Animatable(Color initial, Color target, Action<Color> setter)
	{
		SLayoutAnimator.instance.Animatable(t => setter(Color.Lerp(initial, target, t)));
	}

	public void CancelAnimations()
	{
		SLayoutAnimator.instance.CancelAnimations(this);
	}

	public void CompleteAnimations()
	{
		SLayoutAnimator.instance.CompleteAnimations(this);
	}

	public Canvas canvas {
		get {
			if( _canvas == null )
				_canvas = transform.GetComponentInParent<Canvas>();
			return _canvas;
		}
	}
	Canvas _canvas;

	/// <summary>
	/// Width of canvas, taking into account scaling mode.
	/// </summary>
	public float canvasWidth {
		get {
			return ((RectTransform)canvas.transform).sizeDelta.x;
		}
	}

	/// <summary>
	/// Height of canvas, taking into account scaling mode.
	/// </summary>
	public float canvasHeight {
		get {
			return ((RectTransform)canvas.transform).sizeDelta.y;
		}
	}

	public Vector2 canvasSize {
		get {
			return ((RectTransform)canvas.transform).sizeDelta;
		}
	}

	public CanvasGroup canvasGroup {
		get {
			if( _canvasGroup == null ) _canvasGroup = GetComponent<CanvasGroup>();
			return _canvasGroup;
		}
	}
	CanvasGroup _canvasGroup;

	public Image image {
		get {
			return graphic as Image;
		}
	}

	public Text text {
		get {
			return graphic as Text;
		}
	}

	public Graphic graphic {
		get {
			if( _graphic == null ) _graphic = GetComponent<Graphic>();
			return _graphic;
		}
	}
	Graphic _graphic;

	public SLayout parent {
		get {
			return transform.parent.GetComponent<SLayout>();
		}
	}

	public Rect parentRect {
		get {
			var parentRT = parentRectTransform;
			var localRect = parentRT.rect;
			return new Rect(GetRectTransformX(parentRT), GetRectTransformY(parentRT), localRect.width, localRect.height);
		}
	}

	public RectTransform parentRectTransform {
		get {
			return transform.parent as RectTransform;
		}
	}
		
	public float x {
		get {
			InitX();
			return _x.value;
		}
		set {
			InitX();
			_x.value = value; 
		}
	}

	public float targetX {
		get {
			InitX();
			return _x.animatedProperty != null ? _x.animatedProperty.end : _x.value;
		}
	}

	public float y {
		get {
			InitY();
			return _y.value;
		}
		set {
			InitY();
			_y.value = value;
		}
	}

	public float targetY {
		get {
			InitY();
			return _y.animatedProperty != null ? _y.animatedProperty.end : _y.value;
		}
	}

	public float width {
		get {
			InitWidth();
			return _width.value;
		}
		set {
			InitWidth();
			_width.value = value;
		}
	}

	public float targetWidth {
		get {
			InitWidth();
			return _width.animatedProperty != null ? _width.animatedProperty.end : _width.value;
		}
	}

	public float height {
		get {
			InitHeight();
			return _height.value;
		}
		set {
			InitHeight();
			_height.value = value;
		}
	}

	public float targetHeight {
		get {
			InitHeight();
			return _height.animatedProperty != null ? _height.animatedProperty.end : _height.value;
		}
	}

	public Vector2 position {
		get {
			return new Vector2(x, y);
		}
		set {
			x = value.x;
			y = value.y;
		}
	}

	public Vector2 targetPosition {
		get {
			return new Vector2(targetX, targetY);
		}
	}

	public Vector2 size {
		get {
			return new Vector2(width, height);
		}
		set {
			width = value.x;
			height = value.y;
		}
	}

	public Vector2 targetSize {
		get {
			return new Vector2(targetWidth, targetHeight);
		}
	}

	public float rotation {
		get {
			InitRotation();
			return _rotation.value;
		}
		set {
			InitRotation();
			_rotation.value = value;
		}
	}

	public float targetRotation {
		get {
			InitRotation();
			return _rotation.animatedProperty != null ? _rotation.animatedProperty.end : _rotation.value;
		}
	}

	public float scale {
		get {
			InitScale();
			return _scale.value;
		}
		set {
			InitScale();
			_scale.value = value;
		}
	}

	public float targetScale {
		get {
			InitScale();
			return _scale.animatedProperty != null ? _scale.animatedProperty.end : _scale.value;
		}
	}

	public float groupAlpha {
		get {
			InitGroupAlpha();
			return _groupAlpha.value;
		}
		set {
			InitGroupAlpha();
			_groupAlpha.value = value;
		}
	}

	public float targetGroupAlpha {
		get {
			InitGroupAlpha();
			return _groupAlpha.animatedProperty != null ? _groupAlpha.animatedProperty.end : _groupAlpha.value;
		}
	}

	public Color color {
		get {
			InitColor();
			return _color.value;
		}
		set {
			InitColor();
			_color.value = value;
		}
	}

	public Color targetColor {
		get {
			InitColor();
			return _color.animatedProperty != null ? _color.animatedProperty.end : _color.value;
		}
	}

	public float alpha {
		get {
			InitColor();
			return _color.value.a;
		}
		set {
			InitColor();
			var color = _color.value;
			color.a = value;
			_color.value = color;
		}
	}

	public float targetAlpha {
		get {
			return targetColor.a;
		}
	}

	public Rect rect {
		get {
			return new Rect(x, y, width, height);
		}
		set {
			x = value.x;
			y = value.y;
			width = value.width;
			height = value.height;
		}
	}

	public Rect targetRect {
		get {
			return new Rect(targetX, targetY, targetWidth, targetHeight);
		}
	}

	public Rect localRect {
		get {
			return new Rect(0.0f, 0.0f, width, height);
		}
		set {
			x = x + value.x;
			y = y + value.y;
			width = value.width;
			height = value.height;
		}
	}

	public Rect targetLocalRect {
		get {
			return new Rect(0.0f, 0.0f, targetWidth, targetHeight);
		}
	}

	public Vector2 center {
		get {
			return new Vector2(centerX, centerY);
		}
		set {
			centerX = value.x;
			centerY = value.y;
		}
	}

	public Vector2 targetCenter {
		get {
			return new Vector2(targetCenterX, targetCenterY);
		}
	}

	public float centerX {
		get {
			return x + 0.5f*width;
		}
		set {
			x = value - 0.5f*width;
		}
	}

	public float targetCenterX {
		get {
			return targetX + 0.5f*targetWidth;
		}
	}

	public float centerY {
		get {
			return y + 0.5f*height;
		}
		set {
			y = value - 0.5f*height;
		}
	}

	public float targetCenterY {
		get {
			return targetY + 0.5f*targetHeight;
		}
	}

	/// <summary>
	/// X position of own pivot in parent's space
	/// </summary>
	public float originX {
		get {
			float pivotX = rectTransform.pivot.x * width;
			return x + pivotX;
		}
		set {
			float pivotX = rectTransform.pivot.x * width;
			x = value - pivotX;
		}
	}

	/// <summary>
	/// Y position of own pivot in parent's space
	/// </summary>
	public float originY {
		get {
			float pivotY = rectTransform.pivot.y * height;
			return y + pivotY;
		}
		set {
			float pivotY = rectTransform.pivot.y * height;
			y = value - pivotY;
		}
	}

	/// <summary>
	/// Position of own pivot in parent's space
	/// </summary>
	public Vector2 origin {
		get {
			return new Vector2(originX, originY);
		}
		set {
			originX = value.x;
			originY = value.y;
		}
	}

	public float rightX {
		get {
			return x + width;
		}
		set {
			x = value - width;
		}
	}

	public float targetRightX {
		get {
			return targetX + targetWidth;
		}
	}

	public float bottomY {
		get {
			if( originTopLeft )
				return y + height;
			else
				return y;
		}
		set {
			if( originTopLeft )
				y = value - height;
			else
				y = value;
		}
	}

	public float targetBottomY {
		get {
			if( originTopLeft )
				return targetY + targetHeight;
			else
				return targetY;
		}
	}

	public float topY {
		get {
			if( originTopLeft )
				return y;
			else
				return y + height;
		}
		set {
			if( originTopLeft )
				y = value;
			else
				y = value - height;
		}
	}

	public float targetTopY {
		get {
			if( originTopLeft )
				return targetY;
			else
				return targetY + targetHeight;
		}
	}
		
	Vector2 GetPivotPos(RectTransform rt)
	{
		var rectSize = rt.rect.size;
		var rectPivot = rt.pivot;
		return new Vector2(
			rectSize.x * rectPivot.x,
			rectSize.y * rectPivot.y
		);
	}

	/// <summary>
	/// Converts a point in local space of this SLayout to the local space of another SLayout.
	/// If you pass a null SLayout, it will get the point in the space of the canvas.
	/// </summary>
	public Vector2 ConvertPositionToTarget(Vector2 localLayoutPos, SLayout targetLayout)
	{
		if( originTopLeft ) localLayoutPos.y = height - localLayoutPos.y;
		
		var localPos = localLayoutPos - GetPivotPos(rectTransform);
		var worldSpacePoint = rectTransform.TransformPoint(localPos);

		RectTransform targetRectTransform = targetLayout ? targetLayout.rectTransform : null;
		if( targetRectTransform == null ) targetRectTransform = canvas.transform as RectTransform;

		var targetLocalPos = (Vector2) targetRectTransform.InverseTransformPoint(worldSpacePoint);
		var targetLayoutPos = targetLocalPos + GetPivotPos(targetRectTransform);

		if( targetLayout != null && targetLayout.originTopLeft )
			targetLayoutPos.y = targetLayout.height - targetLayoutPos.y;

		return targetLayoutPos;
	}

	/// <summary>
	/// Converts a rect in local space of this SLayout to the local space of another SLayout.
	/// If you pass a null SLayout, it will get the rect in the space of the canvas.
	/// </summary>
	public Rect ConvertRectToTarget(Rect localRect, SLayout targetLayout)
	{
		var convertedMin = ConvertPositionToTarget(localRect.min, targetLayout);
		var convertedMax = ConvertPositionToTarget(localRect.max, targetLayout);

		// Coordinate system may be flipped compared between SLayouts
		// (or if converting to canvas space)
		return new Rect(
			convertedMin.x,
			Mathf.Min(convertedMin.y, convertedMax.y),
			convertedMax.x - convertedMin.x,
			Mathf.Abs(convertedMin.y - convertedMax.y)
		);
	}
		
	public RectTransform rectTransform {
		get {
			return transform as RectTransform;
		}
	}

	float GetRectTransformX(RectTransform rt) {
		float toLeftEdge = rt.pivot.x * rt.rect.width;
		var parentRectT = rt.parent as RectTransform;
		if( parentRectT == null )
			return 0.0f;
		
		float parentToLeftEdge = parentRectT.pivot.x * parentRectT.rect.width;
		float leftInset = parentToLeftEdge + transform.localPosition.x - toLeftEdge;
		return leftInset;
	}

	float GetRectTransformY(RectTransform rt) {
		var parentRectT = rt.parent as RectTransform;
		if( parentRectT == null )
			return 0.0f;
		
		if( originTopLeft ) {
			float toTopEdge = (1.0f-rt.pivot.y) * rt.rect.height;
			float parentToTopEdge = (1.0f-parentRectT.pivot.y) * parentRectT.rect.height;
			float topInset = parentToTopEdge - transform.localPosition.y - toTopEdge;
			return topInset;
		} else {
			float toBottomEdge = rt.pivot.y * rt.rect.height;
			float parentToBottomEdge = parentRectT.pivot.y * parentRectT.rect.height;
			float bottomInset = parentToBottomEdge + transform.localPosition.y - toBottomEdge;
			return bottomInset;
		}
	}

	void SetRectTransformX(float x)
	{
		var parentRT = rectTransform.parent as RectTransform;

		var parentPivotPosX = parentRT.pivot.x * parentRT.rect.width;
		var ownPivotPosX = rectTransform.pivot.x * rectTransform.rect.width;

		// X local to parent pivot (i.e. the localPosition)
		var localX = -parentPivotPosX + x + ownPivotPosX;

		var localPos = rectTransform.localPosition;
		localPos.x = localX;
		rectTransform.localPosition = localPos;
	}

	void SetRectTransformY(float y)
	{
		var parentRT = parentRectTransform;

		// Find Y local to parent pivot (i.e. the localPosition)
		float localY;

		if( originTopLeft ) {
			var parentPivotPosToTop = (1.0f-parentRT.pivot.y) * parentRT.rect.height;
			var ownPivotPosToTop = (1.0f-rectTransform.pivot.y) * rectTransform.rect.height;
			localY = parentPivotPosToTop - y - ownPivotPosToTop;
		} else {
			var parentPivotPosY = parentRT.pivot.y * parentRT.rect.height;
			var ownPivotPosY = rectTransform.pivot.y * rectTransform.rect.height;
			localY = -parentPivotPosY + y + ownPivotPosY;
		}
			
		var localPos = rectTransform.localPosition;
		localPos.y = localY;
		rectTransform.localPosition = localPos;
	}

	void SetRectTransformWidth(float width)
	{
		// Always grow/shrink outward from left edge.
		// Reason is so that we have a consistent model and so it doesn't depend on the anchoring.
		// This can be faff sometimes (for example, if you only want to set the width while it's anchored
		// automatically to the right edge of the screen). But on balance we prefer the "full manual" model
		// since if you *do* want to change the X position it becomes a lot more complicated.
		var originalLeftX = GetRectTransformX(rectTransform);
			
		var anchorsSep = (rectTransform.anchorMax.x - rectTransform.anchorMin.x) * parentRect.width;

		var sizeDelta = rectTransform.sizeDelta;
		sizeDelta.x = width - anchorsSep;
		rectTransform.sizeDelta = sizeDelta;

		// Restore original X position
		SetRectTransformX(originalLeftX);
	}

	void SetRectTransformHeight(float height)
	{
		// Always grow/shrink outward from consistent edge based on originTopLeft flag.
		// Reason is so that we have a consistent model and so it doesn't depend on the anchoring.
		// This can be faff sometimes (for example, if you only want to set the height while it's anchored
		// automatically to the top edge of the screen). But on balance we prefer the "full manual" model
		// since if you *do* want to change the Y position it becomes a lot more complicated.
		var originalY = GetRectTransformY(rectTransform);

		var anchorsSep = (rectTransform.anchorMax.y - rectTransform.anchorMin.y) * parentRect.height;

		var sizeDelta = rectTransform.sizeDelta;
		sizeDelta.y = height - anchorsSep;
		rectTransform.sizeDelta = sizeDelta;

		// Restore original Y position after size change
		SetRectTransformY(originalY);
	}

	void InitX() {
		if( _x == null ) {
			_x = new SLayoutFloatProperty {
				getter = () => GetRectTransformX(rectTransform),
				setter = SetRectTransformX
			};
		}
	}

	void InitY() {
		if( _y == null ) {
			_y = new SLayoutFloatProperty {
				getter = () => GetRectTransformY(rectTransform),
				setter = SetRectTransformY
			};
		}
	}

	void InitWidth() {
		if( _width == null ) {
			_width = new SLayoutFloatProperty {
				getter = () => rectTransform.rect.width,
				setter = SetRectTransformWidth
			};
		}
	}

	void InitHeight() {
		if( _height == null ) {
			_height = new SLayoutFloatProperty {
				getter = () => rectTransform.rect.height,
				setter = SetRectTransformHeight
			};
		}
	}

	void InitRotation() {
		if( _rotation == null ){
			_rotation = new SLayoutAngleProperty {
				getter = () => transform.rotation.eulerAngles.z,
				setter = r => transform.rotation = Quaternion.Euler(0.0f, 0.0f, r)
			};
		}
	}

	void InitScale() {
		if( _scale == null ) {
			_scale = new SLayoutFloatProperty {
				getter = () => transform.localScale.x,
				setter = s =>  transform.localScale = new Vector3(s, s, s)
			};
		}
	}

	void InitGroupAlpha() {
		if( _groupAlpha == null ) {
			_groupAlpha = new SLayoutFloatProperty {
				getter = () => canvasGroup ? canvasGroup.alpha : 1.0f,
				setter = a => { if( canvasGroup ) canvasGroup.alpha = a; }
			};
		}
	}

	void InitColor() {
		if( _color == null ) {
			_color = new SLayoutColorProperty {
				getter = () => graphic ? graphic.color : Color.white,
				setter = c => {  if( graphic ) graphic.color = c;  }
			};
		}
	}
		


	SLayoutFloatProperty _x;
	SLayoutFloatProperty _y;
	SLayoutFloatProperty _width;
	SLayoutFloatProperty _height;
	SLayoutAngleProperty _rotation;
	SLayoutFloatProperty _scale;
									 
	SLayoutFloatProperty _groupAlpha;
	SLayoutColorProperty _color;

}
