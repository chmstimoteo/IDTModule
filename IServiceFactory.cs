using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IDTModule.Messages;
using IDTModule.Service;

namespace IDTModule
{
    public interface IServiceFactory
    {
        ServiceRequest Create(IDTRequest msg, bool force0,object param);
    }
}
