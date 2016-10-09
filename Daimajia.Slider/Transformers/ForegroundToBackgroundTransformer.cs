using Android.Views;
using NineOldAndroids.View;
using System;

namespace Daimajia.Slider.Transformers
{
	public class ForegroundToBackgroundTransformer : BaseTransformer
	{

		protected internal override void onTransform(View view, float position)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float height = view.getHeight();
			float height = view.Height;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float width = view.getWidth();
			float width = view.Width;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float scale = min(position > 0 ? 1f : Math.abs(1f + position), 0.5f);
			float scale = min(position > 0 ? 1f : Math.Abs(1f + position), 0.5f);

			ViewHelper.SetScaleX(view,scale);
			ViewHelper.SetScaleY(view,scale);
			ViewHelper.SetPivotX(view,width * 0.5f);
			ViewHelper.SetPivotY(view,height * 0.5f);
			ViewHelper.SetTranslationX(view,position > 0 ? width * position : -width * position * 0.25f);
		}

		private static float min(float val, float min)
		{
			return val < min ? min : val;
		}

	}

}