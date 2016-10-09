using Android.Views;
using NineOldAndroids.Animation;
using NineOldAndroids.View;
using R = Daimajia.Slider.Resource;

namespace Daimajia.Slider.Animations
{

    

    /// <summary>
    /// A demo class to show how to use <seealso cref="com.daimajia.slider.library.Animations.BaseAnimationInterface"/>
    /// to make  your custom animation in <seealso cref="com.daimajia.slider.library.Tricks.ViewPagerEx.PageTransformer"/> action.
    /// </summary>
    public class DescriptionAnimation : BaseAnimationInterface
	{

		public void onPrepareCurrentItemLeaveScreen(View current)
		{
			View descriptionLayout = current.FindViewById(R.Id.description_layout);
			if (descriptionLayout != null)
			{
				current.FindViewById(R.Id.description_layout).Visibility = Android.Views.ViewStates.Invisible;
			}
		}

		/// <summary>
		/// When next item is coming to show, let's hide the description layout. </summary>
		/// <param name="next"> </param>
		public void onPrepareNextItemShowInScreen(View next)
		{
			View descriptionLayout = next.FindViewById(R.Id.description_layout);
			if (descriptionLayout != null)
			{
				next.FindViewById(R.Id.description_layout).Visibility =  Android.Views.ViewStates.Invisible;
			}
		}


		public void onCurrentItemDisappear(View view)
		{

		}

		/// <summary>
		/// When next item show in ViewPagerEx, let's make an animation to show the
		/// description layout. </summary>
		/// <param name="view"> </param>
		public void onNextItemAppear(View view)
		{

			View descriptionLayout = view.FindViewById(R.Id.description_layout);
			if (descriptionLayout != null)
			{
				float layoutY = ViewHelper.GetY(descriptionLayout);
				view.FindViewById(R.Id.description_layout).Visibility = Android.Views.ViewStates.Visible;
				var animator = ObjectAnimator.OfFloat(descriptionLayout,"y",layoutY + descriptionLayout.Height, layoutY).SetDuration(500);
				animator.Start();
			}

		}
	}

}