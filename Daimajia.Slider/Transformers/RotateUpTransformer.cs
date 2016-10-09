using Android.Views;
using NineOldAndroids.View;

namespace Daimajia.Slider.Transformers
{
	public class RotateUpTransformer : BaseTransformer
	{

		private const float ROT_MOD = -15f;

		protected internal override void onTransform(View view, float position)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float width = view.getWidth();
			float width = view.Width;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float rotation = ROT_MOD * position;
			float rotation = ROT_MOD * position;

			ViewHelper.SetPivotX(view,width * 0.5f);
			ViewHelper.SetPivotY(view,0f);
			ViewHelper.SetTranslationX(view,0f);
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