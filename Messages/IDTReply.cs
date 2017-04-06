using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDTModule.Messages 
{
    public class IDTReply : IDTMessage
    {
        public byte FunctionResult
        {
            get { return Buffer[1]; }
            set { Buffer[1] = value; }
        }

        public byte ErrorStatus1
        {
            get { return Buffer[2]; }
            set { Buffer[2] = value; }
        }

        public byte ErrorStatus2
        {
            get { return Buffer[3]; }
            set { Buffer[3] = value; }
        }

        public byte InformationStatus1
        {
            get { return Buffer[4]; }
            set { Buffer[4] = value; }
        }

        public byte InformationStatus2
        {
            get { return Buffer[5]; }
            set { Buffer[5] = value; }
        }

        public ushort TimeInformation
        {
            get { return GetBigEndianUShort(36);}
            set {WriteBigEndian(36,value);}
        }

        public byte TestStationId
        {
            get { return Buffer[38]; }
            set { Buffer[38] = value; }
        }

        public byte SendToggleByte
        {
            get { return Buffer[11]; }
            set { Buffer[11] = value; }
        }

     


        public byte CheckSum
        {
            get { return Buffer[42]; }
            set { Buffer[42] = value; }
        }


        public byte DiagnosticByte
        {
            get { return Buffer[43]; }
            set { Buffer[43] = value; }
        }

    }
}
