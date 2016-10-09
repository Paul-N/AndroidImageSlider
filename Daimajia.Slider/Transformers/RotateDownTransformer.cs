using Android.Views;
using NineOldAndroids.View;

namespace Daimajia.Slider.Transformers
{
	public class RotateDownTransformer : BaseTransformer
	{

		private const float ROT_MOD = -15f;

		protected internal override void onTransform(View view, float position)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float width = view.getWidth();
			float width = view.Width;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float height = view.getHeight();
			float height = view.Height;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float rotation = ROT_MOD * position * -1.25f;
			float rotation = ROT_MOD * position * -1.25f;

			ViewHelper.SetPivotX(view,width * 0.5f);
			ViewHelper.SetPivotY(view,height);
			ViewHelper.SetRotation(view,rotation);
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