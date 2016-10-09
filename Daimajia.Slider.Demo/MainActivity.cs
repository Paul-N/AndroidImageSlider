using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.Linq;
using System.Collections.Generic;
using Android.Views;
using Android.Content;
using Android.Util;
using Daimajia.Slider.SliderTypes;
using Daimajia.Slider.Tricks;
using Daimajia.Slider.Animations;
using Daimajia.Slider.Indicators;

namespace Daimajia.Slider.Demo
{
    [Activity(Label = "Daimajia.Slider.Demo", MainLauncher = true, Icon = "@drawable/ic_launcher")]
    public class MainActivity : Activity, BaseSliderView.OnSliderClickListener, ViewPagerEx.OnPageChangeListener
    {
        private SliderLayout mDemoSlider;

        public void onSliderClick(BaseSliderView slider)
        {
            throw new NotImplementedException();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.activity_main);
            mDemoSlider = FindViewById<SliderLayout>(Resource.Id.slider);

            var url_maps = new Dictionary<string, string>();
            url_maps.Add("Hannibal", "http://static2.hypable.com/wp-content/uploads/2013/12/hannibal-season-2-release-date.jpg");
            url_maps.Add("Big Bang Theory", "http://tvfiles.alphacoders.com/100/hdclearart-10.png");
            url_maps.Add("House of Cards", "http://cdn3.nflximg.net/images/3093/2043093.jpg");
            url_maps.Add("Game of Thrones", "http://images.boomsbeat.com/data/images/full/19640/game-of-thrones-season-4-jpg.jpg");

            var file_maps = new Dictionary<string, int>();
            file_maps.Add("Hannibal", Resource.Drawable.hannibal);
            file_maps.Add("Big Bang Theory", Resource.Drawable.bigbang);
            file_maps.Add("House of Cards", Resource.Drawable.house);
            file_maps.Add("Game of Thrones", Resource.Drawable.game_of_thrones);

            foreach (var name in file_maps.Keys)
            {
                TextSliderView textSliderView = new TextSliderView(this);
                // initialize a SliderLayout
                textSliderView
                        .description(name)
                        .image(file_maps[name])
                        .setScaleType(ScaleType.Fit)
                        .setOnSliderClickListener(this);

                //add your extra information
                textSliderView.bundle(new Bundle());
                textSliderView.Bundle.PutString("extra", name);

                mDemoSlider.addSlider(textSliderView);
            }
            mDemoSlider.SetPresetTransformer(Transformer.Accordion);
            mDemoSlider.SetPresetIndicator(PresetIndicators.Center_Bottom);
            mDemoSlider.CustomAnimation = (new DescriptionAnimation());
            mDemoSlider.Duration = (4000);
            mDemoSlider.addOnPageChangeListener(this);
            ListView l = FindViewById<ListView>(Resource.Id.transformers);
            l.SetAdapter(new TransformerAdapter(this));
            l.OnItemClickListener = new ListViewOnClickItemListener(mDemoSlider, this);
        }


        protected override void OnStop()
        {
            // To prevent a memory leak on rotation, make sure to call stopAutoCycle() on the slider before activity or fragment is destroyed
            mDemoSlider.stopAutoCycle();
            base.OnStop();
        }


        public void OnSliderClick(BaseSliderView slider)
        {
            Toast.MakeText(this, slider.Bundle.Get("extra") + "", ToastLength.Short).Show();
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater menuInflater = this.MenuInflater;
            menuInflater.Inflate(Resource.Menu.main, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_custom_indicator:
                    mDemoSlider.CustomIndicator = ((PagerIndicator)FindViewById(Resource.Id.custom_indicator));
                    break;
                case Resource.Id.action_custom_child_animation:
                    mDemoSlider.CustomAnimation = (new ChildAnimationExample());
                    break;
                case Resource.Id.action_restore_default:
                    mDemoSlider.SetPresetIndicator((PresetIndicators.Center_Bottom));
                    mDemoSlider.CustomAnimation = (new DescriptionAnimation());
                    break;
                case Resource.Id.action_github:
                    Intent browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse("https://github.com/daimajia/AndroidImageSlider"));
                    StartActivity(browserIntent);
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        public void onPageScrolled(int position, float positionOffset, int positionOffsetPixels) { }


        public void onPageSelected(int position)
        {
            Log.Debug("Slider Demo", "Page Changed: " + position);
        }


        public void onPageScrollStateChanged(int state) { }
    }

    internal class ListViewOnClickItemListener : Java.Lang.Object, AdapterView.IOnItemClickListener
    {
        private SliderLayout mDemoSlider;
        private Context _ctx;

        public ListViewOnClickItemListener(SliderLayout demoSlider, Context ctx)
        {
            mDemoSlider = demoSlider;
            _ctx = ctx;
        }
        public void OnItemClick(AdapterView parent, View view, int position, long id)
        {
            mDemoSlider.SetPresetTransformer(((TextView)view).Text);
            Toast.MakeText(_ctx, ((TextView)view).Text, ToastLength.Short).Show();
        }
    }
}

