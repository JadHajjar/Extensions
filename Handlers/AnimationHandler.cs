using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Timer = System.Windows.Forms.Timer;

namespace Extensions
{
	[Flags]
	public enum AnimationOption
	{
		None = 0,
		IgnoreX = 1,
		IgnoreY = 2,
		IgnoreWidth = 4,
		IgnoreHeight = 8,
		Padding = 16,
		IgnoreLeft = 32,
		IgnoreTop = 64,
		IgnoreRight = 128,
		IgnoreBottom = 256,
		CustomAnimation = 512,
	}

	public class AnimationHandler : IDisposable
	{
		private const int INTERVAL = 14;

		public delegate void AnimationTick(AnimationHandler handler, Control control, bool finished);

		public event AnimationTick OnAnimationTick;

		public event AnimationTick OnEnd;

		public static bool NoAnimations { get; set; }

		public Control AnimatedControl { get; }
		public ReadOnlyCollection<IAnimatable> Animations { get; }
		public Rectangle NewBounds { get; }
		public Padding NewPadding { get; }
		public AnimationOption Options { get; }
		public double Speed { get; set; } = 1;
		public bool Animating { get; private set; }

		private Timer timer;
		private Action endAction;
		private bool disposedValue;

		private static readonly List<AnimationHandler> runningHandlers = new List<AnimationHandler>();
		private static readonly object lockObj = new object();

		public static bool IsAnimating
		{
			get
			{
				lock (lockObj)
					return runningHandlers.Any(x => x.Animating);
			}
		}

		#region Constructors

		public AnimationHandler(Control animatedControl, IList<IAnimatable> animations, double speed = 1, AnimationOption options = AnimationOption.None)
		{
			AnimatedControl = animatedControl;
			Animations = new ReadOnlyCollection<IAnimatable>(animations);
			Options = options | AnimationOption.CustomAnimation;
			Speed = speed;
		}

		public AnimationHandler(Control animatedControl, Rectangle newBounds, double speed, AnimationOption options = AnimationOption.None)
		{
			AnimatedControl = animatedControl;
			NewBounds = newBounds;
			Options = options;
			Speed = speed;
		}

		public AnimationHandler(Control animatedControl, Padding newPadding, double speed, AnimationOption options = AnimationOption.None)
		{
			AnimatedControl = animatedControl;
			NewPadding = newPadding;
			Options = options | AnimationOption.Padding;
			Speed = speed;
		}

		public AnimationHandler(Control animatedControl, Size newSize, double speed, AnimationOption options = AnimationOption.None)
			: this(animatedControl, new Rectangle(Point.Empty, newSize), speed, options)
		{
			Options |= AnimationOption.IgnoreX;
			Options |= AnimationOption.IgnoreY;
		}

		public AnimationHandler(Control animatedControl, Point newLoc, double speed, AnimationOption options = AnimationOption.None)
			: this(animatedControl, new Rectangle(newLoc, Size.Empty), speed, options)
		{
			Options |= AnimationOption.IgnoreWidth;
			Options |= AnimationOption.IgnoreHeight;
		}

		public AnimationHandler(Control animatedControl, Size newSize, AnimationOption options = AnimationOption.None)
			: this(animatedControl, newSize, 1, options) { }

		public AnimationHandler(Control animatedControl, Point newLoc, AnimationOption options = AnimationOption.None)
			: this(animatedControl, newLoc, 1, options) { }

		public AnimationHandler(Control animatedControl, Rectangle newBounds, AnimationOption options = AnimationOption.None)
			: this(animatedControl, newBounds, 1, options) { }

		public AnimationHandler(Control animatedControl, IAnimatable animations, double speed = 1, AnimationOption options = AnimationOption.None)
			: this(animatedControl, new[] { animations }, speed, options) { }

		#endregion Constructors

		#region Static Animations

		public static void Animate(Control animatedControl, IList<IAnimatable> animations, double speed = 1, AnimationOption options = AnimationOption.None, Action action = null)
			=> new AnimationHandler(animatedControl, animations, speed, options).StartAnimation(action);

		public static void Animate(Control animatedControl, IAnimatable animations, double speed = 1, AnimationOption options = AnimationOption.None, Action action = null)
			=> new AnimationHandler(animatedControl, new[] { animations }, speed, options).StartAnimation(action);

		public static void Animate<T>(T animatedControl, double speed = 1, AnimationOption options = AnimationOption.None, Action action = null) where T : Control, IAnimatable
			=> new AnimationHandler(animatedControl, new[] { animatedControl }, speed, options).StartAnimation(action);

		public static void Animate(Control animatedControl, Padding newPadding, double speed, AnimationOption options, Action action = null)
			=> new AnimationHandler(animatedControl, newPadding, speed, options).StartAnimation(action);

		public static void Animate(Control animatedControl, Padding newPadding, double speed, Action action = null)
			=> new AnimationHandler(animatedControl, newPadding, speed, AnimationOption.None).StartAnimation(action);

		public static void Animate(Control animatedControl, Padding newPadding, AnimationOption options, Action action = null)
			=> new AnimationHandler(animatedControl, newPadding, 1, options).StartAnimation(action);

		public static void Animate(Control animatedControl, Padding newPadding, Action action = null)
			=> new AnimationHandler(animatedControl, newPadding, 1, AnimationOption.None).StartAnimation(action);

		public static void Animate(Control animatedControl, Rectangle newBounds, double speed, AnimationOption options, Action action = null)
			=> new AnimationHandler(animatedControl, newBounds, speed, options).StartAnimation(action);

		public static void Animate(Control animatedControl, Rectangle newBounds, double speed, Action action = null)
			=> new AnimationHandler(animatedControl, newBounds, speed, AnimationOption.None).StartAnimation(action);

		public static void Animate(Control animatedControl, Rectangle newBounds, AnimationOption options, Action action = null)
			=> new AnimationHandler(animatedControl, newBounds, options).StartAnimation(action);

		public static void Animate(Control animatedControl, Rectangle newBounds, Action action = null)
			=> new AnimationHandler(animatedControl, newBounds, AnimationOption.None).StartAnimation(action);

		public static void Animate(Control animatedControl, Point newBounds, double speed, AnimationOption options, Action action = null)
			=> new AnimationHandler(animatedControl, newBounds, speed, options).StartAnimation(action);

		public static void Animate(Control animatedControl, Point newBounds, double speed, Action action = null)
			=> new AnimationHandler(animatedControl, newBounds, speed, AnimationOption.None).StartAnimation(action);

		public static void Animate(Control animatedControl, Point newBounds, AnimationOption options, Action action = null)
			=> new AnimationHandler(animatedControl, newBounds, options).StartAnimation(action);

		public static void Animate(Control animatedControl, Point newBounds, Action action = null)
			=> new AnimationHandler(animatedControl, newBounds, AnimationOption.None).StartAnimation(action);

		public static void Animate(Control animatedControl, Size newBounds, double speed, AnimationOption options, Action action = null)
			=> new AnimationHandler(animatedControl, newBounds, speed, options).StartAnimation(action);

		public static void Animate(Control animatedControl, Size newBounds, double speed, Action action = null)
			=> new AnimationHandler(animatedControl, newBounds, speed, AnimationOption.None).StartAnimation(action);

		public static void Animate(Control animatedControl, Size newBounds, AnimationOption options, Action action = null)
			=> new AnimationHandler(animatedControl, newBounds, options).StartAnimation(action);

		public static void Animate(Control animatedControl, Size newBounds, Action action = null)
			=> new AnimationHandler(animatedControl, newBounds, AnimationOption.None).StartAnimation(action);

		#endregion Static Animations

		public static void CancelAnimations(Control control, AnimationOption? options = null)
		{
			var toStop = new List<AnimationHandler>();

			lock (lockObj)
				toStop.AddRange(runningHandlers.Where(x => x.AnimatedControl == control && (options == null || x.Options == options)));

			foreach (var item in toStop) item.Dispose();
		}

		public static bool IsAnimated(Control control, AnimationOption? options = null)
		{
			lock (lockObj)
				return runningHandlers.Any(x => x.Animating && x.AnimatedControl == control && (options == null || x.Options == options));
		}

		public static AnimationHandler GetAnimation(Control control, AnimationOption? options = null)
		{
			lock (lockObj)
				return runningHandlers.FirstOrDefault(x => x.Animating && x.AnimatedControl == control && (options == null || x.Options == options));
		}

		public void StartAnimation(Action action = null)
		{
			if (AnimatedControl?.IsDisposed ?? true) return;

			endAction = action;

			if (!NoAnimations)
			{
				var toStop = new List<AnimationHandler>();

				lock (lockObj)
					toStop.AddRange(runningHandlers.Where(x => x.AnimatedControl == AnimatedControl && x.Options == Options));

				foreach (var item in toStop) item.Dispose();

				lock (lockObj)
					runningHandlers.Add(this);

				try
				{
					AnimatedControl?.TryInvoke(() =>
					{
						timer = new Timer { Interval = INTERVAL };
						timer.Tick += Timer_Tick;
						timer.Enabled = Animating = true;
					});
				}
				catch { }
			}
			else
			{
				applyBounds(NewBounds, NewPadding, Animations?.Select(x => x.TargetAnimationValue).ToArray());

				Dispose();
			}
		}

		public void StopAnimation()
		{
			Animating = false;

			try
			{
				AnimatedControl?.TryInvoke(() =>
				{
					timer?.Dispose();
					OnAnimationTick?.Invoke(this, AnimatedControl, true);
					endAction?.Invoke();
					OnEnd?.Invoke(this, AnimatedControl, true);

					lock (lockObj)
						runningHandlers.Remove(this);
				});
			}
			catch { }
		}

		public void ResumeAnimation() => AnimatedControl?.TryInvoke(() =>
		{
			timer?.Start();
			Animating = true;
		});

		public void PauseAnimation() => AnimatedControl?.TryInvoke(() =>
		{
			timer?.Stop();
			Animating = false;
		});

		private bool applyBounds(Rectangle bounds, Padding padding, int[] newValues) => AnimatedControl?.TryInvoke(() =>
		{
			if (Options.HasFlag(AnimationOption.CustomAnimation))
			{
				var done = true;
				for (int i = 0; i < Animations.Count; i++)
				{
					Animations[i].AnimatedValue = newValues[i];
					done &= Animations[i].TargetAnimationValue == newValues[i];
				}

				if (done)
				{
					AnimatedControl.Invalidate();
					Dispose();
				}
				else
					OnAnimationTick?.Invoke(this, AnimatedControl, false);
			}
			else if (!Options.HasFlag(AnimationOption.Padding))
			{
				var current = AnimatedControl.Bounds;

				AnimatedControl.Bounds = new Rectangle(
					Options.HasFlag(AnimationOption.IgnoreX) ? current.X : bounds.X,
					Options.HasFlag(AnimationOption.IgnoreY) ? current.Y : bounds.Y,
					Options.HasFlag(AnimationOption.IgnoreWidth) ? current.Width : bounds.Width,
					Options.HasFlag(AnimationOption.IgnoreHeight) ? current.Height : bounds.Height
					);

				if ((Options.HasFlag(AnimationOption.IgnoreX) || NewBounds.X == bounds.X) &&
					(Options.HasFlag(AnimationOption.IgnoreY) || NewBounds.Y == bounds.Y) &&
					(Options.HasFlag(AnimationOption.IgnoreWidth) || NewBounds.Width == bounds.Width) &&
					(Options.HasFlag(AnimationOption.IgnoreHeight) || NewBounds.Height == bounds.Height))
				{
					AnimatedControl.Invalidate();
					Dispose();
				}
				else
					OnAnimationTick?.Invoke(this, AnimatedControl, false);
			}
			else
			{
				var current = AnimatedControl.Padding;

				AnimatedControl.Padding = new Padding(
					Options.HasFlag(AnimationOption.IgnoreLeft) ? current.Left : padding.Left,
					Options.HasFlag(AnimationOption.IgnoreTop) ? current.Top : padding.Top,
					Options.HasFlag(AnimationOption.IgnoreRight) ? current.Right : padding.Right,
					Options.HasFlag(AnimationOption.IgnoreBottom) ? current.Bottom : padding.Bottom
					);

				if ((Options.HasFlag(AnimationOption.IgnoreLeft) || NewPadding.Left == padding.Left) &&
					(Options.HasFlag(AnimationOption.IgnoreTop) || NewPadding.Top == padding.Top) &&
					(Options.HasFlag(AnimationOption.IgnoreRight) || NewPadding.Right == padding.Right) &&
					(Options.HasFlag(AnimationOption.IgnoreBottom) || NewPadding.Bottom == padding.Bottom))
				{
					AnimatedControl.Invalidate();
					Dispose();
				}
				else
					OnAnimationTick?.Invoke(this, AnimatedControl, false);
			}

			AnimatedControl.Invalidate();
		}) ?? false;

		private void Timer_Tick(object sender, EventArgs e)
		{
			if (Animating && AnimatedControl != null)
			{
				try
				{
					var current = AnimatedControl.Bounds;

					var newBounds = Options.HasFlag(AnimationOption.Padding) ? Rectangle.Empty : new Rectangle(
						Options.HasFlag(AnimationOption.IgnoreX) ? 0 : current.X + getStep(NewBounds.X - current.X),
						Options.HasFlag(AnimationOption.IgnoreY) ? 0 : current.Y + getStep(NewBounds.Y - current.Y),
						Options.HasFlag(AnimationOption.IgnoreWidth) ? 0 : current.Width + getStep(NewBounds.Width - current.Width),
						Options.HasFlag(AnimationOption.IgnoreHeight) ? 0 : current.Height + getStep(NewBounds.Height - current.Height)
					);

					var currentPadding = AnimatedControl.Padding;

					var newPadding = !Options.HasFlag(AnimationOption.Padding) ? Padding.Empty : new Padding(
						Options.HasFlag(AnimationOption.IgnoreLeft) ? 0 : currentPadding.Left + getStep(NewPadding.Left - currentPadding.Left),
						Options.HasFlag(AnimationOption.IgnoreTop) ? 0 : currentPadding.Top + getStep(NewPadding.Top - currentPadding.Top),
						Options.HasFlag(AnimationOption.IgnoreRight) ? 0 : currentPadding.Right + getStep(NewPadding.Right - currentPadding.Right),
						Options.HasFlag(AnimationOption.IgnoreBottom) ? 0 : currentPadding.Bottom + getStep(NewPadding.Bottom - currentPadding.Bottom)
					);

					var newValues = new int[Animations?.Count ?? 0];

					if (Options.HasFlag(AnimationOption.CustomAnimation))
					{
						for (int i = 0; i < Animations.Count; i++)
							newValues[i] = Animations[i].AnimatedValue + getStep(Animations[i].TargetAnimationValue - Animations[i].AnimatedValue);
					}

					if (!applyBounds(newBounds, newPadding, newValues))
						Dispose();
				}
				catch { Dispose(); }
			}

			int getStep(double diff)
			{
				if (diff == 0) return 0;

				var sign = diff.Sign();
				var x = Math.Abs(diff) + 500;

				return sign * (int)(Speed * 15 * (x - 450 - (x * x / 50000)) / (8 * Math.Sqrt(x))).Between(1, Math.Abs(diff));
			}
		}

		#region IDisposable Support

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
					StopAnimation();

				disposedValue = true;
			}
		}

		~AnimationHandler()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		#endregion IDisposable Support
	}

	public interface IAnimatable
	{
		int AnimatedValue { get; set; }
		int TargetAnimationValue { get; }
	}
}