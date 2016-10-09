using System;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Java.Util;
using R = Daimajia.Slider.Resource;
using System.Collections.Generic;
using Daimajia.Slider.Tricks;
using Daimajia.Slider.Indicators;
using Daimajia.Slider.Transformers;
using Daimajia.Slider.Animations;
using Daimajia.Slider.SliderTypes;
using Android.Util;
using Android.Views.Animations;

namespace Daimajia.Slider
{



    /// <summary>
    /// SliderLayout is compound layout. This is combined with <seealso cref="com.daimajia.slider.library.Indicators.PagerIndicator"/>
    /// and <seealso cref="com.daimajia.slider.library.Tricks.ViewPagerEx"/> .
    /// 
    /// There is some properties you can set in XML:
    /// 
    /// indicator_visibility
    ///      visible
    ///      invisible
    /// 
    /// indicator_shape
    ///      oval
    ///      rect
    /// 
    /// indicator_selected_color
    /// 
    /// indicator_unselected_color
    /// 
    /// indicator_selected_drawable
    /// 
    /// indicator_unselected_drawable
    /// 
    /// pager_animation
    ///      Default
    ///      Accordion
    ///      Background2Foreground
    ///      CubeIn
    ///      DepthPage
    ///      Fade
    ///      FlipHorizontal
    ///      FlipPage
    ///      Foreground2Background
    ///      RotateDown
    ///      RotateUp
    ///      Stack
    ///      Tablet
    ///      ZoomIn
    ///      ZoomOutSlide
    ///      ZoomOut
    /// 
    /// pager_animation_span
    /// 
    /// 
    /// </summary>
    //[Android.Runtime.Register("com.daimajia.slider.library.SliderLayout")]
    [Android.Runtime.Register("com.daimajia.slider.library.SliderLayout")]
    public class SliderLayout : RelativeLayout
	{

		private Context mContext;
		/// <summary>
		/// InfiniteViewPager is extended from ViewPagerEx. As the name says, it can scroll without bounder.
		/// </summary>
		private InfiniteViewPager mViewPager;

		/// <summary>
		/// InfiniteViewPager adapter.
		/// </summary>
		private SliderAdapter mSliderAdapter;

		/// <summary>
		/// <seealso cref="com.daimajia.slider.library.Tricks.ViewPagerEx"/> indicator.
		/// </summary>
		private PagerIndicator mIndicator;


		/// <summary>
		/// A timer and a TimerTask using to cycle the <seealso cref="com.daimajia.slider.library.Tricks.ViewPagerEx"/>.
		/// </summary>
		private Timer mCycleTimer;
		private TimerTask mCycleTask;

		/// <summary>
		/// For resuming the cycle, after user touch or click the <seealso cref="com.daimajia.slider.library.Tricks.ViewPagerEx"/>.
		/// </summary>
		private Timer mResumingTimer;
		private TimerTask mResumingTask;

		/// <summary>
		/// If <seealso cref="com.daimajia.slider.library.Tricks.ViewPagerEx"/> is Cycling
		/// </summary>
		private bool mCycling;

		/// <summary>
		/// Determine if auto recover after user touch the <seealso cref="com.daimajia.slider.library.Tricks.ViewPagerEx"/>
		/// </summary>
		private bool mAutoRecover = true;

		private int mTransformerId;

		/// <summary>
		/// <seealso cref="com.daimajia.slider.library.Tricks.ViewPagerEx"/> transformer time span.
		/// </summary>
		private int mTransformerSpan = 1100;

		private bool mAutoCycle;

		/// <summary>
		/// the duration between animation.
		/// </summary>
		private long mSliderDuration = 4000;

		/// <summary>
		/// Visibility of <seealso cref="com.daimajia.slider.library.Indicators.PagerIndicator"/>
		/// </summary>
		private IndicatorVisibility mIndicatorVisibility = IndicatorVisibility.Visible;

		/// <summary>
		/// <seealso cref="com.daimajia.slider.library.Tricks.ViewPagerEx"/> 's transformer
		/// </summary>
		private BaseTransformer mViewPagerTransformer;

		/// <seealso cref= com.daimajia.slider.library.Animations.BaseAnimationInterface </seealso>
		private BaseAnimationInterface mCustomAnimation;

		/// <summary>
		/// <seealso cref="com.daimajia.slider.library.Indicators.PagerIndicator"/> shape, rect or oval.
		/// </summary>

		public SliderLayout(Context context) : this(context,null)
		{
		}

		public SliderLayout(Context context, IAttributeSet attrs) : this(context,attrs,R.Attribute.SliderStyle)
		{
		}

		public SliderLayout(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
		{
            mh = new HandlerAnonymousInnerClassHelper(this);
            var _PresetIndicatorsToViewIdAccordance = new Dictionary<PresetIndicators, int>
            {
                { PresetIndicators.Center_Bottom, R.Id.default_center_bottom_indicator },
                { PresetIndicators.Right_Bottom, R.Id.default_bottom_right_indicator},
                { PresetIndicators.Left_Bottom, R.Id.default_bottom_left_indicator},
                { PresetIndicators.Center_Top, R.Id.default_center_top_indicator },
                { PresetIndicators.Right_Top, R.Id.default_center_top_right_indicator },
                { PresetIndicators.Left_Top, R.Id.default_center_top_left_indicator}
            };

            mContext = context;
			LayoutInflater.From(context).Inflate(R.Layout.slider_layout, this, true);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.content.res.TypedArray attributes = context.getTheme().obtainStyledAttributes(attrs,R.styleable.SliderLayout, defStyle,0);
			TypedArray attributes = context.Theme.ObtainStyledAttributes(attrs,R.Styleable.SliderLayout, defStyle,0);

			mTransformerSpan = attributes.GetInteger(R.Styleable.SliderLayout_pager_animation_span, 1100);
			mTransformerId = attributes.GetInt(R.Styleable.SliderLayout_pager_animation, Transformer.Default.Ordinal());
			mAutoCycle = attributes.GetBoolean(R.Styleable.SliderLayout_auto_cycle,true);
			int visibility = attributes.GetInt(R.Styleable.SliderLayout_indicator_visibility,0);
			foreach (IndicatorVisibility v in Enum.GetValues(typeof(IndicatorVisibility)))
			{
				if (v.Ordinal() == visibility)
				{
					mIndicatorVisibility = v;
					break;
				}
			}
			mSliderAdapter = new SliderAdapter(mContext);
			PagerAdapter wrappedAdapter = new InfinitePagerAdapter(mSliderAdapter);

			mViewPager = FindViewById<InfiniteViewPager>(R.Id.daimajia_slider_viewpager);
			mViewPager.Adapter = wrappedAdapter;

			mViewPager.SetOnTouchListener(new OnTouchListenerAnonymousInnerClassHelper(this));

			attributes.Recycle();
			SetPresetIndicator(PresetIndicators.Center_Bottom);
			SetPresetTransformer(mTransformerId);
			setSliderTransformDuration(mTransformerSpan,null);
			IndicatorVisibility = mIndicatorVisibility;
			if (mAutoCycle)
			{
				startAutoCycle();
			}
		}

		private class OnTouchListenerAnonymousInnerClassHelper : Java.Lang.Object, IOnTouchListener
		{
			private readonly SliderLayout outerInstance;

			public OnTouchListenerAnonymousInnerClassHelper(SliderLayout outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public bool OnTouch(View v, MotionEvent @event)
			{
				var action = @event.Action;
				switch (action)
				{
					 case MotionEventActions.Up:
						outerInstance.recoverCycle();
						break;
				}
				return false;
			}
		}

		public virtual void addOnPageChangeListener(ViewPagerEx.OnPageChangeListener onPageChangeListener)
		{
			if (onPageChangeListener != null)
			{
				mViewPager.addOnPageChangeListener(onPageChangeListener);
			}
		}

		public virtual void removeOnPageChangeListener(ViewPagerEx.OnPageChangeListener onPageChangeListener)
		{
			mViewPager.removeOnPageChangeListener(onPageChangeListener);
		}

		public virtual PagerIndicator CustomIndicator
		{
			set
			{
				if (mIndicator != null)
				{
					mIndicator.destroySelf();
				}
				mIndicator = value;
				mIndicator.IndicatorVisibility = mIndicatorVisibility;
				mIndicator.ViewPager = mViewPager;
				mIndicator.redraw();
			}
		}

		public virtual void addSlider<T>(T imageContent) where T : BaseSliderView
		{
			mSliderAdapter.addSlider(imageContent);
		}

		private Handler mh;

		private class HandlerAnonymousInnerClassHelper : Handler
		{
            private SliderLayout outerInstance;

            public HandlerAnonymousInnerClassHelper(SliderLayout sliderLayout)
			{
                outerInstance = sliderLayout;
            }

			public override void HandleMessage(Message msg)
			{
				base.HandleMessage(msg);
				outerInstance.moveNextPosition(true);
			}
		}

		public virtual void startAutoCycle()
		{
			startAutoCycle(mSliderDuration, mSliderDuration, mAutoRecover);
		}

		/// <summary>
		/// start auto cycle. </summary>
		/// <param name="delay"> delay time </param>
		/// <param name="duration"> animation duration time. </param>
		/// <param name="autoRecover"> if recover after user touches the slider. </param>
		public virtual void startAutoCycle(long delay, long duration, bool autoRecover)
		{
			if (mCycleTimer != null)
			{
				mCycleTimer.Cancel();
			}
			if (mCycleTask != null)
			{
				mCycleTask.Cancel();
			}
			if (mResumingTask != null)
			{
				mResumingTask.Cancel();
			}
			if (mResumingTimer != null)
			{
				mResumingTimer.Cancel();
			}
			mSliderDuration = duration;
			mCycleTimer = new Timer();
			mAutoRecover = autoRecover;
			mCycleTask = new TimerTaskAnonymousInnerClassHelper(this);
			mCycleTimer.Schedule(mCycleTask,delay,mSliderDuration);
			mCycling = true;
			mAutoCycle = true;
		}

		private class TimerTaskAnonymousInnerClassHelper : TimerTask
		{
			private readonly SliderLayout outerInstance;

			public TimerTaskAnonymousInnerClassHelper(SliderLayout outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void Run()
			{
                outerInstance.mh.SendEmptyMessage(0);
			}
		}

		/// <summary>
		/// pause auto cycle.
		/// </summary>
		private void pauseAutoCycle()
		{
			if (mCycling)
			{
				mCycleTimer.Cancel();
				mCycleTask.Cancel();
				mCycling = false;
			}
			else
			{
				if (mResumingTimer != null && mResumingTask != null)
				{
					recoverCycle();
				}
			}
		}

		/// <summary>
		/// set the duration between two slider changes. the duration value must >= 500 </summary>
		/// <param name="duration"> </param>
		public virtual long Duration
		{
			set
			{
				if (value >= 500)
				{
					mSliderDuration = value;
					if (mAutoCycle && mCycling)
					{
						startAutoCycle();
					}
				}
			}
		}

		/// <summary>
		/// stop the auto circle
		/// </summary>
		public virtual void stopAutoCycle()
		{
			if (mCycleTask != null)
			{
				mCycleTask.Cancel();
			}
			if (mCycleTimer != null)
			{
				mCycleTimer.Cancel();
			}
			if (mResumingTimer != null)
			{
				mResumingTimer.Cancel();
			}
			if (mResumingTask != null)
			{
				mResumingTask.Cancel();
			}
			mAutoCycle = false;
			mCycling = false;
		}

		/// <summary>
		/// when paused cycle, this method can weak it up.
		/// </summary>
		private void recoverCycle()
		{
			if (!mAutoRecover || !mAutoCycle)
			{
				return;
			}

			if (!mCycling)
			{
				if (mResumingTask != null && mResumingTimer != null)
				{
					mResumingTimer.Cancel();
					mResumingTask.Cancel();
				}
				mResumingTimer = new Timer();
				mResumingTask = new TimerTaskAnonymousInnerClassHelper2(this);
				mResumingTimer.Schedule(mResumingTask, 6000);
			}
		}

		private class TimerTaskAnonymousInnerClassHelper2 : TimerTask
		{
			private readonly SliderLayout outerInstance;

			public TimerTaskAnonymousInnerClassHelper2(SliderLayout outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void Run()
			{
				outerInstance.startAutoCycle();
			}
		}



		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			var action = ev.Action;
			switch (action)
			{
				case MotionEventActions.Down:
					pauseAutoCycle();
					break;
			}
			return false;
		}

		/// <summary>
		/// set ViewPager transformer. </summary>
		/// <param name="reverseDrawingOrder"> </param>
		/// <param name="transformer"> </param>
		public virtual void setPagerTransformer(bool reverseDrawingOrder, BaseTransformer transformer)
		{
			mViewPagerTransformer = transformer;
			mViewPagerTransformer.CustomAnimationInterface = mCustomAnimation;
			mViewPager.setPageTransformer(reverseDrawingOrder,mViewPagerTransformer);
		}



		/// <summary>
		/// set the duration between two slider changes. </summary>
		/// <param name="period"> </param>
		/// <param name="interpolator"> </param>
		public virtual void setSliderTransformDuration(int period, IInterpolator interpolator)
		{
			try
			{
				//var mScroller = Java.Lang.Class.FromType(typeof(ViewPagerEx)).GetDeclaredField("mScroller");
				//mScroller.Accessible = true;
				//FixedSpeedScroller scroller = new FixedSpeedScroller(mViewPager.Context,interpolator, period);
				//mScroller.Set(mViewPager,scroller);
                mViewPager.mScroller = new FixedSpeedScroller(mViewPager.Context, interpolator, period);
            }
			catch (Exception)
			{

			}
		}

		/// <summary>
		/// preset transformers and their names
		/// </summary>
		
	

		/// <summary>
		/// set a preset viewpager transformer by id. </summary>
		/// <param name="transformerId"> </param>
		public virtual void SetPresetTransformer(int val)
		{
			foreach (Transformer t in Enum.GetValues(typeof(Transformer)))
				{
					if (t.Ordinal() == val)
					{
						SetPresetTransformer(t);
						break;
					}
				}
			
		}

		/// <summary>
		/// set preset PagerTransformer via the name of transforemer. </summary>
		/// <param name="transformerName"> </param>
		public virtual void SetPresetTransformer(string val)
		{
			foreach (Transformer t in Enum.GetValues(typeof(Transformer)))
				{
					if (t.StringEquals(val))
					{
						SetPresetTransformer(t);
						return;
					}
				}
		}

		/// <summary>
		/// Inject your custom animation into PageTransformer, you can know more details in
		/// <seealso cref="com.daimajia.slider.library.Animations.BaseAnimationInterface"/>,
		/// and you can see a example in <seealso cref="com.daimajia.slider.library.Animations.DescriptionAnimation"/> </summary>
		/// <param name="animation"> </param>
		public virtual BaseAnimationInterface CustomAnimation
		{
			set
			{
				mCustomAnimation = value;
				if (mViewPagerTransformer != null)
				{
					mViewPagerTransformer.CustomAnimationInterface = mCustomAnimation;
				}
			}
		}

		/// <summary>
		/// pretty much right? enjoy it. :-D
		/// </summary>
		/// <param name="ts"> </param>
		public virtual void SetPresetTransformer(Transformer val)
		{
			
				//
				// special thanks to https://github.com/ToxicBakery/ViewPagerTransforms
				//
				BaseTransformer t = null;
				switch (val)
				{
					case Transformer.Default:
						t = new DefaultTransformer();
						break;
					case Transformer.Accordion:
						t = new AccordionTransformer();
						break;
					case Transformer.Background2Foreground:
						t = new BackgroundToForegroundTransformer();
						break;
					case Transformer.CubeIn:
						t = new CubeInTransformer();
						break;
					case Transformer.DepthPage:
						t = new DepthPageTransformer();
						break;
					case Transformer.Fade:
						t = new FadeTransformer();
						break;
					case Transformer.FlipHorizontal:
						t = new FlipHorizontalTransformer();
						break;
					case Transformer.FlipPage:
						t = new FlipPageViewTransformer();
						break;
					case Transformer.Foreground2Background:
						t = new ForegroundToBackgroundTransformer();
						break;
					case Transformer.RotateDown:
						t = new RotateDownTransformer();
						break;
					case Transformer.RotateUp:
						t = new RotateUpTransformer();
						break;
					case Transformer.Stack:
						t = new StackTransformer();
						break;
					case Transformer.Tablet:
						t = new TabletTransformer();
						break;
					case Transformer.ZoomIn:
						t = new ZoomInTransformer();
						break;
					case Transformer.ZoomOutSlide:
						t = new ZoomOutSlideTransformer();
						break;
					case Transformer.ZoomOut:
						t = new ZoomOutTransformer();
						break;
				}
				setPagerTransformer(true,t);
		}



		/// <summary>
		/// Set the visibility of the indicators. </summary>
		/// <param name="visibility"> </param>
		public virtual IndicatorVisibility IndicatorVisibility
		{
			set
			{
				if (mIndicator == null)
				{
					return;
				}
    
				mIndicator.IndicatorVisibility = value;
			}
			get
			{
				if (mIndicator == null)
				{
					return mIndicator.IndicatorVisibility;
				}
				return IndicatorVisibility.Invisible;
    
			}
		}


		/// <summary>
		/// get the <seealso cref="com.daimajia.slider.library.Indicators.PagerIndicator"/> instance.
		/// You can manipulate the properties of the indicator.
		/// @return
		/// </summary>
		public virtual PagerIndicator PagerIndicator
		{
			get
			{
				return mIndicator;
			}
		}

        private Dictionary<PresetIndicators, int> _presetIndicatorsToViewIdAccordance;

        public virtual void SetPresetIndicator(PresetIndicators val)
		{
            int id;
            if (_presetIndicatorsToViewIdAccordance != null && _presetIndicatorsToViewIdAccordance.TryGetValue(val, out id))
            {
                PagerIndicator pagerIndicator = FindViewById<PagerIndicator>(id);
                CustomIndicator = pagerIndicator;
            }
		}

		private InfinitePagerAdapter WrapperAdapter
		{
			get
			{
				PagerAdapter adapter = mViewPager.Adapter;
				if (adapter != null)
				{
					return (InfinitePagerAdapter)adapter;
				}
				else
				{
					return null;
				}
			}
		}

		private SliderAdapter RealAdapter
		{
			get
			{
				PagerAdapter adapter = mViewPager.Adapter;
				if (adapter != null)
				{
					return ((InfinitePagerAdapter)adapter).RealAdapter;
				}
				return null;
			}
		}

		/// <summary>
		/// get the current item position
		/// @return
		/// </summary>
		public virtual int CurrentPosition
		{
			get
			{
    
				if (RealAdapter == null)
				{
					throw new Java.Lang.IllegalStateException("You did not set a slider adapter");
				}
    
				return mViewPager.CurrentItem % RealAdapter.Count;
    
			}
			set
			{
				setCurrentPosition(value, true);
			}
		}

		/// <summary>
		/// get current slider.
		/// @return
		/// </summary>
		public virtual BaseSliderView CurrentSlider
		{
			get
			{
    
				if (RealAdapter == null)
				{
					throw new Java.Lang.IllegalStateException("You did not set a slider adapter");
				}
    
				int count = RealAdapter.Count;
				int realCount = mViewPager.CurrentItem % count;
				return RealAdapter.getSliderView(realCount);
			}
		}

		/// <summary>
		/// remove  the slider at the position. Notice: It's a not perfect method, a very small bug still exists.
		/// </summary>
		public virtual void removeSliderAt(int position)
		{
			if (RealAdapter != null)
			{
				RealAdapter.removeSliderAt(position);
				mViewPager.setCurrentItem(mViewPager.CurrentItem,false);
			}
		}

		/// <summary>
		/// remove all the sliders. Notice: It's a not perfect method, a very small bug still exists.
		/// </summary>
		public virtual void removeAllSliders()
		{
			if (RealAdapter != null)
			{
				int count = RealAdapter.Count;
				RealAdapter.removeAllSliders();
				//a small bug, but fixed by this trick.
				//bug: when remove adapter's all the sliders.some caching slider still alive.
				mViewPager.setCurrentItem(mViewPager.CurrentItem + count,false);
			}
		}

		/// <summary>
		/// set current slider </summary>
		/// <param name="position"> </param>
		public virtual void setCurrentPosition(int position, bool smooth)
		{
			if (RealAdapter == null)
			{
				throw new Java.Lang.IllegalStateException("You did not set a slider adapter");
			}
			if (position >= RealAdapter.Count)
			{
				throw new Java.Lang.IllegalStateException("Item position is not exist");
			}
			int p = mViewPager.CurrentItem % RealAdapter.Count;
			int n = (position - p) + mViewPager.CurrentItem;
			mViewPager.setCurrentItem(n, smooth);
		}


		/// <summary>
		/// move to prev slide.
		/// </summary>
		public virtual void movePrevPosition(bool smooth)
		{

			if (RealAdapter == null)
			{
				throw new Java.Lang.IllegalStateException("You did not set a slider adapter");
			}

			mViewPager.setCurrentItem(mViewPager.CurrentItem - 1, smooth);
		}

		public virtual void movePrevPosition()
		{
			movePrevPosition(true);
		}

		/// <summary>
		/// move to next slide.
		/// </summary>
		public virtual void moveNextPosition(bool smooth)
		{

			if (RealAdapter == null)
			{
				throw new Java.Lang.IllegalStateException("You did not set a slider adapter");
			}

			mViewPager.setCurrentItem(mViewPager.CurrentItem + 1, smooth);
		}

		public virtual void moveNextPosition()
		{
			moveNextPosition(true);
		}
	}


    public enum Transformer
    {
        Default,
        Accordion,
        Background2Foreground,
        CubeIn,
        DepthPage,
        Fade,
        FlipHorizontal,
        FlipPage,
        Foreground2Background,
        RotateDown,
        RotateUp,
        Stack,
        Tablet,
        ZoomIn,
        ZoomOutSlide,
        ZoomOut


    }

    public enum PresetIndicators
    {
        //Center_Bottom("Center_Bottom", R.id.default_center_bottom_indicator),
        //Right_Bottom("Right_Bottom", R.id.default_bottom_right_indicator),
        //Left_Bottom("Left_Bottom", R.id.default_bottom_left_indicator),
        //Center_Top("Center_Top", R.id.default_center_top_indicator),
        //Right_Top("Right_Top", R.id.default_center_top_right_indicator),
        //Left_Top("Left_Top", R.id.default_center_top_left_indicator);

        Center_Bottom,
        Right_Bottom,
        Left_Bottom,
        Center_Top,
        Right_Top,
        Left_Top


    }


    public static partial class EnumExtensionMethods
    {
        public static string ToString(this Transformer instance)
        {
            return instance.ToString();
        }
        public static bool StringEquals(this Transformer instance, string other)
        {
            //return (other == null)? false:name.Equals(other);
            Transformer res;
            if (Enum.TryParse<Transformer>(other, out res))
            {
                return res == instance;
            }
            else
                return false;
        }
    }
}