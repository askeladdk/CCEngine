using System;
using OpenTK;

namespace CCEngine.Logic
{
	class NullState : IGameState
	{
		public void HandleMessage(IMessage message) { }
		public void Show() { }
		public void Hide() { }
		public void Update(float dt) { }
		public void Render(float dt) { }
	}
}
