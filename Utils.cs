using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace CheckContamination
{
    class Utils
    {
        public static TensorFlow.TFTensor ImageToTensor ( string file )
        {
            using ( Bitmap image = (Bitmap) Image.FromFile ( file ) )
            {
                Bitmap smallerImage = ResizeImage ( image, 300, 300 );
                var matrix = new float[1, smallerImage.Size.Height, smallerImage.Size.Width, 3];
                for ( var iy = 0; iy < smallerImage.Size.Height; iy++ )
                {
                    for ( int ix = 0; ix < smallerImage.Size.Width; ix++ )
                    {
                        Color pixel = smallerImage.GetPixel ( ix, iy );
                        matrix[0, iy, ix, 0] = pixel.B / 255.0f;
                        matrix[0, iy, ix, 1] = pixel.G / 255.0f;
                        matrix[0, iy, ix, 2] = pixel.R / 255.0f;
                    }
                }
                TensorFlow.TFTensor tensor = matrix;
                return tensor;
            }
        }

        public static Bitmap ResizeImage ( Image image, int width, int height )
        {
            var destRect = new Rectangle ( 0, 0, width, height );
            var destImage = new Bitmap ( width, height );

            destImage.SetResolution ( image.HorizontalResolution, image.VerticalResolution );

            using ( var graphics = Graphics.FromImage ( destImage ) )
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using ( var wrapMode = new ImageAttributes () )
                {
                    wrapMode.SetWrapMode ( WrapMode.TileFlipXY );
                    graphics.DrawImage ( image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode );
                }
            }

            return destImage;
        }

        //Silly repetitions here! I was running out of time.
        internal static int[] Quantized ( float[,] results )
        {
            int[] q = new int[]
            {
                results[0,0]>0.5?1:0,
                results[0,1]>0.5?1:0,
            };
            return q;
        }
    }
}
