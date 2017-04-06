using IDTModule.Messages;

namespace IDTModule
{
    public class DataFromMFT
    {
        /// <summary>
        /// MFT Fault windows is open
        /// </summary>
        public bool FaultWindowOpened { get; set; }

        /// <summary>
        /// Input from MFT
        /// </summary>
        public bool FaultReadingFromMDS { get; set; }

        /// <summary>
        /// Input from MFT
        /// </summary>
        public bool FaultWritingToMDS { get; set; }

        /// <summary>
        /// Input from MFT
        /// </summary>
        public bool CommunicationErrorWithPRODIS { get; set; }



        public virtual void UpdateFromMessage(IDTRequest msg)
        {
            FaultWindowOpened = msg.FaultWindowOpen;
            FaultReadingFromMDS = msg.FaultReadingMDS;
            FaultWritingToMDS = msg.FaultWritingMDS;
            CommunicationErrorWithPRODIS = msg.CommunicationErrorWithPRODIS;
        }

    }
}
