using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using SlimDX.DirectSound;
using NAudio.Wave;
using System.Threading;


namespace FaceBubbleMusicToy
{
    
    class SoundSource
    {
        static SoundSource defaultSource = null;
        SlimDX.DirectSound.DirectSound device;
        SlimDX.DirectSound.SecondarySoundBuffer sBuffer;
        SlimDX.Multimedia.WaveFormat waveFormat = new SlimDX.Multimedia.WaveFormat();
        SlimDX.DirectSound.SoundBufferDescription bufferDescription;
        SlimDX.DirectSound.Capabilities buffercaP; 
        int amountOfTimeToRecord = 3;
        private const int AudioPollingInterval = 50;
        private const int SamplesPerMillisecond = 16;
        private const int BytesPerSample = 2;
        private readonly byte[] audioBuffer = new byte[AudioPollingInterval * SamplesPerMillisecond * BytesPerSample];
        Stream audioStream;
        MemoryStream inputStream;
        public  bool Ready = false;
        private Process foo;
        int totalCount;
        static long samples;
        BufferFlags bflags = new BufferFlags();



 byte[] byteBuffer = new byte[1024];



        internal static SoundSource getDefaultSource()
        {
                if (defaultSource == null)
                    defaultSource = new SoundSource();
            
                return defaultSource;
        }

        private SoundSource()
        {

        }

        public DirectSound GetDevice()
        {
            return device;
        }

        public void initialize(Stream audioStream)
        {
            inputStream = new MemoryStream();
            device = new DirectSound(SlimDX.DirectSound.DirectSoundGuid.DefaultPlaybackDevice);
            waveFormat.FormatTag = SlimDX.Multimedia.WaveFormatTag.Pcm;
            waveFormat.BitsPerSample = 16;
            waveFormat.BlockAlignment = 2;
            waveFormat.Channels = 1;
            waveFormat.SamplesPerSecond = 16000;
            waveFormat.AverageBytesPerSecond = 32000;
            bufferDescription = new SlimDX.DirectSound.SoundBufferDescription();
            bufferDescription.Format = waveFormat;
            bufferDescription.Flags = BufferFlags.ControlFrequency | BufferFlags.ControlVolume;

            bufferDescription.SizeInBytes = amountOfTimeToRecord * 2 * waveFormat.AverageBytesPerSecond;
            this.audioStream = audioStream;            
            Ready = true;
        }
        

        int recordingLength;
      
        public void RecordAudio()
        {
            BubbleList.DesignateInputStream(inputStream);
            Console.WriteLine("SoundSource_recordAudio");
            var t = new Thread(new ParameterizedThreadStart((Record)));
            t.Start();
         
                     recordingLength = amountOfTimeToRecord * 2 * 16000;
           
           
           WriteWavHeader(inputStream, recordingLength);             
            
         
        }

        private void Record(object foo)
        {

            int count, totalCount = 0;
            while ((count = audioStream.Read(byteBuffer, 0, byteBuffer.Length)) > 0 && totalCount < recordingLength)
            {
                inputStream.Write(byteBuffer, 0, count);
                //sBuffer.Write(byteBuffer, totalCount, LockFlags.None);

                totalCount += count;
                }
            samples = totalCount;
            FinishedRecording();
            
           
        }

        public SecondarySoundBuffer GetSound(MemoryStream inputStream)
        {
           sBuffer = new SlimDX.DirectSound.SecondarySoundBuffer(device, bufferDescription);           
           sBuffer = MakeForwardAndReverseSound(inputStream, recordingLength, sBuffer);
            
            return sBuffer;
        }
        
        private SecondarySoundBuffer MakeForwardAndReverseSound(MemoryStream inputStream, int recordingLength, SecondarySoundBuffer sBuffer)
        {
            using (MemoryStream outputStream = new MemoryStream())
            {
                WriteWavHeader(outputStream, recordingLength);
                inputStream.Position = 0;


                using (WaveFileReader reader = new WaveFileReader(inputStream))
                {
                    int blockAlign = reader.WaveFormat.BlockAlign;
                    using (WaveFileWriter writer = new WaveFileWriter(outputStream, reader.WaveFormat))
                    {
                        byte[] buffer = new byte[blockAlign];
                        long samples = reader.Length / blockAlign;
                       totalCount = 0;
                        ///copy sound to new byteBuffer
                        for (long sample = 0; sample < samples - 1; sample++)
                        {
                            reader.Position = sample * blockAlign;
                            reader.Read(buffer, 0, blockAlign);
                            writer.Write(buffer, 0, blockAlign);
                        }

                        outputStream.Position = 0;
                        int count = 0;
                        buffer = new byte[1024];
                        while ((count = outputStream.Read(buffer, 0, buffer.Length)) > 0 && totalCount < recordingLength)
                        {
                            sBuffer.Write(buffer, totalCount, LockFlags.None);
                            totalCount += count;
                        }

                        long halfPosition = outputStream.Position;
                        //// REVERSE WRITE

                        for (long sample = samples - 1; sample >= 0; sample--)
                        {
                            reader.Position = sample * blockAlign;
                            reader.Read(buffer, 0, blockAlign);
                            writer.Write(buffer, 0, blockAlign);
                        }

                        // write the reversed audio to the sound buffer
                        outputStream.Position = halfPosition;
                        count = 0;
                        buffer = new byte[1024];
                        while ((count = outputStream.Read(buffer, 0, buffer.Length)) > 0 && totalCount < (recordingLength * 2))
                        {
                            sBuffer.Write(buffer, totalCount, LockFlags.None);
                            totalCount += count;
                        }
                  //      samples = totalCount;
                    }
                }
            }

            return sBuffer;
        }


       void FinishedRecording()
        {
         long faceFrameTime =   ImageSource.GetDefaultSource().StopRecording();
         
           BubbleList.GetDefaultBubbleList().SoundReady(faceFrameTime, samples);
        }

       static void WriteWavHeader(Stream stream, int dataLength)
       {
           //We need to use a memory stream because the BinaryWriter will close the underlying stream when it is closed
           using (var memStream = new MemoryStream(64))
           {
               int cbFormat = 18; //sizeof(WAVEFORMATEX)
               WAVEFORMATEX format = new WAVEFORMATEX()
               {
                   wFormatTag = 1,
                   nChannels = 1,
                   nSamplesPerSec = 16000,
                   nAvgBytesPerSec = 32000,
                   nBlockAlign = 2,
                   wBitsPerSample = 16,
                   cbSize = 0
               };

               using (var bw = new BinaryWriter(memStream))
               {
                   //RIFF header
                   WriteString(memStream, "RIFF");
                   bw.Write(dataLength + cbFormat + 4); //File size - 8
                   WriteString(memStream, "WAVE");
                   WriteString(memStream, "fmt ");
                   bw.Write(cbFormat);

                   //WAVEFORMATEX
                   bw.Write(format.wFormatTag);
                   bw.Write(format.nChannels);
                   bw.Write(format.nSamplesPerSec);
                   bw.Write(format.nAvgBytesPerSec);
                   bw.Write(format.nBlockAlign);
                   bw.Write(format.wBitsPerSample);
                   bw.Write(format.cbSize);

                   //data header
                   WriteString(memStream, "data");
                   bw.Write(dataLength);
                   memStream.WriteTo(stream);
               }
           }
       }

       static void WriteString(Stream stream, string s)
       {
           byte[] bytes = Encoding.ASCII.GetBytes(s);
           stream.Write(bytes, 0, bytes.Length);
       }

       struct WAVEFORMATEX
       {
           public ushort wFormatTag;
           public ushort nChannels;
           public uint nSamplesPerSec;
           public uint nAvgBytesPerSec;
           public ushort nBlockAlign;
           public ushort wBitsPerSample;
           public ushort cbSize;
       }

      
    }
}
