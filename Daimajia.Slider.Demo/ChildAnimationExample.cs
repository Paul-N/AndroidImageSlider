using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using Daimajia.Slider.Demo.AndroidViewAnimations;
using Daimajia.Slider.Animations;

namespace Daimajia.Slider.Demo
{
    public class ChildAnimationExample : BaseAnimationInterface
    {
        private readonly static string TAG = "ChildAnimationExample";

        public void onCurrentItemDisappear(View view)
        {
            Log.Error(TAG, "onCurrentItemDisappear called");
        }

        public void onNextItemAppear(View view)
        {
            View descriptionLayout = view.FindViewById(Daimajia.Slider.Resource.Id.description_layout);
            if (descriptionLayout != null)
            {
                view.FindViewById(Daimajia.Slider.Resource.Id.description_layout).Visibility = (ViewStates.Visible);
                //            ValueAnimator animator = ObjectAnimator.ofFloat(
                //                    descriptionLayout, "y", -descriptionLayout.getHeight(),
                //                    0).setDuration(500);
                //            animator.start();
                //            new BounceInAnimator().animate(descriptionLayout);
                var animator = new StandUpAnimator();
                animator.setTarget(descriptionLayout);
                animator.animate();
            }
            Log.Error(TAG, "onCurrentItemDisappear called");
        }

        public void onPrepareCurrentItemLeaveScreen(View current)
        {
            View descriptionLayout = current.FindViewById(Daimajia.Slider.Resource.Id.description_layout);
            if (descriptionLayout != null)
            {
                current.FindViewById(Daimajia.Slider.Resource.Id.description_layout).Visibility = (ViewStates.Invisible);
            }
            Log.Error(TAG, "onPrepareCurrentItemLeaveScreen called");
        }

        public void onPrepareNextItemShowInScreen(View next)
        {
            View descriptionLayout = next.FindViewById(Daimajia.Slider.Resource.Id.description_layout);
            if (descriptionLayout != null)
            {
                next.FindViewById(Daimajia.Slider.Resource.Id.description_layout).Visibility = (ViewStates.Invisible);
            }
            Log.Error(TAG, "onPrepareNextItemShowInScreen called");
        }
    }
}