using System;

namespace CCEngine.Logic
{
	interface IGameState
	{
		void HandleMessage(IMessage message);
		void Show();
		void Hide();
		void Update(float dt);
		void Render(float dt);
	}
}
