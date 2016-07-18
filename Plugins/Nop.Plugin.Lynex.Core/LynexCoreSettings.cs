using Nop.Core.Configuration;

namespace Nop.Plugin.Lynex.Core
{
	public partial class LynexCoreSettings : ISettings
	{
		public int PictureId { get; set; }

		public int Positions { get; set; }

		public bool Enable { get; set; }

		public int Scale { get; set; }

		public int Transparency { get; set; }

		public int OnlyLargerThen { get; set; }

		public bool ApplyOnProductPictures { get; set; }

		public bool ApplyOnCategoryPictures { get; set; }

		public bool ApplyOnProductVariantAttributeValuePictures { get; set; }
	}
}
