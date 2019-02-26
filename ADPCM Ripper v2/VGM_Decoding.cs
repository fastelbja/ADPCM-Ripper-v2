using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NAudio;

namespace VGMStream
{
    public class VGM_Decoding : NAudio.Wave.IWaveProvider
    {
        private VGM_Stream m_vgmStream;
        private StreamReader.IReader m_fileReader;
        private NAudio.Wave.WaveFormat m_WavFormat;
        protected double m_FadeSamples;
        protected bool m_Flush = true;
        protected int m_FlushCount = 3;

        protected const int m_FadeSeconds = 10;


        public VGM_Decoding()
        {
        }

        public VGM_Decoding(VGM_Stream vgmStream, StreamReader.IReader fileReader)
        {
            m_vgmStream = vgmStream;
            m_fileReader = fileReader;

            if (m_WavFormat != null)
                m_WavFormat = null;

            m_WavFormat = new NAudio.Wave.WaveFormat(vgmStream.vgmSampleRate, vgmStream.vgmChannelCount);
            m_FadeSamples = (int)(m_FadeSeconds * vgmStream.vgmSampleRate);
            m_vgmStream.vgmDecodedSamples = 0;
            m_vgmStream.vgmTotalSamplesWithLoop = VGM_Utils.get_vgmstream_play_samples(2, m_FadeSeconds, 0, vgmStream);
        }

        public NAudio.Wave.WaveFormat WaveFormat
        {
            get
            {
                return m_WavFormat;
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            int i, j,k;

            int sampleToAdd = count / 2 / m_vgmStream.vgmChannelCount;

            short[] sampleBuffer = new short[sampleToAdd*m_vgmStream.vgmChannelCount];

            int sample_done = m_vgmStream.vgmLayout.update(ref sampleBuffer, sampleToAdd, m_vgmStream);

            if (sample_done < sampleToAdd)
            {
                for (i = sample_done; i < sampleToAdd; i++)
                {
                    sampleBuffer[i] = 0;
                }
            }

            /* fade! */
            if (m_vgmStream.vgmLoopFlag && m_FadeSamples > 0)
            {
                int samples_into_fade = (int)(m_vgmStream.vgmDecodedSamples - (m_vgmStream.vgmTotalSamplesWithLoop - m_FadeSamples));
                if (samples_into_fade + sampleToAdd > 0)
                {
                    for (j = 0; j < sampleToAdd; j++, samples_into_fade++)
                    {
                        if (samples_into_fade > 0)
                        {
                            double fadedness = (double)(m_FadeSamples - samples_into_fade) / m_FadeSamples;
                            for (k = 0; k < m_vgmStream.vgmChannelCount; k++)
                            {
                                sampleBuffer[j * m_vgmStream.vgmChannelCount + k] =
                                    (short)(sampleBuffer[j * m_vgmStream.vgmChannelCount + k] * fadedness);
                            }
                        }
                    }
                }
            }

            for (i = 0, j=0 ; i < count; i+=2, j++)
            {
                buffer[i] = (byte)(sampleBuffer[j] & 0x00ff);
                buffer[i + 1] = (byte)((sampleBuffer[j] >> 8) & 0x00ff);
            }

            return count;
        }
    }
}
