// This Class is Auto-Generated by the GenerateStrings Program.
using System.ComponentModel;
using Helios.Settings.Strings;
using Helios.Settings;

namespace Helios.Settings.Strings
{
	public static class Midi
	{
		public const string GroupName = "Midi";
		public const string DeviceIndex = "deviceIndex";
		public const string RangeLow = "rangeLow";
		public const string RangeHigh = "rangeHigh";
		public const string FadeInSpeed = "fadeInSpeed";
		public const string FadeOutSpeed = "fadeOutSpeed";
	}
}

public partial class SROptions
{
	[Category("Midi")]
	[DisplayName("Reset")]
	public void ResetMidi()
	{
		Group.Get("Midi").Reset();
	}
	[Category("Midi")]
	[DisplayName("DeviceIndex")]
	public System.Int32 Midi_DeviceIndex
	{
		get { return Group.Get("Midi").Get<System.Int32>("deviceIndex"); }
		set { Group.Get("Midi").Set("deviceIndex", value); }
	}
	[Category("Midi")]
	[DisplayName("RangeLow")]
	public System.Int32 Midi_RangeLow
	{
		get { return Group.Get("Midi").Get<System.Int32>("rangeLow"); }
		set { Group.Get("Midi").Set("rangeLow", value); }
	}
	[Category("Midi")]
	[DisplayName("RangeHigh")]
	public System.Int32 Midi_RangeHigh
	{
		get { return Group.Get("Midi").Get<System.Int32>("rangeHigh"); }
		set { Group.Get("Midi").Set("rangeHigh", value); }
	}
	[Category("Midi")]
	[DisplayName("FadeInSpeed")]
	public System.Single Midi_FadeInSpeed
	{
		get { return Group.Get("Midi").Get<System.Single>("fadeInSpeed"); }
		set { Group.Get("Midi").Set("fadeInSpeed", value); }
	}
	[Category("Midi")]
	[DisplayName("FadeOutSpeed")]
	public System.Single Midi_FadeOutSpeed
	{
		get { return Group.Get("Midi").Get<System.Single>("fadeOutSpeed"); }
		set { Group.Get("Midi").Set("fadeOutSpeed", value); }
	}
}
