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

namespace Daimajia.Slider
{
    public static class CSharpForJavaEx
    {
        public static int Ordinal(this Enum en)
        {
            var vals = Enum.GetValues(en.GetType());
            var res = Array.IndexOf(vals, en);
            return res;
        }
    }
}