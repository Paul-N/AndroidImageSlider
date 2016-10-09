using Android.Content;
using Android.Views.Animations;
using Android.Widget;

namespace Daimajia.Slider.Tricks
{
  	public class FixedSpeedScroller : Scroller
	{

		private int mDuration = 1000;

		public FixedSpeedScroller(Context context) : base(context)
		{
		}

		public FixedSpeedScroller(Context context, IInterpolator interpolator) : base(context, interpolator)
		{
		}

		public FixedSpeedScroller(Context context, IInterpolator interpolator, int period) : this(context,interpolator)
		{
			mDuration = period;
		}

		public override void StartScroll(int startX, int startY, int dx, int dy, int duration)
		{
			// Ignore received duration, use fixed one instead
			base.StartScroll(startX, startY, dx, dy, mDuration);
		}

		public override void StartScroll(int startX, int startY, int dx, int dy)
		{
			// Ignore received duration, use fixed one instead
			base.StartScroll(startX, startY, dx, dy, mDuration);
		}
	}

}