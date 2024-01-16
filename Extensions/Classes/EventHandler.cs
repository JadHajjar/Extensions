#if !NET47
namespace Extensions
{
	public delegate void EventHandler<T>(object sender, T e);
}
#endif