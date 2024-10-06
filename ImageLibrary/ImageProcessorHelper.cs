using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using ImageProcessor.Processors;
using Libraries.CommonUtilities;
using Libraries.CommonUtilities.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageLibrary
{
    public class ImageProcessorHelper : Libraries.CommonUtilities.Interfaces.IImageActions
    {
        public void ConvertToScanned(string filename)
        {
            throw new System.NotImplementedException();
        }

        public void ImageToPdf(List<string> inputfiles, string outputlocation = "")
        {
            throw new System.NotImplementedException();
        }

        public Image Optimize(string inputfile, int finalsize = 1024, bool retobj = false, float hdpi = 0, float vdpi = 0)
        {
            var outputpath = inputfile.GetOutputPath(ActionType.OPTIMIZE);

            Size size = new Size(finalsize, 0);

            using (ImageFactory imageFactory = new ImageFactory(preserveExifData: false))
            {
                if (retobj)
                {
                    using (Image sourceImg = ResizeImage(imageFactory.Load(inputfile), size).Image)
                    {
                        var clonedImg = new Bitmap(sourceImg.Width, sourceImg.Height, PixelFormat.Format32bppArgb);

                        clonedImg.SetResolution(hdpi == 0 ? Image.FromFile(inputfile).HorizontalResolution : hdpi, vdpi == 0 ? Image.FromFile(inputfile).VerticalResolution : vdpi);

                        using (var copy = Graphics.FromImage(clonedImg))
                        {
                            copy.DrawImage(sourceImg, 0, 0);
                        }

                        return clonedImg;
                    }
                }
                else
                    ResizeImage(imageFactory.Load(inputfile), size).Save(outputpath);

                return imageFactory.Load(inputfile).Image;
            }
        }

        private ImageFactory ResizeImage(ImageFactory image, Size finalsize)
        {
            return image.Format(new JpegFormat()).Resolution(8,10,ImageProcessor.Imaging.MetaData.PropertyTagResolutionUnit.Inch).Quality(40).Resize(finalsize);
        }

        public Image Rotate(string inputfile, float rotation, bool retobj = false)
        {
            var outputpath = inputfile.GetOutputPath(ActionType.ROTATE, additionalData: rotation.ToString());

            using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true).Resolution((int)Image.FromFile(inputfile).HorizontalResolution, (int)Image.FromFile(inputfile).VerticalResolution))
            {
                if (retobj)
                {
                    using (Image sourceImg = imageFactory.Load(inputfile).RotateBounded(rotation, true).Image)
                    {
                        var clonedImg = new Bitmap(sourceImg.Width, sourceImg.Height, PixelFormat.Format32bppArgb);
                        clonedImg.SetResolution(Image.FromFile(inputfile).HorizontalResolution, Image.FromFile(inputfile).VerticalResolution);

                        using (var copy = Graphics.FromImage(clonedImg))
                        {
                            copy.DrawImage(sourceImg, 0, 0);
                        }

                        return clonedImg;
                    }
                }
                else
                    imageFactory.Load(inputfile).Rotate(rotation).Save(outputpath);

                return imageFactory.Load(inputfile).Image;
            }
        }
    }
}
