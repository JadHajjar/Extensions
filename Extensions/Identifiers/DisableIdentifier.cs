using System;

namespace Extensions
{
	public class DisableIdentifier
	{
		private static ulong IdList = 0;

		public DisableIdentifier() => ID = IdList++;

		public DisableIdentifier(bool enabled)
		{
			Disabled = !enabled; ID = IdList++;
		}

		public bool Disabled { get; private set; } = false;
		public bool Enabled => !Disabled;
		public ulong ID { get; protected set; }

		public virtual void Disable() => Disabled = true;

		public bool Disable(int milliseconds)
		{
			if (Disabled)
				return true;
			Disabled = true;

			new BackgroundAction(Enable).RunIn(milliseconds);
			return false;
		}

		public virtual void Enable() => Disabled = false;
	}
}