using Android.Views;

namespace Daimajia.Slider.Animations
{ 

	/// <summary>
	/// This interface gives you chance to inject your own animation or do something when the
	/// <seealso cref="com.daimajia.slider.library.Tricks.ViewPagerEx"/> animation (PagerTransformer) starts or ends.
	/// 
	/// 
	/// There are two items you have to know. The first item is the slider you are dragging. This item
	/// I call it Current Item. The second is the slider that gonna to show. I call that Next Item.
	/// 
	/// When you start to drag the slider in front of you, onPrepareCurrentItemLeaveScreen() and
	/// onPrepareNextItemShowInScreen will be called.
	/// 
	/// When you finish drag, the onCurrentItemDisappear and onNextItemAppear will be invoked.
	/// 
	/// You can see a demo class <seealso cref="com.daimajia.slider.library.Animations.DescriptionAnimation"/>,
	/// this class gives the description text an animation.
	/// </summary>
	public interface BaseAnimationInterface
	{

		/// <summary>
		/// When the current item prepare to start leaving the screen. </summary>
		/// <param name="current"> </param>
		void onPrepareCurrentItemLeaveScreen(View current);

		/// <summary>
		/// The next item which will be shown in ViewPager/ </summary>
		/// <param name="next"> </param>
		void onPrepareNextItemShowInScreen(View next);

		/// <summary>
		/// Current item totally disappear from screen. </summary>
		/// <param name="view"> </param>
		void onCurrentItemDisappear(View view);

		/// <summary>
		/// Next item totally show in screen. </summary>
		/// <param name="view"> </param>
		void onNextItemAppear(View view);
	}

}