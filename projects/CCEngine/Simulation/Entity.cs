using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CCEngine.Simulation
{
	public interface IComponent
	{
		void Initialize(IAttributeTable table);
		IComponent Copy();
	}

	public interface IAttributeTable
	{
		bool Contains(string key);
		string GetString(string key, string otherwise = null);
		int GetInt(string key, int otherwise = 0);
		float GetFloat(string key, float otherwise = 0.0f);
		bool GetBool(string key, bool otherwise = false);
	}

	public class Blueprint
	{
		private readonly string name;
		private readonly IAttributeTable attributeTable;
		private readonly HashSet<Type> componentTypes = new HashSet<Type>();

		public string Name { get { return this.name; } }
		public IAttributeTable AttributeTable { get { return attributeTable; } }
		public IEnumerable<Type> ComponentTypes { get { return componentTypes; } }

		public Blueprint(string name, IAttributeTable attributeTable = null)
		{
			this.name = name;
			this.attributeTable = attributeTable;
		}

		public void Add<T>()
			where T : IComponent
		{
			componentTypes.Add(typeof(T));
		}
	}

	public abstract class EntityProcessor : IMessageHandler
	{
		private readonly List<int> entities = new List<int>();

		public void Process()
		{
			Process(this.entities);
		}

		public abstract HashSet<Type> ComponentSet { get; }

		public void HandleMessage(IMessage msg)
		{
			MsgSpawnEntity msgSpawn;
			MsgKillEntity msgKill;

			if (msg.Is(out msgSpawn))
			{
				var entityRegistry = Game.Instance.EntityRegistry;
				var entityId = msgSpawn.entityId;
				if (ComponentSet.IsSubsetOf(entityRegistry.GetEntityComponentTypes(entityId)))
					entities.Add(entityId);
			}
			else if (msg.Is(out msgKill))
			{
				entities.Remove(msgKill.entityId);
			}
			else
			{
				ProcessMessage(msg);
			}
		}

		protected virtual void ProcessMessage(IMessage msg) { }

		protected virtual void Process(List<int> entities)
		{
			foreach (var e in entities)
				Process(e);
		}

		protected virtual void Process(int e) { }
	}

	public class EntityRegistry
	{
		private readonly Dictionary<Type, Dictionary<int, IComponent>> components =
			new Dictionary<Type, Dictionary<int, IComponent>>();
		private int idCounter = 0;

		private Dictionary<int, IComponent> GetComponents(Type t)
		{
			Dictionary<int, IComponent> c;
			if (!components.TryGetValue(t, out c))
				c = components[t] = new Dictionary<int, IComponent>();
			return c;
		}

		public T GetComponent<T>(int entityId)
			where T : IComponent
		{
			IComponent c;
			GetComponents(typeof(T)).TryGetValue(entityId, out c);
			return (T)c;
		}

		public HashSet<Type> GetEntityComponentTypes(int entityId)
		{
			return new HashSet<Type>(
				from c in components
				where c.Value.ContainsKey(entityId)
				select c.Key
			);
		}

		public int Spawn(Blueprint blueprint, IAttributeTable attributes)
		{
			var entityId = ++idCounter;
			foreach (var t in blueprint.ComponentTypes)
			{
				var c = (IComponent)Activator.CreateInstance(t);
				GetComponents(t).Add(entityId, c);
				c.Initialize(attributes);
			}
			Game.Instance.SendMessage(new MsgSpawnEntity(entityId));
			return entityId;
		}

		public void Kill(int entityId)
		{
			Game.Instance.SendMessage(new MsgKillEntity(entityId));
			foreach (var v in components.Values)
				v.Remove(entityId);
		}

		public bool IsAlive(int entityId)
		{
			return components.Any(x => x.Value.ContainsKey(entityId));
		}
	}
}
