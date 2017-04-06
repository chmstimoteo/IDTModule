using System;
using System.Text;

namespace IDTModule.Messages
{

    public static class MFTAlarms
    {
        public const int IncorrectDataBlockFromMFT = 0;
        public const int IncorrectServiceRequest = 1;
        public const int ErrorDuringWheelAlignment = 2;
        public const int ErrorVentilationSystemTestStation = 3;
        public const int CollectiveErrorDiskBrake = 4;
        public const int CollectiveErrorBeltRollerConnection = 5;
        public const int CollectiveErrorRollerRollerConnection = 6;
        public const int CollectiveErrorDriveMotorBeltConnection = 7;
        public const int IncorrectNominalValueForMaximumTorque = 8;
        public const int Spare0 = 9;
        public const int IncorrectNominalSpeedValue = 10;
        public const int SlipDetected = 11;
        public const int IncorrectNominalValueForMotorMode = 12;
        public const int ErrorInPusherControl = 13;
        public const int OtherError = 14;
        public const int ErrorInServiceRequest = 15;
    }

    public static class MFTStatusInformation
    {
        public const int PusherInOperatingPosition = 0;
        public const int EndOfTest = 1;
        public const int RollerStopped = 2;
        public const int MoveVehicleOnLeft = 3;
        public const int MoveVehicleOfRight = 4;
        public const int TestWithoutMDS = 5;
        public const int SendVehicleId = 6;
        public const int RepeatMeasurement = 7;
        public const int EmergencyStop = 8;
        public const int Error = 9;
        public const int SystemIsActive = 10;
        public const int FaultAcknowledged = 11;
        public const int AutomaticMode = 12;
        public const int ManualMode = 13;
        public const int CalibrationMode = 14;
        public const int TestStationReady = 15;
    }

    /**
    /// <summary>   An idt message. check the DSA documentation for more information</summary>
    ///
    /// <remarks>   Lishi, 06/11/2013.</remarks>
    **/
    public class IDTMessage
    {
        public const int Lenght = 44;

        public IDTMessage()
        {
            Buffer = new byte[Lenght];
        }


        public string ToHexString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var b in Buffer)
            {
                sb.AppendFormat("{0:X2} ", b);
            }

            return sb.ToString();
        }


        public void SetBuffer(byte[] buffer)
        {
            System.Buffer.BlockCopy(buffer, 0, Buffer, 0, buffer.Length);
        }

        public IDTMessage(byte[] buffer)
        {
            //Ricordarsi di fare la copia! dei byte, non copiare solo la reference
            Buffer = new byte[Lenght];
            System.Buffer.BlockCopy(buffer, 0, Buffer, 0, buffer.Length);
        }

        /**
        <summary>   Writes a big endian short in the buffer</summary>
        
        <param name="index">    Zero-based index of the. </param>
        <param name="value">    The value. </param>
        **/
        public void WriteBigEndian(int index, ushort value)
        {
            var b = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
            {
                Buffer[index + 0] = b[0];
                Buffer[index + 1] = b[1];
            }
            else
            {
                Buffer[index + 0] = b[1];
                Buffer[index + 1] = b[0];
            }
        }

        /**
        <summary>   Writes a big endian short in the buffer.</summary>
        
        <param name="index">    Zero-based index of the. </param>
        <param name="value">    The value. </param>
        **/
        public void WriteBigEndian(int index, short value)
        {
            var b = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
            {
                Buffer[index + 0] = b[0];
                Buffer[index + 1] = b[1];
            }
            else
            {
                Buffer[index + 0] = b[1];
                Buffer[index + 1] = b[0];
            }
        }

        /**
        <summary>   get a big endian ushort from buffer.</summary>
        
        <param name="index">    Zero-based index of the. </param>
        
        <returns>   The big endian u short.</returns>
        **/
        public ushort GetBigEndianUShort(int index)
        {
            var b = new byte[2];
            if (!BitConverter.IsLittleEndian)
            {
                b[0] = Buffer[index + 0];
                b[1] = Buffer[index + 1];
            }
            else
            {
                b[0] = Buffer[index + 1];
                b[1] = Buffer[index + 0];
            }
/*

            if (BitConverter.IsLittleEndian)
            {
                b[0] = Buffer[index + 0];
                b[1] = Buffer[index + 1];
            }
            else
            {
                b[0] = Buffer[index + 1];
                b[1] = Buffer[index + 0];
            }*/

            return BitConverter.ToUInt16(b, 0);
        }

        /**
        <summary>   Gets big endian short.</summary>
        
        <param name="index">    Zero-based index of the. </param>
        
        <returns>   The big endian short.</returns>
        **/
        public short GetBigEndianShort(int index)
        {
            var b = new byte[2];
            if (!BitConverter.IsLittleEndian)
            {
                b[0] = Buffer[index + 0];
                b[1] = Buffer[index + 1];
            }
            else
            {
                b[0] = Buffer[index + 1];
                b[1] = Buffer[index + 0];
            }
            return BitConverter.ToInt16(b, 0);
        }

        /**
        /// <summary>   Makes a deep copy of this object.</summary>
        ///
        /// <remarks>   Lishi, 06/11/2013.</remarks>
        ///
        /// <returns>   A copy of this object.</returns>
        **/
        public IDTMessage Clone()
        {
            IDTMessage copy = new IDTMessage();
            System.Buffer.BlockCopy(Buffer,0,copy.Buffer,0,Buffer.Length);
            return copy;
        } 

        /**
        /// <summary>  raw bytes of this message</summary>
        ///
        /// <value> The buffer.</value>
        **/
        public byte[] Buffer { get; private set; }

        public byte TestStationIdentification
        {
            get { return Buffer[0]; }
            set { Buffer[0] = value; }
        }        

        
        
        /// <summary>
        /// Valore per la comunicazione sincrona: qui l'MFT mi scrive il valore che legge nel byte 11
        /// </summary>
        public byte ToggleByte
        {
            get { return Buffer[3]; }
            set { Buffer[3] = value; }
        }

        public byte ContinuousMFTNumber
        {
            get { return Buffer[41]; }
            set { Buffer[41] = value; }
        }
    }
}
