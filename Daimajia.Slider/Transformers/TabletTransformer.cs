using Android.Graphics;
using Android.Views;
using NineOldAndroids.View;
using System;

namespace Daimajia.Slider.Transformers
{

	public class TabletTransformer : BaseTransformer
	{

		private static readonly Matrix OFFSET_MATRIX = new Matrix();
		private static readonly Camera OFFSET_CAMERA = new Camera();
		private static readonly float[] OFFSET_TEMP_FLOAT = new float[2];

		protected internal override void onTransform(View view, float position)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float rotation = (position < 0 ? 30f : -30f) * Math.abs(position);
			float rotation = (position < 0 ? 30f : -30f) * Math.Abs(position);

			ViewHelper.SetTranslationX(view,getOffsetXForRotation(rotation, view.Width, view.Height));
			ViewHelper.SetPivotX(view,view.Width * 0.5f);
			ViewHelper.SetPivotY(view,0);
			ViewHelper.SetRotationY(view,rotation);
		}

		protected internal static float getOffsetXForRotation(float degrees, int width, int height)
		{
			OFFSET_MATRIX.Reset();
			OFFSET_CAMERA.Save();
			OFFSET_CAMERA.RotateY(Math.Abs(degrees));
			OFFSET_CAMERA.GetMatrix(OFFSET_MATRIX);
			OFFSET_CAMERA.Restore();

			OFFSET_MATRIX.PreTranslate(-width * 0.5f, -height * 0.5f);
			OFFSET_MATRIX.PostTranslate(width * 0.5f, height * 0.5f);
			OFFSET_TEMP_FLOAT[0] = width;
			OFFSET_TEMP_FLOAT[1] = height;
			OFFSET_MATRIX.MapPoints(OFFSET_TEMP_FLOAT);
			return (width - OFFSET_TEMP_FLOAT[0]) * (degrees > 0.0f ? 1.0f : -1.0f);
		}

	}

}