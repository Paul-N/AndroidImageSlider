using Android.Views;
using NineOldAndroids.View;

namespace Daimajia.Slider.Transformers
{
    /// <summary>
    /// Created by daimajia on 14-5-29.
    /// </summary>

    public class AccordionTransformer : BaseTransformer
	{

		protected internal override void onTransform(View view, float position)
		{
			ViewHelper.SetPivotX(view,position < 0 ? 0 : view.Width);
			ViewHelper.SetScaleX(view,position < 0 ? 1f + position : 1f - position);
		}

	}
}