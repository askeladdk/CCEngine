using System;

namespace CCEngine.Logic
{
	public class GameLogic : IMessageHandler
	{
		private IGameState[] states =
		{
			new NullState(),
			new MainMenu(),
			// new SimulationTest(),
		};

		private int curState = 0;

		private IGameState State { get { return states[curState]; } }

		public GameLogic(Game g)
		{
		}

		public void OnMessage(IMessage msg)
		{
			MsgGotoState gostate;
			if(msg.Is<MsgGotoState>(out gostate))
			{
				State.Hide();
				curState = gostate.state;
				State.Show();
			}
			else
			{
				State.HandleMessage(msg);
			}
		}

		public bool Update(float dt)
		{
			if (curState == 0)
				return false;
			State.Update(dt);
			return true;
		}

		public void Render(float dt)
		{
			State.Render(dt);
		}
	}
}
