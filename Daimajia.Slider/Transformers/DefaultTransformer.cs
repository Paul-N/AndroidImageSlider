using Android.Views;

namespace Daimajia.Slider.Transformers
{
    public class DefaultTransformer : BaseTransformer
	{

		protected internal override void onTransform(View view, float position)
		{
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