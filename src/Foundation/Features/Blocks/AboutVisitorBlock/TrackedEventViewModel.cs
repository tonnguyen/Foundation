using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Foundation.Features.Blocks.AboutVisitorBlock
{
    public class TrackedEventViewModel
    {
        public TrackedEventViewModel()
        {

        }

        public string EventType { get; set; }
        public string EventTime { get; set; }
        public string Value { get; set; }
        public string PageUri { get; set; }
    }
}