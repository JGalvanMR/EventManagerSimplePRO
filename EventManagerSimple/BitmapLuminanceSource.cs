using ZXing;

namespace EventManagerSimple
{
    internal class BitmapLuminanceSource : LuminanceSource
    {
        private Bitmap imagen;

        public BitmapLuminanceSource(Bitmap imagen)
        {
            this.imagen = imagen;
        }
    }
}