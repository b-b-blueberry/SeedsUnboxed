using System;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace SeedsUnpacked
{
	public sealed class ModEntry : Mod
	{
		private static readonly string[] InvalidSeeds =
		{
			"Spring",
			"Summer",
			"Fall",
			"Winter",
			"Mixed",
			"Sesame",
			"Sunflower",
		};

		const string AssetKey = @"Maps/springobjects";

		public override void Entry(IModHelper helper)
		{
			this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;
			this.Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
		}

		private void OnSaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
		{
			this.Helper.GameContent.InvalidateCache(ModEntry.AssetKey);
		}

		private void OnAssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
		{
			if (Game1.objectInformation is not null && Game1.cropSpriteSheet is not null && e.NameWithoutLocale.IsEquivalentTo(ModEntry.AssetKey))
			{
				e.Edit(apply: this.Edit);
			}
		}

		private void Edit(IAssetData asset)
		{
			try
			{
				foreach (int id in Game1.objectInformation.Keys.Where((int id) => Game1.objectInformation[id].Split('/') is string[] split
					// No starters or saplings (stage 0 is taller than 16px):
					&& (split[0].ToLower().EndsWith("seeds") || split[0].ToLower().EndsWith("bulb"))
					// No non-standard (multiple possible harvest) seeds:
					&& ModEntry.InvalidSeeds.All((string prefix) => !split[0].StartsWith(prefix))
					// No trellis seeds (stage 0 is taller than 16px):
					&& !split[5].EndsWith("trellis.")))
				{
					try
					{
						Crop crop = new(seedIndex: id, tileX: 0, tileY: 0);

						Rectangle source = crop.getSourceRect(number: 0);
						source.Y += source.Height - Game1.smallestTileSize;
						source.Height = Game1.smallestTileSize;
						source.Width = Game1.smallestTileSize;

						Rectangle target = Game1.getSourceRectForStandardTileSheet(
							tileSheet: Game1.objectSpriteSheet,
							tilePosition: id,
							width: source.Width,
							height: source.Height);

						asset.AsImage().PatchImage(
							source: Game1.cropSpriteSheet,
							sourceArea: source,
							targetArea: target,
							patchMode: PatchMode.Replace);
					}
					catch (Exception e)
					{
						this.Monitor.Log($"Error editing crop {id}:{Environment.NewLine}{e}", LogLevel.Error);
						continue;
					}
				}
			}
			catch (Exception e)
			{
				this.Monitor.Log($"Error editing crops. Some sprites may not be changed.{Environment.NewLine}{e}", LogLevel.Error);
			}
		}
	}
}
