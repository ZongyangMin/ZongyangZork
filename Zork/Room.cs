﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;

namespace Zork
{
	public class Room: IEquatable<Room>
	{
		[JsonProperty(Order = 1)]
		public string Name { get; private set; }

		[JsonProperty(Order = 2)]
		public string Description { get; private set; }

		[JsonProperty(PropertyName = "Neighbors", Order = 3)]
		private Dictionary<Directions, string> NeiborNames { get; set; }


		[JsonIgnore]
		public IReadOnlyDictionary<Directions, Room> Neighbors { get; private set; }

		public static bool operator ==(Room lhs, Room rhs)
		{
			if (ReferenceEquals(lhs, rhs))
			{
				return true;
			}

			if (lhs is null || rhs is null)
			{
				return false;
			}

			return lhs.Name == rhs.Name;
		}

		public static bool operator !=(Room lhs, Room rhs) => !(lhs == rhs);

		public override bool Equals(object obj) => obj is Room room ? this == room : false;

		public bool Equals(Room other) => this == other;

		public override string ToString() => Name;

		public override int GetHashCode() => Name.GetHashCode();

		public void UpdateNeibors(World world) => Neighbors = (from entry in NeighborsNames
															   let room = world.RoomsByName.GetValueOrDefault(entry.Value)
															   where room != null
															   select (Direction: entry.Key, Room: room))
															.ToDictionary(KeyValuePair => KeyValuePair.Direction, pair => pair.Room);
	}
	}
