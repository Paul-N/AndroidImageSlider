using System.Collections.Generic;
using Daimajia.Slider;
using Android.Content;
using Android.Content.Res;
using Android.Database;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;
using Daimajia.Slider.Tricks;

namespace Daimajia.Slider.Indicators
{

    public enum IndicatorVisibility
    {
        Visible,
        Invisible
    }

    /// <summary>
    /// Pager Indicator.
    /// </summary>
    //[Android.Runtime.Register("com.daimajia.slider.library.Indicators.PagerIndicator")]
    [Android.Runtime.Register("com.daimajia.slider.library.Indicators.PagerIndicator")]
    public class PagerIndicator : LinearLayout, ViewPagerEx.OnPageChangeListener
	{

		private Context mContext;

		/// <summary>
		/// bind this Indicator with <seealso cref="com.daimajia.slider.library.Tricks.ViewPagerEx"/>
		/// </summary>
		private ViewPagerEx mPager;

		/// <summary>
		/// Variable to remember the previous selected indicator.
		/// </summary>
		private ImageView mPreviousSelectedIndicator;

		/// <summary>
		/// Previous selected indicator position.
		/// </summary>
		private int mPreviousSelectedPosition;

		/// <summary>
		/// Custom selected indicator style resource id.
		/// </summary>
		private int mUserSetUnSelectedIndicatorResId;


		/// <summary>
		/// Custom unselected indicator style resource id.
		/// </summary>
		private int mUserSetSelectedIndicatorResId;

		private Drawable mSelectedDrawable;
		private Drawable mUnselectedDrawable;

		/// <summary>
		/// This value is from <seealso cref="com.daimajia.slider.library.SliderAdapter"/> getRealCount() represent
		/// 
		/// the indicator count that we should draw.
		/// </summary>
		private int mItemCount = 0;

		private Shape mIndicatorShape = Shape.Oval;

		private IndicatorVisibility mVisibility = IndicatorVisibility.Visible;

		private int mDefaultSelectedColor;
		private int mDefaultUnSelectedColor;

		private float mDefaultSelectedWidth;
		private float mDefaultSelectedHeight;

		private float mDefaultUnSelectedWidth;
		private float mDefaultUnSelectedHeight;

		

		private GradientDrawable mUnSelectedGradientDrawable;
		private GradientDrawable mSelectedGradientDrawable;

		private LayerDrawable mSelectedLayerDrawable;
		private LayerDrawable mUnSelectedLayerDrawable;

		private float mPadding_left;
		private float mPadding_right;
		private float mPadding_top;
		private float mPadding_bottom;

		private float mSelectedPadding_Left;
		private float mSelectedPadding_Right;
		private float mSelectedPadding_Top;
		private float mSelectedPadding_Bottom;

		private float mUnSelectedPadding_Left;
		private float mUnSelectedPadding_Right;
		private float mUnSelectedPadding_Top;
		private float mUnSelectedPadding_Bottom;

		/// <summary>
		/// Put all the indicators into a ArrayList, so we can remove them easily.
		/// </summary>
		private List<ImageView> mIndicators = new List<ImageView>();


		public PagerIndicator(Context context) : this(context,null)
		{
		}

		public PagerIndicator(Context context, IAttributeSet attrs) : base(context, attrs)
		{
            dataChangeObserver = new DataSetObserverAnonymousInnerClassHelper(this);
            mContext = context;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.content.res.TypedArray attributes = context.obtainStyledAttributes(attrs,com.daimajia.slider.library.R.styleable.PagerIndicator,0,0);
			TypedArray attributes = context.ObtainStyledAttributes(attrs, Resource.Styleable.PagerIndicator,0,0);

			int visibility = attributes.GetInt(Resource.Styleable.PagerIndicator_visibility, IndicatorVisibility.Visible.Ordinal());

			foreach (IndicatorVisibility v in System.Enum.GetValues(typeof(IndicatorVisibility)))
			{
				if (v.Ordinal() == visibility)
				{
					mVisibility = v;
					break;
				}
			}

			int shape = attributes.GetInt(Resource.Styleable.PagerIndicator_shape, Shape.Oval.Ordinal());
			foreach (Shape s in System.Enum.GetValues(typeof(Shape)))
			{
				if (s.Ordinal() == shape)
				{
					mIndicatorShape = s;
					break;
				}
			}

			mUserSetSelectedIndicatorResId = attributes.GetResourceId(Resource.Styleable.PagerIndicator_selected_drawable, 0);
			mUserSetUnSelectedIndicatorResId = attributes.GetResourceId(Resource.Styleable.PagerIndicator_unselected_drawable, 0);

			mDefaultSelectedColor = attributes.GetColor(Resource.Styleable.PagerIndicator_selected_color, Color.Rgb(255, 255, 255));
			mDefaultUnSelectedColor = attributes.GetColor(Resource.Styleable.PagerIndicator_unselected_color, Color.Argb(33,255,255,255));

			mDefaultSelectedWidth = attributes.GetDimension(Resource.Styleable.PagerIndicator_selected_width,(int)pxFromDp(6));
			mDefaultSelectedHeight = attributes.GetDimensionPixelSize(Resource.Styleable.PagerIndicator_selected_height,(int)pxFromDp(6));

			mDefaultUnSelectedWidth = attributes.GetDimensionPixelSize(Resource.Styleable.PagerIndicator_unselected_width,(int)pxFromDp(6));
			mDefaultUnSelectedHeight = attributes.GetDimensionPixelSize(Resource.Styleable.PagerIndicator_unselected_height,(int)pxFromDp(6));

			mSelectedGradientDrawable = new GradientDrawable();
			mUnSelectedGradientDrawable = new GradientDrawable();

			mPadding_left = attributes.GetDimensionPixelSize(Resource.Styleable.PagerIndicator_padding_left,(int)pxFromDp(3));
			mPadding_right = attributes.GetDimensionPixelSize(Resource.Styleable.PagerIndicator_padding_right,(int)pxFromDp(3));
			mPadding_top = attributes.GetDimensionPixelSize(Resource.Styleable.PagerIndicator_padding_top,(int)pxFromDp(0));
			mPadding_bottom = attributes.GetDimensionPixelSize(Resource.Styleable.PagerIndicator_padding_bottom,(int)pxFromDp(0));

			mSelectedPadding_Left = attributes.GetDimensionPixelSize(Resource.Styleable.PagerIndicator_selected_padding_left,(int)mPadding_left);
			mSelectedPadding_Right = attributes.GetDimensionPixelSize(Resource.Styleable.PagerIndicator_selected_padding_right,(int)mPadding_right);
			mSelectedPadding_Top = attributes.GetDimensionPixelSize(Resource.Styleable.PagerIndicator_selected_padding_top,(int)mPadding_top);
			mSelectedPadding_Bottom = attributes.GetDimensionPixelSize(Resource.Styleable.PagerIndicator_selected_padding_bottom,(int)mPadding_bottom);

			mUnSelectedPadding_Left = attributes.GetDimensionPixelSize(Resource.Styleable.PagerIndicator_unselected_padding_left,(int)mPadding_left);
			mUnSelectedPadding_Right = attributes.GetDimensionPixelSize(Resource.Styleable.PagerIndicator_unselected_padding_right,(int)mPadding_right);
			mUnSelectedPadding_Top = attributes.GetDimensionPixelSize(Resource.Styleable.PagerIndicator_unselected_padding_top,(int)mPadding_top);
			mUnSelectedPadding_Bottom = attributes.GetDimensionPixelSize(Resource.Styleable.PagerIndicator_unselected_padding_bottom,(int)mPadding_bottom);

			mSelectedLayerDrawable = new LayerDrawable(new Drawable[]{mSelectedGradientDrawable});
			mUnSelectedLayerDrawable = new LayerDrawable(new Drawable[]{mUnSelectedGradientDrawable});


			setIndicatorStyleResource(mUserSetSelectedIndicatorResId,mUserSetUnSelectedIndicatorResId);
			DefaultIndicatorShape = mIndicatorShape;
			setDefaultSelectedIndicatorSize(mDefaultSelectedWidth,mDefaultSelectedHeight,Unit.Px);
			setDefaultUnselectedIndicatorSize(mDefaultUnSelectedWidth,mDefaultUnSelectedHeight,Unit.Px);
			setDefaultIndicatorColor(mDefaultSelectedColor, mDefaultUnSelectedColor);
			IndicatorVisibility = mVisibility;
			attributes.Recycle();
		}

		public enum Shape
		{
			Oval,
			Rectangle
		}

		/// <summary>
		/// if you are using the default indicator, this method will help you to set the shape of
		/// indicator, there are two kind of shapes you  can set, oval and rect. </summary>
		/// <param name="shape"> </param>
		public virtual Shape DefaultIndicatorShape
		{
			set
			{
				if (mUserSetSelectedIndicatorResId == 0)
				{
					if (value == Shape.Oval)
					{
						mSelectedGradientDrawable.SetShape(Android.Graphics.Drawables.ShapeType.Oval);
					}
					else
					{
						mSelectedGradientDrawable.SetShape(Android.Graphics.Drawables.ShapeType.Rectangle);
					}
				}
				if (mUserSetUnSelectedIndicatorResId == 0)
				{
					if (value == Shape.Oval)
					{
						mUnSelectedGradientDrawable.SetShape(Android.Graphics.Drawables.ShapeType.Oval);
					}
					else
					{
						mUnSelectedGradientDrawable.SetShape(Android.Graphics.Drawables.ShapeType.Rectangle);
					}
				}
				resetDrawable();
			}
		}


		/// <summary>
		/// Set Indicator style. </summary>
		/// <param name="selected"> page selected drawable </param>
		/// <param name="unselected"> page unselected drawable </param>
		public virtual void setIndicatorStyleResource(int selected, int unselected)
		{
			mUserSetSelectedIndicatorResId = selected;
			mUserSetUnSelectedIndicatorResId = unselected;
			if (selected == 0)
			{
				mSelectedDrawable = mSelectedLayerDrawable;
			}
			else
			{
				mSelectedDrawable = mContext.Resources.GetDrawable(mUserSetSelectedIndicatorResId);
			}
			if (unselected == 0)
			{
				mUnselectedDrawable = mUnSelectedLayerDrawable;
			}
			else
			{
				mUnselectedDrawable = mContext.Resources.GetDrawable(mUserSetUnSelectedIndicatorResId);
			}

			resetDrawable();
		}

		/// <summary>
		/// if you are using the default indicator , this method will help you to set the selected status and
		/// the unselected status color. </summary>
		/// <param name="selectedColor"> </param>
		/// <param name="unselectedColor"> </param>
		public virtual void setDefaultIndicatorColor(int selectedColor, int unselectedColor)
		{
			if (mUserSetSelectedIndicatorResId == 0)
			{
				mSelectedGradientDrawable.SetColor(selectedColor);
			}
			if (mUserSetUnSelectedIndicatorResId == 0)
			{
				mUnSelectedGradientDrawable.SetColor(unselectedColor);
			}
			resetDrawable();
		}

		public enum Unit
		{
			DP,
			Px
		}

		public virtual void setDefaultSelectedIndicatorSize(float width, float height, Unit unit)
		{
			if (mUserSetSelectedIndicatorResId == 0)
			{
				float w = width;
				float h = height;
				if (unit == Unit.DP)
				{
					w = pxFromDp(width);
					h = pxFromDp(height);
				}
				mSelectedGradientDrawable.SetSize((int) w, (int) h);
				resetDrawable();
			}
		}

		public virtual void setDefaultUnselectedIndicatorSize(float width, float height, Unit unit)
		{
			if (mUserSetUnSelectedIndicatorResId == 0)
			{
				float w = width;
				float h = height;
				if (unit == Unit.DP)
				{
					w = pxFromDp(width);
					h = pxFromDp(height);
				}
				mUnSelectedGradientDrawable.SetSize((int) w, (int) h);
				resetDrawable();
			}
		}

		public virtual void setDefaultIndicatorSize(float width, float height, Unit unit)
		{
			setDefaultSelectedIndicatorSize(width,height,unit);
			setDefaultUnselectedIndicatorSize(width,height,unit);
		}

		private float dpFromPx(float px)
		{
			return px / this.Context.Resources.DisplayMetrics.Density;
		}

		private float pxFromDp(float dp)
		{
			return dp * this.Context.Resources.DisplayMetrics.Density;
		}

		/// <summary>
		/// set the visibility of indicator. </summary>
		/// <param name="visibility"> </param>
		public virtual IndicatorVisibility IndicatorVisibility
		{
			set
			{
				if (value == IndicatorVisibility.Visible)
				{
					Visibility = Android.Views.ViewStates.Visible;
				}
				else
				{
					Visibility = Android.Views.ViewStates.Invisible;
				}
				resetDrawable();
			}
			get
			{
				return mVisibility;
			}
		}

		/// <summary>
		/// clear self means unregister the dataset observer and remove all the child views(indicators).
		/// </summary>
		public virtual void destroySelf()
		{
			if (mPager == null || mPager.Adapter == null)
			{
				return;
			}
			InfinitePagerAdapter wrapper = (InfinitePagerAdapter)mPager.Adapter;
			var adapter = wrapper.RealAdapter as PagerAdapter;
			if (adapter != null)
			{
				adapter.UnregisterDataSetObserver(dataChangeObserver);
			}
			RemoveAllViews();
		}

		/// <summary>
		/// bind indicator with viewpagerEx. </summary>
		/// <param name="pager"> </param>
		public virtual ViewPagerEx ViewPager
		{
			set
			{
				if (value.Adapter == null)
				{
					throw new Java.Lang.IllegalStateException("Viewpager does not have adapter instance");
				}
				mPager = value;
				mPager.addOnPageChangeListener(this);
				((InfinitePagerAdapter)mPager.Adapter).RealAdapter.RegisterDataSetObserver(dataChangeObserver);
			}
		}


		private void resetDrawable()
		{
			foreach (View i in mIndicators)
			{
				if (mPreviousSelectedIndicator != null && mPreviousSelectedIndicator.Equals(i))
				{
					((ImageView)i).SetImageDrawable(mSelectedDrawable);
				}
				else
				{
					((ImageView)i).SetImageDrawable(mUnselectedDrawable);
				}
			}
		}

		/// <summary>
		/// redraw the indicators.
		/// </summary>
		public virtual void redraw()
		{
			mItemCount = ShouldDrawCount;
			mPreviousSelectedIndicator = null;
			foreach (View i in mIndicators)
			{
				RemoveView(i);
			}


			for (int i = 0 ;i < mItemCount; i++)
			{
				ImageView indicator = new ImageView(mContext);
				indicator.SetImageDrawable(mUnselectedDrawable);
				indicator.SetPadding((int)mUnSelectedPadding_Left, (int)mUnSelectedPadding_Top, (int)mUnSelectedPadding_Right, (int)mUnSelectedPadding_Bottom);
				AddView(indicator);
				mIndicators.Add(indicator);
			}
			ItemAsSelected = mPreviousSelectedPosition;
		}

		/// <summary>
		/// since we used a adapter wrapper, so we can't getCount directly from wrapper.
		/// @return
		/// </summary>
		private int ShouldDrawCount
		{
			get
			{
				if (mPager.Adapter is InfinitePagerAdapter)
				{
					return ((InfinitePagerAdapter)mPager.Adapter).RealCount;
				}
				else
				{
					return mPager.Adapter.Count;
				}
			}
		}

        private DataSetObserver dataChangeObserver;// = new DataSetObserverAnonymousInnerClassHelper(this);

		private class DataSetObserverAnonymousInnerClassHelper : DataSetObserver
		{
            private PagerIndicator outerInstance;

            public DataSetObserverAnonymousInnerClassHelper(PagerIndicator pageIndicator)
			{
                outerInstance = pageIndicator;
			}

			public override void OnChanged()
			{
				PagerAdapter adapter = outerInstance.mPager.Adapter;
				int count = 0;
				if (adapter is InfinitePagerAdapter)
				{
					count = ((InfinitePagerAdapter)adapter).RealCount;
				}
				else
				{
					count = adapter.Count;
				}
				if (count > outerInstance.mItemCount)
				{
					for (int i = 0 ; i < count - outerInstance.mItemCount;i++)
					{
						ImageView indicator = new ImageView(outerInstance.mContext);
						indicator.SetImageDrawable(outerInstance.mUnselectedDrawable);
						indicator.SetPadding((int)outerInstance.mUnSelectedPadding_Left, (int)outerInstance.mUnSelectedPadding_Top, (int)outerInstance.mUnSelectedPadding_Right, (int)outerInstance.mUnSelectedPadding_Bottom);
                        outerInstance.AddView(indicator);
						outerInstance.mIndicators.Add(indicator);
					}
				}
				else if (count < outerInstance.mItemCount)
				{
					for (int i = 0; i < outerInstance.mItemCount - count;i++)
					{
                        outerInstance.RemoveView(outerInstance.mIndicators[0]);
						outerInstance.mIndicators.RemoveAt(0);
					}
				}
				outerInstance.mItemCount = count;
				outerInstance.mPager.CurrentItem = outerInstance.mItemCount * 20 + outerInstance.mPager.CurrentItem;
			}

			public override void OnInvalidated()
			{
				base.OnInvalidated();
				outerInstance.redraw();
			}
		}

		private int ItemAsSelected
		{
			set
			{
				if (mPreviousSelectedIndicator != null)
				{
					mPreviousSelectedIndicator.SetImageDrawable(mUnselectedDrawable);
					mPreviousSelectedIndicator.SetPadding((int)mUnSelectedPadding_Left, (int)mUnSelectedPadding_Top, (int)mUnSelectedPadding_Right, (int)mUnSelectedPadding_Bottom);
				}
				ImageView currentSelected = (ImageView)GetChildAt(value + 1);
				if (currentSelected != null)
				{
					currentSelected.SetImageDrawable(mSelectedDrawable);
					currentSelected.SetPadding((int)mSelectedPadding_Left, (int)mSelectedPadding_Top, (int)mSelectedPadding_Right, (int)mSelectedPadding_Bottom);
					mPreviousSelectedIndicator = currentSelected;
				}
				mPreviousSelectedPosition = value;
			}
		}

		public void onPageScrolled(int position, float positionOffset, int positionOffsetPixels)
		{
		}


		public void onPageSelected(int position)
		{
			 if (mItemCount == 0)
			 {
				return;
			 }
			ItemAsSelected = position - 1;
		}
		public void onPageScrollStateChanged(int state)
		{
		}

		public virtual int SelectedIndicatorResId
		{
			get
			{
				return mUserSetSelectedIndicatorResId;
			}
		}

		public virtual int UnSelectedIndicatorResId
		{
			get
			{
				return mUserSetUnSelectedIndicatorResId;
			}
		}

	}

}