using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
namespace Portraiture.HDP
{
	public class MetadataModel
	{
		public readonly LazyAsset<Texture2D> originalTexture;

		public readonly LazyAsset<Texture2D> overrideTexture;
		internal string originalPath = null;
		private string portraitPath;

		public MetadataModel()
		{
			overrideTexture = new LazyAsset<Texture2D>(PortraitureMod.helper, () => portraitPath)
			{
				CatchErrors = true
			};
			originalTexture = new LazyAsset<Texture2D>(PortraitureMod.helper, () => originalPath);
		}
		//https://github.com/tlitookilakin/HDPortraits/blob/master/HDPortraits/Models/MetadataModel.cs
		public int Size { set; get; } = 64;
		public AnimationModel Animation { get; set; } = null;
		public string Portrait
		{
			get
			{
				return portraitPath;
			}
			set
			{
				portraitPath = value;
				overrideTexture.Reload();
			}
		}

		public bool TryGetTexture(out Texture2D texture)
		{
			if (portraitPath is null)
			{
				texture = originalTexture.Value;
				return true;
			}
			texture = overrideTexture.Value;
			return overrideTexture.LastError is null;
		}
		public Rectangle GetRegion(int which, int millis = -1)
		{
			bool missing = !TryGetTexture(out Texture2D tex);
			int size = missing ? 64 : Size;
			return Animation?.GetSourceRegion(tex, size, which, millis) ?? Game1.getSourceRectForStandardTileSheet(tex, which, size, size);
		}
	}
}
