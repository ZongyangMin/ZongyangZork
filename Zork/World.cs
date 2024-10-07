using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Zork
{

	public class World
	{
		public HashSet<Room> Rooms { get; set; }

		[JsonIgnore]
		public IReadOnlyDictionary<string, Room> RoomsByName => mRoomsbyName;

		public Player SpawnPlyaer() => new Player(this, StartingLocation);

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{
			mRoomsByName = Rooms.ToDictionary(room => room.Name, room => room);

			foreach (Room room in Rooms)
			{
				room.UpdateNeighbors(this);
			}
		}

		[JsonProperty]
		private string STartingLocation { get; set; }

		private Dictionary<string, Room> mRoomsByName;
}
