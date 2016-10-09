using Android.Views;
using NineOldAndroids.View;
using System;

namespace Daimajia.Slider.Transformers
{
	public class ZoomOutSlideTransformer : BaseTransformer
	{

		private const float MIN_SCALE = 0.85f;
		private const float MIN_ALPHA = 0.5f;

		protected internal override void onTransform(View view, float position)
		{
			if (position >= -1 || position <= 1)
			{
				// Modify the default slide transition to shrink the page as well
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float height = view.getHeight();
				float height = view.Height;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float scaleFactor = Math.max(MIN_SCALE, 1 - Math.abs(position));
				float scaleFactor = Math.Max(MIN_SCALE, 1 - Math.Abs(position));
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float vertMargin = height * (1 - scaleFactor) / 2;
				float vertMargin = height * (1 - scaleFactor) / 2;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float horzMargin = view.getWidth() * (1 - scaleFactor) / 2;
				float horzMargin = view.Width * (1 - scaleFactor) / 2;

				// Center vertically
				ViewHelper.SetPivotY(view,0.5f * height);


				if (position < 0)
				{
					ViewHelper.SetTranslationX(view,horzMargin - vertMargin / 2);
				}
				else
				{
					ViewHelper.SetTranslationX(view,-horzMargin + vertMargin / 2);
				}

				// Scale the page down (between MIN_SCALE and 1)
				ViewHelper.SetScaleX(view,scaleFactor);
				ViewHelper.SetScaleY(view,scaleFactor);

				// Fade the page relative to its size.
				ViewHelper.SetAlpha(view,MIN_ALPHA + (scaleFactor - MIN_SCALE) / (1 - MIN_SCALE) * (1 - MIN_ALPHA));
			}
		}

	}

}