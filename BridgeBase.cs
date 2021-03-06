﻿using System.Runtime.Serialization;
using IDTModule.Messages;
using IDTModule.Service;
using System;
using System.Diagnostics;
using log4net;
using UtilityLib.Alarm;
using UtilityLib.Config;
using UtilityLib.Thread;

namespace IDTModule
{
    public abstract class BridgeBase : ThreadEx
    {

        private static readonly ILog Log = LogManager.GetLogger( typeof(BridgeBase));

        public class DiagnosticEventArgs : EventArgs
        {
            public string Message { get; private set; }

            public DiagnosticEventArgs(string message)
            {
                Message = message;
            }
        }

        /// <summary>
        /// Needed to manage the message FROM MFT after the service is complete but before
        /// communication for current service ended
        /// </summary>
        private bool _forceService0;

        private readonly IServiceFactory _serviceFactory;

        public ApplicationConfiguration Configuration { get; set; }

      //  protected readonly IIDTCommunicator _communicator;
        //(°°) 29-09-2014 reso pullico  e non protected
        public readonly IIDTCommunicator _communicator;
        private ServiceRequest _request;
        private readonly Stopwatch _cycleTimeST = new Stopwatch();
        private LoopStatus _loopStatus;

        protected BridgeBase(ApplicationConfiguration configuration, IIDTCommunicator communicator, IServiceFactory serviceFactory)
        {
            Alarms = new AlarmArray(128);
            Configuration = configuration;
            _communicator = communicator;
            _serviceFactory = serviceFactory;
        }

        public AlarmArray Alarms { get; private set; }

        protected bool RequestInProgress
        {
            get
            {
                return _request != null && _request.Started;
            }
        }

        protected bool RequestCompleted
        {
            get
            {
                return _request == null || _request.Completed;
            }
        }

        public virtual event EventHandler<BridgeBase.DiagnosticEventArgs> DiagnosticMessage;

        protected virtual void OnDiagnosticMessage(string msg)
        {
            EventHandler<BridgeBase.DiagnosticEventArgs> handler = DiagnosticMessage;
            if (handler != null)
            {
                BridgeBase.DiagnosticEventArgs e = new BridgeBase.DiagnosticEventArgs(msg);
                handler(this, e);
            }
        }

        protected enum LoopStatus
        {
            NoActiveService,
            RunningService,
            WaitingServiceEnded,
        }

        protected override void Func(object param)
        {
            _forceService0 = false;
            _loopStatus = LoopStatus.NoActiveService;

            _communicator.Start();

            OnDiagnosticMessage("Staring main loop");

            long? avrCycleTime = null;

            while (ShouldStop(5) == false)
            {
                _cycleTimeST.Restart();

                this.Cycle();

                

                switch (_loopStatus)
                {
                    case LoopStatus.NoActiveService:
                    {
                        var req = CheckForNewRequest();

                        if (req != null)
                        {
                            if (req.ServiceNumber != 0 )
                            {
                                _request = req;
                                _loopStatus = LoopStatus.RunningService;
                                _request.Start();
                                OnDiagnosticMessage(String.Format("Started service {0}", _request.GetType().Name));
                            }
                            else
                            {
                                //Service 0 is special
                                req.Start();
                                var rep = req.GetCompletedReply();
                                Debug.Assert(rep != null);
                                //                                    OnDiagnosticMessage(String.Format("Received service {0}", req.GetType().Name));
                                _communicator.QueueMessage(rep);
                            }
                        }
                    }
                    break;

                    case LoopStatus.RunningService:
                    {
                          
                        ServiceRunningState();
                          
                    }
                    break;

                    case LoopStatus.WaitingServiceEnded:
                    {
                        WaitServiceEnded();
                    }
                    break;
                }

                if (avrCycleTime == null)
                {
                    avrCycleTime = _cycleTimeST.ElapsedMilliseconds;
                }
                else 
                {
                    avrCycleTime = (_cycleTimeST.ElapsedMilliseconds + avrCycleTime.Value) / 2;
                }
            }

            _communicator.AskStop();
            _communicator.WaitStop();
        }

        public abstract void Cycle();

        private void WaitServiceEnded()
        {
            var msg = _communicator.GetMessage<IDTRequest>();
            if (msg != null)
            {
                bool ok = msg.ServiceRequest == 0;

                if (ok)
                {
                    //Richiesta finita
                    _loopStatus = LoopStatus.NoActiveService;
                    _forceService0 = true;
                    OnDiagnosticMessage(String.Format("Service {0} ended", _request.GetType().Name));
                }
                else
                {
                    //Devo spedire che la richiesta è completata
                    var reply = _request.GetCompletedReply();
                    Debug.Assert(reply != null);
                    _communicator.QueueMessage(reply);
                }

                if (_communicator.IsConnected == false)
                {
                    Log.Info("WAIT SERVICE ---> PERSA COMUNICAZIONE CON FIRAP");
                 //   _loopStatus = LoopStatus.NoActiveService;
                }
            }
        }

        private void ServiceRunningState()
        {
            var msg = _communicator.GetMessage<IDTRequest>();

            if (msg != null)
            {
                bool abort = msg.ServiceRequest == 0;
                if (abort)
                    _request.Abort();
            }

            _request.Executing();

            if (msg != null && _request.Completed == false)
            {
              //  Log.Info("contrololo se franco mi restituisce 1");
                //Devo spedire una risposta
                var reply = _request.GetProgressReply();
                OnDiagnosticMessage(String.Format("sending progress {0}", _request.GetType().Name));
                if (reply != null)
                    _communicator.QueueMessage(reply);
            }
            else if (_request.Completed == true)
            {
                this._loopStatus = LoopStatus.WaitingServiceEnded;
            }
           /* else if (msg == null) // A.B. aggiunto per uscire dal loop wait se cade la comunicazione
            {
                this._loopStatus = LoopStatus.NoActiveService;
            }*/
            //else if (_communicator != null && _communicator.IsConnected)// A.B. 18.09.2014 aggiunto per uscire dal loop wait se cade la comunicazione
            //{
            //    this._loopStatus = LoopStatus.NoActiveService;
            //}

            if (_communicator.IsConnected == false)
            {
                Log.Info("RUNNING SERVICE ---> PERSA COMUNICAZIONE CON FIRAP");
                //(°°) 13-10-2014 inserita condizione di reset a ciclo 0 quando perdo la comunicazione con DSA
                _loopStatus = LoopStatus.NoActiveService;
            }

        }

        private ServiceRequest CheckForNewRequest()
        {
            var msg = _communicator.GetMessage<IDTRequest>();

            if (msg != null)
            {
                if (msg.ServiceRequest == 0)
                    _forceService0 = false;
                //(°°) 13-10-2014 Inserita condizione di forzatura memoria a false
                if (msg.ServiceRequest != 0 && _forceService0 == true)
                    _forceService0 = false;
                return _serviceFactory.Create( msg,_forceService0, this);
            }
            return null;
        }
    }


    [Serializable]
    public class BridgeInitException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public BridgeInitException()
        {
        }

        public BridgeInitException(string message)
            : base(message)
        {
        }

        public BridgeInitException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected BridgeInitException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }


    [Serializable]
    public class ServiceRequestException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //


        public ServiceRequestException(string message)
            : base(message)
        {

        }

        public ServiceRequestException(string message, Exception inner)
            : base(message, inner)
        {

        }

        protected ServiceRequestException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {

        }
    }
}