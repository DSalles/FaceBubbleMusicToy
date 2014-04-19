using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using SlimDX.DirectSound;
using System.Diagnostics;
using System.Windows.Media.Animation;
using WpfApplication1;

namespace FaceBubbleMusicToy
{
    class BubbleList : List<Bubble>
    {
        static MemoryStream inputStream;
        static SoundSource soundSource = SoundSource.getDefaultSource();
        static DirectSound device = soundSource.GetDevice();  
        static BubbleList bubbles;
        static Grid mainGrid;
        static FacesList facesList;
        static int[] frequencyList = new int[9] {8987, 10079, 10680, 12000, 13500, 15000, 16000, 18000, 20000 };
        private bool SoundBufferReady = false;
        private static BubbleList defaultBufferList;
        private static long faceFrameTime;
        private static long bufferSamples;

        internal static BubbleList GetDefaultBubbleList()
        {
            if (defaultBufferList == null)
                defaultBufferList = new BubbleList();
            return defaultBufferList;
        }

        internal static void RecordMediaForBubbles(Grid grid)
        {
            while (soundSource.Ready == false)
            { }

            mainGrid = grid;
        soundSource.RecordAudio();
        facesList = ImageSource.GetDefaultSource().RecordFaces();
         
        }

       static internal void DesignateInputStream(MemoryStream memoryStream)
    {
        inputStream = memoryStream;
    }

        internal BubbleList MakeBubbles()
        { 
            // if sound is not ready, bubbles will not be made. 
            if (SoundBufferReady == true)
                {
                    SoundBufferReady = false;
                    bubbles = new BubbleList();

                Designer designer = Designer.getDefaultDesigner();

                    for (int i = 0; i < frequencyList.Count(); i++)
                        {
                            int left = 300 * i - 1200;

                            Bubble bubble = new Bubble(soundSource.GetSound(inputStream),frequencyList[i], device, designer.placement1(i),facesList, faceFrameTime, bufferSamples, mainGrid);

                        
                        // Bubble bubble = new Bubble(new Thickness(0, 0, 0, 0));
                           // bubble.BeginAnimation(Image.WidthProperty, new DoubleAnimation(11.0, 110.0, TimeSpan.FromSeconds(1)));
                          //  bubble.BeginAnimation(Image.MarginProperty, new ThicknessAnimation(new Thickness(left, 500, 0, 0), new Thickness(left, 0, 0, 0), TimeSpan.FromSeconds(1)));
                        //    bubble.faceImage.BeginAnimation(Image.WidthProperty, new DoubleAnimation(11.0, 110.0, TimeSpan.FromSeconds(1)));
                          //  bubble.faceImage.BeginAnimation(Image.MarginProperty, new ThicknessAnimation(new Thickness(left, 500, 0, 0), new Thickness(left, 0, 0, 0), TimeSpan.FromSeconds(1)));
                            bubbles.Add(bubble);
                     //      mainGrid.Children.Add(bubble);
                     //       mainGrid.Children.Add(bubble.faceImage);
                        } 
            }
            return bubbles;
        }

        internal void SoundReady(long facesFrameTime, long howLongSoundBuffer)
        {
            // each Bubble will get this for timing face image animation 
            faceFrameTime = facesFrameTime;
            SoundBufferReady = true;
            bufferSamples = howLongSoundBuffer;
        }              
    }
}
