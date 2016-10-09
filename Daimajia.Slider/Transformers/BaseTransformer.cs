using System.Collections.Generic;
using Android.Views;
using NineOldAndroids.View;
using Daimajia.Slider.Tricks;
using Daimajia.Slider.Animations;

namespace Daimajia.Slider.Transformers
{



	/// <summary>
	/// This is all transformers father.
	/// 
	/// BaseTransformer implement <seealso cref="com.daimajia.slider.library.Tricks.ViewPagerEx.PageTransformer"/>
	/// which is just same as <seealso cref="android.support.v4.view.ViewPager.PageTransformer"/>.
	/// 
	/// After you call setPageTransformer(), transformPage() will be called by <seealso cref="com.daimajia.slider.library.Tricks.ViewPagerEx"/>
	/// when your slider are animating.
	/// 
	/// In onPreTransform() function, that will make <seealso cref="com.daimajia.slider.library.Animations.BaseAnimationInterface"/>
	/// work.
	/// 
	/// if you want to make an acceptable transformer, please do not forget to extend from this class.
	/// </summary>
	public abstract class BaseTransformer : ViewPagerEx.PageTransformer
	{

		private BaseAnimationInterface mCustomAnimationInterface;

		/// <summary>
		/// Called each <seealso cref="#transformPage(View, float)"/>.
		/// </summary>
		/// <param name="view"> </param>
		/// <param name="position"> </param>
		protected internal abstract void onTransform(View view, float position);

		private Dictionary<View, List<float>> h = new Dictionary<View, List<float>>();

		public void transformPage(View view, float position)
		{
			onPreTransform(view, position);
			onTransform(view, position);
			onPostTransform(view, position);
		}

		/// <summary>
		/// If the position offset of a fragment is less than negative one or greater than one, returning true will set the
		/// visibility of the fragment to <seealso cref="View#GONE"/>. Returning false will force the fragment to <seealso cref="View#VISIBLE"/>.
		/// 
		/// @return
		/// </summary>
		protected internal virtual bool hideOffscreenPages()
		{
			return true;
		}

		/// <summary>
		/// Indicates if the default animations of the view pager should be used.
		/// 
		/// @return
		/// </summary>
		protected internal virtual bool PagingEnabled
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Called each <seealso cref="#transformPage(View, float)"/> before {<seealso cref="#onTransform(View, float)"/> is called.
		/// </summary>
		/// <param name="view"> </param>
		/// <param name="position"> </param>
		protected internal virtual void onPreTransform(View view, float position)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float width = view.getWidth();
			float width = view.Width;

			ViewHelper.SetRotationX(view,0);
			ViewHelper.SetRotationY(view,0);
			ViewHelper.SetRotation(view,0);
			ViewHelper.SetScaleX(view,1);
			ViewHelper.SetScaleY(view,1);
			ViewHelper.SetPivotX(view,0);
			ViewHelper.SetPivotY(view,0);
			ViewHelper.SetTranslationY(view,0);
			ViewHelper.SetTranslationX(view,PagingEnabled ? 0f : -width * position);

			if (hideOffscreenPages())
			{
				ViewHelper.SetAlpha(view,position <= -1f || position >= 1f ? 0f : 1f);
			}
			else
			{
				ViewHelper.SetAlpha(view,1f);
			}
			if (mCustomAnimationInterface != null)
			{
				if (h.ContainsKey(view) == false || h[view].Count == 1)
				{
					if (position > -1 && position < 1)
					{
						if (!h.ContainsKey(view) || h[view] == null)
						{
							h[view] = new List<float>();
						}
						h[view].Add(position);
						if (h[view].Count == 2)
						{
							float zero = h[view][0];
							float cha = h[view][1] - h[view][0];
							if (zero > 0)
							{
								if (cha > -1 && cha < 0)
								{
									//in
									mCustomAnimationInterface.onPrepareNextItemShowInScreen(view);
								}
								else
								{
									//out
									mCustomAnimationInterface.onPrepareCurrentItemLeaveScreen(view);
								}
							}
							else
							{
								if (cha > -1 && cha < 0)
								{
									//out
									mCustomAnimationInterface.onPrepareCurrentItemLeaveScreen(view);
								}
								else
								{
									//in
									mCustomAnimationInterface.onPrepareNextItemShowInScreen(view);
								}
							}
						}
					}
				}
			}
		}
		internal bool isApp, isDis;
		/// <summary>
		/// Called each <seealso cref="#transformPage(View, float)"/> call after <seealso cref="#onTransform(View, float)"/> is finished.
		/// </summary>
		/// <param name="view"> </param>
		/// <param name="position"> </param>
		protected internal virtual void onPostTransform(View view, float position)
		{
			if (mCustomAnimationInterface != null)
			{
				if (position == -1 || position == 1)
				{
					mCustomAnimationInterface.onCurrentItemDisappear(view);
					isApp = true;
				}
				else if (position == 0)
				{
					mCustomAnimationInterface.onNextItemAppear(view);
					isDis = true;
				}
				if (isApp && isDis)
				{
					h.Clear();
					isApp = false;
					isDis = false;
				}
			}
		}


		public virtual BaseAnimationInterface CustomAnimationInterface
		{
			set
			{
				mCustomAnimationInterface = value;
			}
		}

	}
}