using StardewValley;
using StardewModdingAPI;
using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace SeedsUnboxed
{
	public class ModEntry : Mod, IAssetEditor
	{
		private static readonly string[] InvalidSeeds =
		{
			"Spring",
			"Summer",
			"Fall",
			"Winter",
			"Mixed",
			"Sesame",
		};

		public override void Entry(IModHelper helper)
		{
			Helper.Events.GameLoop.SaveLoaded += delegate { Helper.Content.InvalidateCache(@"Maps/springobjects"); };
		}

		public bool CanEdit<T>(IAssetInfo asset)
		{
			return Game1.objectInformation != null && asset.AssetNameEquals(@"Maps/springobjects");
		}

		public void Edit<T>(IAssetData asset)
		{
			foreach (var id in Game1.objectInformation.Keys.Where(id => Game1.objectInformation[id].Split('/') is string[] split
				&& (split[0].EndsWith("Seeds") || split[0].EndsWith("Bulb"))
				&& InvalidSeeds.All(prefix => !split[0].StartsWith(prefix))
				&& !split[5].EndsWith("trellis.")))
			{
				try
				{
					var crop = new Crop(id, 0, 0);
					var sourceArea = Helper.Reflection.GetMethod(crop, "getSourceRect").Invoke<Rectangle>(0);
					sourceArea.Height = 16;
					sourceArea.Y += sourceArea.Height;
					var targetArea = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, id, sourceArea.Width, sourceArea.Height);
					asset.AsImage().PatchImage(Game1.cropSpriteSheet, sourceArea, targetArea, PatchMode.Replace);
				}
				catch (Exception e)
				{
					Monitor.Log($"{e}", LogLevel.Error);
					continue;
				}
			}
		}
	}
}
