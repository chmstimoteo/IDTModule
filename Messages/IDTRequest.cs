using System;
using UtilityLib.Bits;

namespace IDTModule.Messages
{
    /**
    /// <summary>   message send from MFT to test stand</summary>
    ///
    /// <remarks>   Lishi, 06/11/2013.</remarks>
    **/
    public class IDTRequest : IDTMessage
    {
        public byte ServiceRequest
        {
            get { return Buffer[1]; }
            set { Buffer[1] = value; }
        }

        #region byte2
        public byte Status
        {
            get { return Buffer[2]; }
            set { Buffer[2] = value; }
        }

        public bool FaultWindowOpen
        {
            get { return Buffer[2].Bit(1); }
        }

        public bool FaultReadingMDS
        {
            get { return Buffer[2].Bit(3); }
        }

        public bool FaultWritingMDS
        {
            get { return Buffer[2].Bit(4); }
        }

        public bool CommunicationErrorWithPRODIS
        {
            get { return Buffer[2].Bit(7); }
        }
        #endregion


        public ushort Param1
        {
            get { return GetBigEndianUShort(10); }
            set{ WriteBigEndian(10,value);}
        }


        public byte Param1HB
        {
            get { return Buffer[10]; }
            set { Buffer[10] = value; }
        }

        public byte Param1LB
        {
            get { return Buffer[11]; }
            set { Buffer[11] = value; }
        }

        public byte MFTStandNumber
        {
            get { return Buffer[19]; }
            set { Buffer[19] = value; }
        }









        public override string ToString()
        {
            return String.Format("Service {0}", ServiceRequest);
        }
    }
}
