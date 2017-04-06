using System;
using IDTModule.Messages;
using UtilityLib.Thread;

namespace IDTModule.Service
{

    public abstract class ServiceRequest
    {

        protected IDTMessage CompleteReply ;
        protected IDTMessage ProgressReply;

        /**
        <summary>   The number associated with service </summary>
        
        <value> The service number.</value>
        **/
        public abstract int ServiceNumber { get; }

        protected bool AbortFlag ;

        public enum RequestStatus
        {
            NoStarted = 0,
            Completed = 1,
            InProgress = 2,
            Error = -1
        }


        public  byte TraslateStatusToMFT(RequestStatus status)
        {
            switch (status)
            {
                case RequestStatus.NoStarted:
                    return 0;
                case RequestStatus.Completed:
                    return 1;
                case RequestStatus.InProgress:
                    return 2;
                case RequestStatus.Error:
                    return 255;

            }
            throw new ArgumentOutOfRangeException("status");
        }


        /**
        <summary>   the service status </summary>
        
        <value> The service status.</value>
        **/
        public RequestStatus ServiceStatus
        {
            get { return _serviceStatus.Value; }
            protected set { _serviceStatus.Value = value; }
        }

        
        private readonly LockedAccessValue<RequestStatus> _serviceStatus = new LockedAccessValue<RequestStatus>();


        protected ServiceRequest()
        {
            ServiceStatus = RequestStatus.NoStarted;
        }

        /**
        <summary>   Start the request</summary>
        **/
        public abstract void Start();

        /**
        <summary>   Progress the command execution</summary>
        **/
        public abstract void Executing();

        /**
        <summary>   Gets the reply message if available</summary>
        

        /**
        <summary>   eheck if CurrentService is completed</summary>
        
        <value> true if completed, false if not.</value>
        **/
        public bool Completed
        {
            get { 
                var stat = this.ServiceStatus;
                return stat == RequestStatus.Error || stat == RequestStatus.Completed || stat == RequestStatus.NoStarted;
            }
        }

        /**
        <summary>   Gets a value indicating whether the request have started</summary>
        
        <value> true if started, false if not.</value>
        **/
        public bool Started
        {
            get { return this.ServiceStatus != RequestStatus.NoStarted; }
            
        }

        public void Abort()
        {
            AbortFlag = true;
        }

        /**
        <summary>   Gets progress reply, null if no reply is available.</summary>
        
        <returns>   The progress reply.</returns>
        **/
        public IDTMessage GetProgressReply()
        {
            var ret = ProgressReply;
            ProgressReply = null;
            return ret;
        }

        /**
        <summary>   Gets completed reply.</summary>
        
        <returns>   The completed reply.</returns>
        **/
        public IDTMessage GetCompletedReply()
        {
            var ret = CompleteReply;
            return ret;
        }

       
    }
}
