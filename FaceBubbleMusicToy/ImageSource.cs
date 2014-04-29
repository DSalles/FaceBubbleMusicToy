using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using System.Diagnostics;

namespace FaceBubbleMusicToy
{
    class ImageSource : Image
    {
       private static readonly int Bgra32BytesPerPixel = (PixelFormats.Bgra32.BitsPerPixel + 7) / 8;
       static ImageSource defaultSource = null;
       static FacesList facesList = new FacesList();
       private ColorImageFormat currentColorImageFormat = ColorImageFormat.Undefined;
       private byte[] colorImageData;
       private int faceImageSize = 192;
       private int colorImageWidth = 0;
       int colorDepth = 4;
       private FaceTrackingViewer faceTrackingViewer;
       public bool Ready = false;
       public bool Recording = false;
       private static Stopwatch stopwatch;
       private long faceFrameTime;
        
        internal static ImageSource GetDefaultSource()
        {
            if(defaultSource == null)
                defaultSource = new ImageSource();
            return defaultSource;
        }
        internal static void ClearFaces()
        {
            facesList.Clear();
        }

        internal FacesList RecordFaces()
        {  
            stopwatch = new Stopwatch();
            stopwatch.Start();
            Recording = true;
            return facesList;
        }

        internal void initialize(FaceTrackingViewer faceTrackingViewer)
        {
            this.faceTrackingViewer = faceTrackingViewer;           
        }

        internal void FaceTrackedColorFrameReady(Microsoft.Kinect.ColorImageFrame colorImageFrame)
        {
            Ready = true;
            if (Recording == true)
            {
                var haveNewFormat = this.currentColorImageFormat != colorImageFrame.Format;
                if (haveNewFormat)
                {
                    colorImageWidth = colorImageFrame.Width;
                    this.currentColorImageFormat = colorImageFrame.Format;
                    this.colorImageData = new byte[colorImageFrame.PixelDataLength];
                }

                colorImageFrame.CopyPixelDataTo(this.colorImageData);

                int colorImageStride = colorImageWidth * colorDepth;
                int croppedImageStride = faceImageSize * colorDepth;
                byte[] croppedData = new byte[croppedImageStride * faceImageSize];
                int Xoffset = (int)(faceTrackingViewer.faceLeft) * colorDepth;
                int Yoffset = (int)(faceTrackingViewer.faceTop);
                int b = 1;
                for (int i = 0; i < faceImageSize * colorDepth * faceImageSize - 3; i += 4)
                {
                    if (i == (faceImageSize * colorDepth * b))
                    {
                        b++;
                    }
                    int j = (Xoffset + colorImageStride * (b - 1) + colorImageStride * Yoffset + i - (faceImageSize * colorDepth) * (b - 1));
                    if (j > 0 && j < (colorImageData.Length - 2))
                    {
                        croppedData[i] = colorImageData[j];
                        croppedData[i + 1] = colorImageData[j + 1];
                        croppedData[i + 2] = colorImageData[j + 2];
                        croppedData[i + 3] = 0xFF;
                    }
                }

                FaceImage faceImageFrame = new FaceImage(croppedData, croppedImageStride, faceImageSize);
                facesList.Add(faceImageFrame);

            }
            else
            {
            }
        }

        internal long StopRecording()
        {   
            Recording = false;
            long faceRecordingTime = stopwatch.Elapsed.Ticks;
            faceFrameTime = (long)(faceRecordingTime / (facesList.Count*1.5));
            return faceFrameTime;
        }
    }
}
