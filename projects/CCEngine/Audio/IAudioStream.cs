using OpenTK.Audio.OpenAL;

namespace CCEngine.Audio
{
	public interface IAudioStream
	{
		int SampleRate { get; }
		ALFormat Format { get; }
		bool Empty { get; }
		int ReadSamples(byte[] samples);
	}
}