using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Runtime.CompilerServices;
namespace Portraiture.HDP
{
	//https://github.com/tlitookilakin/AeroCore/blob/master/Generics/LazyAsset.cs
	public abstract class LazyAsset
	{

		internal static readonly ConditionalWeakTable<LazyAsset, IModHelper> Watchers = new ConditionalWeakTable<LazyAsset, IModHelper>();
		internal Func<string> getPath;
		internal bool ignoreLocale;
		internal static void Init()
		{
			PortraitureMod.helper.Events.Content.AssetsInvalidated += CheckWatchers;
		}
		internal static void CheckWatchers(object _, AssetsInvalidatedEventArgs ev)
		{
			foreach ((LazyAsset asset, IModHelper _) in Watchers)
			{
				string path = asset.getPath();
				foreach (IAssetName name in asset.ignoreLocale ? ev.NamesWithoutLocale : ev.Names)
				{
					if (name.IsEquivalentTo(path))
					{
						asset.Reload();
						break;
					}
				}
			}
		}
		public abstract void Reload();
	}
	public class LazyAsset<T> : LazyAsset
	{
		private readonly IModHelper helper;
		private T cached;
		private bool isCached;

		public LazyAsset(IModHelper Helper, Func<string> AssetPath, bool IgnoreLocale = true)
		{
			getPath = AssetPath;
			helper = Helper;
			ignoreLocale = IgnoreLocale;

			Watchers.Add(this, Helper);
		}

		public T Value
		{
			get
			{
				return GetAsset();
			}
		}
		public string LastError { get; private set; }
		public bool CatchErrors { get; set; } = false;
		public event Action<LazyAsset<T>> AssetReloaded;
		public T GetAsset()
		{
			if (!isCached)
			{
				LastError = null;
				isCached = true;
				if (CatchErrors)
				{
					try
					{
						cached = helper.GameContent.Load<T>(getPath());
					}
					catch (Exception e)
					{
						LastError = e.ToString();
						cached = default;
					}
				}
				else
				{
					cached = helper.GameContent.Load<T>(getPath());
				}
			}
			return cached;
		}
		public override void Reload()
		{
			cached = default;
			isCached = false;
			LastError = null;
			AssetReloaded?.Invoke(this);
		}
	}
}
