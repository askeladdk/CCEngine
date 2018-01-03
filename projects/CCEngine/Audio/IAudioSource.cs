using OpenTK.Audio.OpenAL;

namespace CCEngine.Audio
{
	public interface IAudioSource
	{
		IAudioStream GetStream();
	}
}