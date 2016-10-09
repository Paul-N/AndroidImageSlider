using System;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.Content.Res;
using Android.Database;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.OS;
using Android.Support.V4.View;
using Android.Support.V4.View.Accessibility;
using Android.Support.V4.Widget;
using Android.Util;
using Android.Views;
using Android.Views.Accessibility;
using Android.Widget;
using System.Reflection;
using Android.Views.Animations;

namespace Daimajia.Slider.Tricks
{


    /// <summary>
    /// Used internally to monitor when adapters are switched.
    /// </summary>
    internal interface OnAdapterChangeListener
    {
        void onAdapterChanged(PagerAdapter oldAdapter, PagerAdapter newAdapter);
    }


    /// <summary>
    /// Layout manager that allows the user to flip left and right
    /// through pages of data.  You supply an implementation of a
    /// <seealso cref="PagerAdapter"/> to generate the pages that the view shows.
    /// 
    /// <para>Note this class is currently under early design and
    /// development.  The API will likely change in later updates of
    /// the compatibility library, requiring changes to the source code
    /// of apps when they are compiled against the newer version.</para>
    /// 
    /// <para>ViewPager is most often used in conjunction with <seealso cref="android.app.Fragment"/>,
    /// which is a convenient way to supply and manage the lifecycle of each page.
    /// There are standard adapters implemented for using fragments with the ViewPager,
    /// which cover the most common use cases.  These are
    /// <seealso cref="android.support.v4.app.FragmentPagerAdapter"/> and
    /// <seealso cref="android.support.v4.app.FragmentStatePagerAdapter"/>; each of these
    /// classes have simple code showing how to build a full user interface
    /// with them.
    /// 
    /// </para>
    /// <para>Here is a more complicated example of ViewPager, using it in conjuction
    /// with <seealso cref="android.app.ActionBar"/> tabs.  You can find other examples of using
    /// ViewPager in the API 4+ Support Demos and API 13+ Support Demos sample code.
    /// 
    /// {@sample development/samples/Support13Demos/src/com/example/android/supportv13/app/ActionBarTabsPager.java
    ///      complete}
    /// </para>
    /// </summary>


    /// <summary>
    /// @author daimajia : I just remove the if condition in setPageTransformer() to make it compatiable with Android 2.0+
    /// of course, with the help of the NineOldDroid.
    /// Thanks to JakeWharton.
    /// http://github.com/JakeWharton/NineOldAndroids
    /// </summary>
    [Android.Runtime.Register("com.daimajia.slider.library.Tricks.ViewPagerEx")]
    public class ViewPagerEx : ViewGroup
	{
		private const string TAG = "ViewPagerEx";
		private bool DEBUG = false;

		private const bool USE_CACHE = false;

		private const int DEFAULT_OFFSCREEN_PAGES = 1;
		private const int MAX_SETTLE_DURATION = 600; // ms
		private const int MIN_DISTANCE_FOR_FLING = 25; // dips

		private const int DEFAULT_GUTTER_SIZE = 16; // dips

		private const int MIN_FLING_VELOCITY = 400; // dips

		private static readonly int[] LAYOUT_ATTRS = new int[] {Android.Resource.Attribute.LayoutGravity};

		/// <summary>
		/// Used to track what the expected number of items in the adapter should be.
		/// If the app changes this when we don't expect it, we'll throw a big obnoxious exception.
		/// </summary>
		private int mExpectedAdapterCount;

		internal class ItemInfo
		{
			internal Java.Lang.Object @object;
			internal int position;
			internal bool scrolling;
			internal float widthFactor;
			internal float offset;
		}

		private static readonly IComparer<ItemInfo> COMPARATOR = new ComparatorAnonymousInnerClassHelper();

		private class ComparatorAnonymousInnerClassHelper : IComparer<ItemInfo>
		{
			public ComparatorAnonymousInnerClassHelper()
			{
			}

			public virtual int Compare(ItemInfo lhs, ItemInfo rhs)
			{
				return lhs.position - rhs.position;
			}
		}

		private static readonly IInterpolator sInterpolator = new InterpolatorAnonymousInnerClassHelper();

		private class InterpolatorAnonymousInnerClassHelper : Java.Lang.Object, IInterpolator
		{
			public InterpolatorAnonymousInnerClassHelper()
			{
			}

			public virtual float GetInterpolation(float t)
			{
				t -= 1.0f;
				return t * t * t * t * t + 1.0f;
			}
		}

		private readonly List<ItemInfo> mItems = new List<ItemInfo>();
		private readonly ItemInfo mTempItem = new ItemInfo();

		private readonly Rect mTempRect = new Rect();

		private PagerAdapter mAdapter;
		private int mCurItem; // Index of currently displayed page.
		private int mRestoredCurItem = -1;
		private Android.OS.IParcelable mRestoredAdapterState = null;//TODO: Port??
		private Java.Lang.ClassLoader mRestoredClassLoader = null;
		internal Scroller mScroller;
		private PagerObserver mObserver;

		private int mPageMargin;
		private Drawable mMarginDrawable;
		private int mTopPageBounds;
		private int mBottomPageBounds;

		// Offsets of the first and last items, if known.
		// Set during population, used to determine if we are at the beginning
		// or end of the pager data set during touch scrolling.
		private float mFirstOffset = -float.MaxValue;
		private float mLastOffset = float.MaxValue;

		private int mChildWidthMeasureSpec;
		private int mChildHeightMeasureSpec;
		private bool mInLayout;

		private bool mScrollingCacheEnabled;

		private bool mPopulatePending;
		private int mOffscreenPageLimit = DEFAULT_OFFSCREEN_PAGES;

		private bool mIsBeingDragged;
		private bool mIsUnableToDrag;
		private bool mIgnoreGutter;
		private int mDefaultGutterSize;
		private int mGutterSize;
		private int mTouchSlop;
		/// <summary>
		/// Position of the last motion event.
		/// </summary>
		private float mLastMotionX;
		private float mLastMotionY;
		private float mInitialMotionX;
		private float mInitialMotionY;
		/// <summary>
		/// ID of the active pointer. This is used to retain consistency during
		/// drags/flings if multiple pointers are used.
		/// </summary>
		private int mActivePointerId = INVALID_POINTER;
		/// <summary>
		/// Sentinel value for no current active pointer.
		/// Used by <seealso cref="#mActivePointerId"/>.
		/// </summary>
		private const int INVALID_POINTER = -1;

		/// <summary>
		/// Determines speed during touch scrolling
		/// </summary>
		private VelocityTracker mVelocityTracker;
		private int mMinimumVelocity;
		private int mMaximumVelocity;
		private int mFlingDistance;
		private int mCloseEnough;

		// If the pager is at least this close to its final position, complete the scroll
		// on touch down and let the user interact with the content inside instead of
		// "catching" the flinging pager.
		private const int CLOSE_ENOUGH = 2; // dp

		private bool mFakeDragging;
		private long mFakeDragBeginTime;

		private EdgeEffectCompat mLeftEdge;
		private EdgeEffectCompat mRightEdge;

		private bool mFirstLayout = true;
		private bool mNeedCalculatePageOffsets = false;
		private bool mCalledSuper;
		private int mDecorChildCount;

		private List<OnPageChangeListener> mOnPageChangeListeners = new List<OnPageChangeListener>();
		private OnPageChangeListener mInternalPageChangeListener;
		private OnAdapterChangeListener mAdapterChangeListener;
		private PageTransformer mPageTransformer;
		private PropertyInfo mSetChildrenDrawingOrderEnabled;

		private const int DRAW_ORDER_DEFAULT = 0;
		private const int DRAW_ORDER_FORWARD = 1;
		private const int DRAW_ORDER_REVERSE = 2;
		private int mDrawingOrder;
		private List<View> mDrawingOrderedChildren;
		private static readonly ViewPositionComparator sPositionComparator = new ViewPositionComparator();

		/// <summary>
		/// Indicates that the pager is in an idle, settled state. The current page
		/// is fully in view and no animation is in progress.
		/// </summary>
		public const int SCROLL_STATE_IDLE = 0;

		/// <summary>
		/// Indicates that the pager is currently being dragged by the user.
		/// </summary>
		public const int SCROLL_STATE_DRAGGING = 1;

		/// <summary>
		/// Indicates that the pager is in the process of settling to a final position.
		/// </summary>
		public const int SCROLL_STATE_SETTLING = 2;

        private Java.Lang.Runnable mEndScrollRunnable;


		private int mScrollState = SCROLL_STATE_IDLE;

		/// <summary>
		/// Callback interface for responding to changing state of the selected page.
		/// </summary>
		public interface OnPageChangeListener
		{

			/// <summary>
			/// This method will be invoked when the current page is scrolled, either as part
			/// of a programmatically initiated smooth scroll or a user initiated touch scroll.
			/// </summary>
			/// <param name="position"> Position index of the first page currently being displayed.
			///                 Page position+1 will be visible if positionOffset is nonzero. </param>
			/// <param name="positionOffset"> Value from [0, 1) indicating the offset from the page at position. </param>
			/// <param name="positionOffsetPixels"> Value in pixels indicating the offset from position. </param>
			void onPageScrolled(int position, float positionOffset, int positionOffsetPixels);

			/// <summary>
			/// This method will be invoked when a new page becomes selected. Animation is not
			/// necessarily complete.
			/// </summary>
			/// <param name="position"> Position index of the new selected page. </param>
			void onPageSelected(int position);

			/// <summary>
			/// Called when the scroll state changes. Useful for discovering when the user
			/// begins dragging, when the pager is automatically settling to the current page,
			/// or when it is fully stopped/idle.
			/// </summary>
			/// <param name="state"> The new scroll state. </param>
			/// <seealso cref= ViewPagerEx#SCROLL_STATE_IDLE </seealso>
			/// <seealso cref= ViewPagerEx#SCROLL_STATE_DRAGGING </seealso>
			/// <seealso cref= ViewPagerEx#SCROLL_STATE_SETTLING </seealso>
			void onPageScrollStateChanged(int state);
		}

		/// <summary>
		/// Simple implementation of the <seealso cref="OnPageChangeListener"/> interface with stub
		/// implementations of each method. Extend this if you do not intend to override
		/// every method of <seealso cref="OnPageChangeListener"/>.
		/// </summary>
		public class SimpleOnPageChangeListener : Java.Lang.Object, OnPageChangeListener
		{
			public void onPageScrolled(int position, float positionOffset, int positionOffsetPixels)
			{
				// This space for rent
			}

			public void onPageSelected(int position)
			{
				// This space for rent
			}

			public void onPageScrollStateChanged(int state)
			{
				// This space for rent
			}
		}

		private void triggerOnPageChangeEvent(int position)
		{
			foreach (OnPageChangeListener eachListener in mOnPageChangeListeners)
			{
				if (eachListener != null)
				{
					InfinitePagerAdapter infiniteAdapter = (InfinitePagerAdapter)mAdapter;
					if (infiniteAdapter.RealCount == 0)
					{
						return;
					}
					int n = position % infiniteAdapter.RealCount;
					eachListener.onPageSelected(n);
				}
			}
			if (mInternalPageChangeListener != null)
			{
				mInternalPageChangeListener.onPageSelected(position);
			}
		}
		/// <summary>
		/// A PageTransformer is invoked whenever a visible/attached page is scrolled.
		/// This offers an opportunity for the application to apply a custom transformation
		/// to the page views using animation properties.
		/// 
		/// <para>As property animation is only supported as of Android 3.0 and forward,
		/// setting a PageTransformer on a ViewPager on earlier platform versions will
		/// be ignored.</para>
		/// </summary>
		public interface PageTransformer
		{
			/// <summary>
			/// Apply a property transformation to the given page.
			/// </summary>
			/// <param name="page"> Apply the transformation to this page </param>
			/// <param name="position"> Position of page relative to the current front-and-center
			///                 position of the pager. 0 is front and center. 1 is one full
			///                 page position to the right, and -1 is one page position to the left. </param>
			void transformPage(View page, float position);

		}

		

		/// <summary>
		/// Used internally to tag special types of child views that should be added as
		/// pager decorations by default.
		/// </summary>
		internal interface Decor
		{
		}

		public ViewPagerEx(Context context) : base(context)
		{
			initViewPager();
		}

		public ViewPagerEx(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			initViewPager();
		}

		internal virtual void initViewPager()
		{
#if DEBUG
            DEBUG = true;
#else
            DEBUG = false;
#endif

            mEndScrollRunnable = new Java.Lang.Runnable(() =>
            {
                ScrollState = SCROLL_STATE_IDLE;
                populate();
            });

            SetWillNotDraw(false);
			DescendantFocusability =  Android.Views.DescendantFocusability.AfterDescendants;
			Focusable = true;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.content.Context context = getContext();
			Context context = Context;
			mScroller = new Scroller(context, sInterpolator);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.ViewConfiguration configuration = android.view.ViewConfiguration.get(context);
			ViewConfiguration configuration = ViewConfiguration.Get(context);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float density = context.getResources().getDisplayMetrics().density;
			float density = context.Resources.DisplayMetrics.Density;

			mTouchSlop = ViewConfigurationCompat.GetScaledPagingTouchSlop(configuration);
			mMinimumVelocity = (int)(MIN_FLING_VELOCITY * density);
			mMaximumVelocity = configuration.ScaledMaximumFlingVelocity;
			mLeftEdge = new EdgeEffectCompat(context);
			mRightEdge = new EdgeEffectCompat(context);

			mFlingDistance = (int)(MIN_DISTANCE_FOR_FLING * density);
			mCloseEnough = (int)(CLOSE_ENOUGH * density);
			mDefaultGutterSize = (int)(DEFAULT_GUTTER_SIZE * density);

			ViewCompat.SetAccessibilityDelegate(this, new MyAccessibilityDelegate(this));

			if (ViewCompat.GetImportantForAccessibility(this) == ViewCompat.ImportantForAccessibilityAuto)
			{
				ViewCompat.SetImportantForAccessibility(this, ViewCompat.ImportantForAccessibilityYes);
			}
		}

		protected override void OnDetachedFromWindow()
		{
			RemoveCallbacks(mEndScrollRunnable);
			base.OnDetachedFromWindow();
		}

		private int ScrollState
		{
			set
			{
				if (mScrollState == value)
				{
					return;
				}
    
				mScrollState = value;
				if (mPageTransformer != null)
				{
					// PageTransformers can do complex things that benefit from hardware layers.
					enableLayers(value != SCROLL_STATE_IDLE);
				}
				foreach (OnPageChangeListener eachListener in mOnPageChangeListeners)
				{
					if (eachListener != null)
					{
						eachListener.onPageScrollStateChanged(value);
					}
				}
			}
		}

		/// <summary>
		/// Set a PagerAdapter that will supply views for this pager as needed.
		/// </summary>
		/// <param name="adapter"> Adapter to use </param>
		public virtual PagerAdapter Adapter
		{
			set
			{
				if (mAdapter != null)
				{
					mAdapter.UnregisterDataSetObserver(mObserver);
					mAdapter.StartUpdate(this);
					for (int i = 0; i < mItems.Count; i++)
					{
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final ItemInfo ii = mItems.get(i);
						ItemInfo ii = mItems[i];
						mAdapter.DestroyItem(this, ii.position, ii.@object);
					}
					mAdapter.FinishUpdate(this);
					mItems.Clear();
					removeNonDecorViews();
					mCurItem = 0;
					ScrollTo(0, 0);
				}
    
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final android.support.v4.view.PagerAdapter oldAdapter = mAdapter;
				PagerAdapter oldAdapter = mAdapter;
				mAdapter = value;
				mExpectedAdapterCount = 0;
    
				if (mAdapter != null)
				{
					if (mObserver == null)
					{
						mObserver = new PagerObserver(this);
					}
					mAdapter.RegisterDataSetObserver(mObserver);
					mPopulatePending = false;
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final boolean wasFirstLayout = mFirstLayout;
					bool wasFirstLayout = mFirstLayout;
					mFirstLayout = true;
					mExpectedAdapterCount = mAdapter.Count;
					if (mRestoredCurItem >= 0)
					{
						mAdapter.RestoreState(mRestoredAdapterState, mRestoredClassLoader);
						setCurrentItemInternal(mRestoredCurItem, false, true);
						mRestoredCurItem = -1;
						mRestoredAdapterState = null;
						mRestoredClassLoader = null;
					}
					else if (!wasFirstLayout)
					{
						populate();
					}
					else
					{
						RequestLayout();
					}
				}
    
				if (mAdapterChangeListener != null && oldAdapter != value)
				{
					mAdapterChangeListener.onAdapterChanged(oldAdapter, value);
				}
			}
			get
			{
				return mAdapter;
			}
		}

		private void removeNonDecorViews()
		{
			for (int i = 0; i < ChildCount; i++)
			{

				View child = GetChildAt(i);
				LayoutParams lp = (LayoutParams) child.LayoutParameters;
				if (!lp.isDecor)
				{
					RemoveViewAt(i);
					i--;
				}
			}
		}


		internal virtual OnAdapterChangeListener OnAdapterChangeListener
		{
			set
			{
				mAdapterChangeListener = value;
			}
		}

		private int ClientWidth
		{
			get
			{
				return MeasuredWidth - PaddingLeft - PaddingRight;
			}
		}

		/// <summary>
		/// Set the currently selected page. If the ViewPager has already been through its first
		/// layout with its current adapter there will be a smooth animated transition between
		/// the current item and the specified item.
		/// </summary>
		/// <param name="item"> Item index to select </param>
		public virtual int CurrentItem
		{
			set
			{
				mPopulatePending = false;
				setCurrentItemInternal(value, !mFirstLayout, false);
			}
			get
			{
				return mCurItem;
			}
		}

		/// <summary>
		/// Set the currently selected page.
		/// </summary>
		/// <param name="item"> Item index to select </param>
		/// <param name="smoothScroll"> True to smoothly scroll to the new item, false to transition immediately </param>
		public virtual void setCurrentItem(int item, bool smoothScroll)
		{
			mPopulatePending = false;
			setCurrentItemInternal(item, smoothScroll, false);
		}


		internal virtual void setCurrentItemInternal(int item, bool smoothScroll, bool always)
		{
			setCurrentItemInternal(item, smoothScroll, always, 0);
		}

		internal virtual void setCurrentItemInternal(int item, bool smoothScroll, bool always, int velocity)
		{
			if (mAdapter == null || mAdapter.Count <= 0)
			{
				ScrollingCacheEnabled = false;
				return;
			}
			if (!always && mCurItem == item && mItems.Count != 0)
			{
				ScrollingCacheEnabled = false;
				return;
			}

			if (item < 0)
			{
				item = 0;
			}
			else if (item >= mAdapter.Count)
			{
				item = mAdapter.Count - 1;
			}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int pageLimit = mOffscreenPageLimit;
			int pageLimit = mOffscreenPageLimit;
			if (item > (mCurItem + pageLimit) || item < (mCurItem - pageLimit))
			{
				// We are doing a jump by more than one page.  To avoid
				// glitches, we want to keep all current pages in the view
				// until the scroll ends.
				for (int i = 0; i < mItems.Count; i++)
				{
					mItems[i].scrolling = true;
				}
			}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean dispatchSelected = mCurItem != item;
			bool dispatchSelected = mCurItem != item;

			if (mFirstLayout)
			{
				// We don't have any idea how big we are yet and shouldn't have any pages either.
				// Just set things up and let the pending layout handle things.
				mCurItem = item;
				triggerOnPageChangeEvent(item);
				RequestLayout();
			}
			else
			{
				populate(item);
				scrollToItem(item, smoothScroll, velocity, dispatchSelected);
			}
		}

		private void scrollToItem(int item, bool smoothScroll, int velocity, bool dispatchSelected)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ItemInfo curInfo = infoForPosition(item);
			ItemInfo curInfo = infoForPosition(item);
			int destX = 0;
			if (curInfo != null)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width = getClientWidth();
				int width = ClientWidth;
				destX = (int)(width * Math.Max(mFirstOffset, Math.Min(curInfo.offset, mLastOffset)));
			}
			if (smoothScroll)
			{
				smoothScrollTo(destX, 0, velocity);
				if (dispatchSelected)
				{
					triggerOnPageChangeEvent(item);
				}
			}
			else
			{
				if (dispatchSelected)
				{
					triggerOnPageChangeEvent(item);
				}
				completeScroll(false);
				ScrollTo(destX, 0);
				pageScrolled(destX);
			}
		}

		/// <summary>
		/// Add a listener that will be invoked whenever the page changes or is incrementally
		/// scrolled. See <seealso cref="OnPageChangeListener"/>.
		/// </summary>
		/// <param name="listener"> Listener to add </param>
		public virtual void addOnPageChangeListener(OnPageChangeListener listener)
		{
			if (!mOnPageChangeListeners.Contains(listener))
			{
				mOnPageChangeListeners.Add(listener);
			}
		}

		/// <summary>
		/// Remove a listener that was added with addOnPageChangeListener
		/// See <seealso cref="OnPageChangeListener"/>.
		/// </summary>
		/// <param name="listener"> Listener to remove </param>
		public virtual void removeOnPageChangeListener(OnPageChangeListener listener)
		{
			mOnPageChangeListeners.Remove(listener);
		}

		/// <summary>
		/// Set a <seealso cref="PageTransformer"/> that will be called for each attached page whenever
		/// the scroll position is changed. This allows the application to apply custom property
		/// transformations to each page, overriding the default sliding look and feel.
		/// 
		/// <para><em>Note:</em> Prior to Android 3.0 the property animation APIs did not exist.
		/// As a result, setting a PageTransformer prior to Android 3.0 (API 11) will have no effect.</para>
		/// </summary>
		/// <param name="reverseDrawingOrder"> true if the supplied PageTransformer requires page views
		///                            to be drawn from last to first instead of first to last. </param>
		/// <param name="transformer"> PageTransformer that will modify each page's animation properties </param>
		public virtual void setPageTransformer(bool reverseDrawingOrder, PageTransformer transformer)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean hasTransformer = transformer != null;
			bool hasTransformer = transformer != null;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean needsPopulate = hasTransformer != (mPageTransformer != null);
			bool needsPopulate = hasTransformer != (mPageTransformer != null);
			mPageTransformer = transformer;
			ChildrenDrawingOrderEnabledCompat = hasTransformer;
			if (hasTransformer)
			{
				mDrawingOrder = reverseDrawingOrder ? DRAW_ORDER_REVERSE : DRAW_ORDER_FORWARD;
			}
			else
			{
				mDrawingOrder = DRAW_ORDER_DEFAULT;
			}
			if (needsPopulate)
			{
				populate();
			}
		}

		internal virtual bool ChildrenDrawingOrderEnabledCompat
		{
			set
			{
				if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.EclairMr1)
				{
					if (mSetChildrenDrawingOrderEnabled == null)
					{
						try
						{
                            mSetChildrenDrawingOrderEnabled = typeof(ViewGroup).GetProperty("ChildrenDrawingOrderEnabled"); //Java.Lang.Class.FromType(typeof(ViewGroup)).GetDeclaredMethod("setChildrenDrawingOrderEnabled", new Java.Lang.Class[] { Java.Lang.Class.FromType(typeof(Boolean)) });//TODO: may cause bugs

                        }
						catch (Java.Lang.NoSuchMethodException e)
						{
							Log.Error(TAG, "Can't find setChildrenDrawingOrderEnabled", e);
						}
					}
					try
					{
						mSetChildrenDrawingOrderEnabled.SetValue(this, value);
					}
					catch (Exception e)
					{
						Log.Error(TAG, "Error changing children drawing order", e);
					}
				}
			}
		}

		protected override int GetChildDrawingOrder(int childCount, int i)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int index = mDrawingOrder == DRAW_ORDER_REVERSE ? childCount - 1 - i : i;
			int index = mDrawingOrder == DRAW_ORDER_REVERSE ? childCount - 1 - i : i;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int result = ((LayoutParams) mDrawingOrderedChildren.get(index).getLayoutParams()).childIndex;
			int result = ((LayoutParams) mDrawingOrderedChildren[index].LayoutParameters).childIndex;
			return result;
		}

		/// <summary>
		/// Set a separate OnPageChangeListener for internal use by the support library.
		/// </summary>
		/// <param name="listener"> Listener to set </param>
		/// <returns> The old listener that was set, if any. </returns>
		internal virtual OnPageChangeListener setInternalPageChangeListener(OnPageChangeListener listener)
		{
			OnPageChangeListener oldListener = mInternalPageChangeListener;
			mInternalPageChangeListener = listener;
			return oldListener;
		}

		/// <summary>
		/// Returns the number of pages that will be retained to either side of the
		/// current page in the view hierarchy in an idle state. Defaults to 1.
		/// </summary>
		/// <returns> How many pages will be kept offscreen on either side </returns>
		/// <seealso cref= #setOffscreenPageLimit(int) </seealso>
		public virtual int OffscreenPageLimit
		{
			get
			{
				return mOffscreenPageLimit;
			}
			set
			{
				if (value < DEFAULT_OFFSCREEN_PAGES)
				{
					Log.Warn(TAG, "Requested offscreen page limit " + value + " too small; defaulting to " + DEFAULT_OFFSCREEN_PAGES);
					value = DEFAULT_OFFSCREEN_PAGES;
				}
				if (value != mOffscreenPageLimit)
				{
					mOffscreenPageLimit = value;
					populate();
				}
			}
		}


		/// <summary>
		/// Set the margin between pages.
		/// </summary>
		/// <param name="marginPixels"> Distance between adjacent pages in pixels </param>
		/// <seealso cref= #getPageMargin() </seealso>
		/// <seealso cref= #setPageMarginDrawable(Drawable) </seealso>
		/// <seealso cref= #setPageMarginDrawable(int) </seealso>
		public virtual int PageMargin
		{
			set
			{
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final int oldMargin = mPageMargin;
				int oldMargin = mPageMargin;
				mPageMargin = value;
    
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final int width = getWidth();
				int width = Width;
				recomputeScrollPosition(width, width, value, oldMargin);
    
				RequestLayout();
			}
			get
			{
				return mPageMargin;
			}
		}


		/// <summary>
		/// Set a drawable that will be used to fill the margin between pages.
		/// </summary>
		/// <param name="d"> Drawable to display between pages </param>
		public virtual void SetPageMarginDrawable(Drawable val)
		{

				mMarginDrawable = val;
				if (val != null)
				{
					RefreshDrawableState();
				}
				SetWillNotDraw(val == null);
				Invalidate();

		}

		/// <summary>
		/// Set a drawable that will be used to fill the margin between pages.
		/// </summary>
		/// <param name="resId"> Resource ID of a drawable to display between pages </param>
		public virtual void SetPageMarginDrawable(int val)
		{
			SetPageMarginDrawable(Context.Resources.GetDrawable(val));
		}

		protected override bool VerifyDrawable(Drawable who)
		{
			return base.VerifyDrawable(who) || who == mMarginDrawable;
		}

		protected override void DrawableStateChanged()
		{
			base.DrawableStateChanged();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.graphics.drawable.Drawable d = mMarginDrawable;
			Drawable d = mMarginDrawable;
			if (d != null && d.IsStateful)
			{
				d.SetState(this.GetDrawableState());
			}
		}

		// We want the duration of the page snap animation to be influenced by the distance that
		// the screen has to travel, however, we don't want this duration to be effected in a
		// purely linear fashion. Instead, we use this method to moderate the effect that the distance
		// of travel has on the overall snap duration.
		internal virtual float distanceInfluenceForSnapDuration(float f)
		{
			f -= 0.5f; // center the values about 0.
			f *= 0.3f * ((float)Math.PI) / 2.0f;
			return (float) Math.Sin(f);
		}

		/// <summary>
		/// Like <seealso cref="View#scrollBy"/>, but scroll smoothly instead of immediately.
		/// </summary>
		/// <param name="x"> the number of pixels to scroll by on the X axis </param>
		/// <param name="y"> the number of pixels to scroll by on the Y axis </param>
		internal virtual void smoothScrollTo(int x, int y)
		{
			smoothScrollTo(x, y, 0);
		}

		/// <summary>
		/// Like <seealso cref="View#scrollBy"/>, but scroll smoothly instead of immediately.
		/// </summary>
		/// <param name="x"> the number of pixels to scroll by on the X axis </param>
		/// <param name="y"> the number of pixels to scroll by on the Y axis </param>
		/// <param name="velocity"> the velocity associated with a fling, if applicable. (0 otherwise) </param>
		internal virtual void smoothScrollTo(int x, int y, int velocity)
		{
			if (ChildCount == 0)
			{
				// Nothing to do.
				ScrollingCacheEnabled = false;
				return;
			}
			int sx = ScrollX;
			int sy = ScrollY;
			int dx = x - sx;
			int dy = y - sy;
			if (dx == 0 && dy == 0)
			{
				completeScroll(false);
				populate();
				ScrollState = SCROLL_STATE_IDLE;
				return;
			}

			ScrollingCacheEnabled = true;
			ScrollState = SCROLL_STATE_SETTLING;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width = getClientWidth();
			int width = ClientWidth;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int halfWidth = width / 2;
			int halfWidth = width / 2;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float distanceRatio = Math.min(1f, 1.0f * Math.abs(dx) / width);
			float distanceRatio = Math.Min(1f, 1.0f * Math.Abs(dx) / width);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float distance = halfWidth + halfWidth * distanceInfluenceForSnapDuration(distanceRatio);
			float distance = halfWidth + halfWidth * distanceInfluenceForSnapDuration(distanceRatio);

			int duration = 0;
			velocity = Math.Abs(velocity);
			if (velocity > 0)
			{
				duration = Convert.ToInt32(4 * Math.Round(1000 * Math.Abs(distance / velocity)));
			}
			else
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float pageWidth = width * mAdapter.getPageWidth(mCurItem);
				float pageWidth = width * mAdapter.GetPageWidth(mCurItem);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float pageDelta = (float) Math.abs(dx) / (pageWidth + mPageMargin);
				float pageDelta = (float) Math.Abs(dx) / (pageWidth + mPageMargin);
				duration = (int)((pageDelta + 1) * 100);
			}
			duration = Math.Min(duration, MAX_SETTLE_DURATION);

			mScroller.StartScroll(sx, sy, dx, dy, duration);
			ViewCompat.PostInvalidateOnAnimation(this);
		}

		internal virtual ItemInfo addNewItem(int position, int index)
		{
			ItemInfo ii = new ItemInfo();
			ii.position = position;
			ii.@object = mAdapter.InstantiateItem(this, position);
			ii.widthFactor = mAdapter.GetPageWidth(position);
			if (index < 0 || index >= mItems.Count)
			{
				mItems.Add(ii);
			}
			else
			{
				mItems.Insert(index, ii);
			}
			return ii;
		}

		internal virtual void dataSetChanged()
		{
			// This method only gets called if our observer is attached, so mAdapter is non-null.

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int adapterCount = mAdapter.getCount();
			int adapterCount = mAdapter.Count;
			mExpectedAdapterCount = adapterCount;
			bool needPopulate = mItems.Count < mOffscreenPageLimit * 2 + 1 && mItems.Count < adapterCount;
			int newCurrItem = mCurItem;

			bool isUpdating = false;
			for (int i = 0; i < mItems.Count; i++)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ItemInfo ii = mItems.get(i);
				ItemInfo ii = mItems[i];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int newPos = mAdapter.getItemPosition(ii.object);
				int newPos = mAdapter.GetItemPosition(ii.@object);

				if (newPos == PagerAdapter.PositionUnchanged)
				{
					continue;
				}

				if (newPos == PagerAdapter.PositionNone)
				{
					mItems.RemoveAt(i);
					i--;

					if (!isUpdating)
					{
						mAdapter.StartUpdate(this);
						isUpdating = true;
					}

					mAdapter.DestroyItem(this, ii.position, ii.@object);
					needPopulate = true;

					if (mCurItem == ii.position)
					{
						// Keep the current item in the valid range
						newCurrItem = Math.Max(0, Math.Min(mCurItem, adapterCount - 1));
						needPopulate = true;
					}
					continue;
				}

				if (ii.position != newPos)
				{
					if (ii.position == mCurItem)
					{
						// Our current item changed position. Follow it.
						newCurrItem = newPos;
					}

					ii.position = newPos;
					needPopulate = true;
				}
			}

			if (isUpdating)
			{
				mAdapter.FinishUpdate(this);
			}

			mItems.Sort(COMPARATOR);

			if (needPopulate)
			{
				// Reset our known page widths; populate will recompute them.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int childCount = getChildCount();
				int childCount = ChildCount;
				for (int i = 0; i < childCount; i++)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View child = getChildAt(i);
					View child = GetChildAt(i);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LayoutParams lp = (LayoutParams) child.getLayoutParams();
					LayoutParams lp = (LayoutParams) child.LayoutParameters;
					if (!lp.isDecor)
					{
						lp.widthFactor = 0.0f;
					}
				}

				setCurrentItemInternal(newCurrItem, false, true);
				RequestLayout();
			}
		}

		internal virtual void populate()
		{
			populate(mCurItem);
		}

		internal virtual void populate(int newCurrentItem)
		{
			ItemInfo oldCurInfo = null;
			var focusDirection = Android.Views.FocusSearchDirection.Forward;
			if (mCurItem != newCurrentItem)
			{
				focusDirection = mCurItem < newCurrentItem ? Android.Views.FocusSearchDirection.Right : Android.Views.FocusSearchDirection.Left;
				oldCurInfo = infoForPosition(mCurItem);
				mCurItem = newCurrentItem;
			}

			if (mAdapter == null)
			{
				sortChildDrawingOrder();
				return;
			}

			// Bail now if we are waiting to populate.  This is to hold off
			// on creating views from the time the user releases their finger to
			// fling to a new position until we have finished the scroll to
			// that position, avoiding glitches from happening at that point.
			if (mPopulatePending)
			{
				if (DEBUG)
				{
					Log.Info(TAG, "populate is pending, skipping for now...");
				}
				sortChildDrawingOrder();
				return;
			}

			// Also, don't populate until we are attached to a window.  This is to
			// avoid trying to populate before we have restored our view hierarchy
			// state and conflicting with what is restored.
			if (WindowToken == null)
			{
				return;
			}

			mAdapter.StartUpdate(this);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int pageLimit = mOffscreenPageLimit;
			int pageLimit = mOffscreenPageLimit;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int startPos = Math.max(0, mCurItem - pageLimit);
			int startPos = Math.Max(0, mCurItem - pageLimit);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int N = mAdapter.getCount();
			int N = mAdapter.Count;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int endPos = Math.min(N-1, mCurItem + pageLimit);
			int endPos = Math.Min(N - 1, mCurItem + pageLimit);

			if (N != mExpectedAdapterCount)
			{
				string resName;
				try
				{
					resName = Resources.GetResourceName(Id);
				}
				catch (Resources.NotFoundException)
				{
					resName = Id.ToString("x");
				}
				throw new Java.Lang.IllegalStateException("The application's PagerAdapter changed the adapter's" + " contents without calling PagerAdapter#notifyDataSetChanged!" + " Expected adapter item count: " + mExpectedAdapterCount + ", found: " + N + " Pager id: " + resName + " Pager class: " + this.GetType() + " Problematic adapter: " + mAdapter.GetType());
			}

			// Locate the currently focused item or add it if needed.
			int curIndex = -1;
			ItemInfo curItem = null;
			for (curIndex = 0; curIndex < mItems.Count; curIndex++)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ItemInfo ii = mItems.get(curIndex);
				ItemInfo ii = mItems[curIndex];
				if (ii.position >= mCurItem)
				{
					if (ii.position == mCurItem)
					{
						curItem = ii;
					}
					break;
				}
			}

			if (curItem == null && N > 0)
			{
				curItem = addNewItem(mCurItem, curIndex);
			}

			// Fill 3x the available width or up to the number of offscreen
			// pages requested to either side, whichever is larger.
			// If we have no current item we have no work to do.
			if (curItem != null)
			{
				float extraWidthLeft = 0.0f;
				int itemIndex = curIndex - 1;
				ItemInfo ii = itemIndex >= 0 ? mItems[itemIndex] : null;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int clientWidth = getClientWidth();
				int clientWidth = ClientWidth;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float leftWidthNeeded = clientWidth <= 0 ? 0 : 2.0f - curItem.widthFactor + (float) getPaddingLeft() / (float) clientWidth;
				float leftWidthNeeded = clientWidth <= 0 ? 0 : 2.0f - curItem.widthFactor + (float) PaddingLeft / (float) clientWidth;
				for (int pos = mCurItem - 1; pos >= 0; pos--)
				{
					if (extraWidthLeft >= leftWidthNeeded && pos < startPos)
					{
						if (ii == null)
						{
							break;
						}
						if (pos == ii.position && !ii.scrolling)
						{
							mItems.RemoveAt(itemIndex);
							mAdapter.DestroyItem(this, pos, ii.@object);
							if (DEBUG)
							{
								Log.Info(TAG, "populate() - destroyItem() with pos: " + pos + " view: " + ((View) ii.@object));
							}
							itemIndex--;
							curIndex--;
							ii = itemIndex >= 0 ? mItems[itemIndex] : null;
						}
					}
					else if (ii != null && pos == ii.position)
					{
						extraWidthLeft += ii.widthFactor;
						itemIndex--;
						ii = itemIndex >= 0 ? mItems[itemIndex] : null;
					}
					else
					{
						ii = addNewItem(pos, itemIndex + 1);
						extraWidthLeft += ii.widthFactor;
						curIndex++;
						ii = itemIndex >= 0 ? mItems[itemIndex] : null;
					}
				}

				float extraWidthRight = curItem.widthFactor;
				itemIndex = curIndex + 1;
				if (extraWidthRight < 2.0f)
				{
					ii = itemIndex < mItems.Count ? mItems[itemIndex] : null;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float rightWidthNeeded = clientWidth <= 0 ? 0 : (float) getPaddingRight() / (float) clientWidth + 2.0f;
					float rightWidthNeeded = clientWidth <= 0 ? 0 : (float) PaddingRight / (float) clientWidth + 2.0f;
					for (int pos = mCurItem + 1; pos < N; pos++)
					{
						if (extraWidthRight >= rightWidthNeeded && pos > endPos)
						{
							if (ii == null)
							{
								break;
							}
							if (pos == ii.position && !ii.scrolling)
							{
								mItems.RemoveAt(itemIndex);
								mAdapter.DestroyItem(this, pos, ii.@object);
								if (DEBUG)
								{
									Log.Info(TAG, "populate() - destroyItem() with pos: " + pos + " view: " + ((View) ii.@object));
								}
								ii = itemIndex < mItems.Count ? mItems[itemIndex] : null;
							}
						}
						else if (ii != null && pos == ii.position)
						{
							extraWidthRight += ii.widthFactor;
							itemIndex++;
							ii = itemIndex < mItems.Count ? mItems[itemIndex] : null;
						}
						else
						{
							ii = addNewItem(pos, itemIndex);
							itemIndex++;
							extraWidthRight += ii.widthFactor;
							ii = itemIndex < mItems.Count ? mItems[itemIndex] : null;
						}
					}
				}

				calculatePageOffsets(curItem, curIndex, oldCurInfo);
			}

			if (DEBUG)
			{
				Log.Info(TAG, "Current page list:");
				for (int i = 0; i < mItems.Count; i++)
				{
					Log.Info(TAG, "#" + i + ": page " + mItems[i].position);
				}
			}

			mAdapter.SetPrimaryItem(this, mCurItem, curItem != null ? curItem.@object : null);

			mAdapter.FinishUpdate(this);

			// Check width measurement of current pages and drawing sort order.
			// Update LayoutParams as needed.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int childCount = getChildCount();
			int childCount = ChildCount;
			for (int i = 0; i < childCount; i++)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View child = getChildAt(i);
				View child = GetChildAt(i);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LayoutParams lp = (LayoutParams) child.getLayoutParams();
				LayoutParams lp = (LayoutParams) child.LayoutParameters;
				lp.childIndex = i;
				if (!lp.isDecor && lp.widthFactor == 0.0f)
				{
					// 0 means requery the adapter for this, it doesn't have a valid width.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ItemInfo ii = infoForChild(child);
					ItemInfo ii = infoForChild(child);
					if (ii != null)
					{
						lp.widthFactor = ii.widthFactor;
						lp.position = ii.position;
					}
				}
			}
			sortChildDrawingOrder();

			if (HasFocus)
			{
				View currentFocused = FindFocus();
				ItemInfo ii = currentFocused != null ? infoForAnyChild(currentFocused) : null;
				if (ii == null || ii.position != mCurItem)
				{
					for (int i = 0; i < ChildCount; i++)
					{
						View child = GetChildAt(i);
						ii = infoForChild(child);
						if (ii != null && ii.position == mCurItem)
						{
							if (child.RequestFocus(focusDirection))
							{
								break;
							}
						}
					}
				}
			}
		}

		private void sortChildDrawingOrder()
		{
			if (mDrawingOrder != DRAW_ORDER_DEFAULT)
			{
				if (mDrawingOrderedChildren == null)
				{
					mDrawingOrderedChildren = new List<View>();
				}
				else
				{
					mDrawingOrderedChildren.Clear();
				}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int childCount = getChildCount();
				int childCount = ChildCount;
				for (int i = 0; i < childCount; i++)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View child = getChildAt(i);
					View child = GetChildAt(i);
					mDrawingOrderedChildren.Add(child);
				}
				mDrawingOrderedChildren.Sort(sPositionComparator);
			}
		}

		private void calculatePageOffsets(ItemInfo curItem, int curIndex, ItemInfo oldCurInfo)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int N = mAdapter.getCount();
			int N = mAdapter.Count;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width = getClientWidth();
			int width = ClientWidth;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float marginOffset = width > 0 ? (float) mPageMargin / width : 0;
			float marginOffset = width > 0 ? (float) mPageMargin / width : 0;
            float offset = 0;
            int pos = 0;
            // Fix up offsets for later layout.
            if (oldCurInfo != null)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int oldCurPosition = oldCurInfo.position;
				int oldCurPosition = oldCurInfo.position;
                // Base offsets off of oldCurInfo.
                if (oldCurPosition < curItem.position)
				{
					int itemIndex = 0;
					ItemInfo ii = null;
					offset = oldCurInfo.offset + oldCurInfo.widthFactor + marginOffset;
					for (pos = oldCurPosition + 1; pos <= curItem.position && itemIndex < mItems.Count; pos++)
					{
						ii = mItems[itemIndex];
						while (pos > ii.position && itemIndex < mItems.Count - 1)
						{
							itemIndex++;
							ii = mItems[itemIndex];
						}
						while (pos < ii.position)
						{
							// We don't have an item populated for this,
							// ask the adapter for an offset.
							offset += mAdapter.GetPageWidth(pos) + marginOffset;
							pos++;
						}
						ii.offset = offset;
						offset += ii.widthFactor + marginOffset;
					}
				}
				else if (oldCurPosition > curItem.position)
				{
					int itemIndex = mItems.Count - 1;
					ItemInfo ii = null;
					offset = oldCurInfo.offset;
					for (pos = oldCurPosition - 1; pos >= curItem.position && itemIndex >= 0; pos--)
					{
						ii = mItems[itemIndex];
						while (pos < ii.position && itemIndex > 0)
						{
							itemIndex--;
							ii = mItems[itemIndex];
						}
						while (pos > ii.position)
						{
							// We don't have an item populated for this,
							// ask the adapter for an offset.
							offset -= mAdapter.GetPageWidth(pos) + marginOffset;
							pos--;
						}
						offset -= ii.widthFactor + marginOffset;
						ii.offset = offset;
					}
				}
			}

			// Base all offsets off of curItem.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int itemCount = mItems.size();
			int itemCount = mItems.Count;
			offset = curItem.offset;
			pos = curItem.position - 1;
			mFirstOffset = curItem.position == 0 ? curItem.offset : -float.MaxValue;
			mLastOffset = curItem.position == N - 1 ? curItem.offset + curItem.widthFactor - 1 : float.MaxValue;
			// Previous pages
			for (int i = curIndex - 1; i >= 0; i--, pos--)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ItemInfo ii = mItems.get(i);
				ItemInfo ii = mItems[i];
				while (pos > ii.position)
				{
					offset -= mAdapter.GetPageWidth(pos--) + marginOffset;
				}
				offset -= ii.widthFactor + marginOffset;
				ii.offset = offset;
				if (ii.position == 0)
				{
					mFirstOffset = offset;
				}
			}
			offset = curItem.offset + curItem.widthFactor + marginOffset;
			pos = curItem.position + 1;
			// Next pages
			for (int i = curIndex + 1; i < itemCount; i++, pos++)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ItemInfo ii = mItems.get(i);
				ItemInfo ii = mItems[i];
				while (pos < ii.position)
				{
					offset += mAdapter.GetPageWidth(pos++) + marginOffset;
				}
				if (ii.position == N - 1)
				{
					mLastOffset = offset + ii.widthFactor - 1;
				}
				ii.offset = offset;
				offset += ii.widthFactor + marginOffset;
			}

			mNeedCalculatePageOffsets = false;
		}

		/// <summary>
		/// This is the persistent state that is saved by ViewPager.  Only needed
		/// if you are creating a sublass of ViewPager that must save its own
		/// state, in which case it should implement a subclass of this which
		/// contains that state.
		/// </summary>
		public class SavedState : BaseSavedState, Android.OS.IParcelable
        {
			internal int position;
			internal Android.OS.IParcelable adapterState;
			internal Java.Lang.ClassLoader loader;

			public SavedState(Android.OS.IParcelable superState) : base(superState)
			{
			}

			public override void WriteToParcel(Parcel @out, Android.OS.ParcelableWriteFlags flags)
			{
				base.WriteToParcel(@out, flags);
				@out.WriteInt(position);
				@out.WriteParcelable(adapterState, flags);
			}

			public override string ToString()
			{
				return "FragmentPager.SavedState{" + Java.Lang.JavaSystem.IdentityHashCode(this).ToString("x") + " position=" + position + "}";
			}

            //orig
            //public static readonly Parcelable.Creator<SavedState> CREATOR = ParcelableCompat.NewCreator(new ParcelableCompatCreatorCallbacksAnonymousInnerClassHelper());

            //private class ParcelableCompatCreatorCallbacksAnonymousInnerClassHelper : ParcelableCompatCreatorCallbacks<SavedState>
            //{
            //	public ParcelableCompatCreatorCallbacksAnonymousInnerClassHelper()
            //	{
            //	}

            //	public override SavedState createFromParcel(Parcel @in, ClassLoader loader)
            //	{
            //		return new SavedState(@in, loader);
            //	}

            //	public override SavedState[] newArray(int size)
            //	{
            //		return new SavedState[size];
            //	}
            //}

            public static readonly Android.OS.IParcelableCreator CREATOR = ParcelableCompat.NewCreator(new ParcelableCompatCreatorCallbacksAnonymousInnerClassHelper());

            private class ParcelableCompatCreatorCallbacksAnonymousInnerClassHelper : Java.Lang.Object, Android.Support.V4.OS.IParcelableCompatCreatorCallbacks
            {
                public ParcelableCompatCreatorCallbacksAnonymousInnerClassHelper()
                {
                }

                public Java.Lang.Object CreateFromParcel(Parcel @in, Java.Lang.ClassLoader loader)
                {
                    return new SavedState(@in, loader);
                }

                public Java.Lang.Object[] NewArray(int size)
                {
                    return new SavedState[size];
                }
            }

            internal SavedState(Parcel @in, Java.Lang.ClassLoader loader) : base(@in)
			{
				if (loader == null)
				{
					loader = Java.Lang.Class.FromType(this.GetType()).ClassLoader;
				}
				position = @in.ReadInt();
				adapterState = @in.ReadParcelable(loader) as Android.OS.IParcelable;//TODO: may cause bugs
				this.loader = loader;
			}
		}

		protected override Android.OS.IParcelable OnSaveInstanceState()
		{
			var superState = base.OnSaveInstanceState();
			SavedState ss = new SavedState(superState);
			ss.position = mCurItem;
			if (mAdapter != null)
			{
				ss.adapterState = mAdapter.SaveState();
			}
			return ss;
		}

		protected override void OnRestoreInstanceState(Android.OS.IParcelable state)
		{
			if (!(state is SavedState))
			{
				base.OnRestoreInstanceState(state);
				return;
			}

			SavedState ss = (SavedState)state;
			base.OnRestoreInstanceState(ss.SuperState);

			if (mAdapter != null)
			{
				mAdapter.RestoreState(ss.adapterState, ss.loader);
				setCurrentItemInternal(ss.position, false, true);
			}
			else
			{
				mRestoredCurItem = ss.position;
				mRestoredAdapterState = ss.adapterState;
				mRestoredClassLoader = ss.loader;
			}
		}

		public override void AddView(View child, int index, ViewGroup.LayoutParams @params)
		{
			if (!CheckLayoutParams(@params))
			{
				@params = GenerateLayoutParams(@params);
			}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LayoutParams lp = (LayoutParams) params;
			LayoutParams lp = (LayoutParams) @params;
			lp.isDecor |= child is Decor;
			if (mInLayout)
			{
				if (lp != null && lp.isDecor)
				{
					throw new Java.Lang.IllegalStateException("Cannot add pager decor view during layout");
				}
				lp.needsMeasure = true;
				AddViewInLayout(child, index, @params);
			}
			else
			{
				base.AddView(child, index, @params);
			}

			if (USE_CACHE)
			{
				if (child.Visibility != Android.Views.ViewStates.Gone)
				{
					child.DrawingCacheEnabled = mScrollingCacheEnabled;
				}
				else
				{
					child.DrawingCacheEnabled = false;
				}
			}
		}

		public override void RemoveView(View view)
		{
			if (mInLayout)
			{
				RemoveViewInLayout(view);
			}
			else
			{
				base.RemoveView(view);
			}
		}

		internal virtual ItemInfo infoForChild(View child)
		{
			for (int i = 0; i < mItems.Count; i++)
			{
				ItemInfo ii = mItems[i];
				if (mAdapter.IsViewFromObject(child, ii.@object))
				{
					return ii;
				}
			}
			return null;
		}

		internal virtual ItemInfo infoForAnyChild(View child)
		{
			IViewParent parent;
			while ((parent = child.Parent) != this)
			{
				if (parent == null || !(parent is View))
				{
					return null;
				}
				child = (View)parent;
			}
			return infoForChild(child);
		}

		internal virtual ItemInfo infoForPosition(int position)
		{
			for (int i = 0; i < mItems.Count; i++)
			{
				ItemInfo ii = mItems[i];
				if (ii.position == position)
				{
					return ii;
				}
			}
			return null;
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();
			mFirstLayout = true;
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			// For simple implementation, our internal size is always 0.
			// We depend on the container to specify the layout size of
			// our view.  We can't really know what it is since we will be
			// adding and removing different arbitrary views and do not
			// want the layout to change as this happens.
			SetMeasuredDimension(GetDefaultSize(0, widthMeasureSpec), GetDefaultSize(0, heightMeasureSpec));

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int measuredWidth = getMeasuredWidth();
			int measuredWidth = MeasuredWidth;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int maxGutterSize = measuredWidth / 10;
			int maxGutterSize = measuredWidth / 10;
			mGutterSize = Math.Min(maxGutterSize, mDefaultGutterSize);

			// Children are just made to fill our space.
			int childWidthSize = measuredWidth - PaddingLeft - PaddingRight;
			int childHeightSize = MeasuredHeight - PaddingTop - PaddingBottom;

			/*
			 * Make sure all children have been properly measured. Decor views first.
			 * Right now we cheat and make this less complicated by assuming decor
			 * views won't intersect. We will pin to edges based on gravity.
			 */
			int size = ChildCount;
			for (int i = 0; i < size; ++i)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View child = getChildAt(i);
				View child = GetChildAt(i);
				if (child.Visibility != Android.Views.ViewStates.Gone)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LayoutParams lp = (LayoutParams) child.getLayoutParams();
					LayoutParams lp = (LayoutParams) child.LayoutParameters;
					if (lp != null && lp.isDecor)
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int hgrav = lp.gravity & android.view.Gravity.HORIZONTAL_GRAVITY_MASK;
						int hgrav = lp.gravity & (int)GravityFlags.HorizontalGravityMask;//TODO: these int casts may cause bugs
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int vgrav = lp.gravity & android.view.Gravity.VERTICAL_GRAVITY_MASK;
						int vgrav = lp.gravity & (int)GravityFlags.VerticalGravityMask;
						var widthMode = Android.Views.MeasureSpecMode.AtMost;
						var heightMode = Android.Views.MeasureSpecMode.AtMost;
						bool consumeVertical = vgrav == (int)GravityFlags.Top || vgrav == (int)GravityFlags.Bottom;
						bool consumeHorizontal = hgrav == (int)GravityFlags.Left || hgrav == (int)GravityFlags.Right;

						if (consumeVertical)
						{
							widthMode = Android.Views.MeasureSpecMode.Exactly;
						}
						else if (consumeHorizontal)
						{
							heightMode = Android.Views.MeasureSpecMode.Exactly;
						}

						int widthSize = childWidthSize;
						int heightSize = childHeightSize;
						if (lp.Width != LayoutParams.WrapContent)
						{
							widthMode = Android.Views.MeasureSpecMode.Exactly;
							if (lp.Width != LayoutParams.FillParent)
							{
								widthSize = lp.Width;
							}
						}
						if (lp.Height != LayoutParams.WrapContent)
						{
							heightMode = Android.Views.MeasureSpecMode.Exactly;
							if (lp.Height != LayoutParams.FillParent)
							{
								heightSize = lp.Height;
							}
						}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int widthSpec = MeasureSpec.makeMeasureSpec(widthSize, widthMode);
						int widthSpec = MeasureSpec.MakeMeasureSpec(widthSize, widthMode);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int heightSpec = MeasureSpec.makeMeasureSpec(heightSize, heightMode);
						int heightSpec = MeasureSpec.MakeMeasureSpec(heightSize, heightMode);
						child.Measure(widthSpec, heightSpec);

						if (consumeVertical)
						{
							childHeightSize -= child.MeasuredHeight;
						}
						else if (consumeHorizontal)
						{
							childWidthSize -= child.MeasuredWidth;
						}
					}
				}
			}

			mChildWidthMeasureSpec = MeasureSpec.MakeMeasureSpec(childWidthSize, Android.Views.MeasureSpecMode.Exactly);
			mChildHeightMeasureSpec = MeasureSpec.MakeMeasureSpec(childHeightSize, Android.Views.MeasureSpecMode.Exactly);

			// Make sure we have created all fragments that we need to have shown.
			mInLayout = true;
			populate();
			mInLayout = false;

			// Page views next.
			size = ChildCount;
			for (int i = 0; i < size; ++i)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View child = getChildAt(i);
				View child = GetChildAt(i);
				if (child.Visibility != Android.Views.ViewStates.Gone)
				{
					if (DEBUG)
					{
						Log.Verbose(TAG, "Measuring #" + i + " " + child + ": " + mChildWidthMeasureSpec);
					}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LayoutParams lp = (LayoutParams) child.getLayoutParams();
					LayoutParams lp = (LayoutParams) child.LayoutParameters;
					if (lp == null || !lp.isDecor)
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int widthSpec = MeasureSpec.makeMeasureSpec((int)(childWidthSize * lp.widthFactor), MeasureSpec.EXACTLY);
						int widthSpec = MeasureSpec.MakeMeasureSpec((int)(childWidthSize * lp.widthFactor), Android.Views.MeasureSpecMode.Exactly);
						child.Measure(widthSpec, mChildHeightMeasureSpec);
					}
				}
			}
		}

		protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
		{
			base.OnSizeChanged(w, h, oldw, oldh);

			// Make sure scroll position is set correctly.
			if (w != oldw)
			{
				recomputeScrollPosition(w, oldw, mPageMargin, mPageMargin);
			}
		}

		private void recomputeScrollPosition(int width, int oldWidth, int margin, int oldMargin)
		{
			if (oldWidth > 0 && mItems.Count > 0)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int widthWithMargin = width - getPaddingLeft() - getPaddingRight() + margin;
				int widthWithMargin = width - PaddingLeft - PaddingRight + margin;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int oldWidthWithMargin = oldWidth - getPaddingLeft() - getPaddingRight() + oldMargin;
				int oldWidthWithMargin = oldWidth - PaddingLeft - PaddingRight + oldMargin;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int xpos = getScrollX();
				int xpos = ScrollX;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float pageOffset = (float) xpos / oldWidthWithMargin;
				float pageOffset = (float) xpos / oldWidthWithMargin;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int newOffsetPixels = (int)(pageOffset * widthWithMargin);
				int newOffsetPixels = (int)(pageOffset * widthWithMargin);

				ScrollTo(newOffsetPixels, ScrollY);
				if (!mScroller.IsFinished)
				{
					// We now return to your regularly scheduled scroll, already in progress.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int newDuration = mScroller.getDuration() - mScroller.timePassed();
					int newDuration = mScroller.Duration - mScroller.TimePassed();
					ItemInfo targetInfo = infoForPosition(mCurItem);
					mScroller.StartScroll(newOffsetPixels, 0, (int)(targetInfo.offset * width), 0, newDuration);
				}
			}
			else
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ItemInfo ii = infoForPosition(mCurItem);
				ItemInfo ii = infoForPosition(mCurItem);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float scrollOffset = ii != null ? Math.min(ii.offset, mLastOffset) : 0;
				float scrollOffset = ii != null ? Math.Min(ii.offset, mLastOffset) : 0;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int scrollPos = (int)(scrollOffset * (width - getPaddingLeft() - getPaddingRight()));
				int scrollPos = (int)(scrollOffset * (width - PaddingLeft - PaddingRight));
				if (scrollPos != ScrollX)
				{
					completeScroll(false);
					ScrollTo(scrollPos, ScrollY);
				}
			}
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int count = getChildCount();
			int count = ChildCount;
			int width = r - l;
			int height = b - t;
			int paddingLeft = PaddingLeft;
			int paddingTop = PaddingTop;
			int paddingRight = PaddingRight;
			int paddingBottom = PaddingBottom;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int scrollX = getScrollX();
			int scrollX = ScrollX;

			int decorCount = 0;

			// First pass - decor views. We need to do this in two passes so that
			// we have the proper offsets for non-decor views later.
			for (int i = 0; i < count; i++)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View child = getChildAt(i);
				View child = GetChildAt(i);
				if (child.Visibility != ViewStates.Gone)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LayoutParams lp = (LayoutParams) child.getLayoutParams();
					LayoutParams lp = (LayoutParams) child.LayoutParameters;
					int childLeft = 0;
					int childTop = 0;
					if (lp.isDecor)
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int hgrav = lp.gravity & android.view.Gravity.HORIZONTAL_GRAVITY_MASK;
						int hgrav = lp.gravity & (int)GravityFlags.HorizontalGravityMask;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int vgrav = lp.gravity & android.view.Gravity.VERTICAL_GRAVITY_MASK;
						int vgrav = lp.gravity & (int)GravityFlags.VerticalGravityMask;
						switch (hgrav)
						{
							default:
								childLeft = paddingLeft;
								break;
							case (int)GravityFlags.Left:
								childLeft = paddingLeft;
								paddingLeft += child.MeasuredWidth;
								break;
							case (int)GravityFlags.CenterHorizontal:
								childLeft = Math.Max((width - child.MeasuredWidth) / 2, paddingLeft);
								break;
							case (int)GravityFlags.Right:
								childLeft = width - paddingRight - child.MeasuredWidth;
								paddingRight += child.MeasuredWidth;
								break;
						}
						switch (vgrav)
						{
							default:
								childTop = paddingTop;
								break;
							case (int)GravityFlags.Top:
								childTop = paddingTop;
								paddingTop += child.MeasuredHeight;
								break;
							case (int)GravityFlags.CenterVertical:
								childTop = Math.Max((height - child.MeasuredHeight) / 2, paddingTop);
								break;
							case (int)GravityFlags.Bottom:
								childTop = height - paddingBottom - child.MeasuredHeight;
								paddingBottom += child.MeasuredHeight;
								break;
						}
						childLeft += scrollX;
						child.Layout(childLeft, childTop, childLeft + child.MeasuredWidth, childTop + child.MeasuredHeight);
						decorCount++;
					}
				}
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int childWidth = width - paddingLeft - paddingRight;
			int childWidth = width - paddingLeft - paddingRight;
			// Page views. Do this once we have the right padding offsets from above.
			for (int i = 0; i < count; i++)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View child = getChildAt(i);
				View child = GetChildAt(i);
				if (child.Visibility != ViewStates.Gone)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LayoutParams lp = (LayoutParams) child.getLayoutParams();
					LayoutParams lp = (LayoutParams) child.LayoutParameters;
					ItemInfo ii;
					if (!lp.isDecor && (ii = infoForChild(child)) != null)
					{
						int loff = (int)(childWidth * ii.offset);
						int childLeft = paddingLeft + loff;
						int childTop = paddingTop;
						if (lp.needsMeasure)
						{
							// This was added during layout and needs measurement.
							// Do it now that we know what we're working with.
							lp.needsMeasure = false;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int widthSpec = MeasureSpec.makeMeasureSpec((int)(childWidth * lp.widthFactor), MeasureSpec.EXACTLY);
							int widthSpec = MeasureSpec.MakeMeasureSpec((int)(childWidth * lp.widthFactor), MeasureSpecMode.Exactly);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int heightSpec = MeasureSpec.makeMeasureSpec((int)(height - paddingTop - paddingBottom), MeasureSpec.EXACTLY);
							int heightSpec = MeasureSpec.MakeMeasureSpec((int)(height - paddingTop - paddingBottom), MeasureSpecMode.Exactly);
							child.Measure(widthSpec, heightSpec);
						}
						if (DEBUG)
						{
							Log.Verbose(TAG, "Positioning #" + i + " " + child + " f=" + ii.@object + ":" + childLeft + "," + childTop + " " + child.MeasuredWidth + "x" + child.MeasuredHeight);
						}
						child.Layout(childLeft, childTop, childLeft + child.MeasuredWidth, childTop + child.MeasuredHeight);
					}
				}
			}
			mTopPageBounds = paddingTop;
			mBottomPageBounds = height - paddingBottom;
			mDecorChildCount = decorCount;

			if (mFirstLayout)
			{
				scrollToItem(mCurItem, false, 0, false);
			}
			mFirstLayout = false;
		}

		public override void ComputeScroll()
		{
			if (!mScroller.IsFinished && mScroller.ComputeScrollOffset())
			{
				int oldX = ScrollX;
				int oldY = ScrollY;
				int x = mScroller.CurrX;
				int y = mScroller.CurrY;

				if (oldX != x || oldY != y)
				{
					ScrollTo(x, y);
					if (!pageScrolled(x))
					{
						mScroller.AbortAnimation();
						ScrollTo(0, y);
					}
				}

				// Keep on drawing until the animation has finished.
				ViewCompat.PostInvalidateOnAnimation(this);
				return;
			}

			// Done with scroll, clean up state.
			completeScroll(true);
		}

		private bool pageScrolled(int xpos)
		{
			if (mItems.Count == 0)
			{
				mCalledSuper = false;
				onPageScrolled(0, 0, 0);
				if (!mCalledSuper)
				{
					throw new Java.Lang.IllegalStateException("onPageScrolled did not call superclass implementation");
				}
				return false;
			}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ItemInfo ii = infoForCurrentScrollPosition();
			ItemInfo ii = infoForCurrentScrollPosition();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width = getClientWidth();
			int width = ClientWidth;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int widthWithMargin = width + mPageMargin;
			int widthWithMargin = width + mPageMargin;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float marginOffset = (float) mPageMargin / width;
			float marginOffset = (float) mPageMargin / width;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int currentPage = ii.position;
			int currentPage = ii.position;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float pageOffset = (((float) xpos / width) - ii.offset) / (ii.widthFactor + marginOffset);
			float pageOffset = (((float) xpos / width) - ii.offset) / (ii.widthFactor + marginOffset);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int offsetPixels = (int)(pageOffset * widthWithMargin);
			int offsetPixels = (int)(pageOffset * widthWithMargin);

			mCalledSuper = false;
			onPageScrolled(currentPage, pageOffset, offsetPixels);
			if (!mCalledSuper)
			{
				throw new Java.Lang.IllegalStateException("onPageScrolled did not call superclass implementation");
			}
			return true;
		}

		/// <summary>
		/// This method will be invoked when the current page is scrolled, either as part
		/// of a programmatically initiated smooth scroll or a user initiated touch scroll.
		/// If you override this method you must call through to the superclass implementation
		/// (e.g. super.onPageScrolled(position, offset, offsetPixels)) before onPageScrolled
		/// returns.
		/// </summary>
		/// <param name="position"> Position index of the first page currently being displayed.
		///                 Page position+1 will be visible if positionOffset is nonzero. </param>
		/// <param name="offset"> Value from [0, 1) indicating the offset from the page at position. </param>
		/// <param name="offsetPixels"> Value in pixels indicating the offset from position. </param>
		protected internal virtual void onPageScrolled(int position, float offset, int offsetPixels)
		{
			// Offset any decor views if needed - keep them on-screen at all times.
			if (mDecorChildCount > 0)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int scrollX = getScrollX();
				int scrollX = ScrollX;
				int paddingLeft = PaddingLeft;
				int paddingRight = PaddingRight;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width = getWidth();
				int width = Width;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int childCount = getChildCount();
				int childCount = ChildCount;
				for (int i = 0; i < childCount; i++)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View child = getChildAt(i);
					View child = GetChildAt(i);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LayoutParams lp = (LayoutParams) child.getLayoutParams();
					LayoutParams lp = (LayoutParams) child.LayoutParameters;
					if (!lp.isDecor)
					{
						continue;
					}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int hgrav = lp.gravity & android.view.Gravity.HORIZONTAL_GRAVITY_MASK;
					int hgrav = lp.gravity & (int)GravityFlags.HorizontalGravityMask;
					int childLeft = 0;
					switch (hgrav)
					{
						default:
							childLeft = paddingLeft;
							break;
						case (int)GravityFlags.Left:
							childLeft = paddingLeft;
							paddingLeft += child.Width;
							break;
						case (int)GravityFlags.CenterHorizontal:
							childLeft = Math.Max((width - child.MeasuredWidth) / 2, paddingLeft);
							break;
						case (int)GravityFlags.Right:
							childLeft = width - paddingRight - child.MeasuredWidth;
							paddingRight += child.MeasuredWidth;
							break;
					}
					childLeft += scrollX;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int childOffset = childLeft - child.getLeft();
					int childOffset = childLeft - child.Left;
					if (childOffset != 0)
					{
						child.OffsetLeftAndRight(childOffset);
					}
				}
			}
			foreach (OnPageChangeListener eachListener in mOnPageChangeListeners)
			{
				if (eachListener != null)
				{
					eachListener.onPageScrolled(position, offset, offsetPixels);
				}
			}
			if (mInternalPageChangeListener != null)
			{
				mInternalPageChangeListener.onPageScrolled(position, offset, offsetPixels);
			}

			if (mPageTransformer != null)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int scrollX = getScrollX();
				int scrollX = ScrollX;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int childCount = getChildCount();
				int childCount = ChildCount;
				for (int i = 0; i < childCount; i++)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View child = getChildAt(i);
					View child = GetChildAt(i);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LayoutParams lp = (LayoutParams) child.getLayoutParams();
					LayoutParams lp = (LayoutParams) child.LayoutParameters;

					if (lp.isDecor)
					{
						continue;
					}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float transformPos = (float)(child.getLeft() - scrollX) / getClientWidth();
					float transformPos = (float)(child.Left - scrollX) / ClientWidth;
					mPageTransformer.transformPage(child, transformPos);
				}
			}

			mCalledSuper = true;
		}

		private void completeScroll(bool postEvents)
		{
			bool needPopulate = mScrollState == SCROLL_STATE_SETTLING;
			if (needPopulate)
			{
				// Done with scroll, no longer want to cache view drawing.
				ScrollingCacheEnabled = false;
				mScroller.AbortAnimation();
				int oldX = ScrollX;
				int oldY = ScrollY;
				int x = mScroller.CurrX;
				int y = mScroller.CurrY;
				if (oldX != x || oldY != y)
				{
					ScrollTo(x, y);
				}
			}
			mPopulatePending = false;
			for (int i = 0; i < mItems.Count; i++)
			{
				ItemInfo ii = mItems[i];
				if (ii.scrolling)
				{
					needPopulate = true;
					ii.scrolling = false;
				}
			}
			if (needPopulate)
			{
				if (postEvents)
				{
					ViewCompat.PostOnAnimation(this, mEndScrollRunnable);
				}
				else
				{
					mEndScrollRunnable.Run();
				}
			}
		}

		private bool isGutterDrag(float x, float dx)
		{
			return (x < mGutterSize && dx > 0) || (x > Width - mGutterSize && dx < 0);
		}

		private void enableLayers(bool enable)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int childCount = getChildCount();
			int childCount = ChildCount;
			for (int i = 0; i < childCount; i++)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int layerType = enable ? android.support.v4.view.ViewCompat.LAYER_TYPE_HARDWARE : android.support.v4.view.ViewCompat.LAYER_TYPE_NONE;
				int layerType = enable ? ViewCompat.LayerTypeHardware : ViewCompat.LayerTypeNone;
				ViewCompat.SetLayerType(GetChildAt(i), layerType, null);
			}
		}

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			/*
			 * This method JUST determines whether we want to intercept the motion.
			 * If we return true, onMotionEvent will be called and we do the actual
			 * scrolling there.
			 */

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int action = ev.getAction() & android.support.v4.view.MotionEventCompat.ACTION_MASK;
			int action = (int)ev.Action & MotionEventCompat.ActionMask;

			// Always take care of the touch gesture being complete.
			if (action == (int)MotionEventActions.Cancel || action == (int)MotionEventActions.Up)
			{
				// Release the drag.
				if (DEBUG)
				{
					Log.Verbose(TAG, "Intercept done!");
				}
				mIsBeingDragged = false;
				mIsUnableToDrag = false;
				mActivePointerId = INVALID_POINTER;
				if (mVelocityTracker != null)
				{
					mVelocityTracker.Recycle();
					mVelocityTracker = null;
				}
				return false;
			}

			// Nothing more to do here if we have decided whether or not we
			// are dragging.
			if (action != (int)MotionEventActions.Down)
			{
				if (mIsBeingDragged)
				{
					if (DEBUG)
					{
						Log.Verbose(TAG, "Intercept returning true!");
					}
					return true;
				}
				if (mIsUnableToDrag)
				{
					if (DEBUG)
					{
						Log.Verbose(TAG, "Intercept returning false!");
					}
					return false;
				}
			}

			switch (action)
			{
				case (int)MotionEventActions.Move:
				{
					/*
					 * mIsBeingDragged == false, otherwise the shortcut would have caught it. Check
					 * whether the user has moved far enough from his original down touch.
					 */

					/*
					* Locally do absolute value. mLastMotionY is set to the y value
					* of the down event.
					*/
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int activePointerId = mActivePointerId;
					int activePointerId = mActivePointerId;
					if (activePointerId == INVALID_POINTER)
					{
						// If we don't have a valid id, the touch down wasn't on content.
						break;
					}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int pointerIndex = android.support.v4.view.MotionEventCompat.findPointerIndex(ev, activePointerId);
					int pointerIndex = MotionEventCompat.FindPointerIndex(ev, activePointerId);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float x = android.support.v4.view.MotionEventCompat.getX(ev, pointerIndex);
					float x = MotionEventCompat.GetX(ev, pointerIndex);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float dx = x - mLastMotionX;
					float dx = x - mLastMotionX;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float xDiff = Math.abs(dx);
					float xDiff = Math.Abs(dx);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float y = android.support.v4.view.MotionEventCompat.getY(ev, pointerIndex);
					float y = MotionEventCompat.GetY(ev, pointerIndex);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float yDiff = Math.abs(y - mInitialMotionY);
					float yDiff = Math.Abs(y - mInitialMotionY);
					if (DEBUG)
					{
						Log.Verbose(TAG, "Moved x to " + x + "," + y + " diff=" + xDiff + "," + yDiff);
					}

					if (dx != 0 && !isGutterDrag(mLastMotionX, dx) && canScroll(this, false, (int) dx, (int) x, (int) y))
					{
						// Nested view has scrollable area under this point. Let it be handled there.
						mLastMotionX = x;
						mLastMotionY = y;
						mIsUnableToDrag = true;
						return false;
					}
					if (xDiff > mTouchSlop && xDiff * 0.5f > yDiff)
					{
						if (DEBUG)
						{
							Log.Verbose(TAG, "Starting drag!");
						}
						mIsBeingDragged = true;
						requestParentDisallowInterceptTouchEvent(true);
						ScrollState = SCROLL_STATE_DRAGGING;
						mLastMotionX = dx > 0 ? mInitialMotionX + mTouchSlop : mInitialMotionX - mTouchSlop;
						mLastMotionY = y;
						ScrollingCacheEnabled = true;
					}
					else if (yDiff > mTouchSlop)
					{
						// The finger has moved enough in the vertical
						// direction to be counted as a drag...  abort
						// any attempt to drag horizontally, to work correctly
						// with children that have scrolling containers.
						if (DEBUG)
						{
							Log.Verbose(TAG, "Starting unable to drag!");
						}
						mIsUnableToDrag = true;
					}
					if (mIsBeingDragged)
					{
						// Scroll to follow the motion event
						if (performDrag(x))
						{
							ViewCompat.PostInvalidateOnAnimation(this);
						}
					}
					break;
				}

				case (int)MotionEventActions.Down:
				{
					/*
					 * Remember location of down touch.
					 * ACTION_DOWN always refers to pointer index 0.
					 */
					mLastMotionX = mInitialMotionX = ev.GetX();
					mLastMotionY = mInitialMotionY = ev.GetY();
					mActivePointerId = MotionEventCompat.GetPointerId(ev, 0);
					mIsUnableToDrag = false;

					mScroller.ComputeScrollOffset();
					if (mScrollState == SCROLL_STATE_SETTLING && Math.Abs(mScroller.FinalX - mScroller.CurrX) > mCloseEnough)
					{
						// Let the user 'catch' the pager as it animates.
						mScroller.AbortAnimation();
						mPopulatePending = false;
						populate();
						mIsBeingDragged = true;
						requestParentDisallowInterceptTouchEvent(true);
						ScrollState = SCROLL_STATE_DRAGGING;
					}
					else
					{
						completeScroll(false);
						mIsBeingDragged = false;
					}

					if (DEBUG)
					{
						Log.Verbose(TAG, "Down at " + mLastMotionX + "," + mLastMotionY + " mIsBeingDragged=" + mIsBeingDragged + "mIsUnableToDrag=" + mIsUnableToDrag);
					}
					break;
				}

				case MotionEventCompat.ActionPointerUp:
					onSecondaryPointerUp(ev);
					break;
			}

			if (mVelocityTracker == null)
			{
				mVelocityTracker = VelocityTracker.Obtain();
			}
			mVelocityTracker.AddMovement(ev);

			/*
			 * The only time we want to intercept motion events is if we are in the
			 * drag mode.
			 */
			return mIsBeingDragged;
		}

		public override bool OnTouchEvent(MotionEvent ev)
		{
			if (mFakeDragging)
			{
				// A fake drag is in progress already, ignore this real one
				// but still eat the touch events.
				// (It is likely that the user is multi-touching the screen.)
				return true;
			}

			if (ev.Action == MotionEventActions.Down && ev.EdgeFlags != 0)
			{
				// Don't handle edge touches immediately -- they may actually belong to one of our
				// descendants.
				return false;
			}

			if (mAdapter == null || mAdapter.Count == 0)
			{
				// Nothing to present or scroll; nothing to touch.
				return false;
			}

			if (mVelocityTracker == null)
			{
				mVelocityTracker = VelocityTracker.Obtain();
			}
			mVelocityTracker.AddMovement(ev);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int action = ev.getAction();
			var action = (int)ev.Action;
			bool needsInvalidate = false;

			switch (action & MotionEventCompat.ActionMask)
			{
				case (int)MotionEventActions.Down:
				{
					mScroller.AbortAnimation();
					mPopulatePending = false;
					populate();

					// Remember where the motion event started
					mLastMotionX = mInitialMotionX = ev.GetX();
					mLastMotionY = mInitialMotionY = ev.GetY();
					mActivePointerId = MotionEventCompat.GetPointerId(ev, 0);
					break;
				}
				case (int)MotionEventActions.Move:
					if (!mIsBeingDragged)
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int pointerIndex = android.support.v4.view.MotionEventCompat.findPointerIndex(ev, mActivePointerId);
						int pointerIndex = MotionEventCompat.FindPointerIndex(ev, mActivePointerId);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float x = android.support.v4.view.MotionEventCompat.getX(ev, pointerIndex);
						float x = MotionEventCompat.GetX(ev, pointerIndex);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float xDiff = Math.abs(x - mLastMotionX);
						float xDiff = Math.Abs(x - mLastMotionX);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float y = android.support.v4.view.MotionEventCompat.getY(ev, pointerIndex);
						float y = MotionEventCompat.GetY(ev, pointerIndex);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float yDiff = Math.abs(y - mLastMotionY);
						float yDiff = Math.Abs(y - mLastMotionY);
						if (DEBUG)
						{
							Log.Verbose(TAG, "Moved x to " + x + "," + y + " diff=" + xDiff + "," + yDiff);
						}
						if (xDiff > mTouchSlop && xDiff > yDiff)
						{
							if (DEBUG)
							{
								Log.Verbose(TAG, "Starting drag!");
							}
							mIsBeingDragged = true;
							requestParentDisallowInterceptTouchEvent(true);
							mLastMotionX = x - mInitialMotionX > 0 ? mInitialMotionX + mTouchSlop : mInitialMotionX - mTouchSlop;
							mLastMotionY = y;
							ScrollState = SCROLL_STATE_DRAGGING;
							ScrollingCacheEnabled = true;

							// Disallow Parent Intercept, just in case
							IViewParent parent = Parent;
							if (parent != null)
							{
								parent.RequestDisallowInterceptTouchEvent(true);
							}
						}
					}
					// Not else! Note that mIsBeingDragged can be set above.
					if (mIsBeingDragged)
					{
						// Scroll to follow the motion event
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int activePointerIndex = android.support.v4.view.MotionEventCompat.findPointerIndex(ev, mActivePointerId);
						int activePointerIndex = MotionEventCompat.FindPointerIndex(ev, mActivePointerId);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float x = android.support.v4.view.MotionEventCompat.getX(ev, activePointerIndex);
						float x = MotionEventCompat.GetX(ev, activePointerIndex);
						needsInvalidate |= performDrag(x);
					}
					break;
				case (int)MotionEventActions.Up:
					if (mIsBeingDragged)
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.VelocityTracker velocityTracker = mVelocityTracker;
						VelocityTracker velocityTracker = mVelocityTracker;
						velocityTracker.ComputeCurrentVelocity(1000, mMaximumVelocity);
						int initialVelocity = (int) VelocityTrackerCompat.GetXVelocity(velocityTracker, mActivePointerId);
						mPopulatePending = true;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width = getClientWidth();
						int width = ClientWidth;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int scrollX = getScrollX();
						int scrollX = ScrollX;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ItemInfo ii = infoForCurrentScrollPosition();
						ItemInfo ii = infoForCurrentScrollPosition();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int currentPage = ii.position;
						int currentPage = ii.position;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float pageOffset = (((float) scrollX / width) - ii.offset) / ii.widthFactor;
						float pageOffset = (((float) scrollX / width) - ii.offset) / ii.widthFactor;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int activePointerIndex = android.support.v4.view.MotionEventCompat.findPointerIndex(ev, mActivePointerId);
						int activePointerIndex = MotionEventCompat.FindPointerIndex(ev, mActivePointerId);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float x = android.support.v4.view.MotionEventCompat.getX(ev, activePointerIndex);
						float x = MotionEventCompat.GetX(ev, activePointerIndex);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int totalDelta = (int)(x - mInitialMotionX);
						int totalDelta = (int)(x - mInitialMotionX);
						int nextPage = determineTargetPage(currentPage, pageOffset, initialVelocity, totalDelta);
						setCurrentItemInternal(nextPage, true, true, initialVelocity);

						mActivePointerId = INVALID_POINTER;
						endDrag();
						needsInvalidate = mLeftEdge.OnRelease() | mRightEdge.OnRelease();
					}
					break;
				case (int)MotionEventActions.Cancel:
					if (mIsBeingDragged)
					{
						scrollToItem(mCurItem, true, 0, false);
						mActivePointerId = INVALID_POINTER;
						endDrag();
						needsInvalidate = mLeftEdge.OnRelease() | mRightEdge.OnRelease();
					}
					break;
				case MotionEventCompat.ActionPointerDown:
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int index = android.support.v4.view.MotionEventCompat.getActionIndex(ev);
					int index = MotionEventCompat.GetActionIndex(ev);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float x = android.support.v4.view.MotionEventCompat.getX(ev, index);
					float x = MotionEventCompat.GetX(ev, index);
					mLastMotionX = x;
					mActivePointerId = MotionEventCompat.GetPointerId(ev, index);
					break;
				}
				case MotionEventCompat.ActionPointerUp:
					onSecondaryPointerUp(ev);
					mLastMotionX = MotionEventCompat.GetX(ev, MotionEventCompat.FindPointerIndex(ev, mActivePointerId));
					break;
			}
			if (needsInvalidate)
			{
				ViewCompat.PostInvalidateOnAnimation(this);
			}
			return true;
		}

		private void requestParentDisallowInterceptTouchEvent(bool disallowIntercept)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.ViewParent parent = getParent();
			IViewParent parent = Parent;
			if (parent != null)
			{
				parent.RequestDisallowInterceptTouchEvent(disallowIntercept);
			}
		}

		private bool performDrag(float x)
		{
			bool needsInvalidate = false;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float deltaX = mLastMotionX - x;
			float deltaX = mLastMotionX - x;
			mLastMotionX = x;

			float oldScrollX = ScrollX;
			float scrollX = oldScrollX + deltaX;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width = getClientWidth();
			int width = ClientWidth;

			float leftBound = width * mFirstOffset;
			float rightBound = width * mLastOffset;
			bool leftAbsolute = true;
			bool rightAbsolute = true;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ItemInfo firstItem = mItems.get(0);
			ItemInfo firstItem = mItems[0];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ItemInfo lastItem = mItems.get(mItems.size() - 1);
			ItemInfo lastItem = mItems[mItems.Count - 1];
			if (firstItem.position != 0)
			{
				leftAbsolute = false;
				leftBound = firstItem.offset * width;
			}
			if (lastItem.position != mAdapter.Count - 1)
			{
				rightAbsolute = false;
				rightBound = lastItem.offset * width;
			}

			if (scrollX < leftBound)
			{
				if (leftAbsolute)
				{
					float over = leftBound - scrollX;
					needsInvalidate = mLeftEdge.OnPull(Math.Abs(over) / width);
				}
				scrollX = leftBound;
			}
			else if (scrollX > rightBound)
			{
				if (rightAbsolute)
				{
					float over = scrollX - rightBound;
					needsInvalidate = mRightEdge.OnPull(Math.Abs(over) / width);
				}
				scrollX = rightBound;
			}
			// Don't lose the rounded component
			mLastMotionX += scrollX - (int) scrollX;
			ScrollTo((int) scrollX, ScrollY);
			pageScrolled((int) scrollX);

			return needsInvalidate;
		}

		/// <returns> Info about the page at the current scroll position.
		///         This can be synthetic for a missing middle page; the 'object' field can be null. </returns>
		private ItemInfo infoForCurrentScrollPosition()
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width = getClientWidth();
			int width = ClientWidth;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float scrollOffset = width > 0 ? (float) getScrollX() / width : 0;
			float scrollOffset = width > 0 ? (float) ScrollX / width : 0;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float marginOffset = width > 0 ? (float) mPageMargin / width : 0;
			float marginOffset = width > 0 ? (float) mPageMargin / width : 0;
			int lastPos = -1;
			float lastOffset = 0.0f;
			float lastWidth = 0.0f;
			bool first = true;

			ItemInfo lastItem = null;
			for (int i = 0; i < mItems.Count; i++)
			{
				ItemInfo ii = mItems[i];
				float offset;
				if (!first && ii.position != lastPos + 1)
				{
					// Create a synthetic item for a missing page.
					ii = mTempItem;
					ii.offset = lastOffset + lastWidth + marginOffset;
					ii.position = lastPos + 1;
					ii.widthFactor = mAdapter.GetPageWidth(ii.position);
					i--;
				}
				offset = ii.offset;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float leftBound = offset;
				float leftBound = offset;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float rightBound = offset + ii.widthFactor + marginOffset;
				float rightBound = offset + ii.widthFactor + marginOffset;
				if (first || scrollOffset >= leftBound)
				{
					if (scrollOffset < rightBound || i == mItems.Count - 1)
					{
						return ii;
					}
				}
				else
				{
					return lastItem;
				}
				first = false;
				lastPos = ii.position;
				lastOffset = offset;
				lastWidth = ii.widthFactor;
				lastItem = ii;
			}

			return lastItem;
		}

		private int determineTargetPage(int currentPage, float pageOffset, int velocity, int deltaX)
		{
			int targetPage;
			if (Math.Abs(deltaX) > mFlingDistance && Math.Abs(velocity) > mMinimumVelocity)
			{
				targetPage = velocity > 0 ? currentPage : currentPage + 1;
			}
			else
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float truncator = currentPage >= mCurItem ? 0.4f : 0.6f;
				float truncator = currentPage >= mCurItem ? 0.4f : 0.6f;
				targetPage = (int)(currentPage + pageOffset + truncator);
			}

			if (mItems.Count > 0)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ItemInfo firstItem = mItems.get(0);
				ItemInfo firstItem = mItems[0];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ItemInfo lastItem = mItems.get(mItems.size() - 1);
				ItemInfo lastItem = mItems[mItems.Count - 1];

				// Only let the user target pages we have items for
				targetPage = Math.Max(firstItem.position, Math.Min(targetPage, lastItem.position));
			}

			return targetPage;
		}

		public override void Draw(Canvas canvas)
		{
			base.Draw(canvas);
			bool needsInvalidate = false;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int overScrollMode = android.support.v4.view.ViewCompat.getOverScrollMode(this);
			int overScrollMode = ViewCompat.GetOverScrollMode(this);
			if (overScrollMode == ViewCompat.OverScrollAlways || (overScrollMode == ViewCompat.OverScrollIfContentScrolls && mAdapter != null && mAdapter.Count > 1))
			{
				if (!mLeftEdge.IsFinished)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int restoreCount = canvas.save();
					int restoreCount = canvas.Save();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int height = getHeight() - getPaddingTop() - getPaddingBottom();
					int height = Height - PaddingTop - PaddingBottom;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width = getWidth();
					int width = Width;

					canvas.Rotate(270);
					canvas.Translate(-height + PaddingTop, mFirstOffset * width);
					mLeftEdge.SetSize(height, width);
					needsInvalidate |= mLeftEdge.Draw(canvas);
					canvas.RestoreToCount(restoreCount);
				}
				if (!mRightEdge.IsFinished)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int restoreCount = canvas.save();
					int restoreCount = canvas.Save();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width = getWidth();
					int width = Width;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int height = getHeight() - getPaddingTop() - getPaddingBottom();
					int height = Height - PaddingTop - PaddingBottom;

					canvas.Rotate(90);
					canvas.Translate(-PaddingTop, -(mLastOffset + 1) * width);
					mRightEdge.SetSize(height, width);
					needsInvalidate |= mRightEdge.Draw(canvas);
					canvas.RestoreToCount(restoreCount);
				}
			}
			else
			{
				mLeftEdge.Finish();
				mRightEdge.Finish();
			}

			if (needsInvalidate)
			{
				// Keep animating
				ViewCompat.PostInvalidateOnAnimation(this);
			}
		}

		protected override void OnDraw(Canvas canvas)
		{
			base.OnDraw(canvas);

			// Draw the margin drawable between pages if needed.
			if (mPageMargin > 0 && mMarginDrawable != null && mItems.Count > 0 && mAdapter != null)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int scrollX = getScrollX();
				int scrollX = ScrollX;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width = getWidth();
				int width = Width;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float marginOffset = (float) mPageMargin / width;
				float marginOffset = (float) mPageMargin / width;
				int itemIndex = 0;
				ItemInfo ii = mItems[0];
				float offset = ii.offset;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int itemCount = mItems.size();
				int itemCount = mItems.Count;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int firstPos = ii.position;
				int firstPos = ii.position;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int lastPos = mItems.get(itemCount - 1).position;
				int lastPos = mItems[itemCount - 1].position;
				for (int pos = firstPos; pos < lastPos; pos++)
				{
					while (pos > ii.position && itemIndex < itemCount)
					{
						ii = mItems[++itemIndex];
					}

					float drawAt;
					if (pos == ii.position)
					{
						drawAt = (ii.offset + ii.widthFactor) * width;
						offset = ii.offset + ii.widthFactor + marginOffset;
					}
					else
					{
						float widthFactor = mAdapter.GetPageWidth(pos);
						drawAt = (offset + widthFactor) * width;
						offset += widthFactor + marginOffset;
					}

					if (drawAt + mPageMargin > scrollX)
					{
						mMarginDrawable.SetBounds((int) drawAt, mTopPageBounds, (int)(drawAt + mPageMargin + 0.5f), mBottomPageBounds);
						mMarginDrawable.Draw(canvas);
					}

					if (drawAt > scrollX + width)
					{
						break; // No more visible, no sense in continuing
					}
				}
			}
		}

		/// <summary>
		/// Start a fake drag of the pager.
		/// 
		/// <para>A fake drag can be useful if you want to synchronize the motion of the ViewPager
		/// with the touch scrolling of another view, while still letting the ViewPager
		/// control the snapping motion and fling behavior. (e.g. parallax-scrolling tabs.)
		/// Call <seealso cref="#fakeDragBy(float)"/> to simulate the actual drag motion. Call
		/// <seealso cref="#endFakeDrag()"/> to complete the fake drag and fling as necessary.
		/// 
		/// </para>
		/// <para>During a fake drag the ViewPager will ignore all touch events. If a real drag
		/// is already in progress, this method will return false.
		/// 
		/// </para>
		/// </summary>
		/// <returns> true if the fake drag began successfully, false if it could not be started.
		/// </returns>
		/// <seealso cref= #fakeDragBy(float) </seealso>
		/// <seealso cref= #endFakeDrag() </seealso>
		public virtual bool beginFakeDrag()
		{
			if (mIsBeingDragged)
			{
				return false;
			}
			mFakeDragging = true;
			ScrollState = SCROLL_STATE_DRAGGING;
			mInitialMotionX = mLastMotionX = 0;
			if (mVelocityTracker == null)
			{
				mVelocityTracker = VelocityTracker.Obtain();
			}
			else
			{
				mVelocityTracker.Clear();
			}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long time = android.os.SystemClock.uptimeMillis();
			long time = SystemClock.UptimeMillis();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.MotionEvent ev = android.view.MotionEvent.obtain(time, time, android.view.MotionEvent.ACTION_DOWN, 0, 0, 0);
			MotionEvent ev = MotionEvent.Obtain(time, time, MotionEventActions.Down, 0, 0, 0);
			mVelocityTracker.AddMovement(ev);
			ev.Recycle();
			mFakeDragBeginTime = time;
			return true;
		}

		/// <summary>
		/// End a fake drag of the pager.
		/// </summary>
		/// <seealso cref= #beginFakeDrag() </seealso>
		/// <seealso cref= #fakeDragBy(float) </seealso>
		public virtual void endFakeDrag()
		{
			if (!mFakeDragging)
			{
				throw new Java.Lang.IllegalStateException("No fake drag in progress. Call beginFakeDrag first.");
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.VelocityTracker velocityTracker = mVelocityTracker;
			VelocityTracker velocityTracker = mVelocityTracker;
			velocityTracker.ComputeCurrentVelocity(1000, mMaximumVelocity);
			int initialVelocity = (int) VelocityTrackerCompat.GetXVelocity(velocityTracker, mActivePointerId);
			mPopulatePending = true;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width = getClientWidth();
			int width = ClientWidth;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int scrollX = getScrollX();
			int scrollX = ScrollX;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ItemInfo ii = infoForCurrentScrollPosition();
			ItemInfo ii = infoForCurrentScrollPosition();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int currentPage = ii.position;
			int currentPage = ii.position;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float pageOffset = (((float) scrollX / width) - ii.offset) / ii.widthFactor;
			float pageOffset = (((float) scrollX / width) - ii.offset) / ii.widthFactor;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int totalDelta = (int)(mLastMotionX - mInitialMotionX);
			int totalDelta = (int)(mLastMotionX - mInitialMotionX);
			int nextPage = determineTargetPage(currentPage, pageOffset, initialVelocity, totalDelta);
			setCurrentItemInternal(nextPage, true, true, initialVelocity);
			endDrag();

			mFakeDragging = false;
		}

		/// <summary>
		/// Fake drag by an offset in pixels. You must have called <seealso cref="#beginFakeDrag()"/> first.
		/// </summary>
		/// <param name="xOffset"> Offset in pixels to drag by. </param>
		/// <seealso cref= #beginFakeDrag() </seealso>
		/// <seealso cref= #endFakeDrag() </seealso>
		public virtual void fakeDragBy(float xOffset)
		{
			if (!mFakeDragging)
			{
				throw new Java.Lang.IllegalStateException("No fake drag in progress. Call beginFakeDrag first.");
			}

			mLastMotionX += xOffset;

			float oldScrollX = ScrollX;
			float scrollX = oldScrollX - xOffset;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width = getClientWidth();
			int width = ClientWidth;

			float leftBound = width * mFirstOffset;
			float rightBound = width * mLastOffset;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ItemInfo firstItem = mItems.get(0);
			ItemInfo firstItem = mItems[0];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ItemInfo lastItem = mItems.get(mItems.size() - 1);
			ItemInfo lastItem = mItems[mItems.Count - 1];
			if (firstItem.position != 0)
			{
				leftBound = firstItem.offset * width;
			}
			if (lastItem.position != mAdapter.Count - 1)
			{
				rightBound = lastItem.offset * width;
			}

			if (scrollX < leftBound)
			{
				scrollX = leftBound;
			}
			else if (scrollX > rightBound)
			{
				scrollX = rightBound;
			}
			// Don't lose the rounded component
			mLastMotionX += scrollX - (int) scrollX;
			ScrollTo((int) scrollX, ScrollY);
			pageScrolled((int) scrollX);

			// Synthesize an event for the VelocityTracker.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long time = android.os.SystemClock.uptimeMillis();
			long time = SystemClock.UptimeMillis();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.MotionEvent ev = android.view.MotionEvent.obtain(mFakeDragBeginTime, time, android.view.MotionEvent.ACTION_MOVE, mLastMotionX, 0, 0);
			MotionEvent ev = MotionEvent.Obtain(mFakeDragBeginTime, time, MotionEventActions.Move, mLastMotionX, 0, 0);
			mVelocityTracker.AddMovement(ev);
			ev.Recycle();
		}

		/// <summary>
		/// Returns true if a fake drag is in progress.
		/// </summary>
		/// <returns> true if currently in a fake drag, false otherwise.
		/// </returns>
		/// <seealso cref= #beginFakeDrag() </seealso>
		/// <seealso cref= #fakeDragBy(float) </seealso>
		/// <seealso cref= #endFakeDrag() </seealso>
		public virtual bool FakeDragging
		{
			get
			{
				return mFakeDragging;
			}
		}

		private void onSecondaryPointerUp(MotionEvent ev)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int pointerIndex = android.support.v4.view.MotionEventCompat.getActionIndex(ev);
			int pointerIndex = MotionEventCompat.GetActionIndex(ev);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int pointerId = android.support.v4.view.MotionEventCompat.getPointerId(ev, pointerIndex);
			int pointerId = MotionEventCompat.GetPointerId(ev, pointerIndex);
			if (pointerId == mActivePointerId)
			{
				// This was our active pointer going up. Choose a new
				// active pointer and adjust accordingly.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int newPointerIndex = pointerIndex == 0 ? 1 : 0;
				int newPointerIndex = pointerIndex == 0 ? 1 : 0;
				mLastMotionX = MotionEventCompat.GetX(ev, newPointerIndex);
				mActivePointerId = MotionEventCompat.GetPointerId(ev, newPointerIndex);
				if (mVelocityTracker != null)
				{
					mVelocityTracker.Clear();
				}
			}
		}

		private void endDrag()
		{
			mIsBeingDragged = false;
			mIsUnableToDrag = false;

			if (mVelocityTracker != null)
			{
				mVelocityTracker.Recycle();
				mVelocityTracker = null;
			}
		}

		private bool ScrollingCacheEnabled
		{
			set
			{
				if (mScrollingCacheEnabled != value)
				{
					mScrollingCacheEnabled = value;
					if (USE_CACHE)
					{
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final int size = getChildCount();
						int size = ChildCount;
						for (int i = 0; i < size; ++i)
						{
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final android.view.View child = getChildAt(i);
							View child = GetChildAt(i);
							if (child.Visibility != ViewStates.Gone)
							{
								child.DrawingCacheEnabled = value;
							}
						}
					}
				}
			}
		}

		public virtual bool canScrollHorizontally(int direction)
		{
			if (mAdapter == null)
			{
				return false;
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width = getClientWidth();
			int width = ClientWidth;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int scrollX = getScrollX();
			int scrollX = ScrollX;
			if (direction < 0)
			{
				return (scrollX > (int)(width * mFirstOffset));
			}
			else if (direction > 0)
			{
				return (scrollX < (int)(width * mLastOffset));
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Tests scrollability within child views of v given a delta of dx.
		/// </summary>
		/// <param name="v"> View to test for horizontal scrollability </param>
		/// <param name="checkV"> Whether the view v passed should itself be checked for scrollability (true),
		///               or just its children (false). </param>
		/// <param name="dx"> Delta scrolled in pixels </param>
		/// <param name="x"> X coordinate of the active touch point </param>
		/// <param name="y"> Y coordinate of the active touch point </param>
		/// <returns> true if child views of v can be scrolled by delta of dx. </returns>
		protected internal virtual bool canScroll(View v, bool checkV, int dx, int x, int y)
		{
			if (v is ViewGroup)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.ViewGroup group = (android.view.ViewGroup) v;
				ViewGroup group = (ViewGroup) v;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int scrollX = v.getScrollX();
				int scrollX = v.ScrollX;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int scrollY = v.getScrollY();
				int scrollY = v.ScrollY;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int count = group.getChildCount();
				int count = group.ChildCount;
				// Count backwards - let topmost views consume scroll distance first.
				for (int i = count - 1; i >= 0; i--)
				{
					// TODO: Add versioned support here for transformed views.
					// This will not work for transformed views in Honeycomb+
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View child = group.getChildAt(i);
					View child = group.GetChildAt(i);
					if (x + scrollX >= child.Left && x + scrollX < child.Right && y + scrollY >= child.Top && y + scrollY < child.Bottom && canScroll(child, true, dx, x + scrollX - child.Left, y + scrollY - child.Top))
					{
						return true;
					}
				}
			}

			return checkV && ViewCompat.CanScrollHorizontally(v, -dx);
		}

		public override bool DispatchKeyEvent(KeyEvent @event)
		{
			// Let the focused view and/or our descendants get the key first
			return base.DispatchKeyEvent(@event) || executeKeyEvent(@event);
		}

		/// <summary>
		/// You can call this function yourself to have the scroll view perform
		/// scrolling from a key event, just as if the event had been dispatched to
		/// it by the view hierarchy.
		/// </summary>
		/// <param name="event"> The key event to execute. </param>
		/// <returns> Return true if the event was handled, else false. </returns>
		public virtual bool executeKeyEvent(KeyEvent @event)
		{
			bool handled = false;
			if (@event.Action == KeyEventActions.Down)
			{
				switch (@event.KeyCode)
				{
					case Keycode.DpadLeft:
						handled = arrowScroll(FocusSearchDirection.Left);
						break;
					case Keycode.DpadRight:
						handled = arrowScroll(FocusSearchDirection.Right);
						break;
					case Keycode.Tab:
						if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Honeycomb)
						{
							// The focus finder had a bug handling FOCUS_FORWARD and FOCUS_BACKWARD
							// before Android 3.0. Ignore the tab key on those devices.
							if (KeyEventCompat.HasNoModifiers(@event))
							{
								handled = arrowScroll(FocusSearchDirection.Forward);
							}
							else if (KeyEventCompat.HasModifiers(@event, (int)MetaKeyStates.ShiftOn))
							{
								handled = arrowScroll(FocusSearchDirection.Backward);
							}
						}
						break;
				}
			}
			return handled;
		}

		public virtual bool arrowScroll(FocusSearchDirection direction)
		{
			View currentFocused = FindFocus();
			if (currentFocused == this)
			{
				currentFocused = null;
			}
			else if (currentFocused != null)
			{
				bool isChild = false;
				for (IViewParent parent = currentFocused.Parent; parent is ViewGroup; parent = parent.Parent)
				{
					if (parent == this)
					{
						isChild = true;
						break;
					}
				}
				if (!isChild)
				{
					// This would cause the focus search down below to fail in fun ways.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final StringBuilder sb = new StringBuilder();
					StringBuilder sb = new StringBuilder();
					sb.Append(currentFocused.GetType().Name);
					for (IViewParent parent = currentFocused.Parent; parent is ViewGroup; parent = parent.Parent)
					{
						sb.Append(" => ").Append(parent.GetType().Name);
					}
					Log.Error(TAG, "arrowScroll tried to find focus based on non-child " + "current focused view " + sb.ToString());
					currentFocused = null;
				}
			}

			bool handled = false;

			View nextFocused = FocusFinder.Instance.FindNextFocus(this, currentFocused, direction);
			if (nextFocused != null && nextFocused != currentFocused)
			{
				if (direction == FocusSearchDirection.Left)
				{
					// If there is nothing to the left, or this is causing us to
					// jump to the right, then what we really want to do is page left.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int nextLeft = getChildRectInPagerCoordinates(mTempRect, nextFocused).left;
					int nextLeft = getChildRectInPagerCoordinates(mTempRect, nextFocused).Left;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int currLeft = getChildRectInPagerCoordinates(mTempRect, currentFocused).left;
					int currLeft = getChildRectInPagerCoordinates(mTempRect, currentFocused).Left;
					if (currentFocused != null && nextLeft >= currLeft)
					{
						handled = pageLeft();
					}
					else
					{
						handled = nextFocused.RequestFocus();
					}
				}
				else if (direction == FocusSearchDirection.Right)
				{
					// If there is nothing to the right, or this is causing us to
					// jump to the left, then what we really want to do is page right.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int nextLeft = getChildRectInPagerCoordinates(mTempRect, nextFocused).left;
					int nextLeft = getChildRectInPagerCoordinates(mTempRect, nextFocused).Left;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int currLeft = getChildRectInPagerCoordinates(mTempRect, currentFocused).left;
					int currLeft = getChildRectInPagerCoordinates(mTempRect, currentFocused).Left;
					if (currentFocused != null && nextLeft <= currLeft)
					{
						handled = pageRight();
					}
					else
					{
						handled = nextFocused.RequestFocus();
					}
				}
			}
			else if (direction == FocusSearchDirection.Left || direction == FocusSearchDirection.Backward)
			{
				// Trying to move left and nothing there; try to page.
				handled = pageLeft();
			}
			else if (direction == FocusSearchDirection.Right || direction == FocusSearchDirection.Forward)
			{
				// Trying to move right and nothing there; try to page.
				handled = pageRight();
			}
			if (handled)
			{
				PlaySoundEffect(SoundEffectConstants.GetContantForFocusDirection(direction));
			}
			return handled;
		}

		private Rect getChildRectInPagerCoordinates(Rect outRect, View child)
		{
			if (outRect == null)
			{
				outRect = new Rect();
			}
			if (child == null)
			{
				outRect.Set(0, 0, 0, 0);
				return outRect;
			}
			outRect.Left = child.Left;
			outRect.Right = child.Right;
			outRect.Top = child.Top;
			outRect.Bottom = child.Bottom;

			IViewParent parent = child.Parent;
			while (parent is ViewGroup && parent != this)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.ViewGroup group = (android.view.ViewGroup) parent;
				ViewGroup group = (ViewGroup) parent;
				outRect.Left += group.Left;
				outRect.Right += group.Right;
				outRect.Top += group.Top;
				outRect.Bottom += group.Bottom;

				parent = group.Parent;
			}
			return outRect;
		}

		internal virtual bool pageLeft()
		{
			if (mCurItem > 0)
			{
				setCurrentItem(mCurItem - 1, true);
				return true;
			}
			return false;
		}

		internal virtual bool pageRight()
		{
			if (mAdapter != null && mCurItem < (mAdapter.Count - 1))
			{
				setCurrentItem(mCurItem + 1, true);
				return true;
			}
			return false;
		}

		/// <summary>
		/// We only want the current page that is being shown to be focusable.
		/// </summary>
		public override void AddFocusables(IList<View> views, FocusSearchDirection direction, FocusablesFlags focusableMode)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int focusableCount = views.size();
			int focusableCount = views.Count;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int descendantFocusability = getDescendantFocusability();
			var descendantFocusability = DescendantFocusability;

			if (descendantFocusability != DescendantFocusability.BlockDescendants)
			{
				for (int i = 0; i < ChildCount; i++)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View child = getChildAt(i);
					View child = GetChildAt(i);
					if (child.Visibility == ViewStates.Visible)
					{
						ItemInfo ii = infoForChild(child);
						if (ii != null && ii.position == mCurItem)
						{
							child.AddFocusables(views, direction, focusableMode);
						}
					}
				}
			}

			// we add ourselves (if focusable) in all cases except for when we are
			// FOCUS_AFTER_DESCENDANTS and there are some descendants focusable.  this is
			// to avoid the focus search finding layouts when a more precise search
			// among the focusable children would be more interesting.
			if (descendantFocusability != DescendantFocusability.AfterDescendants|| (focusableCount == views.Count))
			{
							// No focusable descendants
				// Note that we can't call the superclass here, because it will
				// add all views in.  So we need to do the same thing View does.
				if (!Focusable)
				{
					return;
				}
				if ((focusableMode & FocusablesFlags.TouchMode) == FocusablesFlags.TouchMode && IsInTouchMode && !FocusableInTouchMode)
				{
					return;
				}
				if (views != null)
				{
					views.Add(this);
				}
			}
		}

		/// <summary>
		/// We only want the current page that is being shown to be touchable.
		/// </summary>
		public override void AddTouchables(IList<View> views)
		{
			// Note that we don't call super.addTouchables(), which means that
			// we don't call View.addTouchables().  This is okay because a ViewPager
			// is itself not touchable.
			for (int i = 0; i < ChildCount; i++)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View child = getChildAt(i);
				View child = GetChildAt(i);
				if (child.Visibility == ViewStates.Visible)
				{
					ItemInfo ii = infoForChild(child);
					if (ii != null && ii.position == mCurItem)
					{
						child.AddTouchables(views);
					}
				}
			}
		}

		/// <summary>
		/// We only want the current page that is being shown to be focusable.
		/// </summary>
		protected override bool OnRequestFocusInDescendants(int direction, Rect previouslyFocusedRect)
		{
			int index;
			int increment;
			int end;
			int count = ChildCount;
			if ((direction & (int)FocusSearchDirection.Forward) != 0)
			{
				index = 0;
				increment = 1;
				end = count;
			}
			else
			{
				index = count - 1;
				increment = -1;
				end = -1;
			}
			for (int i = index; i != end; i += increment)
			{
				View child = GetChildAt(i);
				if (child.Visibility == ViewStates.Visible)
				{
					ItemInfo ii = infoForChild(child);
					if (ii != null && ii.position == mCurItem)
					{
						if (child.RequestFocus((FocusSearchDirection)direction, previouslyFocusedRect))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public override bool DispatchPopulateAccessibilityEvent(AccessibilityEvent @event)
		{
			// Dispatch scroll events from this ViewPager.
			if ((int)@event.EventType == AccessibilityEventCompat.TypeViewScrolled)
			{
				return base.DispatchPopulateAccessibilityEvent(@event);
			}

			// Dispatch all other accessibility events from the current page.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int childCount = getChildCount();
			int childCount = ChildCount;
			for (int i = 0; i < childCount; i++)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View child = getChildAt(i);
				View child = GetChildAt(i);
				if (child.Visibility == ViewStates.Visible)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ItemInfo ii = infoForChild(child);
					ItemInfo ii = infoForChild(child);
					if (ii != null && ii.position == mCurItem && child.DispatchPopulateAccessibilityEvent(@event))
					{
						return true;
					}
				}
			}

			return false;
		}

		protected override ViewGroup.LayoutParams GenerateDefaultLayoutParams()
		{
			return new LayoutParams();
		}

		protected override ViewGroup.LayoutParams GenerateLayoutParams(ViewGroup.LayoutParams p)
		{
			return GenerateDefaultLayoutParams();
		}

		protected override bool CheckLayoutParams(ViewGroup.LayoutParams p)
		{
			return p is LayoutParams && base.CheckLayoutParams(p);
		}

		public override ViewGroup.LayoutParams GenerateLayoutParams(IAttributeSet attrs)
		{
			return new LayoutParams(Context, attrs);
		}

		internal class MyAccessibilityDelegate : AccessibilityDelegateCompat
		{
			private readonly ViewPagerEx outerInstance;

			public MyAccessibilityDelegate(ViewPagerEx outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void OnInitializeAccessibilityEvent(View host, AccessibilityEvent @event)
			{
				base.OnInitializeAccessibilityEvent(host, @event);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				@event.ClassName = typeof(ViewPagerEx).FullName;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.support.v4.view.accessibility.AccessibilityRecordCompat recordCompat = android.support.v4.view.accessibility.AccessibilityRecordCompat.obtain();
				AccessibilityRecordCompat recordCompat = AccessibilityRecordCompat.Obtain();
				recordCompat.Scrollable = canScroll();
				if ((int)@event.EventType == AccessibilityEventCompat.TypeViewScrolled && outerInstance.mAdapter != null)
				{
					recordCompat.ItemCount = outerInstance.mAdapter.Count;
					recordCompat.FromIndex = outerInstance.mCurItem;
					recordCompat.ToIndex = outerInstance.mCurItem;
				}
			}

			public override void OnInitializeAccessibilityNodeInfo(View host, AccessibilityNodeInfoCompat info)
			{
				base.OnInitializeAccessibilityNodeInfo(host, info);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				info.ClassName = typeof(ViewPagerEx).FullName;
				info.Scrollable = canScroll();
				if (outerInstance.canScrollHorizontally(1))
				{
					info.AddAction(AccessibilityNodeInfoCompat.ActionScrollForward);
				}
				if (outerInstance.canScrollHorizontally(-1))
				{
					info.AddAction(AccessibilityNodeInfoCompat.ActionScrollBackward);
				}
			}

			public override bool PerformAccessibilityAction(View host, int action, Bundle args)
			{
				if (base.PerformAccessibilityAction(host, action, args))
				{
					return true;
				}
				switch (action)
				{
					case AccessibilityNodeInfoCompat.ActionScrollForward:
					{
						if (outerInstance.canScrollHorizontally(1))
						{
							outerInstance.CurrentItem = outerInstance.mCurItem + 1;
							return true;
						}
					}
					return false;
					case AccessibilityNodeInfoCompat.ActionScrollBackward:
					{
						if (outerInstance.canScrollHorizontally(-1))
						{
							outerInstance.CurrentItem = outerInstance.mCurItem - 1;
							return true;
						}
					}
					return false;
				}
				return false;
			}

			internal virtual bool canScroll()
			{
				return (outerInstance.mAdapter != null) && (outerInstance.mAdapter.Count > 1);
			}
		}

		private class PagerObserver : DataSetObserver
		{
			private readonly ViewPagerEx outerInstance;

			public PagerObserver(ViewPagerEx outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void OnChanged()
			{
				outerInstance.dataSetChanged();
			}
			public override void OnInvalidated()
			{
				outerInstance.dataSetChanged();
			}
		}

		/// <summary>
		/// Layout parameters that should be supplied for views added to a
		/// ViewPager.
		/// </summary>
		public class LayoutParams : ViewGroup.LayoutParams
		{
			/// <summary>
			/// true if this view is a decoration on the pager itself and not
			/// a view supplied by the adapter.
			/// </summary>
			public bool isDecor;

			/// <summary>
			/// Gravity setting for use on decor views only:
			/// Where to position the view page within the overall ViewPager
			/// container; constants are defined in <seealso cref="android.view.Gravity"/>.
			/// </summary>
			public int gravity;

			/// <summary>
			/// Width as a 0-1 multiplier of the measured pager width
			/// </summary>
			internal float widthFactor = 0.0f;

			/// <summary>
			/// true if this view was added during layout and needs to be measured
			/// before being positioned.
			/// </summary>
			internal bool needsMeasure;

			/// <summary>
			/// Adapter position this view is for if !isDecor
			/// </summary>
			internal int position;

			/// <summary>
			/// Current child index within the ViewPager that this view occupies
			/// </summary>
			internal int childIndex;

			public LayoutParams() : base(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent)
			{
			}

			public LayoutParams(Context context, IAttributeSet attrs) : base(context, attrs)
			{

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.content.res.TypedArray a = context.obtainStyledAttributes(attrs, LAYOUT_ATTRS);
				TypedArray a = context.ObtainStyledAttributes(attrs, LAYOUT_ATTRS);
				gravity = a.GetInteger(0, (int)GravityFlags.Top);
				a.Recycle();
			}
		}

		internal class ViewPositionComparator : IComparer<View>
		{
			public virtual int Compare(View lhs, View rhs)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LayoutParams llp = (LayoutParams) lhs.getLayoutParams();
				LayoutParams llp = (LayoutParams) lhs.LayoutParameters;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LayoutParams rlp = (LayoutParams) rhs.getLayoutParams();
				LayoutParams rlp = (LayoutParams) rhs.LayoutParameters;
				if (llp.isDecor != rlp.isDecor)
				{
					return llp.isDecor ? 1 : -1;
				}
				return llp.position - rlp.position;
			}
		}
	}

}