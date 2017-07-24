using Models.Classes;
using System.Collections.Generic;

namespace SmartClass.Models.Classes
{
    public class Floors
    {
        public string Name { get; set; }
        public List<ClassRoom> ClassRooms { get; set; }
    }
}