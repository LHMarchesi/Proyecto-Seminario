using UnityEngine;
using UnityEngine.Assertions;

namespace DunGen
{
	public class DungeonAttachmentSettings
	{
		/// <summary>
		/// The doorway to attach the dungeon to. If set, the new dungeon must be attached to this doorway
		/// </summary>
		public Doorway AttachmentDoorway { get; private set; }

		/// <summary>
		/// The tile to attach the dungeon to. If set, the new dungeon will attach to this tile, but the doorway
		/// will be chosen randomly
		/// </summary>
		public Tile AttachmentTile { get; private set; }

		public TileProxy TileProxy { get; private set; }


		public DungeonAttachmentSettings(Doorway attachmentDoorway)
		{
			Assert.IsNotNull(attachmentDoorway, "attachmentDoorway cannot be null");
			AttachmentDoorway = attachmentDoorway;
		}

		public DungeonAttachmentSettings(Tile attachmentTile)
		{
			Assert.IsNotNull(attachmentTile, "attachmentTile cannot be null");
			AttachmentTile = attachmentTile;
		}

		public TileProxy GenerateAttachmentProxy(Vector3 upVector, RandomStream randomStream)
		{
			if (AttachmentTile != null)
			{
				// This tile wasn't placed by DunGen so we'll need to do
				// some extra setup to ensure we have all the data we'll need later
				if (AttachmentTile.Prefab == null)
					PrepareManuallyPlacedTile(AttachmentTile, upVector, randomStream);

				TileProxy = new TileProxy(AttachmentTile.Prefab,
					(doorway, index) => AttachmentTile.UnusedDoorways.Contains(AttachmentTile.AllDoorways[index])); // Ensure chosen doorway is unused

				TileProxy.Placement.Position = AttachmentTile.transform.localPosition;
				TileProxy.Placement.Rotation = AttachmentTile.transform.localRotation;
			}
			else if (AttachmentDoorway != null)
			{
				var attachmentTile = AttachmentDoorway.Tile;

				if(attachmentTile == null)
				{
					attachmentTile = AttachmentDoorway.GetComponentInParent<Tile>();

					if(attachmentTile == null)
					{
						Debug.LogError($"Cannot attach to a doorway that doesn't belong to a Tile. Ensure the Doorway is parented to a GameObject with a Tile component");
						return null;
					}
				}

				if(attachmentTile.Prefab == null)
					PrepareManuallyPlacedTile(attachmentTile, upVector, randomStream);

				if (AttachmentDoorway.Tile.UsedDoorways.Contains(AttachmentDoorway))
					Debug.LogError($"Cannot attach dungeon to doorway '{AttachmentDoorway.name}' as it is already in use");

				TileProxy = new TileProxy(AttachmentDoorway.Tile.Prefab,
					(doorway, index) => index == attachmentTile.AllDoorways.IndexOf(AttachmentDoorway));

				TileProxy.Placement.Position = AttachmentDoorway.Tile.transform.localPosition;
				TileProxy.Placement.Rotation = AttachmentDoorway.Tile.transform.localRotation;
			}

			return TileProxy;
		}

		private void PrepareManuallyPlacedTile(Tile tileToPrepare, Vector3 upVector, RandomStream randomStream)
		{
			tileToPrepare.Prefab = tileToPrepare.gameObject;

			foreach (var doorway in tileToPrepare.GetComponentsInChildren<Doorway>())
			{
				doorway.Tile = tileToPrepare;

				tileToPrepare.AllDoorways.Add(doorway);
				tileToPrepare.UnusedDoorways.Add(doorway);

				doorway.ProcessDoorwayObjects(false, randomStream);
			}

			Bounds bounds;

			if (tileToPrepare.OverrideAutomaticTileBounds)
				bounds = tileToPrepare.TileBoundsOverride;
			else
				bounds = UnityUtil.CalculateProxyBounds(tileToPrepare.gameObject, upVector);

			tileToPrepare.Placement.LocalBounds = UnityUtil.CondenseBounds(bounds, tileToPrepare.AllDoorways);
		}

		public Tile GetAttachmentTile()
		{
			Tile attachmentTile = null;

			if (AttachmentTile != null)
				attachmentTile = AttachmentTile;
			else if (AttachmentDoorway != null)
				attachmentTile = AttachmentDoorway.Tile;

			return attachmentTile;
		}
	}
}