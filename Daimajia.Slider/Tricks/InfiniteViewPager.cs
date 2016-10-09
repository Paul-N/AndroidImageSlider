using Android.Content;
using Android.Support.V4.View;
using Android.Util;

namespace Daimajia.Slider.Tricks
{


    /// <summary>
    /// A <seealso cref="ViewPager"/> that allows pseudo-infinite paging with a wrap-around effect. Should be used with an {@link
    /// InfinitePagerAdapter}.
    /// </summary>
    [Android.Runtime.Register("com.daimajia.slider.library.Tricks.InfiniteViewPager")]
    public class InfiniteViewPager : ViewPagerEx
	{

		public InfiniteViewPager(Context context) : base(context)
		{
		}

		public InfiniteViewPager(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		public override PagerAdapter Adapter
		{
			set
			{
				base.Adapter = value;
			}
		}

	}
}