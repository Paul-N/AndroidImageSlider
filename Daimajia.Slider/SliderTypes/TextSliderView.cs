using R = Daimajia.Slider.Resource;

namespace Daimajia.Slider.SliderTypes
{

	using Context = Android.Content.Context;
	using LayoutInflater = Android.Views.LayoutInflater;
	using View = Android.Views.View;
	using ImageView = Android.Widget.ImageView;
	using TextView = Android.Widget.TextView;

	/// <summary>
	/// This is a slider with a description TextView.
	/// </summary>
	public class TextSliderView : BaseSliderView
	{
		public TextSliderView(Context context) : base(context)
		{
		}

		public override View View
		{
			get
			{
				View v = LayoutInflater.From(Context).Inflate(R.Layout.render_type_text,null);
				ImageView target = (ImageView)v.FindViewById(R.Id.daimajia_slider_image);
				TextView description = (TextView)v.FindViewById(R.Id.description);
				description.Text = Description;
				bindEventAndShow(v, target);
				return v;
			}
		}
	}

}