using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StreamReader;
using VGMStream;

namespace ADPCM_Ripper_v2
{
    class VGM_Ripper
    {
        //private delegate Int32 IsFormat(StreamReader.IReader fileReader, Int64 offset, Int32 length);
        public delegate void FileFound();
        public delegate void UpdateProgress(UInt64 value, UInt64 max);

        private bool m_Cancel;
        public bool Cancel 
        {
            get { return m_Cancel; }
            set { m_Cancel = value; }
        }

        private const uint MAX_NEEDED_BLOCK = 0x800;

        public void OldDoRip(IReader fReader, UInt64 Offset, UInt64 fileLength,string fileOwner, string sPath,byte sessionID, UpdateProgress updatep, bool deepSearch)
        {
            UInt64 LengthOfRip;
            UInt64 currentOffset=0;
            string filename;

            this.Cancel = false;

            VGM_Stream vgmStream = new VGM_Stream();

            List<IVGMFormat> fntToCall = new List<IVGMFormat>();

            fntToCall.Add(new PS2_SFS()); fntToCall.Add(new PS2_VIG()); fntToCall.Add(new CMN_ADX());
            fntToCall.Add(new PS2_SVAG()); fntToCall.Add(new PS2_VAG()); fntToCall.Add(new WII_BRSTM());
            fntToCall.Add(new NGC_CAF()); fntToCall.Add(new NGC_THP()); fntToCall.Add(new PS2_ADS());

            do
            {
                LengthOfRip = 0;
                filename = string.Empty;

                updatep(currentOffset, fileLength);
                foreach (IVGMFormat ripFormat in fntToCall)
                {
                    fReader.SetSessionID(sessionID);
                    LengthOfRip = ripFormat.IsFormat(fReader, currentOffset + Offset, fileLength);
                    if (LengthOfRip != 0)
                    {
                        ripFormat.Init(fReader, currentOffset + Offset, ref vgmStream,false, fileLength);
                        filename = ripFormat.Filename;

                        if (LengthOfRip == fileLength)
                            filename = sPath;

                        if (ripFormat.IsPlayable)
                        {
                            vgmStream.vgmTotalSamplesWithLoop = VGM_Utils.get_vgmstream_play_samples(2, 10, 0, vgmStream);
                        }

                        Mediafile.MediaList.Add(new Mediafile.FileEntry(filename, currentOffset + Offset, LengthOfRip,
                            vgmStream.vgmSampleRate, vgmStream.vgmChannelCount,
                            vgmStream.vgmTotalSamplesWithLoop, Mediafile.TypeFileEntry.AUDIO,
                            ripFormat.Description, ripFormat.Extension, ripFormat, vgmStream.vgmLoopFlag, fileOwner, vgmStream.vgmLoopStartSample, vgmStream.vgmLoopEndSample, vgmStream.vgmDecoder.Description, vgmStream.vgmLayout.GetDescription(), (uint)vgmStream.vgmInterleaveBlockSize, sessionID));
                        currentOffset += LengthOfRip;
                    }
                }

                if ((currentOffset % 0x10) != 0)
                    currentOffset += (currentOffset % 0x10);

                if (LengthOfRip == 0)
                    currentOffset += 0x10;

                if (this.Cancel)
                    break;

            } while ((currentOffset < fileLength) && (deepSearch));
        }

        public void DoRip(IReader fReader, UInt64 Offset, UInt64 fileLength, string fileOwner, string sPath, byte sessionID, UpdateProgress updatep, bool deepSearch)
        {
            byte[] buffer = new byte[0x8000];
            
            UInt64 currentOffset=0;
            UInt64 startOffset = Offset;

            UInt64 LengthOfRip = 0;
            uint i=0;
            uint j;
            uint previous_interleave = 0;

            IVGMFormat ripFormat = null;
            VGM_Stream vgmStream = new VGM_Stream();

            if (sPath.ToString().Contains("00.brstm"))
                i=0;

            uint channel_count=0;
            uint sample_rate=0;
            uint interleave=0;
            int blockCount;

            string filename;
            byte[] emptyLine = new byte[16*3];
            byte[] endPS2ADPCM = { 0x07, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77 };

            byte[] tstBuffer = new byte[16*3];

            for (i = 0; i < 16; i++)
                emptyLine[i] = 0;

            do
            {

                updatep(currentOffset, fileLength);

                filename = string.Empty;
                i = 0;

                if (Offset >= 23855104)
                    i = i;

                if ((currentOffset + (UInt64)buffer.Length) > fileLength)
                    buffer = fReader.Read(Offset, (uint)(fileLength - currentOffset));
                else
                    buffer = fReader.Read(Offset, (uint)buffer.Length);

                if (buffer.Length > MAX_NEEDED_BLOCK)
                {
                    do
                    {
                        LengthOfRip = 0;

                        if (this.Cancel)
                            return;

                        #region CMN_ADX
                        // check for ADX
                        if (MemoryReader.ReadIntBE(ref buffer, i) == 0x8000)
                        {
                            // Check encoding type ...
                            if (buffer[i + 4] == 3)
                            {
                                // Check Frame Size ...
                                if (buffer[i + 5] == 18)
                                {
                                    // Check Bit per Samples ...
                                    if (buffer[i + 6] == 4)
                                    {
                                        // Check for channel count & sample rate
                                        sample_rate = MemoryReader.ReadLongBE(ref buffer, i + 0x08);
                                        channel_count = buffer[i + 0x07];

                                        if((VGM_Utils.CheckChannels(channel_count)) && 
                                           (VGM_Utils.CheckSampleRate(sample_rate)))
                                        {
                                            // Search for the (c)CRI signature ...
                                            UInt64 criOffset = MemoryReader.ReadIntBE(ref buffer, i + 2);
                                            if ((fReader.Read_32bitsBE(Offset + i + criOffset - 4) == 0x00002863) &&
                                                (fReader.Read_32bitsBE(Offset + i + criOffset) == 0x29435249))
                                            {
                                                // Find file length ...
                                                UInt16 version_signature = MemoryReader.ReadIntBE(ref buffer, i + 0x12);
                                                UInt32 totalSamples = MemoryReader.ReadLongBE(ref buffer, i + 0x0C);
                                                UInt64 searchOffset = Offset + (totalSamples * channel_count) / 32 * 18 + criOffset + i + 4;

                                                while (fReader.Read_16bitsBE(searchOffset) != 0x8001)
                                                {
                                                    searchOffset++;
                                                    if (!fReader.CanRead())
                                                        break;
                                                }

                                                if(version_signature==0x408)
                                                    ripFormat = new CMN_ADXENC();
                                                else
                                                    ripFormat = new CMN_ADX();
                                                LengthOfRip = (uint)((searchOffset - (Offset + i)) + fReader.Read_16bitsBE(searchOffset + 2) + 4);
                                                goto found;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        #region NGC_CAF
                        // Check for Mark ID ("CAF ")
                        if (MemoryReader.ReadLongBE(ref buffer, i) == 0x43414620)
                        {
                            blockCount = (int)MemoryReader.ReadLongBE(ref buffer, i + 0x24) - 1;

                            bool isCAF = true;
                            UInt64 cafOffset = Offset + i;

                            if ((blockCount < 0x100) && (blockCount>0))
                            {
                                for (j = 0; j < blockCount; j++)
                                {
                                    uint nextOffset = fReader.Read_32bitsBE(cafOffset + 0x4);

                                    if (fReader.Read_32bitsBE(cafOffset) != 0x43414620)
                                    {
                                        isCAF = false;
                                        break;
                                    }

                                    if (fReader.Read_32bitsBE(cafOffset + 0x08) != j)
                                    {
                                        isCAF = false;
                                        break;
                                    }

                                    cafOffset += nextOffset;
                                }

                                if (isCAF)
                                {
                                    ripFormat = new NGC_CAF();

                                    LengthOfRip = (uint)fileLength;
                                    ripFormat.Init(fReader, Offset + i, ref vgmStream, false,LengthOfRip);
                                    goto found;
                                }
                            }
                        }
                        #endregion

                        #region NGC_THP
                        // Check for Mark ID ("THP")
                        if (MemoryReader.ReadLongBE(ref buffer, i) == 0x54485000)
                        {
                            // check if thp has audio 
                            if (MemoryReader.ReadLongBE(ref buffer, i + 0x0C) != 0)
                            {
                                int thpVersion = buffer[i + 0x06];
                                uint componentTypeOffset = MemoryReader.ReadLongBE(ref buffer, i + 0x20) + i;

                                if (componentTypeOffset < MAX_NEEDED_BLOCK)
                                {
                                    uint numComponents = MemoryReader.ReadLongBE(ref buffer, componentTypeOffset);
                                    uint componentDataOffset = componentTypeOffset + 0x14;

                                    componentTypeOffset += 4;

                                    if (componentTypeOffset < MAX_NEEDED_BLOCK)
                                    {
                                        for (j = 0; j < numComponents; j++)
                                        {
                                            if (buffer[componentTypeOffset + j] == 1) // audio block
                                            {
                                                channel_count = MemoryReader.ReadLongBE(ref buffer, componentDataOffset);
                                                sample_rate = MemoryReader.ReadLongBE(ref buffer, componentDataOffset + 4);
                                            }
                                            else
                                            {
                                                if (thpVersion == 0x10)
                                                    componentDataOffset += 0x0c;
                                                else
                                                    componentDataOffset += 0x08;
                                            }
                                        }

                                        if (VGM_Utils.CheckChannels(channel_count))
                                        {
                                            if (VGM_Utils.CheckSampleRate(sample_rate))
                                            {
                                                ripFormat = new NGC_THP();
                                                LengthOfRip = (uint)fileLength;
                                                ripFormat.Init(fReader, Offset + i, ref vgmStream, false,LengthOfRip);
                                                goto found;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        #region PS2_ADS
                        // Check for Mark ID ("SShd")
                        if (MemoryReader.ReadLongBE(ref buffer, i) == 0x53536864)
                        {
                            // Check for Mark ID ("SSbd")
                            if (MemoryReader.ReadLongBE(ref buffer, i + 0x20) == 0x53536264)
                            {
                                channel_count = MemoryReader.ReadLong(ref buffer, i + 0x10);
                                sample_rate = MemoryReader.ReadLong(ref buffer, i + 0x0C);

                                if ((VGM_Utils.CheckChannels(channel_count)) &&
                                    (VGM_Utils.CheckSampleRate(sample_rate)))
                                {
                                    ripFormat = new PS2_ADS();

                                    LengthOfRip = MemoryReader.ReadLong(ref buffer, i + 0x024) + 0x28;
                                    goto found;
                                }
                            }
                        }
                        #endregion

                        #region PS2_AUS
                        // Check for Mark ID ("AUS ")
                        if (MemoryReader.ReadLongBE(ref buffer, i) == 0x41555320)
                        {
                            channel_count = MemoryReader.ReadLong(ref buffer, i + 0x0C);
                            sample_rate = MemoryReader.ReadLong(ref buffer, i + 0x10);

                            if (VGM_Utils.CheckChannels(channel_count))
                            {
                                if (VGM_Utils.CheckSampleRate(sample_rate))
                                {
                                    ripFormat = new PS2_AUS();

                                    LengthOfRip = (MemoryReader.ReadLong(ref buffer, i + 0x08) / 28 * 16) * channel_count;
                                    LengthOfRip += 0x800;
                                    if (LengthOfRip <= fileLength)
                                        goto found;
                                }
                            }
                        }
                        #endregion

                        #region PS2_NPSF
                        // Check for Mark ID ("NPSF")
                        if (MemoryReader.ReadLongBE(ref buffer, i) == 0x4E505346)
                        {
                            channel_count = MemoryReader.ReadLong(ref buffer, i + 0x0C);
                            sample_rate = MemoryReader.ReadLong(ref buffer, i + 0x18);

                            if (VGM_Utils.CheckChannels(channel_count))
                            {
                                if (VGM_Utils.CheckSampleRate(sample_rate))
                                {
                                    if (MemoryReader.ReadLong(ref buffer, i + 0x10) == 0x800)
                                    {
                                        LengthOfRip = MemoryReader.ReadLong(ref buffer, i + 0x08);

                                        filename = MemoryReader.GetString(ref buffer, i + 0x34);

                                        if (channel_count == 1)
                                            LengthOfRip += 0x800;
                                        else
                                        {
                                            blockCount = (int)(LengthOfRip / 0x800);
                                            if ((LengthOfRip % 0x800) != 0)
                                                blockCount++;

                                            LengthOfRip = (uint)((blockCount * channel_count * 0x800) + 0x800);
                                        }
                                        ripFormat = new PS2_NPSF();
                                        goto found;
                                    }
                                }
                            }
                        }
                        #endregion

                        #region PS2_SFS
                        // Check for Mark ID ("STER")
                        if (MemoryReader.ReadLongBE(ref buffer, i) == 0x53544552)
                        {
                            // 0x04 = Length of each channels in byte
                            // 0x08 = Length of each channels in byte (in Little Endian Format)
                            if (MemoryReader.ReadLong(ref buffer, i + 0x04) == MemoryReader.ReadLongBE(ref buffer, i + 0x0C))
                            {
                                // Check for Sample Rate
                                sample_rate = MemoryReader.ReadLongBE(ref buffer, i + 0x10);

                                if (VGM_Utils.CheckSampleRate(sample_rate))
                                {
                                    // others data must be equal to 0
                                    if ((MemoryReader.ReadLong(ref buffer, i + 0x14) == 0) &&
                                        (MemoryReader.ReadLong(ref buffer, i + 0x18) == 0) &&
                                        (MemoryReader.ReadLong(ref buffer, i + 0x1C) == 0))
                                    {
                                        // Try to find file Length
                                        ripFormat = new PS2_SFS();
                                        LengthOfRip = MemoryReader.ReadLong(ref buffer, i + 0x04) * 2;
                                        goto found;
                                    }
                                }
                            }
                        }
                        #endregion

                        #region PS2_SVAG
                        // Check for Mark ID ("Svag")
                        if (MemoryReader.ReadLongBE(ref buffer, i) == 0x53766167)
                        {
                            // Check for Sample Rate
                            sample_rate = MemoryReader.ReadLong(ref buffer, i + 0x08);
                            if (VGM_Utils.CheckSampleRate(sample_rate))
                            {
                                // Check Channels count
                                channel_count = MemoryReader.ReadLong(ref buffer, i + 0x0c);
                                if (VGM_Utils.CheckChannels(channel_count))
                                {
                                    // Values from 0 -> 0x1C are duplicated at offset 0x400
                                    bool isSvag = true;

                                    for (j = 0; j < 0x1C / 4; j++)
                                    {
                                        if (MemoryReader.ReadLong(ref buffer, i + j) != MemoryReader.ReadLong(ref buffer, i + 0x400 + j))
                                            isSvag = false;
                                    }

                                    if (isSvag)
                                    {
                                        ripFormat = new PS2_SVAG();
                                        LengthOfRip = MemoryReader.ReadLong(ref buffer, i + 0x04)+ 0x800;
                                        goto found;
                                    }
                                }
                            }
                        }
                        #endregion

                        #region PS2_VAG
                        // Check for Mark ID ("VAGx") where x can be "p","i","s"
                        if ((MemoryReader.ReadLongBE(ref buffer, i) & 0xFFFFFF00) == 0x56414700) 
                        {
                            uint vagLength = MemoryReader.ReadLongBE(ref buffer, i+ 0x0C) + 0x30;

                            if (fReader.Read_32bitsBE(currentOffset + i + 0x24) == 0x56414778)
                            {
                                uint k=0;
                                do
                                {
                                    k += 0x10;
                                    vagLength += 0x10;
                                }
                                while (fReader.Read_16bitsBE(currentOffset + i + fReader.Read_32bitsBE(currentOffset + i + 0x0C) + k) != 0x0007);
                            }

                            if ((vagLength > 0) && (vagLength <= fileLength))
                            {
                                // Check for Sample Rate
                                sample_rate = MemoryReader.ReadLongBE(ref buffer, i + 0x10);
                                if (VGM_Utils.CheckSampleRate(sample_rate)) 
                                {
                                    // Filename is stored at offset +0x20
                                    filename = MemoryReader.GetString(ref buffer, i + 0x20, 0x10);

                                    ripFormat = new PS2_VAG();
                                    LengthOfRip = vagLength;
                                    goto found;
                                }
                            }
                        }
                        #endregion

                        #region PS2_VIG
                        // Check for Mark ID
                        if (MemoryReader.ReadLongBE(ref buffer, i) == 0x01006408)
                        {
                            // Check for Start Offset Value (can't be equal to 0)
                            if (MemoryReader.ReadLong(ref buffer, i + 8) != 0)
                            {
                                // Check for Channels Count & Sample Rate
                                sample_rate = MemoryReader.ReadLong(ref buffer, i + 0x18);
                                channel_count = MemoryReader.ReadLong(ref buffer, i + 0x1C);
                                interleave =  MemoryReader.ReadLong(ref buffer, i + 0x24);

                                if (VGM_Utils.CheckChannels(channel_count))
                                {
                                    if (VGM_Utils.CheckSampleRate(sample_rate))
                                    {
                                        // Check for Interleave value
                                        if ((interleave >= 0) && (interleave < 0x10000))
                                        {
                                            // Try to find file Length
                                            ripFormat = new PS2_VIG();
                                            LengthOfRip = MemoryReader.ReadLong(ref buffer, i + 0x0C) + MemoryReader.ReadLong(ref buffer, i + 0x08);
                                            goto found;
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        #region PS2_VPK
                        // Check for Mark ID (" KPV")
                        if (MemoryReader.ReadLongBE(ref buffer, i) == 0x204B5056)
                        {
                            channel_count = MemoryReader.ReadLong(ref buffer, i + 0x14);
                            sample_rate = MemoryReader.ReadLong(ref buffer, i + 0x10);

                            if (VGM_Utils.CheckChannels(channel_count))
                            {
                                if (VGM_Utils.CheckSampleRate(sample_rate))
                                {
                                    LengthOfRip = MemoryReader.ReadLong(ref buffer, i + 0x04);
                                    interleave = MemoryReader.ReadLong(ref buffer, i + 0x0C)/channel_count;
                                    blockCount = (int)(LengthOfRip / interleave);

                                    if ((LengthOfRip % interleave) != 0)
                                        blockCount++;

                                    ripFormat = new PS2_VPK();
                                    LengthOfRip = (uint)(blockCount * interleave * channel_count)+MemoryReader.ReadLong(ref buffer, i + 0x08);
                                    goto found;
                                }
                            }
                        }
                        #endregion

                        #region PS3_XVAG
                        // Check for Mark ID ("XVAG")
                        if (MemoryReader.ReadLongBE(ref buffer, i) == 0x58564147)
                        {
                            channel_count = MemoryReader.ReadLongBE(ref buffer, i + 0x28);
                            sample_rate = MemoryReader.ReadLongBE(ref buffer, i + 0x3C);

                            if (VGM_Utils.CheckChannels(channel_count))
                            {
                                if (VGM_Utils.CheckSampleRate(sample_rate))
                                {
                                    LengthOfRip = MemoryReader.ReadLongBE(ref buffer, i + 0x40) + MemoryReader.ReadLongBE(ref buffer, i + 0x04);
                                    ripFormat = new PS3_XVAG();
                                    goto found;
                                }
                            }
                        }
                        #endregion

                        #region WII_BRSTM
                        if (MemoryReader.ReadLongBE(ref buffer, i) == 0x5253544D) /* "RSTM" */
                        {
                            if ((MemoryReader.ReadLongBE(ref buffer, i + 0x04) == 0xFEFF0100) ||
                                (MemoryReader.ReadLongBE(ref buffer, i + 0x04) == 0xFEFF0001))
                            {
                                /* get head offset, check */
                                uint head_offset = MemoryReader.ReadLongBE(ref buffer, i + 0x10);
                                if (head_offset == 0x48454144)
                                {
                                    head_offset = 0x10;
                                    /* check type details */
                                    byte codec_number = buffer[head_offset + i + 0x08];
                                    channel_count = buffer[head_offset + i + 0x0A];
                                    sample_rate = MemoryReader.ReadIntBE(ref buffer, head_offset + i + 0x0C);

                                    if (VGM_Utils.CheckChannels(channel_count))
                                    {
                                        if (VGM_Utils.CheckSampleRate(sample_rate))
                                        {
                                            if (codec_number <= 2)
                                            {
                                                ripFormat = new WII_BRSTM();
                                                LengthOfRip = MemoryReader.ReadLongBE(ref buffer, i + 0x08);
                                                goto found;
                                            }
                                        }
                                    }
                                }
                                else
                                {

                                    if (MemoryReader.ReadLongBE(ref buffer, head_offset + i) == 0x48454144) /* "HEAD" */
                                    {
                                        /* check type details */
                                        byte codec_number = buffer[head_offset + i + 0x20];
                                        channel_count = buffer[head_offset + i + 0x22];
                                        sample_rate = MemoryReader.ReadIntBE(ref buffer, head_offset + i + 0x24);

                                        if (VGM_Utils.CheckChannels(channel_count))
                                        {
                                            if (VGM_Utils.CheckSampleRate(sample_rate))
                                            {
                                                if (codec_number <= 2)
                                                {
                                                    ripFormat = new WII_BRSTM();
                                                    LengthOfRip = MemoryReader.ReadLongBE(ref buffer, i + 0x08);
                                                    goto found;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        #region PSP_PMSF
                        if (MemoryReader.ReadLongBE(ref buffer, i) == 0x50534D46) /* "PSMF" */
                        {
                            if ((MemoryReader.ReadLongBE(ref buffer, i + 0x04) == 0x30303134) || (MemoryReader.ReadLongBE(ref buffer, i + 0x04) == 0x30303135) || (MemoryReader.ReadLongBE(ref buffer, i + 0x04) == 0x30303132))
                            {
                                /* get head offset, check */
                                ripFormat = new PSP_PMSF();
                                LengthOfRip = MemoryReader.ReadLongBE(ref buffer, i + 0x0C)+0x800;
                                goto found;
                            }
                        }
                        #endregion

                        #region PSP_PAMF
                        if (MemoryReader.ReadLongBE(ref buffer, i) == 0x50414D46) /* "PSMF" */
                        {
                            if ((MemoryReader.ReadLongBE(ref buffer, i + 0x04) == 0x30303431) || (MemoryReader.ReadLongBE(ref buffer, i + 0x04) == 0x30303135) || (MemoryReader.ReadLongBE(ref buffer, i + 0x04) == 0x30303132))
                            {
                                /* get head offset, check */
                                ripFormat = new PS3_PAMF();
                                LengthOfRip = (MemoryReader.ReadLongBE(ref buffer, i + 0x0C)*0x800) + 0x800;
                                goto found;
                            }
                        }
                        #endregion

                        #region RIFF
                        if (MemoryReader.ReadLongBE(ref buffer, i) == 0x52494646) /* "RIFF" */
                        {
                            if (MemoryReader.ReadLongBE(ref buffer, i + 0x08) == 0x57415645)
                            {
                                if ((MemoryReader.ReadInt(ref buffer, i + 0x14) == 0xFFFE) ||
                                    (MemoryReader.ReadInt(ref buffer, i + 0x14) == 0xFFFC))
                                {
                                    if ((MemoryReader.ReadLongBE(ref buffer, i + 0x2C) == 0xBFAA23E9) &&
                                        (MemoryReader.ReadLongBE(ref buffer, i + 0x30) == 0x58CB7144) &&
                                        (MemoryReader.ReadLongBE(ref buffer, i + 0x34) == 0xA119FFFA))
                                        ripFormat = new PSP_AT3Plus();
                                    //                                    else
                                    //                                        ripFormat = new PSVITA_AT9();
                                }
                                if (MemoryReader.ReadInt(ref buffer, i + 0x14) == 0x270)
                                    ripFormat = new PSP_AT3();

                                if (MemoryReader.ReadInt(ref buffer, i + 0x14) == 0x0002)
                                    ripFormat = new CMN_RIFF();

                                if (MemoryReader.ReadInt(ref buffer, i + 0x14) == 0x5050)
                                    ripFormat = new WII_SNS();

                                if ((MemoryReader.ReadInt(ref buffer, i + 0x14) == 0x0166) ||
                                    (MemoryReader.ReadInt(ref buffer, i + 0x14) == 0x0165))
                                    ripFormat = new X360_XMA();

                                if (MemoryReader.ReadInt(ref buffer, i + 0x14) == 0x0001)
                                    ripFormat = new CMN_RIFF();

                                if (ripFormat != null)
                                {
                                    LengthOfRip = MemoryReader.ReadLong(ref buffer, i + 0x04) + 0x08;
                                    goto found;
                                }
                            }
                        }
                        #endregion

                        #region FSB4
                        if (MemoryReader.ReadLongBE(ref buffer, i) == 0x46534234) /* "FSB4" */
                        {
                            if (MemoryReader.ReadLong(ref buffer, i + 0x04) == 0x1)
                            {
                                if ((MemoryReader.ReadLong(ref buffer, i + 0x0c) < fileLength) ||
                                    (MemoryReader.ReadInt(ref buffer, i + 0x14) < fileLength))
                                    ripFormat = new CMN_FSB4();

                                if (ripFormat != null)
                                {
                                    LengthOfRip = MemoryReader.ReadLong(ref buffer, i + 0x0C) + MemoryReader.ReadLong(ref buffer, i + 0x14) + 0x60;
                                    goto found;
                                }
                            }
                        }
                        #endregion

                        #region FSB5
                        if (MemoryReader.ReadLongBE(ref buffer, i) == 0x46534235) /* "FSB5" */
                        {
                            if (MemoryReader.ReadLong(ref buffer, i + 0x04) == 0x1)
                            {
                                if (MemoryReader.ReadLong(ref buffer, i + 0x8) == 0x1) /* Rip only one Sample File */
                                {
                                    if ((MemoryReader.ReadLong(ref buffer, i + 0x0c) < fileLength) ||
                                        (MemoryReader.ReadInt(ref buffer, i + 0x14) < fileLength))
                                        ripFormat = new CMN_FSB5();
                                }

                                if (ripFormat != null)
                                {
                                    LengthOfRip = MemoryReader.ReadLong(ref buffer, i + 0x0C) + MemoryReader.ReadLong(ref buffer, i + 0x14) + 0x3C;
                                    goto found;
                                }
                            }
                        }
                        #endregion

                        #region PSP_RIFX
                        if (MemoryReader.ReadLongBE(ref buffer, i) == 0x52494658)/* "RIFX" */
                        {
                            // Search for 'WAVEfmt '
                            if ((MemoryReader.ReadLongBE(ref buffer, i + 0x08) == 0x57415645) && (MemoryReader.ReadLongBE(ref buffer, i + 0x0C)==0x666D7420))
                            {
                                for(j=0; j<0x100;j+=4) 
                                {
                                    // Search for 'vorb' or 'cue '
                                    if ((MemoryReader.ReadLongBE(ref buffer, i + j) == 0x766F7262) || (MemoryReader.ReadLongBE(ref buffer, i + j) == 0x63756520))
                                    {
                                        ripFormat = new CMN_RIFX();
                                        LengthOfRip = MemoryReader.ReadLongBE(ref buffer, i + 0x04) + 0x08;
                                        goto found;
                                    }
                                }
                            }
                        }
                        #endregion

                        #region WII_RAK
                        if ((MemoryReader.ReadLongBE(ref buffer, i) == 0x52414B49)/* "RAKI" */
                            && (MemoryReader.ReadLongBE(ref buffer, i+0x08) == 0x43616665) /* "Cafe" */
                            && (MemoryReader.ReadLongBE(ref buffer, i+0x0C) == 0x61647063))                                                                                          
                        {
                            ripFormat = new WII_RAK();
                            uint LengthofHeader = MemoryReader.ReadLongBE(ref buffer, i+0x14);
                            if(MemoryReader.ReadLongBE(ref buffer, i + 0x44)==0x6461744C)
                                LengthOfRip = (MemoryReader.ReadLongBE(ref buffer, i + 0x58) *2) + 0x1C + 0x1C + LengthofHeader;
                            else
                                LengthOfRip = MemoryReader.ReadLongBE(ref buffer,i+0x4c)+LengthofHeader;
                        }
                        #endregion 
                    found:
                        // do we found something ?
                        if (LengthOfRip != 0)
                        {
                            if (ripFormat != null)
                            {
                                if (ripFormat.IsFormat(fReader,Offset+i,LengthOfRip)!=0)
                                {
                                    if (ripFormat.Filename != "")
                                        filename = ripFormat.Filename;

                                    ripFormat.Init(fReader, Offset + i, ref vgmStream, false, LengthOfRip);
                                    vgmStream.vgmTotalSamplesWithLoop = VGM_Utils.get_vgmstream_play_samples(2, 10, 0, vgmStream);

                                    if (LengthOfRip == fileLength)
                                        filename = sPath;

                                    if (ripFormat.IsPlayable)
                                    {
                                        Mediafile.MediaList.Add(new Mediafile.FileEntry(filename, Offset + i, LengthOfRip,
                                                                vgmStream.vgmSampleRate, vgmStream.vgmChannelCount,
                                                                vgmStream.vgmTotalSamplesWithLoop, Mediafile.TypeFileEntry.AUDIO,
                                                                ripFormat.Description, ripFormat.Extension, ripFormat, vgmStream.vgmLoopFlag,
                                                                fileOwner, vgmStream.vgmLoopStartSample, vgmStream.vgmLoopEndSample,
                                                                vgmStream.vgmDecoder.Description, vgmStream.vgmLayout.GetDescription(),(uint)vgmStream.vgmInterleaveBlockSize, sessionID));
                                    }
                                    else
                                    {
                                        Mediafile.MediaList.Add(new Mediafile.FileEntry(filename, Offset + i, LengthOfRip,
                                                                0, 0,
                                                                0, Mediafile.TypeFileEntry.AUDIO,
                                                                ripFormat.Description, ripFormat.Extension, ripFormat, false,
                                                                fileOwner, 0, 0, "", "", 0, sessionID));
                                    }
                                }

                                break;
                            }
                            else
                                i++;
                        } else
                            i++;

                    } while (i <= (buffer.Length - MAX_NEEDED_BLOCK)) ;
                }
                else
                    break;

                if (LengthOfRip != 0)
                {
                    currentOffset += LengthOfRip + i;
                    Offset += LengthOfRip + i;
                }
                else
                {
                    #region ps2ADPCM
                    i = 0;
                    interleave = 0;
                    uint loopPointsCount = 0;
                    UInt64 saveOffset = Offset;
                    UInt64 saveCurrentOffset = currentOffset;

                    do 
                    {
                        int MIBFoundCount = 0;
                        UInt64 MIBFoundOffset = 0;
                        UInt64 adpcmread = 0;
                        bool adpcm_end = false;
                        bool bolSaveEmptyLine = true;

                        UInt64[] loopPoints = new UInt64[3];
                        UInt64[] loopStartPoints = new UInt64[100];
                        UInt64[] emptyLineOffset = new UInt64[100];
                        uint emptyLineCount = 0;
                        uint loopStartPointCount = 0;
                        uint max_interleave;
                        uint max_interleave_index;

                        Array.Copy(buffer, i, tstBuffer, 0, 0x30);

                        // no need to test on full 0x10 * 0x00 bytes
                        if (!MemoryReader.memcmp(tstBuffer, emptyLine,0x10,0))
                        {
                            // no need to test on no PS2 ADPCM
                            if ((VGM_Utils.HINIBBLE(buffer[i]) < 5) && (VGM_Utils.LONIBBLE(buffer[i]) <= 0xC))
                            {
                                UInt64 tryOffset = Offset + i;
                                max_interleave = 0;
                                max_interleave_index = 0;
                                emptyLineCount = 0;

                                do
                                {
                                    if (MemoryReader.memcmp(endPS2ADPCM, tstBuffer,0x10, 0) || ((Offset + i > fileLength)))
                                    {
                                        adpcm_end = true;
                                        break;
                                    }
                                    adpcmread += 0x10;
                                    i += 0x10;
                                    Array.Copy(buffer, i, tstBuffer, 0, 0x10);

                                    if ((VGM_Utils.HINIBBLE(buffer[i]) > 5) || (VGM_Utils.LONIBBLE(buffer[i]) > 0xC) || buffer[i + 1] > 7)
                                        break;

                                    adpcm_end = false;

                                    // try to find a 0x20 block full of 0
                                    tstBuffer[0x01] = 0;
                                    if (MemoryReader.memcmp(tstBuffer, emptyLine,0x10,0))
                                    {
                                        Array.Copy(buffer, i + 0x10, tstBuffer, 0, 0x10);
                                        tstBuffer[0x01] = 0;
                                        if (MemoryReader.memcmp(tstBuffer, emptyLine, 0x10, 0))
                                            break;
                                        else
                                        {
                                            MIBFoundCount = 1;
                                            MIBFoundOffset = Offset + i;
                                        }
                                    }

                                    if(MemoryReader.memcmp(tstBuffer,emptyLine,0x0e,2)) 
                                    {
                                        // don't need to stock all empty lines :P
                                        if (emptyLineCount >= 100)
                                            emptyLineCount = 0;

                                        if (emptyLineCount >= 1)
                                            if (((Offset + i) - tryOffset) > 0x60000)
                                                bolSaveEmptyLine = false;

                                        if (bolSaveEmptyLine)
                                        {
                                            emptyLineOffset[emptyLineCount] = Offset + i;
                                            emptyLineCount++;
                                        }

                                    }

                                    if (buffer[i + 1] == 0x07)
                                        break;


                                    if ((buffer[i + 1] == 0x04) || (buffer[i + 1] == 0x01))
                                    {
                                        loopStartPoints[loopStartPointCount] = Offset + i;
                                        loopStartPointCount++;
                                        if (loopStartPointCount > 2)
                                            break;

                                    }

                                    if (buffer[i + 1] == 0x03)
                                    {
                                        if ((buffer[i + 2] != 0x77))
                                        {
                                            loopPoints[loopPointsCount] = Offset + i;
                                            loopPointsCount++;

                                            if (loopPointsCount >= 2)
                                            {
                                                adpcm_end = true;
                                                interleave = (uint)(loopPoints[1] - loopPoints[0]);
                                                if ((interleave != previous_interleave) && (previous_interleave != 0))
                                                {
                                                    if (interleave < 0x300000)
                                                    {
                                                        loopPointsCount--;
                                                        adpcm_end = false;
                                                    }
                                                    else
                                                    {
                                                        interleave = previous_interleave;
                                                        loopPointsCount = 0;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    //if ((buffer[i+1]!=0x02) && (Offset>0x5a0000))
                                    //    break;

                                    if (adpcm_end)
                                        break;

                                    if (i >= (buffer.Length - MAX_NEEDED_BLOCK))
                                    {
                                        if (Offset >= (startOffset + fileLength))
                                        {
                                            adpcm_end = true;
                                            break;
                                        }

                                        Offset += (UInt64)(buffer.Length - MAX_NEEDED_BLOCK);
                                        currentOffset += (UInt64 )(buffer.Length - MAX_NEEDED_BLOCK);
                                        updatep(currentOffset, fileLength);

                                        buffer = fReader.Read(Offset, (uint)buffer.Length);
                                        i = 0;
                                    }

                                } while (0 == 0);

                                adpcmread = (Offset + i) - tryOffset;

                                if ((adpcmread > 0x2000) || (loopPointsCount>=2))
                                {
                                    if (adpcmread < 0x2000)
                                        break;

                                    // try to find interleave value ...
                                    if (MIBFoundCount == 1)
                                    {
                                        if ((MIBFoundOffset - tryOffset) < ((adpcmread / 100) * 10))
                                        {
                                            if(tryOffset!=0)
                                                tryOffset -= 0x10;
                                            interleave = (uint)(MIBFoundOffset - tryOffset);
                                        }
                                    }
                                    else
                                    {
                                        // Try to find biggest interleave value
                                        if (emptyLineCount > 1)
                                        {
                                            for (uint count = 1; count < emptyLineCount; count++)
                                            {
                                                if (((uint)(emptyLineOffset[count] - emptyLineOffset[count-1])) > max_interleave)
                                                {
                                                    max_interleave = (uint)(emptyLineOffset[count] - emptyLineOffset[count - 1]);
                                                    max_interleave_index = count;
                                                }

                                            }
                                            if (max_interleave_index != 0)
                                            {
                                                interleave = (uint)(emptyLineOffset[max_interleave_index] - emptyLineOffset[0]);
                                            }
                                        }
                                    }


                                    ripFormat = new PS2_MIB();
                                    
                                    ripFormat.Init(fReader, tryOffset, ref vgmStream, false, adpcmread);
                                    if (vgmStream.vgmInterleaveBlockSize != interleave)
                                        interleave = (uint)vgmStream.vgmInterleaveBlockSize;

                                    if((interleave == 0) || (emptyLineCount==0))
                                    {
                                        if (loopPointsCount >= 2)
                                        {
                                            interleave = (uint)(loopPoints[loopPointsCount - 1] - loopPoints[loopPointsCount - 2]);
                                        }

                                    }

                                    if (previous_interleave != 0)
                                        if (interleave != previous_interleave)
                                            previous_interleave = interleave;

                                    previous_interleave = interleave;
                                    LengthOfRip = (uint)adpcmread;

                                    if(loopPointsCount<2)
                                        LengthOfRip += interleave;
                                    LengthOfRip /= interleave;
                                    if (((adpcmread + interleave) % interleave) != 0)
                                        LengthOfRip++;
                                    LengthOfRip *= interleave;

                                    if((interleave==0x10) && (vgmStream.vgmChannelCount==2))
                                        LengthOfRip+=interleave;

                                    if (LengthOfRip > fileLength)
                                        LengthOfRip = (uint)fileLength;

                                    ripFormat.Init(fReader, tryOffset, ref vgmStream, false,LengthOfRip);
                                    
                                    Offset = tryOffset;
                                    //if (LengthOfRip > 0x20000)
                                    //{
                                    //    Offset = tryOffset + adpcmread + interleave + MAX_NEEDED_BLOCK;
                                    //    currentOffset = (tryOffset - startOffset) + adpcmread + interleave + MAX_NEEDED_BLOCK;
                                    //    i = (uint)buffer.Length;
                                    //    break;
                                    //}
                                }
                                else
                                {
                                    if ((adpcm_end) && (adpcmread >= 0x10))
                                    {
                                        if (i != 0)
                                            i -= 0x10;
                                    }
                                }

                                // do we found something ?
                                if (LengthOfRip != 0)
                                {
                                    //ripFormat = new PS2_MIB();
                                    //ripFormat.Init(fReader, Offset, ref vgmStream, false, (int)LengthOfRip);
                                    vgmStream.vgmTotalSamplesWithLoop = VGM_Utils.get_vgmstream_play_samples(2, 10, 0, vgmStream);

                                    if (LengthOfRip == fileLength)
                                        filename = sPath;

                                    Mediafile.MediaList.Add(new Mediafile.FileEntry(filename, Offset, LengthOfRip,
                                                            vgmStream.vgmSampleRate, vgmStream.vgmChannelCount,
                                                            vgmStream.vgmTotalSamplesWithLoop, Mediafile.TypeFileEntry.AUDIO,
                                                            ripFormat.Description, ripFormat.Extension, ripFormat, vgmStream.vgmLoopFlag,
                                                            fileOwner, vgmStream.vgmLoopStartSample, vgmStream.vgmLoopEndSample,
                                                            vgmStream.vgmDecoder.Description, vgmStream.vgmLayout.GetDescription(),(uint)vgmStream.vgmInterleaveBlockSize, sessionID));
                                    currentOffset = (UInt64)((tryOffset - startOffset) + LengthOfRip - (UInt64)buffer.Length + MAX_NEEDED_BLOCK);
                                    Offset =(UInt64)(tryOffset + LengthOfRip - (UInt64)buffer.Length + MAX_NEEDED_BLOCK);
                                    break;
                                }
                                else
                                    i += 0x10;
                            }
                            else
                                i += 0x10;
                        }
                        else
                            i += 0x10;
                    } while (i <= (buffer.Length - MAX_NEEDED_BLOCK)) ;

                    if (LengthOfRip == 0)
                    {
                        currentOffset = saveCurrentOffset;
                        Offset = saveOffset;
                    }
                    #endregion
                    currentOffset += (UInt64)(buffer.Length - MAX_NEEDED_BLOCK);
                    Offset += (UInt64)(buffer.Length - MAX_NEEDED_BLOCK);
                }

                if (!deepSearch)
                    break;
            } while (currentOffset < fileLength);
        }
    }
}
