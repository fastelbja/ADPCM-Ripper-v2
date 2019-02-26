using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using VGMStream;

namespace ADPCM_Ripper_v2
{
    [DefaultPropertyAttribute("Name")]
    public class AudioFile
    {
        private UInt64 m_Offset;
        private UInt64 m_Size;
        private uint m_SampleRate;
        private uint m_Channels;
        private string m_Decoder;
        private string m_Layout;
        private string m_Interleave;

        private string m_SampleCount;
        private bool m_Looped;
        private int m_LoopStartSample;
        private string m_LoopStartTime;
        private int m_LoopEndSample;
        private string m_LoopEndTime;

        private string m_Filename;
        private string m_FileOwner;
        private string m_Description;

        [CategoryAttribute("Audio File"), DescriptionAttribute("Start offset")]
        public UInt64 Offset
        {
            get { return m_Offset; }
            set { m_Offset = value; }
        }

        [CategoryAttribute("Audio File"), DescriptionAttribute("Filelength")]
        public UInt64 Size
        {
            get { return m_Size; }
            set { m_Size = value; }
        }

        [CategoryAttribute("Audio File"), DescriptionAttribute("Total Sample count (with loop)")]
        public string SampleCount
        {
            get { return m_SampleCount; }
            set { m_SampleCount = value; }
        }

        [CategoryAttribute("Audio File"), DescriptionAttribute("Sample Rate")]
        public uint SampleRate
        {
            get { return m_SampleRate; }
            set { m_SampleRate = value; }
        }

        [CategoryAttribute("Audio File"), DescriptionAttribute("Channel count")]
        public uint Channels
        {
            get { return m_Channels; }
            set { m_Channels = value; }
        }

        [CategoryAttribute("Audio File"), DescriptionAttribute("Description of the decoder used to play audio file")]
        public string Decoder
        {
            get { return m_Decoder; }
            set { m_Decoder = value; }
        }

        [CategoryAttribute("Audio File"), DescriptionAttribute("Description of the layout used to play audio file")]
        public string Layout
        {
            get { return m_Layout; }
            set { m_Layout = value; }
        }

        [CategoryAttribute("Audio File"), DescriptionAttribute("Value of interleave if exists")]
        public string Interleave
        {
            get { return m_Interleave; }
            set { m_Interleave = value; }
        }

        [CategoryAttribute("Loop Info"), DescriptionAttribute("Indicate if the audio is looped")]
        public bool Looped
        {
            get { return m_Looped; }
            set { m_Looped = value; }
        }

        [CategoryAttribute("Loop Info"), DescriptionAttribute("Start of the loop (in samples)")]
        public int LoopStart
        {
            get { return m_LoopStartSample; }
            set { m_LoopStartSample = value; }
        }

        [CategoryAttribute("Loop Info"), DescriptionAttribute("Start of the loop (in time)")]
        public string LoopTimeStart
        {
            get { return m_LoopStartTime; }
            set { m_LoopStartTime = value; }
        }

        [CategoryAttribute("Loop Info"), DescriptionAttribute("End of the loop (in samples)")]
        public int LoopEnd
        {
            get { return m_LoopEndSample; }
            set { m_LoopEndSample = value; }
        }

        [CategoryAttribute("Loop Info"), DescriptionAttribute("End of the loop (in time)")]
        public string LoopTimeEnd
        {
            get { return m_LoopEndTime; }
            set { m_LoopEndTime = value; }
        }

        [CategoryAttribute("File Info"), DescriptionAttribute("Filename")]
        public string Filename
        {
            get { return m_Filename; }
            set { m_Filename = value; }
        }

        [CategoryAttribute("File Info"), DescriptionAttribute("Description")]
        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }

        [CategoryAttribute("File Info"), DescriptionAttribute("File in which audio file is found")]
        public string Owner
        {
            get { return m_FileOwner; }
            set { m_FileOwner = value; }
        }

    }
}
