using BmFont;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System.Collections.Generic;
using System.IO;
namespace Portraiture.PlatoUI
{
	public sealed class UIFont
	{

		public UIFont(IModHelper helper, string assetName, string id = "")
		{
			if (id == "")
				id = assetName;

			Id = id;

			FontFile = FontLoader.Parse(File.ReadAllText(Path.Combine(helper.DirectoryPath, assetName)));

			CharacterMap = new Dictionary<char, FontChar>();

			foreach (FontChar fontChar in FontFile.Chars)
			{
				char cid = (char)fontChar.ID;
				CharacterMap.Add(cid, fontChar);
			}

			FontPages = new List<Texture2D>();

			foreach (FontPage page in FontFile.Pages)
			{
				FontPages.Add(helper.ModContent.Load<Texture2D>($"{Path.GetDirectoryName(assetName)}/{page.File}"));
			}
		}
		public string Id { get; set; }
		public FontFile FontFile { get; set; }

		public Dictionary<char, FontChar> CharacterMap { get; set; }

		public List<Texture2D> FontPages { get; set; }
	}
}
