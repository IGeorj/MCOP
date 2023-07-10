﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.Core.Services.Shared.Common
{
    public class UptimeInformation
    {
        public DateTimeOffset ProcessStartTime { get; private set; }
        public DateTimeOffset SocketStartTime { get; set; }


        public UptimeInformation(DateTimeOffset processStartTime)
        {
            this.ProcessStartTime = processStartTime;
            this.SocketStartTime = processStartTime;
        }


        public TimeSpan ProgramUptime => DateTimeOffset.Now - this.ProcessStartTime;
        public TimeSpan SocketUptime => DateTimeOffset.Now - this.SocketStartTime;
    }
}
