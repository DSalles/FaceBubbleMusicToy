using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using ShaderEffectsLibrary;
using System.Diagnostics;
using System.Threading;
using SlimDX.DirectSound;
using NAudio.Wave;

namespace FaceBubbleMusicToy
{    

    class Bubble : Image
    {
        DirectSound device;
        public Image faceImage = new Image();
        BitmapImage image = new BitmapImage(new Uri("C:\\Users\\Diana\\Documents\\Games\\FaceBubbles\\FaceBubbles\\Images\\bubble.png"));
        long faceFrameTime;
        FacesList faceImages;
        int lastFrame;
        bool ping = true;
        SecondarySoundBuffer sBuffer;
        Stopwatch frameCounter = new Stopwatch();
        private long facePlayFrameTime;
        private long samples;
        private bool WindingDown = false;
        int faceFrameOffset = 0;
        int bufferOffset = 0;
        private int attenuation = 0;
        Rect bubbleZone;
        Grid mainGrid;

        public Bubble(SecondarySoundBuffer sBuffer, int frequency, DirectSound device, Thickness margin, FacesList facesList, long faceFrameTime, long bufferSamples, Grid mainGrid)
      {
         
          faceImages = facesList;
          effect = new Monochrome();
          circularVignette = new CircularVignette();
          this.expired = false;
          this.Margin = margin;
          faceImage.Margin = this.Margin;
          this.Width = 110;
          faceImage.Width = this.Width;
          this.Source = image;
          this.faceImage.Effect = circularVignette;
          this.Effect = effect;
          IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
          this.device = device;
          this.device.SetCooperativeLevel(handle, CooperativeLevel.Priority);
          this.sBuffer = sBuffer;
          this.faceFrameTime = faceFrameTime;
          samples = bufferSamples;
          sBuffer.Stop();
          sBuffer.Volume = -1000;
          sBuffer.Frequency = frequency;
          this.mainGrid = mainGrid;
          this.mainGrid.Children.Add(this);
          this.mainGrid.Children.Add(faceImage);
          frameCounter.Start();
          bubbleZone = new Rect(Margin.Left - 100, Margin.Top - 100, 200, 200);
          if (sBuffer.Frequency - 10680 < 0)
          {
              bufferOffset = (int)(sBuffer.Capabilities.BufferSize * .1);
              faceFrameOffset = (int)(Math.Abs(sBuffer.Frequency - 10680) / 400);
          }

          float frameScaleFactor = 15000 / (float)sBuffer.Frequency;

          facePlayFrameTime = (long)(faceFrameTime * Math.Pow(frameScaleFactor, 1));
          lastFrame = faceFrameOffset;
          idleEndBufferPosition = sBuffer.Capabilities.BufferSize* (idleEndFrame / faceImages.Count) + bufferOffset;
      }



        private bool StartedWindingUp = false;

      internal void StartWindingUp()
        {
            sBuffer.Play(0, PlayFlags.Looping);
          if (!StartedWindingUp)
          {
              StartedWindingUp = true;
              if (State == "idle")
              {
                  lastFrame = faceFrameOffset - 1;
                  this.sBuffer.CurrentPlayPosition = idleEndBufferPosition;
              }
              State = "windingUp";
              WindingUp();
          }
      }


      List<Bubble> spawnList = new List<Bubble>();

      internal void  WindingUp()
        {
      //    spawnList.Add(new Bubble(this.sBuffer,this.sBuffer.Frequency, device,  (new Thickness(lastFrame*2+this.Margin.Left, lastFrame*2+this.Margin.Top, 0,0)), faceImages, faceFrameTime, samples,mainGrid));
          //this.mainGrid.Children.Add(spawnList[spawnList.Count - 1]);  
          int volume = this.sBuffer.Volume;
            int width = (int)this.Width;
            
            StartedWindingDown = false;
            State = "windingUp";

            if (sBuffer.Volume >= 0)
                sBuffer.Volume = 0;
            else
                volume += 1000;
            if (width <= 500)
                width += 10;
            if (ping)
            {
                if (lastFrame >= faceImages.Count - 1)
                {
                    lastFrame = faceImages.Count - 1;

                    ping = false;
                }
                else
                {
                    lastFrame += 1;
                }
            }
            else
                if (!ping)
                {
                    if (lastFrame <= faceFrameOffset)
                    {
                        lastFrame = faceFrameOffset;
                        ping = true;
                       
                        sBuffer.CurrentPlayPosition = idleEndBufferPosition;
                    }
                    else
                    {
                        lastFrame -= 1;
                    }
                } 
          if (volume >= 0)
                volume = 0;
            UpdateAnimation(width, volume, lastFrame);
         //   bubbleZone = new Rect(Margin.Left - width/2, Margin.Top - width/2, width, width);
        }

      private bool StartedWindingDown = false;

      internal void StartWindingDown()
      {
          if (!StartedWindingDown)
          {
              StartedWindingDown = true;
              State = "windingDown";
              if (sBuffer.CurrentPlayPosition <= sBuffer.Capabilities.BufferSize / 2 && sBuffer.CurrentPlayPosition > 0)
                  sBuffer.CurrentPlayPosition = sBuffer.Capabilities.BufferSize - sBuffer.CurrentPlayPosition;
              WindDown();
          }         
      }

      internal void WindDown()
      {
          StartedWindingUp = false;
          int width = (int)this.Width;
          int volume = this.sBuffer.Volume;
          State = "windingDown";
          if (lastFrame <= 0)
          {
              lastFrame = 0;             
              ping = true;
              width = 110;
              sBuffer.Stop();
              Idle();
          }
          else
          {
              if (this.Width >= 110)
                  width -= (int)((this.Width - 110) / (lastFrame));
              volume -= (volume + 1000) / lastFrame;
              lastFrame -= 1;
         

          UpdateAnimation(width, volume, lastFrame);
          }
      }
    static    int idleEndFrame = 7;
    int idleEndBufferPosition;
      internal void Idle()
      {
          State = "idle";
            if (ping)
                      {
                          if (lastFrame >= idleEndFrame)
                          {
                              lastFrame = idleEndFrame;
                         
                              ping = false;
                          }
                          else
                          {
                              lastFrame += 1;
                          }
                      }
                  else
                      if (!ping)
                      {
                          if (lastFrame <= 0)
                          {
                              lastFrame = 0;
                              ping = true;                            
                          }
                          else
                          {
                              lastFrame -= 1;
                          }
                      }
            sBuffer.CurrentPlayPosition = idleEndBufferPosition;
          UpdateAnimation(110, -10000, lastFrame);
      }

      internal Boolean checkStopWatch()
      {
          if (frameCounter.Elapsed.Ticks >= faceFrameTime)
              return true;
          else
              return false;
      }


      internal void UpdateAnimation(int widthScale, int volume, int lastFrame)
      {
          this.Width = widthScale;
          this.faceImage.Width = widthScale;
          this.sBuffer.Volume = volume;
          this.faceImage.Source = faceImages[lastFrame].faceBitmap;
      }
        string State = "idle";


        internal void Update(Point LHand, Point RHand)
        {
          if(checkStopWatch())
            {
                
                Rect LHandRect = new Rect(LHand.X - 30, LHand.Y - 100, 60, 60);
                Rect RHandRect = new Rect(RHand.X-30, RHand.Y - 100, 60, 60);
                if (bubbleZone.IntersectsWith(LHandRect) || bubbleZone.IntersectsWith(RHandRect))
                { // 
                    if (State == "idle" || State == "windingDown")
                        StartWindingUp();
                    else if (State == "windingUp")
                        WindingUp();
                }
                else
                {
                    if (State == "idle")
                        Idle();
                    else if (State == "windingDown")
                        WindDown();
                    else if (State == "windingUp")
                    {
                        StartWindingDown();
                    }
                }
            }
        }
              

   internal void PlayFaceFrames(int endFrame, int endPosition)
      {
          if (!WindingDown && this.Width + 50 <= 1000)
              this.Width += 10;
          else // winding down
              if (this.Width >= 110)
                  this.Width -= (this.Width - 110) / (lastFrame - (3 + faceFrameOffset));
          this.faceImage.Width = this.Width;
          if (faceImages.Count != 0)
          {              
              if (sBuffer.Volume + attenuation <= 0 && sBuffer.Volume + attenuation >=-1000)
              sBuffer.Volume += attenuation;
              if (sBuffer.Volume >= 0)
                  sBuffer.Volume = 0;
                  if (ping)
                  {
                      if (lastFrame >= faceImages.Count - 1)
                      {
                          lastFrame = faceImages.Count - 1;
                         
                          ping = false;
                      }
                      else
                      {
                          lastFrame += 1;
                      }
                  }
                  else
                      if (!ping)
                      {
                          if (lastFrame <= endFrame)
                          {
                              lastFrame = endFrame;
                              ping = true;
                              
                              sBuffer.CurrentPlayPosition = endPosition;                             
                          }
                          else
                          {
                              lastFrame -= 1;
                          }
                      }
              }

                this.faceImage.Source = faceImages[lastFrame].faceBitmap;          
      }



      internal void Pop()
      {
          //sBuffer.Stop();
          //if(sBuffer.Status != BufferStatus.Playing)
          expired = true;
          sBuffer.Dispose();
      }

      public bool expired { get; set; }

      public Monochrome effect { get; set; }
      public CircularVignette circularVignette { get; set; }
    }

     class BufferSamples 
    {
      public  int samples = 0;
    }

}


