using Android.Views;
using NineOldAndroids.View;

namespace Daimajia.Slider.Transformers
{
	public class CubeInTransformer : BaseTransformer
	{

		protected internal override void onTransform(View view, float position)
		{
			// Rotate the fragment on the left or right edge
			ViewHelper.SetPivotX(view,position > 0 ? 0 : view.Width);
			ViewHelper.SetPivotY(view,0);
			ViewHelper.SetRotation(view,-90f * position);
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