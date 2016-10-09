using Java.Lang;
using Android.OS;
using Android.Support.V4.View;
using Android.Views;
using Android.Util;

namespace Daimajia.Slider.Tricks
{
    /// <summary>
    /// A PagerAdapter that wraps around another PagerAdapter to handle paging wrap-around.
    /// Thanks to: https://github.com/antonyt/InfiniteViewPager
    /// </summary>
    public class InfinitePagerAdapter : PagerAdapter
	{

		private const string TAG = "InfinitePagerAdapter";
		private const bool DEBUG = false;

		private SliderAdapter adapter;

		public InfinitePagerAdapter(SliderAdapter adapter)
		{
			this.adapter = adapter;
		}

		public virtual SliderAdapter RealAdapter
		{
			get
			{
				return this.adapter;
			}
		}

		public override int Count
		{
			get
			{
				// warning: scrolling to very high values (1,000,000+) results in
				// strange drawing behaviour
				return int.MaxValue;
			}
		}

		/// <returns> the <seealso cref="#getCount()"/> result of the wrapped adapter </returns>
		public virtual int RealCount
		{
			get
			{
				return adapter.Count;
			}
		}

		public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
		{
			if (RealCount == 0)
			{
				return null;
			}
			int virtualPosition = position % RealCount;
			debug("instantiateItem: real position: " + position);
			debug("instantiateItem: virtual position: " + virtualPosition);

			// only expose virtual position to the inner adapter
			return adapter.InstantiateItem(container, virtualPosition);
		}

		public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object @object)
		{
			if (RealCount == 0)
			{
				return;
			}
			int virtualPosition = position % RealCount;
			debug("destroyItem: real position: " + position);
			debug("destroyItem: virtual position: " + virtualPosition);

			// only expose virtual position to the inner adapter
			adapter.DestroyItem(container, virtualPosition, @object);
		}

		/*
		 * Delegate rest of methods directly to the inner adapter.
		 */

		public override void FinishUpdate(ViewGroup container)
		{
			adapter.FinishUpdate(container);
		}

		public override bool IsViewFromObject(View view, Java.Lang.Object @object)
		{
			return adapter.IsViewFromObject(view, @object);
		}

		public override void RestoreState(IParcelable bundle, ClassLoader classLoader)
		{
			adapter.RestoreState(bundle, classLoader);
		}

		public override IParcelable SaveState()
		{
			return adapter.SaveState();
		}

		public override void StartUpdate(ViewGroup container)
		{
			adapter.StartUpdate(container);
		}

		/*
		 * End delegation
		 */

		private void debug(string message)
		{
			if (DEBUG)
			{
				Log.Debug(TAG, message);
			}
		}
	}
}