using System.Collections.Generic;
using Android.Content;
using Android.Support.V4.View;
using Android.Views;
using Daimajia.Slider.SliderTypes;

namespace Daimajia.Slider
{
    /// <summary>
    /// A slider adapter
    /// </summary>
    public class SliderAdapter : PagerAdapter, BaseSliderView.ImageLoadListener
	{

		private Context mContext;
		private List<BaseSliderView> mImageContents;

		public SliderAdapter(Context context)
		{
			mContext = context;
			mImageContents = new List<BaseSliderView>();
		}

		public virtual void addSlider<T>(T slider) where T : BaseSliderView
		{
			slider.OnImageLoadListener = this;
			mImageContents.Add(slider);
			NotifyDataSetChanged();
		}

		public virtual BaseSliderView getSliderView(int position)
		{
			if (position < 0 || position >= mImageContents.Count)
			{
				return null;
			}
			else
			{
				return mImageContents[position];
			}
		}

		public override int GetItemPosition(Java.Lang.Object @object)
		{
			return PositionNone;
		}

		public virtual void removeSlider<T>(T slider) where T : BaseSliderView
		{
			if (mImageContents.Contains(slider))
			{
				mImageContents.Remove(slider);
				NotifyDataSetChanged();
			}
		}

		public virtual void removeSliderAt(int position)
		{
			if (mImageContents.Count > position)
			{
				mImageContents.RemoveAt(position);
				NotifyDataSetChanged();
			}
		}

		public virtual void removeAllSliders()
		{
			mImageContents.Clear();
			NotifyDataSetChanged();
		}

		public override int Count
		{
			get
			{
				return mImageContents.Count;
			}
		}

		public override bool IsViewFromObject(Android.Views.View view, Java.Lang.Object objectValue)
        {
			return view == objectValue;
		}

		public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object @object)
		{
			container.RemoveView((View) @object);
		}

		public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
		{
			BaseSliderView b = mImageContents[position];
			View v = b.View;
			container.AddView(v);
			return v;
		}

		public void onStart(BaseSliderView target)
		{

		}

		/// <summary>
		/// When image download error, then remove. </summary>
		/// <param name="result"> </param>
		/// <param name="target"> </param>
		public void onEnd(bool result, BaseSliderView target)
		{
			if (target.ErrorDisappear == false || result == true)
			{
				return;
			}
			foreach (BaseSliderView slider in mImageContents)
			{
				if (slider.Equals(target))
				{
					removeSlider(target);
					break;
				}
			}
		}

	}

}