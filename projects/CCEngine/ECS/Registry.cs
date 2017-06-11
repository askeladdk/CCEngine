using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CCEngine.ECS
{
	using Entity = Dictionary<Type, IComponent>;
	using ReadOnlyEntity = IReadOnlyDictionary<Type, IComponent>;

	/// <summary>
	/// Component interface.
	/// </summary>
	public interface IComponent
	{
		void Initialise(IAttributeTable table);
	}

	/// <summary>
	/// Entity filter interface.
	/// </summary>
	public interface IFilter : IDisposable
	{
		IFilter All(params Type[] signature);
		IFilter One(params Type[] signature);
		IFilter None(params Type[] signature);
		IEnumerable<int> Entities { get; }
	}

	/// <summary>
	/// Entity-Component Registry.
	/// </summary>
	public class Registry
	{
		private class RegistryFilter : IFilter
		{
			private Type[] all, one, none;
			private HashSet<int> entities = new HashSet<int>();
			private Registry registry;
			private bool evaluated = false;

			public RegistryFilter(Registry registry)
			{
				this.registry = registry;
				registry.OnEntityKilled += this.OnEntityKilled;
				registry.OnEntitySpawned += this.OnEntitySpawned;
				registry.OnEntityUpdated += this.OnEntityUpdated;
			}

			public void Dispose()
			{
				registry.OnEntityKilled -= this.OnEntityKilled;
				registry.OnEntitySpawned -= this.OnEntitySpawned;
				registry.OnEntityUpdated -= this.OnEntityUpdated;
				entities.Clear();
			}

			private void Evaluate()
			{
				if(!evaluated)
					foreach (var entityId in registry.Entities)
						if (!entities.Contains(entityId) && Match(registry, entityId))
							entities.Add(entityId);
				evaluated = true;
			}

			private bool Match(Registry registry, int entityId)
			{
				var entity = registry.GetComponents(entityId);
				var matches = true;
				if (all != null)
					matches = matches && all.All(x => entity.ContainsKey(x));
				if (one != null)
					matches = matches && one.Any(x => entity.ContainsKey(x));
				if (none != null)
					matches = matches && !none.Any(x => entity.ContainsKey(x));
				return matches;
			}

			public void OnEntitySpawned(Registry registry, int entityId)
			{
				if (registry.IsAlive(entityId) && Match(registry, entityId))
					entities.Add(entityId);
			}

			public void OnEntityUpdated(Registry registry, int entityId)
			{
				var match = registry.IsAlive(entityId) && Match(registry, entityId);
				if (match)
					entities.Add(entityId);
				else
					entities.Remove(entityId);
			}

			public void OnEntityKilled(Registry registry, int entityId)
			{
				entities.Remove(entityId);
			}

			public IEnumerable<int> Entities
			{
				get
				{
					Evaluate();
					return entities;
				}
			}

			public IFilter All(params Type[] signature)
			{
				this.all = signature;
				return this;
			}

			public IFilter One(params Type[] signature)
			{
				this.one = signature;
				return this;
			}

			public IFilter None(params Type[] signature)
			{
				this.none = signature;
				return this;
			}
		}

		private Dictionary<int, Entity> entities =
			new Dictionary<int, Entity>();
		private Queue<int> killQueue =
			new Queue<int>();
		private SortedSet<Processor> processors =
			new SortedSet<Processor>();
		private int idCounter = 0;

		// Events.
		public event Action<Registry, int> OnEntitySpawned;
		public event Action<Registry, int> OnEntityUpdated;
		public event Action<Registry, int> OnEntityKilled;

		private IComponent CreateComponent(Type type, IAttributeTable table)
		{
			IComponent component = (IComponent)Activator.CreateInstance(type);
			component.Initialise(table);
			return component;
		}

		private Entity GetComponentsMutable(int entityId)
		{
			Entity entity;
			entities.TryGetValue(entityId, out entity);
			return entity;
		}

		protected ReadOnlyEntity GetComponents(int entityId)
		{
			return GetComponentsMutable(entityId);
		}

		public bool TryGetComponent<TComponent>(int entityId, out TComponent component)
			where TComponent : IComponent
		{
			var components = GetComponents(entityId);
			var success = false;
			IComponent c = null;
			if (components != null)
				success = components.TryGetValue(typeof(TComponent), out c);
			component = (TComponent)c;
			return success;
		}

		public TComponent GetComponent<TComponent>(int entityId)
			where TComponent : IComponent
		{
			TComponent component;
			if (!TryGetComponent<TComponent>(entityId, out component))
				throw new NotFound("Component [{0}.{1}] Not Found".F(entityId, typeof(TComponent).Name));
			return component;
		}

		public TComponent AddComponent<TComponent>(int entityId, IAttributeTable attrTable)
			where TComponent : IComponent
		{
			var entity = GetComponentsMutable(entityId);
			var type = typeof(TComponent);
			IComponent c;
			if (entity == null)
				return default(TComponent);
			else if (entity.TryGetValue(type, out c))
				return (TComponent)c;
			c = CreateComponent(type, attrTable);
			c.Initialise(attrTable);
			entity[type] = c;
			OnEntityUpdated.Invoke(this, entityId);
			return (TComponent)c;
		}

		public bool RemoveComponent<TComponent>(int entityId)
			where TComponent : IComponent
		{
			var entity = GetComponentsMutable(entityId);
			var type = typeof(TComponent);
			if (entity == null)
				return true;
			else if (!entity.ContainsKey(type))
				return false;
			entity.Remove(type);
			OnEntityUpdated.Invoke(this, entityId);
			return true;
		}

		private int Spawn(Entity entity)
		{
			var entityId = ++idCounter;
			entities[entityId] = entity;
			OnEntitySpawned.Invoke(this, entityId);
			return entityId;
		}

		public int Spawn()
		{
			return Spawn(new Entity());
		}

		public int Spawn(IBlueprint bluePrint, IAttributeTable attrTable)
		{
			var table = new AttributeTableList
			{
				attrTable,
				bluePrint.Configuration
			};
			var entity = bluePrint.ComponentTypes.ToDictionary(
				x => x,
				x =>
				{
					var c = CreateComponent(x, table);
					c.Initialise(table);
					return c;
				}
			);
			return Spawn(entity);
		}

		public bool IsAlive(int entityId)
		{
			return entities.ContainsKey(entityId);
		}

		public void Kill(int entityId)
		{
			Entity entity;
			if (entities.TryGetValue(entityId, out entity))
				killQueue.Enqueue(entityId);
		}

		public void AddProcessor(Processor processor)
		{
			processors.Add(processor);
		}

		public void RemoveProcessor(Processor processor)
		{
			processors.Remove(processor);
		}

		public void Update(float dt)
		{
			while (killQueue.Count > 0)
			{
				var entityId = killQueue.Dequeue();
				if (entities.ContainsKey(entityId))
				{
					OnEntityKilled.Invoke(this, entityId);
					entities.Remove(entityId);
				}
			}

			foreach (var p in processors)
				if (p.IsActive && !p.IsRenderLoop)
					p.Process(dt);
		}

		public void Render(float dt)
		{
			foreach (var p in processors)
				if (p.IsActive && p.IsRenderLoop)
					p.Process(dt);
		}

		public IEnumerable<int> Entities
		{
			get { return entities.Keys.AsEnumerable(); }
		}

		public IFilter Filter
		{
			get
			{
				var filter = new RegistryFilter(this);
				return filter;
			}
		}
	}
}
