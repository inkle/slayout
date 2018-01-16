using System;
using TMPro;

/// <summary>
/// Shortcut to get a TextMeshPro from an SLayout. Don't want to include it
/// directly in SLayout directly since we don't want a dependency on TMPro.
/// </summary>
public partial class SLayout
{
	public TextMeshProUGUI textMeshPro {
		get {
			return graphic as TextMeshProUGUI;
		}
	}
}