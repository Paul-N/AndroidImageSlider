using Android.Views;
using NineOldAndroids.View;

namespace Daimajia.Slider.Transformers
{	public class StackTransformer : BaseTransformer
	{

		protected internal override void onTransform(View view, float position)
		{
			ViewHelper.SetTranslationX(view,position < 0 ? 0f : -view.Width * position);
		}

	}

}