using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using CCEngine.Collections;

namespace CCEngine.ECS
{
	public interface IComponent<T>
	{
	}

	public class ComponentManager
	{
		private Dictionary<Type, IDictionary> components =
			new Dictionary<Type, IDictionary>();
		private int capacity;

		public ComponentManager(int capacity)
		{
			this.capacity = capacity;
		}

		public void Clear()
		{
			foreach(var kv in components)
				kv.Value.Clear();
		}

		public void RegisterType<T>()
			where T:IComponent<T>
		{
			components.Add(typeof(T), new PackedArray<T>(capacity));
		}

		private IDictionary Components(Type t)
		{
			return components[t];
		}

		private IDictionary<int, T> Components<T>()
			where T:IComponent<T>
		{
			return (IDictionary<int, T>)components[typeof(T)];
		}

		public bool Attach<T>(int eid, T component)
			where T:IComponent<T>
		{
			Components<T>()[eid] = component;
			return true;
		}

		public bool Attach(Type t, int eid, object component)
		{
			Components(t)[eid] = component;
			return true;
		}

		public bool Detach<T>(int eid)
			where T:IComponent<T>
		{
			return Components<T>().Remove(eid);
		}

		public bool TryGet<T>(int eid, out T component)
			where T:IComponent<T>
		{
			return Components<T>().TryGetValue(eid, out component);
		}
	}

	public class EntityManager
	{
		private const uint EID_MASK = 0x00FFFFFF;
		private const uint GEN_MASK = 0xFF000000;
		private const int GEN_SHIFT = 24;

		private byte[] generations;
		private PackedArray<ulong> entities;
		private RingBuffer<int> available;

		public int Capacity { get => entities.Capacity; }

		public EntityManager(int capacity)
		{
			this.generations = new byte[capacity];
			this.entities = new PackedArray<ulong>(capacity);
			this.available = new RingBuffer<int>(capacity);
			for(var i = 0; i < capacity; i++)
				this.available.Enqueue(i);
		}

		/// Check if entity is alive.
		public bool IsAlive(Entity entity)
		{
			return generations[entity.ID] == entity.Generation;
		}

		/// Create a new entity.
		public bool Create(out Entity entity, ulong tag = 0)
		{
			if(available.Count == 0)
			{
				entity = Entity.Invalid;
				return false;
			}
			var eid = available.Dequeue();
			var gen = generations[eid];
			entity = Entity.Create(eid, gen);
			entities[eid] = tag;
			return true;
		}

		public bool Destroy(Entity entity)
		{
			var eid = entity.ID;
			if(!IsAlive(entity))
				return false;
			generations[eid] += 1;
			entities.Remove(eid);
			available.Enqueue(eid);
			return true;
		}

		public ulong Tag(Entity entity)
		{
			if(IsAlive(entity))
				return entities[entity.ID];
			throw new Exception("Invalid entity.");
		}

		public bool Tag(Entity entity, ulong tag)
		{
			var eid = entity.ID;
			if(!IsAlive(entity))
				return false;
			entities[eid] |= tag;
			return true;
		}

		public bool Untag(Entity entity, ulong tag)
		{
			var eid = entity.ID;
			if(!IsAlive(entity))
				return false;
			entities[eid] &= ~tag;
			return true;
		}

		public void Clear()
		{
			entities.Clear();
			available.Clear();
			for(var i = 0; i < entities.Capacity; i++)
				this.available.Enqueue(i);
			for(var i = 0; i < generations.Length; i++)
				generations[i] = 0;
		}

		public IEnumerable<Entity> View()
		{
			IReadOnlyDictionary<int, ulong> ro = entities;
			foreach(var eid in ro.Keys)
				yield return Entity.Create(eid, generations[eid]);
		}

		public IEnumerable<Entity> View(ulong mask)
		{
			IReadOnlyDictionary<int, ulong> ro = entities;
			foreach(var kv in ro)
			{
				var eid = kv.Key;
				if((kv.Value & mask) == mask)
					yield return Entity.Create(eid, generations[eid]);
			}
		}

		public void CopyTo(EntityManager that)
		{
			this.entities.CopyTo(that.entities);
			this.available.CopyTo(that.available);
			Array.Copy(this.generations, that.generations, this.generations.Length);
		}
	}

	public class Registry
	{
		private EntityManager entities;
		private ComponentManager components;
		private Dictionary<Type, int> types = new Dictionary<Type, int>();

		public Registry(int capacity)
		{
			this.entities = new EntityManager(capacity);
			this.components = new ComponentManager(capacity);
		}

		public void RegisterType<T>()
			where T:IComponent<T>
		{
			components.RegisterType<T>();
			types.Add(typeof(T), types.Count);
		}

		public void Clear()
		{
			entities.Clear();
			components.Clear();
		}

		private ulong TypeMask(Type t)
		{
			return 1U << types[t];
		}

		private ulong TypeMask<T>()
			where T:IComponent<T>
		{
			return TypeMask(typeof(T));
		}

		public bool Has<T>(Entity entity)
			where T:IComponent<T>
		{
			return (entities.Tag(entity) & TypeMask<T>()) != 0;
		}

		public bool TryGet<T>(Entity entity, out T component)
			where T:IComponent<T>
		{
			return components.TryGet<T>(entity.ID, out component);
		}

		public T Get<T>(Entity entity)
			where T:IComponent<T>
		{
			T component;
			if(!TryGet<T>(entity, out component))
				throw new ArgumentException("Component type not found.");
			return component;
		}

		public bool IsAlive(Entity entity)
		{
			return entities.IsAlive(entity);
		}

		public Entity Create()
		{
			Entity entity;
			if(!entities.Create(out entity))
				throw new ArgumentException("No entities available.");
			return entity;
		}

		public bool Destroy(Entity entity)
		{
			return entities.Destroy(entity);
		}

		public bool Attach(Type t, Entity entity, object component)
		{
			var typeMask = TypeMask(t);
			if(!entities.Tag(entity, typeMask))
				return false;
			return components.Attach(t, entity.ID, component);			
		}

		public bool Attach<T>(Entity entity, T component)
			where T:IComponent<T>
		{
			return Attach(typeof(T), entity, component);
		}

		public bool Detach<T>(Entity entity)
			where T:IComponent<T>
		{
			var typeMask = TypeMask<T>();
			if(!entities.Untag(entity, typeMask))
				return false;
			return components.Detach<T>(entity.ID);
		}

		public IEnumerable<Entity> View()
		{
			return entities.View();
		}
  
		public IEnumerable<Entity> View<T>()
			where T:IComponent<T>
		{
			return entities.View(TypeMask<T>());
		}

		public IEnumerable<Entity> View<T0, T1>()
			where T0:IComponent<T0>
			where T1:IComponent<T1>
		{
			var mask = 0
				| TypeMask<T0>()
				| TypeMask<T1>();
			return entities.View(mask);
		}

		public IEnumerable<Entity> View<T0, T1, T2>()
			where T0:IComponent<T0>
			where T1:IComponent<T1>
			where T2:IComponent<T2>
		{
			var mask = 0
				| TypeMask<T0>()
				| TypeMask<T1>()
				| TypeMask<T2>();
			return entities.View(mask);
		}

		public IEnumerable<Entity> View<T0, T1, T2, T3>()
			where T0:IComponent<T0>
			where T1:IComponent<T1>
			where T2:IComponent<T2>
			where T3:IComponent<T3>
		{
			var mask = 0
				| TypeMask<T0>()
				| TypeMask<T1>()
				| TypeMask<T2>()
				| TypeMask<T3>();
			return entities.View(mask);
		}
	}
}
