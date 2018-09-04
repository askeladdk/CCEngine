namespace CCEngine
{
	/// <summary>
	/// Interface for objects that can be shallow copied.
	/// </summary>
	public interface ICopyable
	{
		void CopyTo(object dst);
	}
}