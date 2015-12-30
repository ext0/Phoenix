using NAudio.Wave;
using PhoenixLib.TCPLayer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhoenixLib.Datastore.Audio
{
    public static class Microphone
    {
        public static bool recording { get; internal set; }
        public static byte[] dataBacking = new byte[0];
        private static WaveInEvent waveSource;
        public static void startRecording(int rate, int bits, int channel)
        {
            if (recording)
                return;
            waveSource = new WaveInEvent();
            waveSource.WaveFormat = new WaveFormat(rate, bits, channel);
            waveSource.DataAvailable += dataAvailableCallback;
            waveSource.StartRecording();
            waveSource.BufferMilliseconds = 100;
            recording = true;
        }
        public static void stopRecording()
        {
            if (!recording)
                return;
            waveSource.DataAvailable -= dataAvailableCallback;
            waveSource.StopRecording();
            recording = false;
        }
        public static void clearBuffer()
        {

        }
        internal static void dataAvailableCallback(object sender, WaveInEventArgs e)
        {
            try
            {
                dataBacking = dataBacking.Concat(e.Buffer).ToArray();
            }
            catch (Exception exc)
            {
                Debug.WriteLine("Error in data callback! " + exc.Message);
            }
        }
    }

    public static class Playback
    {
        public static WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
        public static Queue<byte[]> playbackData = new Queue<byte[]>();
        public static WaveFormat currentWavFormat;
        public static bool initialized = false;
        public static bool playingContent = false;
        public static bool dataLock = false;
        public static void init(int rate, int bits, int channel)
        {
            if (initialized)
                return;
            currentWavFormat = new WaveFormat(rate, bits, channel);
            initialized = true;
        }
        public static void playback()
        {
            try
            {

                if (playingContent)
                    return;
                if (playbackData.Count != 0)
                {
                    byte[] data = playbackData.Dequeue();
                    if (data.Length == 0)
                    {
                        playback();
                        return;
                    }
                    IWaveProvider provider = new RawSourceWaveStream(
                             new MemoryStream(data), currentWavFormat);
                    playingContent = true;
                    waveOut.Init(provider);
                    waveOut.Play();
                    while (waveOut.PlaybackState.Equals(PlaybackState.Playing))
                    {
                        Thread.Sleep(1);
                    }
                    playingContent = false;
                    playback();
                    return;
                }
                playingContent = false;
            }
            catch
            {
                return;
            }
        }
        public static void addToQueue(byte[] data)
        {
            lock (playbackData)
            {
                playbackData.Enqueue(data);
            }
        }
        public static void writeToFile(byte[] data, String path)
        {
            WaveFileWriter.CreateWaveFile(path, new RawSourceWaveStream(new MemoryStream(data), currentWavFormat));
        }
    }
}
