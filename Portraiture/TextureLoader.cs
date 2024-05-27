using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Portraiture.HDP;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Portraiture
{
	internal class TextureLoader
	{
		private static string contentFolder;
		internal static int activeFolder;
		internal static List<string> folders = new List<string>();
		internal static Dictionary<string, Texture2D> pTextures = new Dictionary<string, Texture2D>();
		internal static PresetCollection presets = new PresetCollection();

		public static void loadTextures()
		{
			activeFolder = 0;
			contentFolder = Path.Combine(PortraitureMod.helper.DirectoryPath, "Portraits");
			folders = new List<string>
			{
				"Vanilla"
			};
			pTextures = new Dictionary<string, Texture2D>();
			loadAllPortraits();

			string loadConfig = PortraitureMod.config.active;

			if (loadConfig == "none")
				activeFolder = 0;
			else
				activeFolder = folders.Contains(loadConfig) ? folders.FindIndex(f => f == loadConfig) : folders.Count > 1 ? 1 : 0;

			saveConfig();

		}

		internal static void setPreset(string name, string folder)
		{
			presets.Presets.RemoveAll(p => p.Character == name);
			if (!string.IsNullOrEmpty(folder))
				presets.Presets.Add(new Preset
				{
					Character = name, Portraits = folder
				});

			PortraitureMod.config.presets = presets;
			saveConfig();
		}

		internal static void loadPreset(IMonitor monitor)
		{
			if (PortraitureMod.config.active is { } d && !string.IsNullOrEmpty(d) && folders.Contains(d))
			{
				monitor.Log("Loaded Active Portraits: " + d, LogLevel.Info);
				activeFolder = folders.FindIndex(f => f == d);
			}

			if (PortraitureMod.config.presets is { } p)
			{
				monitor.Log("Loaded Active Presets for " + string.Join(',', p.Presets.Select(pr => pr.Character + ":" + pr.Portraits)), LogLevel.Info);

				presets = p;
			}

		}

		internal static Rectangle getSoureRectangle(Texture2D texture, int index = 0)
		{
			int textureSize = Math.Max(texture.Width / 2, 64);
			return Game1.getSourceRectForStandardTileSheet(texture, index, textureSize, textureSize);
		}

		public static Texture2D getPortrait(NPC npc, Texture2D tex)
		{
			string name = npc.Name;

			if (!Context.IsWorldReady || folders.Count == 0)
				return null;

			activeFolder = Math.Max(activeFolder, 0);

			if (presets.Presets.FirstOrDefault(pr => pr.Character == name) is { } pre)
				activeFolder = Math.Max(folders.IndexOf(pre.Portraits), 0);

			string folder = folders[activeFolder];

			if (activeFolder == 0 || folders.Count <= activeFolder || folder == "none" || folder == "HDP" && PortraitureMod.helper.ModRegistry.IsLoaded("tlitookilakin.HDPortraits"))
				return null;

			if (folder == "HDP" && !PortraitureMod.helper.ModRegistry.IsLoaded("tlitookilakin.HDPortraits"))
			{
				try
				{
					MetadataModel portraits = PortraitureMod.helper.GameContent.Load<MetadataModel>("Mods/HDPortraits/" + name);

					if (portraits is null || !portraits.TryGetTexture(out Texture2D texture))
						return null;
					if (portraits.Animation == null || portraits.Animation.VFrames == 1 && portraits.Animation.HFrames == 1)
						return ScaledTexture2D.FromTexture(tex, texture, portraits.Size / 64f);
					portraits.Animation.Reset();
					return new AnimatedTexture2D(texture, texture.Width / portraits.Animation.VFrames, texture.Height / portraits.Animation.HFrames, 6, true, portraits.Size / 64f);
				}
				catch
				{
					return null;
				}
			}

			string season = Game1.currentSeason ?? "Spring";
			if (PortraitureMod.config.isFestivalLower) season = season.ToLower();

			if (presets.Presets.FirstOrDefault(p => p.Character == name) is { } preset && folders.Contains(preset.Portraits))
				folder = preset.Portraits;

			if (Game1.isFestival() || Game1.CurrentEvent != null && Game1.CurrentEvent.isWedding)
			{
				string festival = GetDayEvent();
				if (pTextures.ContainsKey(folder + ">" + name + "_" + festival))
					return pTextures[folder + ">" + name + "_" + festival];
			}

			if (Game1.currentLocation is { Name: not null } gl)
			{
				if (pTextures.ContainsKey(folder + ">" + name + "_" + gl.Name + "_" + season))
					return pTextures[folder + ">" + name + "_" + gl.Name + "_" + season];
				if (pTextures.ContainsKey(folders[activeFolder] + ">" + name + "_" + gl.Name))
					return pTextures[folder + ">" + name + "_" + gl.Name];
			}

			if (pTextures.ContainsKey(folder + ">" + name + "_" + season + "_Indoor") && pTextures.ContainsKey(folder + ">" + name + "_" + season + "_Outdoor"))
			{
				return Game1.currentLocation.IsOutdoors ? pTextures[folder + ">" + name + "_" + season + "_Outdoor"] : pTextures[folder + ">" + name + "_" + season + "_Indoor"];
			}

			if (pTextures.ContainsKey(folder + ">" + name + "_" + season))
				return pTextures[folder + ">" + name + "_" + season];

			return pTextures.ContainsKey(folder + ">" + name) ? pTextures[folder + ">" + name] : null;

		}

		private static void loadAllPortraits()
		{
			foreach (string dir in Directory.EnumerateDirectories(contentFolder))
			{
				string folderName = new DirectoryInfo(dir).Name;

				folders.Add(folderName);
				foreach (string file in Directory.EnumerateFiles(dir, "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".png") || s.EndsWith(".xnb")))
				{
					string fileName = Path.GetFileName(file);
					string name = Path.GetFileNameWithoutExtension(file).Split(new[]
					{
						"_anim_"
					}, StringSplitOptions.RemoveEmptyEntries)[0];
					string extension = Path.GetExtension(file).ToLower();

					if (extension == "xnb")
						fileName = name;

					Texture2D texture = PortraitureMod.helper.ModContent.Load<Texture2D>($"Portraits/{folderName}/{fileName}");

					int fps = 12;
					int frames = 1;
					bool loop = false;
					if (fileName.Contains("_anim_"))
					{
						string[] fdata = fileName.Split(new[]
						{
							"_anim_"
						}, StringSplitOptions.RemoveEmptyEntries);
						if (fdata.Length > 1)
							frames = int.Parse(fdata[1]);

						if (fdata.Length > 2)
							fps = int.Parse(fdata[2]);

						if (fdata.Length > 3)
							loop = fdata[3] == "loop";

						if (frames < 1)
							frames = 1;

						if (fps < 1)
							fps = 12;
					}

					double tileWith = Convert.ToDouble(Math.Max(texture.Width / 2, 64)) / frames;
					float scale = (float)(tileWith / 64);
					Texture2D scaled = frames == 1 ? new ScaledTexture2D(texture, scale) : new AnimatedTexture2D(texture, texture.Width / frames, texture.Height, fps, loop, scale);

					if (!pTextures.ContainsKey(folderName + ">" + name))
						pTextures.Add(folderName + ">" + name, scaled);
					else
						pTextures[folderName + ">" + name] = scaled;
				}
			}

			IEnumerable<IContentPack> contentPacks = PortraitureMod.helper.ContentPacks.GetOwned();

			foreach (IContentPack pack in contentPacks)
			{
				string folderName = pack.Manifest.UniqueID;

				folders.Add(folderName);
				foreach (string file in Directory.EnumerateFiles(pack.DirectoryPath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".png") || s.EndsWith(".xnb")))
				{
					string fileName = Path.GetFileName(file);
					string name = Path.GetFileNameWithoutExtension(file);
					string extension = Path.GetExtension(file).ToLower();

					if (extension == "xnb")
						fileName = name;
					Texture2D texture = pack.ModContent.Load<Texture2D>(fileName);
					int tileWith = Math.Max(texture.Width / 2, 64);
					float scale = tileWith / 64f;


					ScaledTexture2D scaled = ScaledTexture2D.FromTexture(Game1.getCharacterFromName(name).Portrait, texture, scale);

					if (!pTextures.ContainsKey(folderName + ">" + name))
						pTextures.Add(folderName + ">" + name, scaled);
					else
						pTextures[folderName + ">" + name] = scaled;
				}
			}

			if (PortraitureMod.config.HPDOption)
				folders.Add("HDP");

			if ((!PortraitureMod.config.SideLoadHDPWhenInstalled || !PortraitureMod.helper.ModRegistry.IsLoaded("tlitookilakin.HDPortraits")) && (!PortraitureMod.config.SideLoadHDPWhenNotInstalled || PortraitureMod.helper.ModRegistry.IsLoaded("tlitookilakin.HDPortraits")))
				return;
			{
				foreach (FileInfo file in Directory.GetParent(PortraitureMod.helper.DirectoryPath)?.EnumerateFiles("manifest.json", SearchOption.AllDirectories)!)
				{
					try
					{
						if (JsonConvert.DeserializeObject<SmapiManifest>(File.ReadAllText(file.FullName)) is not { } manifest)
							continue;
						if (manifest.ContentPackFor?.UniqueID != "Pathoschild.ContentPatcher" || manifest.Dependencies.All(d => d.UniqueID != "tlitookilakin.HDPortraits"))
							continue;
						try
						{
							string folderName = manifest.UniqueID;
							folders.Add(folderName);
							Dictionary<float, int> scales = new Dictionary<float, int>();
							List<ScaledTexture2D> rescale = new List<ScaledTexture2D>();

							foreach (string f in Directory.EnumerateFiles(file.Directory.FullName, "*.png", SearchOption.AllDirectories))
							{
								string name = Path.GetFileNameWithoutExtension(f);

								Texture2D texture = Texture2D.FromFile(Game1.graphics.GraphicsDevice, f);
								PremultiplyTransparency(texture);
								int tileWith = Math.Max(texture.Width / 2, 64);
								float scale = tileWith / 64f;
								ScaledTexture2D scaled;
								try
								{
									scaled = ScaledTexture2D.FromTexture(Game1.getCharacterFromName(name) is { Portrait: { } ptex } ? ptex : Game1.getCharacterFromName("Pierre").Portrait, texture, scale);
								}
								catch
								{
									scaled = ScaledTexture2D.FromTexture(Game1.getCharacterFromName("Pierre").Portrait, texture, scale);

								}

								if (!pTextures.ContainsKey(folderName + ">" + name))
									pTextures.Add(folderName + ">" + name, scaled);
								else
									pTextures[folderName + ">" + name] = scaled;

								if (!scales.TryAdd(scale, 1))
									scales[scale]++;

								float maxScale = scales.ToList().OrderByDescending(s => s.Value).FirstOrDefault().Key;

								if (!scale.Equals(maxScale) && pTextures[folderName + ">" + name] is ScaledTexture2D st)
									rescale.Add(st);
							}

							rescale.ForEach(s => s.Scale = scales.ToList().OrderByDescending(keyValuePair => keyValuePair.Value).FirstOrDefault().Key);

							PortraitureMod.log("Added HD Portraits Pack: " + manifest.UniqueID);
						}
						catch
						{
							// ignored

						}
					}
					catch
					{
						// ignored

					}

				}
			}
		}

		private static void PremultiplyTransparency(Texture2D texture)
		{
			int count = texture.Width * texture.Height;
			Color[] data = ArrayPool<Color>.Shared.Rent(count);
			try
			{
				texture.GetData(data, 0, count);

				bool changed = false;
				for (int i = 0; i < count; i++)
				{
					ref Color pixel = ref data[i];
					if (pixel.A is (byte.MinValue or byte.MaxValue))
						continue;

					data[i] = new Color(pixel.R * pixel.A / byte.MaxValue, pixel.G * pixel.A / byte.MaxValue, pixel.B * pixel.A / byte.MaxValue, pixel.A);
					changed = true;
				}

				if (changed)
					texture.SetData(data, 0, count);
			}
			finally
			{
				ArrayPool<Color>.Shared.Return(data);
			}
		}

		public static string getFolderName()
		{
			return folders[activeFolder];
		}

		public static void nextFolder()
		{
			activeFolder++;
			if (folders.Count <= activeFolder)
				activeFolder = 0;

			saveConfig();
		}

		private static void saveConfig()
		{
			if (folders.Count > activeFolder && activeFolder >= 0)
			{
				PortraitureMod.config.active = folders[activeFolder];
			}
			else
			{
				PortraitureMod.config.active = "none";
			}
			PortraitureMod.helper.WriteConfig(PortraitureMod.config);
		}

		public static string GetDayEvent()
		{
			if (SaveGame.loaded?.weddingToday ?? Game1.weddingToday)
				return "wedding";

			string festival = PortraitureMod.festivalDates.TryGetValue($"{Game1.currentSeason}{Game1.dayOfMonth}", out string festivalName) ? festivalName : "";
			return festival;
		}
	}

	public class SmapiManifest
	{
		public string Name { get; set; } = "";
		public string UniqueID { get; set; } = "";

		public SmapiManifest ContentPackFor { get; set; } = null;

		public List<SmapiManifest> Dependencies { get; set; } = new List<SmapiManifest>();
	}

}
