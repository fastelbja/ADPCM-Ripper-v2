using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StreamReader;

namespace VGMStream
{
    public class PS2_MIB : IVGMFormat
    {
        private string m_Description;

        public string Extension
        {
            get
            {
                return "MIB";
            }
        }

        public string Description
        {
            get
            {
                return "Sony Headerless ADPCM (MIB)";
            }
        }

        public string Filename
        {
            get
            {
                return m_Description;
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
            m_Description = offset.ToString("X8") + ".MIB";
            return length;
        }

        public void Init(StreamReader.IReader fileReader, UInt64 offset, ref VGM_Stream vgmStream, bool InitReader, UInt64 fileLength)
        {
	        byte[] mibBuffer = new byte[0x10];
	        byte[] testBuffer = new byte[0x10];
	        byte[] testBuffer2 = new byte[0x10];
	        byte[] testBuffer3 =new byte[0x10];

            //bool doChannelUpdate=true;
            //bool bDoUpdateInterleave=true;

            UInt64 loopStart = 0;
            UInt64 loopEnd = 0;
            UInt64 interleave = 0;
            UInt64 readOffset = offset;

            UInt64[]	loopStartPoints = new UInt64[0x10];
	        int		loopStartPointsCount=0;

            UInt64[]	loopEndPoints = new UInt64[0x10];
	        int		loopEndPointsCount=0;

            byte[] emptyLine = new byte[0x10];
	        bool	loopToEnd=false;
	        bool	forceNoLoop=false;
            bool bolSaveEmptyLine = true;

	        int i, channel_count=0;
            bool interleave_found = false;
            UInt64 tstCount = 0;

            UInt64[] emptyLineOffset = new UInt64[100];
            uint emptyLineCount = 0;
            uint max_interleave = 0;
            uint max_interleave_index = 0;

	        // Initialize loop point to 0
	        for(i=0; i<0x10; i++) 
            {
		        loopStartPoints[i]=0;
		        loopEndPoints[i]=0;
		        testBuffer[i]=0;
                emptyLine[i] = 0;
	        }

            //if (this.Filename.ToUpper().Contains(".MIB"))

            /* Search for interleave value & loop points */
            /* Get the first 16 values */
            mibBuffer = fileReader.Read(offset, 0x10);
            if (MemoryReader.memcmp(mibBuffer, emptyLine, 0x10, 0))
            {
                tstCount = 0x10;
                //readOffset += 0x10;
            }
            else
                tstCount = 0x0e;

	        do 
            {
                testBuffer = fileReader.Read(readOffset, 0x10);

                // be sure to point to an interleave value
                if (!interleave_found)
                {
                    if (MemoryReader.memcmp(testBuffer, emptyLine, tstCount, 0x10 - tstCount))
                    {
                        // don't need to stock all empty lines :P
                        if (emptyLineCount >= 100)
                            emptyLineCount = 0;

                        if (emptyLineCount >= 1)
                            if ((readOffset - offset) > 0x60000)
                                bolSaveEmptyLine = false;

                        if (bolSaveEmptyLine)
                        {
                            emptyLineOffset[emptyLineCount] = readOffset;
                            emptyLineCount++;
                        }

                    }
                }

		        // Loop Start ...
		        if(testBuffer[0x01]==0x06) 
		        {
			        if(loopStartPointsCount<0x10) 
			        {
				        loopStartPoints[loopStartPointsCount] = readOffset - offset -0x10;
				        loopStartPointsCount++;
			        }
		        }

		        // Loop End ...
		        if(((testBuffer[0x01]==0x03) && (testBuffer[0x03]!=0x77)) ||
			        (testBuffer[0x01]==0x01)) {
			        if(loopEndPointsCount<0x10) 
			        {
				        loopEndPoints[loopEndPointsCount] = readOffset - offset; //-0x10;
				        loopEndPointsCount++;
			        }

			        if(testBuffer[0x01]==0x01)
				        forceNoLoop=true;
		        }

		        if(testBuffer[0x01]==0x04) 
		        {
			        // 0x04 loop points flag can't be with a 0x03 loop points flag
			        if(loopStartPointsCount<0x10) 
			        {
                        if ((readOffset - offset) == 0)
                            loopStartPoints[loopStartPointsCount] = 0;
                        else
                            loopStartPoints[loopStartPointsCount] = readOffset - offset;

				        loopStartPointsCount++;

				        // Loop end value is not set by flags ...
				        // go until end of file
				        loopToEnd=true;
			        }
		        }
                readOffset += 0x10;
	        } while (readOffset< (offset + fileLength));

            // Try to find biggest interleave value
            if (emptyLineCount > 1)
            {
                for (uint count = 1; count < emptyLineCount; count++)
                {
                    if (((uint)(emptyLineOffset[count] - emptyLineOffset[count - 1])) > max_interleave)
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

	         if((testBuffer[0]==0x0c) && (testBuffer[1]==0))
		        forceNoLoop=true;

	        if(channel_count==0)
		        channel_count=1;

	        // Calc Loop Points & Interleave ...
	        if(loopStartPointsCount>=channel_count) 
	        {
		        // can't get more then 0x10 loop point !
                // and need at least 2 loop points
		        if((loopStartPointsCount<=0x0F) && (loopStartPointsCount>=2)) 
                {
			        // Always took the first 2 loop points
			        interleave=loopStartPoints[loopStartPointsCount-1]-loopStartPoints[loopStartPointsCount-2];
			        loopStart=loopStartPoints[1];

			        // Can't be one channel .mib with interleave values
			        if((interleave>0) && (channel_count==1)) 
				        channel_count=2;
		        } else 
			        loopStart=0;
	        }

	        if(loopEndPointsCount>=channel_count) 
	        {
		        // can't get more then 0x10 loop point !
                // and need at least 2 loop points
                if ((loopEndPointsCount <= 0x0F) && (loopEndPointsCount >= 2)) 
                {
                    if (loopEndPointsCount == 4)
                        if (interleave == 0)
                            interleave = loopEndPoints[3] - loopEndPoints[1];
                    else
                        interleave = loopEndPoints[loopEndPointsCount - 1] - loopEndPoints[loopEndPointsCount - 2];
			        loopEnd=loopEndPoints[loopEndPointsCount-1];

			        if (interleave>=0x10) 
                    {
				        if(channel_count==1) 
					        channel_count=2;
			        }
		        } else 
                {
			        loopToEnd=false;
			        loopEnd=0;
		        }
	        }

	        if (loopToEnd) 
		        loopEnd=fileLength;

	        // force no loop 
	        if(forceNoLoop) 
		        loopEnd=0;

	        if((interleave>0x10) && (channel_count==1))
		        channel_count=2;

	        if(interleave<=0) interleave=0x10;

        	/* build the VGMSTREAM */
            VGM_Utils.allocate_vgmStream(ref vgmStream, channel_count, (loopEnd != 0));

            /* fill in the vital statistics */
            vgmStream.vgmDecoder = new PSX_Decoder();

            if (channel_count == 1)
                vgmStream.vgmLayout = new NoLayout();
            else
                vgmStream.vgmLayout = new Interleave();

		    vgmStream.vgmChannelCount = channel_count;
		    vgmStream.vgmInterleaveBlockSize = (int)interleave;
            vgmStream.vgmLoopFlag = (loopEnd!=0);

            if(m_Description==null)
			    vgmStream.vgmSampleRate = 44100;
            else 
            {
                if (System.IO.Path.GetExtension(m_Description).ToUpper() == ".MIB")
                    vgmStream.vgmSampleRate = 44100;

            }

		    vgmStream.vgmTotalSamples = (int)(fileLength/(UInt64)16 / (UInt64)(channel_count * 28));

	        if(loopEnd!=0) {
		        if(vgmStream.vgmChannelCount==1) 
                {
			        vgmStream.vgmLoopStartSample = (int)loopStart/16*18;
			        vgmStream.vgmLoopEndSample = (int)loopEnd/16*28;
		        }
                else 
                {
                    if (loopStart == 0)
                        vgmStream.vgmLoopStartSample = 0;
                    else
                    {
                        vgmStream.vgmLoopStartSample = (int)(((UInt64)(((loopStart / interleave) - 1) * interleave)) / (UInt64)((16 * 14 * channel_count) / channel_count));

                        if (loopStart % interleave != 0)
                        {
                            vgmStream.vgmLoopStartSample += (int)(((UInt64)((loopStart % interleave) - 1) / (UInt64)(16 * 14 * channel_count)));
                        }
                    }

			        if(loopEnd==fileLength) 
			        {
				        vgmStream.vgmLoopEndSample =(int)((UInt64)loopEnd / (UInt64)((16 *28)/channel_count));
			        } 
                    else 
                    {
				        vgmStream.vgmLoopEndSample = (int)((UInt64)(((loopEnd/interleave)-1)*interleave)/ (UInt64)((16 *14*channel_count)/channel_count));

				        if(loopEnd%interleave!=0) 
                        {
					        vgmStream.vgmLoopEndSample += (int)((UInt64)((loopEnd%interleave)-1)/ (UInt64)(16 *14*channel_count));
				        }
			        }
		        }
	        }
            if (InitReader)
            {
                for (i = 0; i < channel_count; i++)
                {
                    vgmStream.vgmChannel[i].currentOffset = offset + (interleave * (UInt64)i);
                    vgmStream.vgmChannel[i].fReader = (StreamReader.IReader)Activator.CreateInstance(fileReader.GetType()); 
                    vgmStream.vgmChannel[i].fReader.Open(fileReader.GetFilename());
                    vgmStream.vgmChannel[i].fReader.SetSessionID(fileReader.GetSessionID());
                }
            }
        }
    }
}
