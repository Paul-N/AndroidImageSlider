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
using Android.Animation;

namespace Daimajia.Slider.Demo.AndroidViewAnimations
{
    public class StandUpAnimator : BaseViewAnimator
    {
        
    protected override void prepare(View target)
    {
        float x = (target.Width - target.PaddingLeft - target.PaddingRight) / 2
                + target.PaddingLeft;
        float y = target.Height - target.PaddingBottom;
        getAnimatorAgent().PlayTogether(
                ObjectAnimator.OfFloat(target, "pivotX", x, x, x, x, x),
                ObjectAnimator.OfFloat(target, "pivotY", y, y, y, y, y),
                ObjectAnimator.OfFloat(target, "rotationX", 55, -30, 15, -15, 0)
        );
    }
}
}