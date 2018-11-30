using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using CCEngine.Collections;

namespace CCEngine.ECS
{
	public class EntityComponentRegistry
		: ICopyable
	{

		#region IComponentRegistry

		private interface IComponentRegistry
			: ICopyable
		{
			void Set(int eid, object component);
			void Remove(int eid);
		}

		#endregion

		#region ComponentRegistry<T>

		private class ComponentRegistry<T>
			: IComponentRegistry
			where T:struct
		{
			private GapBuffer<int> entities =
				new GapBuffer<int>();
			private GapBuffer<T> components =
				new GapBuffer<T>();

			public void Set(int eid, T component)
			{
				var index = this.entities.BinarySearch(eid);
				if(index >= 0)
				{
					this.components[index] = component;
				}
				else
				{
					this.entities.Insert(~index, eid);
					this.components.Insert(~index, component);
				}
			}

			public void Set(int eid, object component)
			{
				Set(eid, (T)component);
			}

			public T Get(int eid)
			{
				var index = this.entities.BinarySearch(eid);
				if(index < 0)
					throw new ArgumentOutOfRangeException();
				return this.components[index];
			}

			public void Remove(int eid)
			{
				var index = this.entities.BinarySearch(eid);
				if(index < 0)
					return;
				this.entities.RemoveAt(index);
				this.components.RemoveAt(index);
			}

			public void CopyTo(object dst)
			{
				var that = (ComponentRegistry<T>)dst;
				this.entities.CopyTo(that.entities);
				this.components.CopyTo(that.components);
			}
		}

		#endregion

		private GapBuffer<int> alive;
		private ulong[] flags;
		private byte[] gens;
		private RingBuffer<int> entities;
		private IComponentRegistry[] components;
		private Dictionary<Type, int> typeIndices =
			new Dictionary<Type, int>();

		private IComponentRegistry CreateComponentRegistry(Type t)
		{
			var gt = typeof(ComponentRegistry<>).MakeGenericType(t);
			return (IComponentRegistry)Activator.CreateInstance(gt);
		}

		public EntityComponentRegistry(int capacity, Type[] types)
		{
			alive = new GapBuffer<int>(capacity);
			flags = new ulong[capacity];
			gens = new byte[capacity];

			entities = new RingBuffer<int>(capacity);
			for(var i = 0; i < capacity; i++)
				entities.Enqueue(i);

			components = new IComponentRegistry[types.Length];
			for(var typeIndex = 0; typeIndex < types.Length; typeIndex++)
			{
				var t = types[typeIndex];
				typeIndices[t] = typeIndex;
				components[typeIndex] = CreateComponentRegistry(t);
			}
		}

		public int Capacity
		{
			get => entities.Capacity;
		}

		public int Count
		{
			get => alive.Count;
		}

		public bool IsAlive(Entity entity)
		{
			var eid = entity.ID;
			var index = this.alive.BinarySearch(eid);
			return (index >= 0) && (gens[eid] == entity.Generation);
		}

		public Entity Create()
		{
			var eid = entities.Dequeue();
			var gen = gens[eid];
			alive.Insert(~alive.BinarySearch(eid), eid);
			return Entity.Create(eid, gen);
		}

		public void Destroy(Entity entity)
		{
			if(!IsAlive(entity))
				return;
			var eid = entity.ID;
			entities.Enqueue(eid);
			alive.RemoveAt(alive.BinarySearch(eid));
			flags[eid] = 0;
			gens[eid] += 1;
			foreach(var componentSet in components)
				componentSet.Remove(eid);
		}

		private int TypeIndex(Type type)
		{
			return typeIndices[type];
		}

		private ulong TypeMask(Type t)
		{
			return 1U << TypeIndex(t);
		}

		private int TypeIndex<T>()
			where T:struct
		{
			return TypeIndex(typeof(T));
		}

		private ulong TypeMask<T>()
			where T:struct
		{
			return TypeMask(typeof(T));
		}

		private void Tag(Type type, Entity entity)
		{
			flags[entity.ID] |= TypeMask(type);
		}

		private void Tag<T>(Entity entity)
			where T:struct
		{
			flags[entity.ID] |= TypeMask<T>();
		}

		private void Untag<T>(Entity entity)
			where T:struct
		{
			flags[entity.ID] &= ~TypeMask<T>();
		}

		private IComponentRegistry GetComponents(Type type)
		{
			return components[TypeIndex(type)];
		}

		private ComponentRegistry<T> GetComponents<T>()
			where T:struct
		{
			return (ComponentRegistry<T>)GetComponents(typeof(T));
		}

		public bool Has<T>(Entity entity)
			where T:struct
		{
			return (flags[entity.ID] & TypeMask<T>()) != 0;
		}

		public bool Has<T0, T1>(Entity entity)
			where T0:struct
			where T1:struct
		{
			var mask = 0
				| TypeMask<T0>()
				| TypeMask<T1>();
			return (flags[entity.ID] & mask) == mask;
		}

		public bool Has<T0, T1, T2>(Entity entity)
			where T0:struct
			where T1:struct
			where T2:struct
		{
			var mask = 0
				| TypeMask<T0>()
				| TypeMask<T1>()
				| TypeMask<T2>();
			return (flags[entity.ID] & mask) == mask;
		}

		public bool Has<T0, T1, T2, T3>(Entity entity)
			where T0:struct
			where T1:struct
			where T2:struct
			where T3:struct
		{
			var mask = 0
				| TypeMask<T0>()
				| TypeMask<T1>()
				| TypeMask<T2>()
				| TypeMask<T3>();
			return (flags[entity.ID] & mask) == mask;
		}

		public void Set(Type componentType, Entity entity, object component)
		{
			if(!IsAlive(entity))
				throw new ArgumentException();
			GetComponents(componentType).Set(entity.ID, component);
			Tag(componentType, entity);
		}

		public void Set<T>(Entity entity, T component)
			where T:struct
		{
			Set(typeof(T), entity, component);
		}

		public bool TryGet<T>(Entity entity, out T component)
			where T:struct
		{
			var eid = entity.ID;
			var ok = IsAlive(entity) && Has<T>(entity);
			component = ok ? GetComponents<T>().Get(eid) : default(T);
			return ok;
		}

		public T Get<T>(Entity entity)
			where T:struct
		{
			T component;
			if(!TryGet(entity, out component))
				throw new ArgumentException();
			return component;
		}

		public void Unset<T>(Entity entity)
			where T:struct
		{
			GetComponents<T>().Remove(entity.ID);
			Untag<T>(entity);
		}

		public IEnumerable<Entity> View()
		{
			foreach(var eid in alive)
				yield return Entity.Create(eid, gens[eid]);
		}

		private IEnumerable<Entity> View(ulong mask)
		{
			foreach(var eid in alive)
				if((flags[eid] & mask) == mask)
					yield return Entity.Create(eid, gens[eid]);
		}

		public IEnumerable<Entity> View<T>()
			where T:struct
		{
			return View(TypeMask<T>());
		}

		public IEnumerable<Entity> View<T0, T1>()
			where T0:struct
			where T1:struct
		{
			var mask = 0
				| TypeMask<T0>()
				| TypeMask<T1>();
			return View(mask);
		}

		public IEnumerable<Entity> View<T0, T1, T2>()
			where T0:struct
			where T1:struct
			where T2:struct
		{
			var mask = 0
				| TypeMask<T0>()
				| TypeMask<T1>()
				| TypeMask<T2>();
			return View(mask);
		}

		public IEnumerable<Entity> View<T0, T1, T2, T3>()
			where T0:struct
			where T1:struct
			where T2:struct
			where T3:struct
		{
			var mask = 0
				| TypeMask<T0>()
				| TypeMask<T1>()
				| TypeMask<T2>()
				| TypeMask<T3>();
			return View(mask);
		}

		#region ICopyable

		public void CopyTo(object dst)
		{
			var that = (EntityComponentRegistry)dst;

			Debug.Assert(this.Capacity == that.Capacity);
			Debug.Assert(this.components.Length == that.components.Length);

			this.alive.CopyTo(that.alive);
			this.entities.CopyTo(that.entities);

			Array.Copy(this.flags, that.flags, this.flags.Length);
			Array.Copy(this.gens, that.gens, this.gens.Length);

			that.typeIndices = new Dictionary<Type, int>(this.typeIndices);

			for(var i = 0; i < this.components.Length; i++)
				this.components[i].CopyTo(that.components[i]);
		}

		#endregion
	}
}
