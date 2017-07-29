using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartClass.Controllers;

namespace SmartClass.Models.Classes
{
    public class Buildings
    {
        public string Name { get; set; }
        public List<Floors> Floors { get; set; }
        public bool AbnormalEquipment { get; set; }
    }
}