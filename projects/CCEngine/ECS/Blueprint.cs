using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCEngine.ECS
{
	public interface IBlueprint
	{
		IAttributeTable Configuration { get; }
		IEnumerable<Type> ComponentTypes { get; }
	}

	public class Blueprint : IBlueprint
	{
		private IAttributeTable config;
		private Type[] componentTypes;

		public Blueprint(IAttributeTable config, IEnumerable<Type> componentTypes)
		{
			this.config = config;
			this.componentTypes = componentTypes.ToArray();
		}

		public Blueprint(IAttributeTable config, params Type[] componentTypes)
		{
			this.config = config;
			this.componentTypes = componentTypes;
		}

		public IAttributeTable Configuration
		{
			get { return config; }
		}

		public IEnumerable<Type> ComponentTypes
		{
			get { return componentTypes; }
		}
	}
}
