using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VGMStream;

namespace ADPCM_Ripper_v2
{
    class Mediafile
    {
        public enum TypeFileEntry
        {
            AUDIO,
            MOVIE,
            GFX,
            BINARY
        }

        public struct FileEntry
        {
            public string filename;
            public UInt64 Offset;
            public UInt64 Size;
            public int Frequency;
            public int Channels;
            public int SampleCount;
            public TypeFileEntry typeFile;
            public string Description;
            public string Extension;
            public bool isChecked;
            public string isLooped;
            public string FileOwner;
            public string decoder;
            public string layout;
            public IVGMFormat Format;
            public byte SessionID;

            public int LoopStartSample;
            public int LoopEndSample;
            public uint Interleave;

            public FileEntry(string _filename, UInt64 _Offset, UInt64 _Size, int _Frequency, int _Channels, int _SampleCount, TypeFileEntry _TypeFileEntry, string _Description, string _Extension, IVGMFormat _Format, bool _isLooped, string _FileOwner, int loopStart, int loopEnd,string _decoder, string _layout, uint _interleave, byte _sessionID)
            {
                filename = _filename;
                
                if (filename == string.Empty)
                {
                    filename = System.IO.Path.GetFileNameWithoutExtension(_FileOwner) + "_" + String.Format("{0:X8}", _Offset);
                }

                Offset = _Offset;
                Size = _Size;
                Frequency = _Frequency;
                Channels = _Channels;
                SampleCount = _SampleCount;
                typeFile = _TypeFileEntry;
                Description = _Description;
                Extension = _Extension;
                FileOwner = _FileOwner;
                isLooped = (_isLooped ? "Yes" : "No");
                Format = _Format;
                isChecked = true;
                LoopStartSample = loopStart;
                LoopEndSample = loopEnd;
                decoder = _decoder;
                layout = _layout;
                SessionID = _sessionID;
                Interleave = _interleave;
            }
        }

        public static List<FileEntry> MediaList = new List<FileEntry>();
    }
}
