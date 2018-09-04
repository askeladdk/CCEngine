using System;
using System.Collections.Generic;
using System.Linq;

namespace CCEngine.ECS
{
	public class Blueprint
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
