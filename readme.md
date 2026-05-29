# Tag Locating in MauiRfidSample

## Document Info

- Author: Chuck Lin
- Local directory: /Users/chucklin/StudioProjects/68_TC22R_Locate/Android_MAUI_sample_app_SDK_2.0.5.275/Zebra_RFIDAPI3_Android_MAUI_SDK_2.0.5.275/MauiRfidSample
- Remote directory: https://github.com/GelatoCookie/MAUI275_locate_mask

This document explains how tag locating works in this app and how `TagPattern` and `TagMask` are used when calling Zebra RFID SDK APIs.

For implementation and architecture details, see `design.md`.

## What `TagPattern` and `TagMask` Mean

- `TagPattern`: The EPC/tag ID pattern to locate.
- `TagMask`: Optional hex mask string that narrows matching behavior.

In this app:

- `TagPattern` is required to actually start locate.
- `TagMask` is optional.
- If `TagMask` is not provided, `null` is passed to the SDK.
- If `TagMask` is provided, it must be:
  - Hex only (`0-9`, `A-F`), and
  - Even number of characters (whole bytes)

## Locate Flow (UI to Reader)

1. User enters `TagPattern` and optional `TagMask` in the Locate page.
2. User taps Start/Stop button (or presses/release handheld trigger).
3. `LocateViewModel` validates mask format.
4. `ReaderModel.Locate(start, tagPattern, tagMask)` is called.
5. `ReaderModel` forwards to SDK:
   - Start: `TagLocationing.Perform(tagPattern, tagMask, null)`
   - Stop: `TagLocationing.Stop()`

## Key Code Snippets

### 1. UI Bindings for Pattern and Mask

```xml
<Entry
	x:Name="tagPattern"
	Placeholder="Tag Pattern"
	Text="{Binding TagPattern, Mode=TwoWay}" />

<Entry
	x:Name="tagMask"
	Placeholder="Tag Mask (optional)"
	Text="{Binding TagMask, Mode=TwoWay}" />
```

### 2. Validation of Optional Mask

```csharp
private string GetTagMaskOrNull()
{
	if (string.IsNullOrWhiteSpace(TagMask))
	{
		return null;
	}

	return TagMask.Trim();
}

private bool TryGetValidatedTagMask(out string tagMask)
{
	tagMask = GetTagMaskOrNull();
	if (tagMask == null)
	{
		return true;
	}

	if (!Regex.IsMatch(tagMask, "^[0-9a-fA-F]+$") || (tagMask.Length % 2 != 0))
	{
		rfidModel.ShowAlert("Tag Mask must be hex (0-9, A-F) and contain an even number of characters.");
		return false;
	}

	tagMask = tagMask.ToUpperInvariant();
	TagMask = tagMask;
	return true;
}
```

### 3. Start/Stop Locate from ViewModel

```csharp
private void OnStartStopClicked()
{
	if (!TryGetValidatedTagMask(out var tagMask))
	{
		return;
	}

	if (IsInventoryRunning)
	{
		IsInventoryRunning = false;
		if (TagPattern != null && TagPattern != "")
			rfidModel.Locate(false, TagPattern, tagMask);
	}
	else
	{
		if (TagPattern != null && TagPattern != "")
			rfidModel.Locate(true, TagPattern, tagMask);
		IsInventoryRunning = true;
	}
}
```

### 4. ReaderModel Bridge to Zebra SDK

```csharp
internal void Locate(bool start, string tagPattern, string tagMask)
{
	try
	{
		if (start)
		{
			rfidReader.Actions.TagLocationing.Perform(tagPattern, tagMask, null);
		}
		else
		{
			rfidReader.Actions.TagLocationing.Stop();
		}
	}
	catch (InvalidUsageException e)
	{
		e.PrintStackTrace();
	}
	catch (OperationFailureException e)
	{
		e.PrintStackTrace();
		ShowAlert(e);
	}
}
```

## Practical Mask Examples

Use uppercase hex for clarity:

- `TagPattern = "300833B2DDD9014000000001"`, `TagMask = null`
  - Locate based on full pattern matching behavior as interpreted by SDK when no mask is provided.
- `TagPattern = "300833B2DDD9014000000001"`, `TagMask = "FFFFFFFFFFFFFFFF00000000"`
  - Match first bytes strongly, ignore trailing bytes (depending on SDK mask semantics).
- `TagMask = "ABC"`
  - Rejected by app (odd length).
- `TagMask = "GG00"`
  - Rejected by app (non-hex chars).

## Relative Distance Feedback

When locate is active and the SDK returns location info, the app updates UI with relative distance:

```csharp
public override void TagReadEvent(TagData[] tags)
{
	if (tags != null)
	{
		for (int index = 0; index < tags.Length; index++)
		{
			if (tags[index].LocationInfo != null)
			{
				RelativeDistance = tags[index].LocationInfo.RelativeDistance.ToString();
				UpdateFillView(tags[index].LocationInfo.RelativeDistance);
			}
		}
	}
}
```

## Notes and Constraints

- Locate is blocked for empty `TagPattern` by the ViewModel checks.
- Mask validation happens in UI layer before calling SDK.
- SDK exceptions are surfaced through `ShowAlert` in `ReaderModel`.
- Certain transports/devices may not support all locate behaviors; unsupported cases are already warned in ViewModel constructor for `ReSerial`.

## Quick Test Checklist

1. Connect reader and open Locate page.
2. Enter valid `TagPattern`, leave `TagMask` empty, press Start.
3. Verify distance value changes while moving tag.
4. Stop locate and verify it halts.
5. Enter invalid mask (odd length or non-hex), press Start, verify validation toast.
6. Enter valid mask and repeat locate.

## Related Documentation

- Architecture/design details: `design.md`
