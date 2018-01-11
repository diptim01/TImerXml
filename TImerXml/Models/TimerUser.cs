using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Xml.Serialization;


namespace TImerXml.Models
{
    public class TimerUser
    {
        [XmlElement("Email")]
        [DisplayName("Email Address")]
        public String TimerUserEmail { get; set; }

        [DisplayName("Your Name")]
        public String Name { get; set; }

        public List<TimerTask> TimerTasks { get; set; }


    }
}