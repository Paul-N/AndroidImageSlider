using Android.OS;
using Android.Views;
using Daimajia.Slider.Tricks;
using NineOldAndroids.View;
using System;

namespace Daimajia.Slider.Transformers
{
    public class FlipPageViewTransformer : BaseTransformer
	{

		protected internal override void onTransform(View view, float position)
		{
			float percentage = 1 - Math.Abs(position);
			if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.HoneycombMr2)
			{
				view.SetCameraDistance(12000);
			}
			setVisibility(view, position);
			Translation = view;
			setSize(view, position, percentage);
			setRotation(view, position, percentage);
		}

		private void setVisibility(View page, float position)
		{
			if (position < 0.5 && position > -0.5)
			{
				page.Visibility = ViewStates.Visible;
			}
			else
			{
				page.Visibility = ViewStates.Invisible;
			}
		}

		private View Translation
		{
			set
			{
				ViewPagerEx viewPager = (ViewPagerEx) value.Parent;
				int scroll = viewPager.ScrollX - value.Left;
				ViewHelper.SetTranslationX(value,scroll);
			}
		}

		private void setSize(View view, float position, float percentage)
		{
			ViewHelper.SetScaleX(view,(position != 0 && position != 1) ? percentage : 1);
			ViewHelper.SetScaleY(view,(position != 0 && position != 1) ? percentage : 1);
		}

		private void setRotation(View view, float position, float percentage)
		{
			if (position > 0)
			{
				ViewHelper.SetRotationY(view,-180 * (percentage + 1));
			}
			else
			{
				ViewHelper.SetRotationY(view,180 * (percentage + 1));
			}
		}
	}
}