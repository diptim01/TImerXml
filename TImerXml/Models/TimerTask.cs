using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace TImerXml.Models
{
    public class TimerTask
    {
        [DisplayName("Name of the Task")]
        public String NameOfTask { get; set; }

        public String Description { get; set; }

        [DisplayName("Timer in Minute(s)")]
        public String Timer { get; set; }

        

    }
}