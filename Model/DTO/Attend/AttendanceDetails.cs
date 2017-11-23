using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO.Attend
{
  public class AttendanceDetails
  {
    public string ID { get; set; }
    public string AttendanceID { get; set; }
    public string StuName { get; set; }
    public string StuNo { get; set; }
    public string AttenTime { get; set; }
    public string ClassNo { get; set; }
    public bool? Flag { get; set; }
    public string Result { get; set; }
    public string ClassRoomNo { get; set; }
    public string ClassName { get; set; }
    public string Teacher { get; set; }
    public string TeacherNo { get; set; }
    public string CourseNo { get; set; }
    public string CourseName { get; set; }
    public string Section { get; set; }
  }
}
