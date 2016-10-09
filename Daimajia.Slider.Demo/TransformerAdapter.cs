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

namespace Daimajia.Slider.Demo
{
    public class TransformerAdapter : BaseAdapter
    {
    private Context mContext;
    public TransformerAdapter(Context context)
    {
        mContext = context;
    }

    public override int Count
    {
        get
        {
            return Enum.GetValues(typeof(Transformer)).Length;
        }
    }

    public override Java.Lang.Object GetItem(int position)
    {
        return Enum.GetValues(typeof(Transformer)).OfType<Transformer>().ElementAt(position).ToString();
    }

    
    public override long GetItemId(int position)
    {
        return position;
    }

    public override View GetView(int position, View convertView, ViewGroup parent)
    {
        TextView t = (TextView)LayoutInflater.From(mContext).Inflate(Resource.Layout.item, null);
        t.Text = (GetItem(position).ToString());
        return t;
    }
}
}