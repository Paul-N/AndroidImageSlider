using Android.Views;
using NineOldAndroids.View;

namespace Daimajia.Slider.Transformers
{

	public class FlipHorizontalTransformer : BaseTransformer
	{

		protected internal override void onTransform(View view, float position)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float rotation = 180f * position;
			float rotation = 180f * position;
			ViewHelper.SetAlpha(view,rotation > 90f || rotation < -90f ? 0 : 1);
			ViewHelper.SetPivotY(view,view.Height * 0.5f);
			ViewHelper.SetPivotX(view,view.Width * 0.5f);
			ViewHelper.SetRotationY(view,rotation);
		}

	}

}