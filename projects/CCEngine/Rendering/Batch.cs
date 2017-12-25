using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace CCEngine.Rendering
{
	/// <summary>
	/// Base class for render batches.
	/// </summary>
	public abstract class Batch<Instance>
		where Instance : struct
	{
		private Instance[] instances;
		private int count;
		private int renderCalls;
		private int renderCallsTotal;
		private int queueOverrun;
		private int maxRendered;
		private BufferObject<Instance> vbo;

		/// <summary>
		/// Number of render calls made last time the batch was flushed.
		/// </summary>
		public int RenderCalls { get => renderCalls; }

		/// <summary>
		/// Total number of render calls ever made.
		/// </summary>
		public int TotalRenderCalls { get => renderCallsTotal; }

		/// <summary>
		/// How many instances failed to queue because the buffer was full since
		/// the last time the batch was flushed.
		/// </summary>
		public int QueueOverrun { get => queueOverrun; }

		/// <summary>
		/// Number of instances currently in the queue.
		/// </summary>
		public int Count { get => count; }

		/// <summary>
		/// Maximum number of instances that fit in the queue.
		/// </summary>
		public int Maximum { get => instances.Length; }

		/// <summary>
		/// Get the underlying vertex buffer object.
		/// </summary>
		protected BufferObject<Instance> VBO { get => vbo; }

		/// <summary>
		/// Get the size in bytes of the instance type stored in the queue.
		/// </summary>
		protected int InstanceStride { get => Helpers.SizeOf<Instance>(); }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected Batch(int maxInstances)
		{
			instances = new Instance[maxInstances];
			vbo = new BufferObject<Instance>(
				maxInstances, BufferTarget.ArrayBuffer, BufferUsageHint.StreamDraw);
			count = 0;
		}

		/// <summary>
		/// Queues an instance and returns whether it succeeded and its index.
		/// </summary>
		protected bool Queue(Instance instance, out int index)
		{
			if(count >= Maximum)
			{
				queueOverrun++;
				index = -1;
				return false;
			}
			else
			{
				instances[count] = instance;
				index = count++;
				return true;
			}
		}

		/// <summary>
		/// Called once before the batch is submitted to the GPU
		/// to set up the render state.
		/// </summary>
		protected abstract void BeginSubmit();

		/// <summary>
		/// Called once after the batch is submitted to the GPU
		/// to reset the render state.
		/// </summary>
		protected abstract void EndSubmit();

		/// <summary>
		/// Begin submitting the batch to the GPU.
		/// </summary>
		protected abstract int Submit(Instance[] instances, int count);

		/// <summary>
		/// Submit the batch to the GPU.
		/// </summary>
		public void Flush()
		{
			if(count == 0)
				return;

			queueOverrun = 0;
			renderCalls = 0;
			BeginSubmit();
			vbo.Bind();

			var nrenders = Submit(instances, count);
			renderCalls += nrenders;
			renderCallsTotal += nrenders;
			maxRendered = Math.Max(maxRendered, count);
			count = 0;

			vbo.Unbind();
			EndSubmit();
		}
	}

	/// <summary>
	/// Base class for unsorted render batches.
	/// </summary>
	public abstract class UnsortedBatch<Instance> : Batch<Instance>
		where Instance : struct
	{
		protected UnsortedBatch(int maxInstances) : base(maxInstances)
		{
		}

		public bool Queue(Instance instance)
		{
			int index;
			return base.Queue(instance, out index);
		}

		protected abstract void Submit(int count);

		protected override int Submit(Instance[] instances, int count)
		{
			VBO.Update(instances, 0, count);
			Submit(count);
			return 1;
		}
	}

	/// <summary>
	/// Base class for sorted render batches.
	/// </summary>
	public abstract class SortedBatch<GroupKey, Instance> : Batch<Instance>
		where GroupKey : IComparable<GroupKey>
		where Instance : struct
	{
		internal struct InstanceKey : IComparable<InstanceKey>
		{
			public GroupKey key;
			public int index;

			int IComparable<InstanceKey>.CompareTo(InstanceKey other)
			{
				var x = this.key.CompareTo(other.key);
				return x != 0 ? x : this.index - other.index;
			}
		};

		private InstanceKey[] keys;

		protected SortedBatch(int maxInstances) : base(maxInstances)
		{
			keys = new InstanceKey[maxInstances];
		}

		public bool Queue(GroupKey key, Instance instance)
		{
			int index;
			if(base.Queue(instance, out index))
			{
				keys[index].key = key;
				keys[index].index = index;
				return true;
			}
			return false;
		}

		protected abstract void Submit(GroupKey key, int count);

		protected override int Submit(Instance[] instances, int count)
		{
			Array.Sort(keys, instances, 0, count);

			var ncalls = 0;

			var i = 0;
			while(i < count)
			{
				// find the range of the group.
				var k = keys[i].key;
				var j = i;
				while(j < count && k.CompareTo(keys[j].key) == 0)
					j++;

				var range = j - i;

				// copy the range to the gpu.
				VBO.Update(instances, i, 0, range);

				// submit the render call
				Submit(k, range);
				ncalls++;

				i = j;
			}

			return ncalls;
		}
	}
}