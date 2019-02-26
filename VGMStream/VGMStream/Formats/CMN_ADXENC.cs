using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StreamReader;

namespace VGMStream
{
    public class CMN_ADXENC : IVGMFormat
    {
        public string Extension
        {
            get
            {
                return "ADX";
            }
        }

        public string Description
        {
            get
            {
                return "ADX Encrypted";
            }
        }

        public string Filename
        {
            get
            {
                return string.Empty;
            }
        }

        public bool IsPlayable
        {
            get
            {
                return true;
            }
        }

        public UInt64 IsFormat(StreamReader.IReader fileReader, UInt64 offset, UInt64 length)
        {
            if (fileReader.Read_16bitsBE(offset) != 0x8000)
                return 0;

            // Check encoding type ...
            if (fileReader.Read_8Bits(offset + 4) != 3)
                return 0;

            // Check Frame Size ...
            if (fileReader.Read_8Bits(offset + 5) != 18)
                return 0;

            // Check Bit per Samples ...
            if (fileReader.Read_8Bits(offset + 6) != 4)
                return 0;

            // Check for channel count & sample rate
            uint sampleRate = fileReader.Read_32bitsBE(offset + 0x08);
            uint channelCount = fileReader.Read_8Bits(offset + 0x07);

            if (((channelCount <= 0) || (channelCount > 8)) ||
                ((sampleRate < 11025) || (sampleRate > 48000)))
                return 0;

            // Search for the (c)CRI signature ...
            UInt64 criOffset = fileReader.Read_16bitsBE(offset + 2);
            if ((fileReader.Read_32bitsBE(offset + criOffset - 4) != 0x00002863) &&
                (fileReader.Read_32bitsBE(offset + criOffset) != 0x29435249))
                return 0;

            // Find file length ...
            UInt32 totalSamples = fileReader.Read_32bitsBE(offset + 0x0C);
            UInt64 searchOffset = offset + (totalSamples * channelCount) / 32 * 18 + criOffset + 4;

            while (fileReader.Read_16bitsBE(searchOffset) != 0x8001)
            {
                searchOffset++;
                if (!fileReader.CanRead())
                    break;
            }

            UInt64 fileLength = (searchOffset - offset) + fileReader.Read_16bitsBE(searchOffset + 2) + 4;
            return fileLength;
        }

        public void Init(StreamReader.IReader fileReader, UInt64 offset, ref VGM_Stream vgmStream, bool InitReader, UInt64 fileLength)
        {
            bool loop_flag = false;
            UInt32 loop_start_sample = 0;
            UInt32 loop_start_offset;
            UInt32 loop_end_sample = 0;
            UInt32 loop_end_offset;

            Int16 coef1 = 0;
            Int16 coef2 = 0;

            UInt16 xor_start = 0;
            UInt16 xor_mult = 0;
            UInt16 xor_add = 0;

            int i;

            int channel_count = fileReader.Read_8Bits(offset + 0x07);

            /* check version signature, read loop info */
            UInt16 version_signature = fileReader.Read_16bitsBE(offset + 0x12);
            UInt64 criOffset = (UInt64)(fileReader.Read_16bitsBE(offset + 2) + 4);

            /* encryption */
            if (version_signature == 0x0408)
            {
                if (!find_key(fileReader, offset, ref xor_start, ref xor_mult, ref xor_add))
                {
                    return;
                }
            }

            UInt32 ainf_info_length = 0;

            if (fileReader.Read_32bitsBE(offset + 0x24) == 0x41494E46) /* AINF Header */
                ainf_info_length = fileReader.Read_32bitsBE(offset + 0x28);

            if (criOffset - ainf_info_length - 6 >= 0x38)
            {   /* enough space for loop info? */
                loop_flag = (fileReader.Read_32bitsBE(offset + 0x24) != 0);
                loop_start_sample = (fileReader.Read_32bitsBE(offset + 0x28));
                loop_start_offset = fileReader.Read_32bitsBE(offset + 0x2C);
                loop_end_sample = fileReader.Read_32bitsBE(offset + 0x30);
                loop_end_offset = fileReader.Read_32bitsBE(offset + 0x34);
            }

            /* build the VGMSTREAM */
            VGM_Utils.allocate_vgmStream(ref vgmStream, channel_count, loop_flag);

            vgmStream.vgmChannelCount = channel_count;
            vgmStream.vgmSampleRate = (int)fileReader.Read_32bitsBE(offset + 0x08);
            vgmStream.vgmTotalSamples = (int)fileReader.Read_32bitsBE(offset + 0x0C);

            vgmStream.vgmDecoder = new ADXENC_Decoder();

            if (channel_count == 1)
                vgmStream.vgmLayout = new NoLayout();
            else
                vgmStream.vgmLayout = new Interleave();

            vgmStream.vgmLoopFlag = loop_flag;

            if (loop_flag)
            {
                vgmStream.vgmLoopStartSample = (int)loop_start_sample;
                vgmStream.vgmLoopEndSample = (int)loop_end_sample;
            }

            /* high-pass cutoff frequency, always 500 that I've seen */
            UInt16 cutoff = fileReader.Read_16bitsBE(offset + 0x10);

            /* calculate filter coefficients */
            {
                double x, y, z, a, b, c;

                x = cutoff;
                y = vgmStream.vgmSampleRate;
                z = Math.Cos(2.0 * Math.PI * x / y);

                a = Math.Sqrt(2) - z;
                b = Math.Sqrt(2) - 1.0;
                c = (a - Math.Sqrt((a + b) * (a - b))) / b;

                coef1 = (Int16)Math.Floor(c * 8192);
                coef2 = (Int16)Math.Floor(c * c * -4096);
            }

            vgmStream.vgmInterleaveBlockSize = 18;

            if (InitReader)
            {
                for (i = 0; i < channel_count; i++)
                {
                    vgmStream.vgmChannel[i].fReader = (StreamReader.IReader)Activator.CreateInstance(fileReader.GetType()); ;
                    vgmStream.vgmChannel[i].fReader.Open(fileReader.GetFilename());
                    vgmStream.vgmChannel[i].fReader.SetSessionID(fileReader.GetSessionID());

                    vgmStream.vgmChannel[i].adpcm_coef[0] = coef1;
                    vgmStream.vgmChannel[i].adpcm_coef[1] = coef2;
                    vgmStream.vgmChannel[i].startOffset = vgmStream.vgmChannel[i].currentOffset = offset + criOffset + (UInt64)((vgmStream.vgmInterleaveBlockSize * i));

                    int j;
                    vgmStream.vgmChannel[i].adx_channels = channel_count;
                    vgmStream.vgmChannel[i].adx_xor = xor_start;
                    vgmStream.vgmChannel[i].adx_mult = xor_mult;
                    vgmStream.vgmChannel[i].adx_add = xor_add;

                    for (j = 0; j < i; j++)
                        VGM_Utils.adx_next_key(ref vgmStream.vgmChannel[i]);
                }


            }
        }

        /* guessadx stuff */

        private struct adx_keys
        {
            public UInt16 start;
            public UInt16 mult;
            public UInt16 add;
        }

        adx_keys[] encadx_key = new adx_keys[15];
        private const int key_count = 15;

        private void InitKeys()
        {
            /* Clover Studio (GOD HAND, Okami) */
            /* I'm pretty sure this is right, based on a decrypted version of some GOD HAND tracks. */
            /* Also it is the 2nd result from guessadx */
            encadx_key[0].start = 0x49e1;
            encadx_key[0].mult = 0x4a57;
            encadx_key[0].add = 0x553d;

            /* Grasshopper Manufacture 0 (Blood+) */
            /* this is estimated */
            encadx_key[1].start = 0x5f5d;
            encadx_key[1].mult = 0x58bd;
            encadx_key[1].add = 0x55ed;

            /* Grasshopper Manufacture 1 (Killer7) */
            /* this is estimated */
            encadx_key[2].start = 0x50fb;
            encadx_key[2].mult = 0x5803;
            encadx_key[2].add = 0x5701;

            /* Grasshopper Manufacture 2 (Samurai Champloo) */
            /* confirmed unique with guessadx */
            encadx_key[3].start = 0x4f3f;
            encadx_key[3].mult = 0x472f;
            encadx_key[3].add = 0x562f;

            /* Moss Ltd (Raiden III) */
            /* this is estimated */
            encadx_key[4].start = 0x66f5;
            encadx_key[4].mult = 0x58bd;
            encadx_key[4].add = 0x4459;

            /* Sonic Team 0 (Phantasy Star Universe) */
            /* this is estimated */
            encadx_key[5].start = 0x5deb;
            encadx_key[5].mult = 0x5f27;
            encadx_key[5].add = 0x673f;

            /* G.dev (Senko no Ronde) */
            /* this is estimated */
            encadx_key[6].start = 0x46d3;
            encadx_key[6].mult = 0x5ced;
            encadx_key[6].add = 0x474d;

            /* Sonic Team 1 (NiGHTS: Journey of Dreams) */
            /* this seems to be dead on, but still estimated */
            encadx_key[7].start = 0x440b;
            encadx_key[7].mult = 0x6539;
            encadx_key[7].add = 0x5723;

            /* from guessadx (unique?), unknown source */
            encadx_key[8].start = 0x586d;
            encadx_key[8].mult = 0x5d65;
            encadx_key[8].add = 0x63eb;

            /* Navel (Shuffle! On the Stage) */
            /* 2nd key from guessadx */
            encadx_key[9].start = 0x4969;
            encadx_key[9].mult = 0x5deb;
            encadx_key[9].add = 0x467f;

            /* Success (Aoishiro) */
            /* 1st key from guessadx */
            encadx_key[10].start = 0x4d65;
            encadx_key[10].mult = 0x5eb7;
            encadx_key[10].add = 0x5dfd;

            /* Sonic Team 2 (Sonic and the Black Knight) */
            /* confirmed unique with guessadx */
            encadx_key[11].start = 0x55b7;
            encadx_key[11].mult = 0x6191;
            encadx_key[11].add = 0x5a77;

            /* Enterbrain (Amagami) */
            /* one of 32 from guessadx */
            encadx_key[12].start = 0x5a17;
            encadx_key[12].mult = 0x509f;
            encadx_key[12].add = 0x5bfd;

            /* Yamasa (Yamasa Digi Portable: Matsuri no Tatsujin) */
            /* confirmed unique with guessadx */
            encadx_key[13].start = 0x4c01;
            encadx_key[13].mult = 0x549d;
            encadx_key[13].add = 0x676f;

            /* Kadokawa Shoten (Fragments Blue) */
            /* confirmed unique with guessadx */
            encadx_key[14].start = 0x5803;
            encadx_key[14].mult = 0x4555;
            encadx_key[14].add = 0x47bf;
        }


        /* return 0 if not found, 1 if found and set parameters */
        private bool find_key(IReader fReader, UInt64 offset, ref UInt16 xor_start, ref UInt16 xor_mult, ref UInt16 xor_add)
        {
            int bruteframe = 0;
            int bruteframecount = -1;
            int startoff;
            int endoff;
            int i;

            UInt16[] scales = new UInt16[0];
            UInt16[] prescales = new UInt16[0];

            InitKeys();
            startoff = fReader.Read_16bitsBE(offset + 2) + 4;
            endoff = (int)((fReader.Read_32bitsBE(offset + 12) + 31) / 32 * 18 * fReader.Read_8Bits(offset + 7)) + startoff;

            /* how many scales? */
            int framecount = (endoff - startoff) / 18;
            if ((framecount < bruteframecount) || (bruteframecount < 0))
                bruteframecount = framecount;

            /* find longest run of nonzero frames */
            int longest = -1, longest_length = -1;
            int length = 0;
            for (i = 0; i < bruteframecount; i++)
            {
                byte[] zeroes = new byte[18];
                byte[] buf = new byte[18];

                buf = fReader.Read(offset + (UInt64)(startoff + (i * 18)), 18);

                if (!MemoryReader.memcmp(zeroes, buf,18,0))
                    length++;
                else
                    length = 0;

                if (length > longest_length)
                {
                    longest_length = length;
                    longest = i - length + 1;
                    if (longest_length >= 0x8000) break;
                }
            }
            if (longest == -1)
            {
                return false;
            }

            bruteframecount = longest_length;
            bruteframe = longest;

            /* try to guess key */
            const int MAX_FRAMES = (2147483647 / 0x8000);
            int scales_to_do;
            int key_id;

            /* allocate storage for scales */
            scales_to_do = (bruteframecount > MAX_FRAMES ? MAX_FRAMES : bruteframecount);
            scales = new UInt16[scales_to_do];

            /* prescales are those scales before the first frame we test
             * against, we use these to compute the actual start */
            if (bruteframe > 0)
            {
                /* allocate memory for the prescales */
                prescales = new UInt16[bruteframe];

                /* read the prescales */
                for (i = 0; i < bruteframe; i++)
                {
                    prescales[i] = fReader.Read_16bitsBE(offset + (UInt64)(startoff + (i * 18)));
                }
            }

            /* read in the scales */
            for (i = 0; i < scales_to_do; i++)
            {
                scales[i] = fReader.Read_16bitsBE(offset + (UInt64)(startoff + ((bruteframe + i) * 18)));
            }

            /* guess each of the keys */
            for (key_id = 0; key_id < key_count; key_id++)
            {
                /* test pre-scales */
                UInt16 xor = encadx_key[key_id].start;
                UInt16 mult = encadx_key[key_id].mult;
                UInt16 add = encadx_key[key_id].add;

                for (i = 0; i < bruteframe &&
                        ((prescales[i] & 0x6000) == (xor & 0x6000) ||
                         prescales[i] == 0);
                        i++)
                {
                    xor = (UInt16)(xor * mult + add);
                }

                if (i == bruteframe)
                {
                    /* test */
                    for (i = 0; i < scales_to_do &&
                            (scales[i] & 0x6000) == (xor & 0x6000); i++)
                    {
                        xor = (UInt16)(xor * mult + add);
                    }
                    if (i == scales_to_do)
                    {
                        xor_start = encadx_key[key_id].start;
                        xor_mult = encadx_key[key_id].mult;
                        xor_add = encadx_key[key_id].add;
                        return true;
                    }
                }
            }

            return true;
        }
    }
}
