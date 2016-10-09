using Java.Lang;
using R = Daimajia.Slider.Resource;
using Square.Picasso;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Daimajia.Slider.SliderTypes
{

    


    public enum ScaleType
    {
        CenterCrop,
        CenterInside,
        Fit,
        FitCenterCrop
    }

    /// <summary>
    /// When you want to make your own slider view, you must extends from this class.
    /// BaseSliderView provides some useful methods.
    /// I provide two example: <seealso cref="com.daimajia.slider.library.SliderTypes.DefaultSliderView"/> and
    /// <seealso cref="com.daimajia.slider.library.SliderTypes.TextSliderView"/>
    /// if you want to show progressbar, you just need to set a progressbar id as @+id/loading_bar.
    /// </summary>
    public abstract class BaseSliderView
    {

        protected internal Context mContext;

        private Bundle mBundle;

        /// <summary>
        /// Error place holder image.
        /// </summary>
        private int mErrorPlaceHolderRes;

        /// <summary>
        /// Empty imageView placeholder.
        /// </summary>
        private int mEmptyPlaceHolderRes;

        private string mUrl;
        private Java.IO.File mFile;
        private int mRes;

        protected internal OnSliderClickListener mOnSliderClickListener;

        private bool mErrorDisappear;

        private ImageLoadListener mLoadListener;

        private string mDescription;

        private Picasso mPicasso;

        /// <summary>
        /// Scale type of the image.
        /// </summary>
        private ScaleType mScaleType = ScaleType.Fit;



        protected internal BaseSliderView(Context context)
        {
            mContext = context;
        }

        /// <summary>
        /// the placeholder image when loading image from url or file. </summary>
        /// <param name="resId"> Image resource id
        /// @return </param>
        public virtual BaseSliderView empty(int resId)
        {
            mEmptyPlaceHolderRes = resId;
            return this;
        }

        /// <summary>
        /// determine whether remove the image which failed to download or load from file </summary>
        /// <param name="disappear">
        /// @return </param>
        public virtual BaseSliderView errorDisappear(bool disappear)
        {
            mErrorDisappear = disappear;
            return this;
        }

        /// <summary>
        /// if you set errorDisappear false, this will set a error placeholder image. </summary>
        /// <param name="resId"> image resource id
        /// @return </param>
        public virtual BaseSliderView error(int resId)
        {
            mErrorPlaceHolderRes = resId;
            return this;
        }

        /// <summary>
        /// the description of a slider image. </summary>
        /// <param name="description">
        /// @return </param>
        public virtual BaseSliderView description(string description)
        {
            mDescription = description;
            return this;
        }

        /// <summary>
        /// set a url as a image that preparing to load </summary>
        /// <param name="url">
        /// @return </param>
        public virtual BaseSliderView image(string url)
        {
            if (mFile != null || mRes != 0)
            {
                throw new IllegalStateException("Call multi image function," + "you only have permission to call it once");
            }
            mUrl = url;
            return this;
        }

        /// <summary>
        /// set a file as a image that will to load </summary>
        /// <param name="file">
        /// @return </param>
        public virtual BaseSliderView image(Java.IO.File file)
        {
            if (mUrl != null || mRes != 0)
            {
                throw new IllegalStateException("Call multi image function," + "you only have permission to call it once");
            }
            mFile = file;
            return this;
        }

        public virtual BaseSliderView image(int res)
        {
            if (mUrl != null || mFile != null)
            {
                throw new IllegalStateException("Call multi image function," + "you only have permission to call it once");
            }
            mRes = res;
            return this;
        }

        /// <summary>
        /// lets users add a bundle of additional information </summary>
        /// <param name="bundle">
        /// @return </param>
        public virtual BaseSliderView bundle(Bundle bundle)
        {
            mBundle = bundle;
            return this;
        }

        public virtual string Url
        {
            get
            {
                return mUrl;
            }
        }

        public virtual bool ErrorDisappear
        {
            get
            {
                return mErrorDisappear;
            }
        }

        public virtual int Empty
        {
            get
            {
                return mEmptyPlaceHolderRes;
            }
        }

        public virtual int Error
        {
            get
            {
                return mErrorPlaceHolderRes;
            }
        }

        public virtual string Description
        {
            get
            {
                return mDescription;
            }
        }

        public virtual Context Context
        {
            get
            {
                return mContext;
            }
        }

        /// <summary>
        /// set a slider image click listener </summary>
        /// <param name="l">
        /// @return </param>
        public virtual BaseSliderView setOnSliderClickListener(OnSliderClickListener l)
        {
            mOnSliderClickListener = l;
            return this;
        }

        /// <summary>
        /// When you want to implement your own slider view, please call this method in the end in `getView()` method </summary>
        /// <param name="v"> the whole view </param>
        /// <param name="targetImageView"> where to place image </param>
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not allowed in .NET:
        //ORIGINAL LINE: protected void bindEventAndShow(final android.view.View v, android.widget.ImageView targetImageView)
        protected internal virtual void bindEventAndShow(View v, ImageView targetImageView)
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final BaseSliderView me = this;
            BaseSliderView me = this;

            v.SetOnClickListener(new OnClickListenerAnonymousInnerClassHelper(this, v, me));

            if (targetImageView == null)
            {
                return;
            }

            if (mLoadListener != null)
            {
                mLoadListener.onStart(me);
            }

            Picasso p = (mPicasso != null) ? mPicasso : Picasso.With(mContext);
            RequestCreator rq = null;
            if (mUrl != null)
            {
                rq = p.Load(mUrl);
            }
            else if (mFile != null)
            {
                rq = p.Load(mFile);
            }
            else if (mRes != 0)
            {
                rq = p.Load(mRes);
            }
            else
            {
                return;
            }

            if (rq == null)
            {
                return;
            }

            if (Empty != 0)
            {
                rq.Placeholder(Empty);
            }

            if (Error != 0)
            {
                rq.Error(Error);
            }

            switch (mScaleType)
            {
                case ScaleType.Fit:
                    rq.Fit();
                    break;
                case ScaleType.CenterCrop:
                    rq.Fit().CenterCrop();
                    break;
                case ScaleType.CenterInside:
                    rq.Fit().CenterInside();
                    break;
            }

            rq.Into(targetImageView, () => OnSuccess(v), () => OnError(v, me));
            //rq.Into(targetImageView, new CallbackAnonymousInnerClassHelper(this, v, me));
        }

        private void OnSuccess(View v)
        {
            if (v.FindViewById(R.Id.loading_bar) != null)
            {
                v.FindViewById(R.Id.loading_bar).Visibility = Android.Views.ViewStates.Invisible;
            }
        }

        private void OnError(View v, BaseSliderView me)
        {
            if (mLoadListener != null)
            {
                mLoadListener.onEnd(false, me);
            }
            if (v.FindViewById(R.Id.loading_bar) != null)
            {
                v.FindViewById(R.Id.loading_bar).Visibility = Android.Views.ViewStates.Invisible;
            }
        }

        private class OnClickListenerAnonymousInnerClassHelper : Java.Lang.Object, View.IOnClickListener
        {
            private readonly BaseSliderView outerInstance;

            private View v;
            private BaseSliderView me;

            public OnClickListenerAnonymousInnerClassHelper(BaseSliderView outerInstance, View v, BaseSliderView me)
            {
                this.outerInstance = outerInstance;
                this.v = v;
                this.me = me;
            }

            public void OnClick(View v)
            {
                if (outerInstance.mOnSliderClickListener != null)
                {
                    outerInstance.mOnSliderClickListener.onSliderClick(me);
                }
            }
        }


        public virtual BaseSliderView setScaleType(ScaleType type)
        {
            mScaleType = type;
            return this;
        }

        public virtual ScaleType ScaleType
        {
            get
            {
                return mScaleType;
            }
        }

        /// <summary>
        /// the extended class have to implement getView(), which is called by the adapter,
        /// every extended class response to render their own view.
        /// @return
        /// </summary>
        public abstract View View { get; }

        /// <summary>
        /// set a listener to get a message , if load error. </summary>
        /// <param name="l"> </param>
        public virtual ImageLoadListener OnImageLoadListener
        {
            set
            {
                mLoadListener = value;
            }
        }

        public interface OnSliderClickListener
        {
            void onSliderClick(BaseSliderView slider);
        }

        /// <summary>
        /// when you have some extra information, please put it in this bundle.
        /// @return
        /// </summary>
        public virtual Bundle Bundle
        {
            get
            {
                return mBundle;
            }
        }

        public interface ImageLoadListener
        {
            void onStart(BaseSliderView target);
            void onEnd(bool result, BaseSliderView target);
        }

        /// <summary>
        /// Get the last instance set via setPicasso(), or null if no user provided instance was set
        /// </summary>
        /// <returns> The current user-provided Picasso instance, or null if none </returns>
        public virtual Picasso Picasso
        {
            get
            {
                return mPicasso;
            }
            set
            {
                mPicasso = value;
            }
        }

    }

}