// This Class is Auto-Generated by the GenerateStrings Program.
using System.ComponentModel;
using Helios.Settings.Strings;
using Helios.Settings;

namespace Helios.Settings.Strings
{
	public static class Serial
	{
		public const string GroupName = "Serial";
		public const string ComPort = "comPort";
		public const string Range1Low = "range1Low";
		public const string Range1High = "range1High";
		public const string Range2Low = "range2Low";
		public const string Range2High = "range2High";
	}
}

public partial class SROptions
{
	[Category("Serial")]
	[DisplayName("Reset")]
	public void ResetSerial()
	{
		Group.Get("Serial").Reset();
	}
	[Category("Serial")]
	[DisplayName("ComPort")]
	public System.Int32 Serial_ComPort
	{
		get { return Group.Get("Serial").Get<System.Int32>("comPort"); }
		set { Group.Get("Serial").Set("comPort", value); }
	}
	[Category("Serial")]
	[DisplayName("Range1Low")]
	public System.Int32 Serial_Range1Low
	{
		get { return Group.Get("Serial").Get<System.Int32>("range1Low"); }
		set { Group.Get("Serial").Set("range1Low", value); }
	}
	[Category("Serial")]
	[DisplayName("Range1High")]
	public System.Int32 Serial_Range1High
	{
		get { return Group.Get("Serial").Get<System.Int32>("range1High"); }
		set { Group.Get("Serial").Set("range1High", value); }
	}
	[Category("Serial")]
	[DisplayName("Range2Low")]
	public System.Int32 Serial_Range2Low
	{
		get { return Group.Get("Serial").Get<System.Int32>("range2Low"); }
		set { Group.Get("Serial").Set("range2Low", value); }
	}
	[Category("Serial")]
	[DisplayName("Range2High")]
	public System.Int32 Serial_Range2High
	{
		get { return Group.Get("Serial").Get<System.Int32>("range2High"); }
		set { Group.Get("Serial").Set("range2High", value); }
	}
}
