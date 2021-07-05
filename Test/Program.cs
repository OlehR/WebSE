using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Merge();
            Console.WriteLine("Hello World!");
        }

        static void Merge()
        {
            int width = 400, height = 500;
            Image playbutton;
            try
            {
                playbutton = Image.FromFile(@"D:\Work\WebSE\WebSE\img\BarCode\8800000442402.png");
            }
            catch (Exception ex)
            {
                return;
            }

            Image frame;
            try
            {
                frame = Image.FromFile(@"d:\Spar-logo.png");
            }
            catch (Exception ex)
            {
                return;
            }

            using (frame)
            {
                using (var bitmap = new Bitmap(width, height))
                {
                    using (var canvas = Graphics.FromImage(bitmap))
                    {
                        canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        canvas.DrawImage(frame,
                                         new Rectangle(0,0,width, height),
                                         new Rectangle(0,0, frame.Width, frame.Height),
                                         GraphicsUnit.Pixel);
                        canvas.DrawImage(playbutton, 0, 100);
                        canvas.Save();
                    }
                    try
                    {
                        bitmap.Save(@"d:\res.png",System.Drawing.Imaging.ImageFormat.Png);
                    }
                    catch (Exception ex)
                    { 
                    }
                }
            }

        }
    }
}