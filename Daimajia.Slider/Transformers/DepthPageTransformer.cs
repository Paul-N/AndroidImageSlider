using Android.Views;
using NineOldAndroids.View;
using System;

namespace Daimajia.Slider.Transformers
{
	public class DepthPageTransformer : BaseTransformer
	{

		private const float MIN_SCALE = 0.75f;

		protected internal override void onTransform(View view, float position)
		{
			if (position <= 0f)
			{
				ViewHelper.SetTranslationX(view,0f);
				ViewHelper.SetScaleX(view,1f);
				ViewHelper.SetScaleY(view,1f);
			}
			else if (position <= 1f)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float scaleFactor = MIN_SCALE + (1 - MIN_SCALE) * (1 - Math.abs(position));
				float scaleFactor = MIN_SCALE + (1 - MIN_SCALE) * (1 - Math.Abs(position));
				ViewHelper.SetAlpha(view,1 - position);
				ViewHelper.SetPivotY(view,0.5f * view.Height);
				ViewHelper.SetTranslationX(view,view.Width * - position);
				ViewHelper.SetScaleX(view,scaleFactor);
				ViewHelper.SetScaleY(view,scaleFactor);
			}
		}

		protected internal override bool PagingEnabled
		{
			get
			{
				return true;
			}
		}

	}

}