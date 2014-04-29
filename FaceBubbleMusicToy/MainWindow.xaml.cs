// -----------------------------------------------------------------------
// <copyright file="Designer.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace FaceBubbleMusicToy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;
    using ShaderEffectsLibrary;
    using System.Windows.Media.Animation;
    using System.Diagnostics;
    
    /// <summary>
    /// Interaction logic for Designer.xaml
    /// </summary>
    public partial class MainWindow : Window
    {      
        private BubbleList bubbleList;
        private static readonly int Bgra32BytesPerPixel = (PixelFormats.Bgra32.BitsPerPixel + 7) / 8;
        private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();

        private const int AudioPollingInterval = 50;
        private const int SamplesPerMillisecond = 16;
        private const int BytesPerSample = 2;
        private readonly byte[] audioBuffer = new byte[AudioPollingInterval * SamplesPerMillisecond * BytesPerSample];
        Ellipse spotlight = new Ellipse();
        Ellipse LHand = new Ellipse();
        Ellipse RHand = new Ellipse();
        Stream audioStream;
        ImageSource imageSource;
        public MainWindow()
        {

            this.Background = Brushes.Black;
         
            InitializeComponent();
            imageSource = ImageSource.GetDefaultSource();


            var faceTrackingViewerBinding = new Binding("Kinect") { Source = sensorChooser };
            faceTrackingViewer.SetBinding(FaceTrackingViewer.KinectProperty, faceTrackingViewerBinding);
            sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            sensorChooser.Start();
            imageSource.initialize(faceTrackingViewer);
           // MouseWheel += mouseWheeled;

            spotlight.Width = 100;
            spotlight.Height = 100;
            spotlight.Fill = Brushes.Gray;
            LHand.Width = 60;
            LHand.Height = 60;
            LHand.Fill = Brushes.Blue;
            RHand.Width = 60;
            RHand.Height = 60;
            RHand.Fill = Brushes.Blue;
            Ellipse testEllipse = new Ellipse();            
            spotlight.Margin = new Thickness(0, 0, 0, 0);
            LHand.Margin = new Thickness(0, 0, 0, 0);
            RHand.Margin = new Thickness(0, 0, 0, 0);
            this.MainGrid.Children.Add(spotlight);
            this.MainGrid.Children.Add(LHand);
            this.MainGrid.Children.Add(RHand);
        }

        void MainWindow_FinishedRecording(object sender, RoutedEventArgs e)
        {

        }
        
        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs kinectChangedEventArgs)
        {
            KinectSensor oldSensor = kinectChangedEventArgs.OldSensor;
            KinectSensor newSensor = kinectChangedEventArgs.NewSensor;
            if (oldSensor != null)
            {
                oldSensor.AudioSource.Stop();
                oldSensor.AllFramesReady -= KinectSensorOnAllFramesReady;
                oldSensor.ColorStream.Disable();
                oldSensor.DepthStream.Disable();
                oldSensor.DepthStream.Range = DepthRange.Default;
                oldSensor.SkeletonStream.Disable();
                oldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                oldSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
            }

            if (newSensor != null)
            {

                try
                {
                    newSensor.ColorStream.Enable(ColorImageFormat.RgbResolution1280x960Fps12);
                    newSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    try
                    {
                        // This will throw on non Kinect For Windows devices.
                        newSensor.DepthStream.Range = DepthRange.Default;
                        // newSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        newSensor.DepthStream.Range = DepthRange.Default;
                        newSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }

                    newSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;

                    TransformSmoothParameters smoothingParams = new TransformSmoothParameters();
                    smoothingParams.Correction = 1;
                    smoothingParams.JitterRadius = 1;
                    smoothingParams.MaxDeviationRadius = 1;
                    smoothingParams.Prediction = 0.01f;
                    smoothingParams.Smoothing = 0;
                    newSensor.SkeletonStream.Enable(smoothingParams);
                    newSensor.AllFramesReady += KinectSensorOnAllFramesReady;
                    audioStream = newSensor.AudioSource.Start();
                    SoundSource.getDefaultSource().initialize(audioStream);

                }
                catch (InvalidOperationException)
                {
                    // This exception can be thrown when we are trying to
                    // enable streams on a device that has gone away.  This
                    // can occur,IMAGE say, in app shutdown scenarios when the sensor
                    // goes away between the time it changed status and the
                    // time we get the sensor changed notification.
                    //
                    // Behavior here is to just eat the exception and assume
                    // another notification will come along if a sensor
                    // comes back.
                }
            }
        }


        private void WindowClosed(object sender, EventArgs e)
        {
            sensorChooser.Stop();
            foreach (Bubble b in bubbleList)
                b.Pop();
            bubbleList.Clear();
        }

        private void KinectSensorOnAllFramesReady(object sender, AllFramesReadyEventArgs allFramesReadyEventArgs)
        {
            using (var colorImageFrame = allFramesReadyEventArgs.OpenColorImageFrame())
            {
                if (colorImageFrame == null)
                {
                    return;
                }
                if (faceTrackingViewer.FaceBeingTracked)
                {
                    imageSource.FaceTrackedColorFrameReady(colorImageFrame);
                }
            }
            // sending the point to intersect with bubbles from same object which processes the kinect skeleton
          if (imageSource.Ready)
          {
              makeBubbles();
              if (imageSource.Recording)
              {
                  spotlight.Fill = Brushes.Red;
                  LHand.Fill = Brushes.DarkSlateGray;
                  RHand.Fill = Brushes.DarkSlateGray;
              }
              else
              {
                  spotlight.Fill = Brushes.Black;                 
              }
          }
          updateBubbles(new Point(faceTrackingViewer.leftHandPosition.X * 1400, faceTrackingViewer.leftHandPosition.Y * -1300 + 100), new Point(faceTrackingViewer.rightHandPosition.X * 1400, faceTrackingViewer.rightHandPosition.Y * -1300 + 100));
          spotlight.Margin = new Thickness(faceTrackingViewer.chestPosition.X * 1400, faceTrackingViewer.chestPosition.Y * -1300 + 100,0,0);

           LHand.Margin = new Thickness(faceTrackingViewer.leftHandPosition.X * 1400, faceTrackingViewer.leftHandPosition.Y*-1300+100, 0,0);
           RHand.Margin = new Thickness(faceTrackingViewer.rightHandPosition.X * 1400, faceTrackingViewer.rightHandPosition.Y * -1300+100, 0, 0);
        }              

        private void updateBubbles(Point LHand, Point RHand)
        {
            if (bubbleList == null || bubbleList.Count == 0)
                bubbleList = BubbleList.GetDefaultBubbleList().MakeBubbles();
            else
            { 
                foreach (Bubble b in bubbleList)
                    b.Update(LHand, RHand);
            }
        }
      
        private bool enableMakeBubbles = true;          

        private void makeBubbles()
        {
            if (SoundSource.getDefaultSource().Ready && imageSource.Ready)
            {
                if (enableMakeBubbles)
                {
                    enableMakeBubbles = false;
                    BubbleList.RecordMediaForBubbles(this.MainGrid);
                }
            }         
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {      
           ImageSource.ClearFaces();
           SoundSource.getDefaultSource().sBuffer.Dispose();
           for(int i = 0; i < bubbleList.Count; i++)
           {
               bubbleList[i].Pop();
               bubbleList[i] = null;
           }
           bubbleList.Clear(); 
           bubbleList = null;  
            enableMakeBubbles = true;
            makeBubbles();
        }
    }
}
