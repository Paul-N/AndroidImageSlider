using Context = Android.Content.Context;
using Android.Views;
using Android.Widget;
using R = Daimajia.Slider.Resource;

namespace Daimajia.Slider.SliderTypes
{
	/// <summary>
	/// a simple slider view, which just show an image. If you want to make your own slider view,
	/// 
	/// just extend BaseSliderView, and implement getView() method.
	/// </summary>
	public class DefaultSliderView : BaseSliderView
	{

		public DefaultSliderView(Context context) : base(context)
		{
		}

		public override View View
		{
			get
			{
				View v = LayoutInflater.From(Context).Inflate(R.Layout.render_type_default,null);
				ImageView target = (ImageView)v.FindViewById(R.Id.daimajia_slider_image);
				bindEventAndShow(v, target);
				return v;
			}
		}
	}

}