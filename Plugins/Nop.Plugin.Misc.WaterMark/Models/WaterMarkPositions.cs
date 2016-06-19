using System;

namespace Nop.Plugin.Misc.WaterMark.Models
{
	[Flags]
	public enum WaterMarkPositions
	{
		TopLeft = 2,
		TopRight = 4,
		TopMiddle = 8,
		BottomLeft = 16,
		BottomRight = 32,
		BottomMiddle = 64,
		MiddleLeft = 128,
		MiddleRight = 256,
		Center = 512,
	}
}
