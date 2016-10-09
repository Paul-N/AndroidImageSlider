using Android.Views;
using NineOldAndroids.View;
using System;

namespace Daimajia.Slider.Transformers
{
	public class ZoomOutTransformer : BaseTransformer
	{

		protected internal override void onTransform(View view, float position)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float scale = 1f + Math.abs(position);
			float scale = 1f + Math.Abs(position);
			ViewHelper.SetScaleX(view,scale);
			ViewHelper.SetScaleY(view,scale);
			ViewHelper.SetPivotX(view,view.Width * 0.5f);
			ViewHelper.SetPivotY(view,view.Width * 0.5f);
			ViewHelper.SetAlpha(view,position < -1f || position > 1f ? 0f : 1f - (scale - 1f));
			if (position < -0.9)
			{
				//-0.9 to prevent a small bug
				ViewHelper.SetTranslationX(view,view.Width * position);
			}
		}

	}
}