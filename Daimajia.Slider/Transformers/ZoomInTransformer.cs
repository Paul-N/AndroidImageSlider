using Android.Views;
using NineOldAndroids.View;
using System;

namespace Daimajia.Slider.Transformers
{
	public class ZoomInTransformer : BaseTransformer
	{

		protected internal override void onTransform(View view, float position)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float scale = position < 0 ? position + 1f : Math.abs(1f - position);
			float scale = position < 0 ? position + 1f : Math.Abs(1f - position);
			ViewHelper.SetScaleX(view,scale);
			ViewHelper.SetScaleY(view,scale);
			ViewHelper.SetPivotX(view,view.Width * 0.5f);
			ViewHelper.SetPivotY(view,view.Height * 0.5f);
			ViewHelper.SetAlpha(view,position < -1f || position > 1f ? 0f : 1f - (scale - 1f));
		}

	}

}