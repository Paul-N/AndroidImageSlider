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
using NineOldAndroids.View;
using Android.Graphics;

namespace Daimajia.Slider.Demo.AndroidViewAnimations
{
    public abstract class BaseViewAnimator
    {

        public static readonly long DURATION = 1000;

        private static AnimatorSet mAnimatorSet;
        private long mDuration = DURATION;

    static BaseViewAnimator(){
        mAnimatorSet = new AnimatorSet();
    }


    protected abstract void prepare(View target);

    public BaseViewAnimator setTarget(View target)
    {
        reset(target);
        prepare(target);
        return this;
    }

    public void animate()
    {
        start();
    }

    /**
     * reset the view to default status
     *
     * @param target
     */
    public void reset(View target)
    {
        ViewHelper.SetAlpha(target, 1);
        ViewHelper.SetScaleX(target, 1);
        ViewHelper.SetScaleY(target, 1);
        ViewHelper.SetTranslationX(target, 0);
        ViewHelper.SetTranslationY(target, 0);
        ViewHelper.SetRotation(target, 0);
        ViewHelper.SetRotationY(target, 0);
        ViewHelper.SetRotationX(target, 0);
        ViewHelper.SetPivotX(target, target.MeasuredWidth / 2.0f);
        ViewHelper.SetPivotY(target, target.MeasuredHeight / 2.0f);
    }

    /**
     * start to animate
     */
    public void start()
    {
        mAnimatorSet.SetDuration(mDuration);
        mAnimatorSet.Start();
    }

    public BaseViewAnimator setDuration(long duration)
    {
        mDuration = duration;
        return this;
    }

    public BaseViewAnimator setStartDelay(long delay)
    {
        getAnimatorAgent().StartDelay = (delay);
        return this;
    }

    public long getStartDelay()
    {
        return mAnimatorSet.StartDelay;
    }

    public BaseViewAnimator addAnimatorListener(Animator.IAnimatorListener l)
    {
        mAnimatorSet.AddListener(l);
        return this;
    }

    public void cancel()
    {
        mAnimatorSet.Cancel();
    }

    public bool isRunning()
    {
        return mAnimatorSet.IsRunning;
    }

    public bool isStarted()
    {
        return mAnimatorSet.IsStarted;
    }

    public void removeAnimatorListener(Animator.IAnimatorListener l)
    {
        mAnimatorSet.RemoveListener(l);
    }

    public void removeAllListener()
    {
        mAnimatorSet.RemoveAllListeners();
    }

    public BaseViewAnimator setInterpolator(ITimeInterpolator interpolator)
    {
        mAnimatorSet.SetInterpolator(interpolator);
        return this;
    }

    public long getDuration()
    {
        return mDuration;
    }

    public AnimatorSet getAnimatorAgent()
    {
        return mAnimatorSet;
    }

}
}