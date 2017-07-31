using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IBLL;
using Model;
using Common;
using Common.Exception;
using Common.Extended;

namespace SmartClass.Models.Job
{
    /// <summary>
    /// 定时同步电子钟
    /// </summary>
    public class SynchronizeElectronicClockTimeJob : IJob
    {
        private readonly IZ_RoomService roomService;
        private readonly SerialPortService PortService;
        public SynchronizeElectronicClockTimeJob(IZ_RoomService roomService, SerialPortService PortService)
        {
            this.roomService = roomService;
            this.PortService = PortService;
        }
        public void Execute(IJobExecutionContext context)
        {
            SynchronizeElectronicClockTime();
        }
        private void SynchronizeElectronicClockTime()
        {
            //查询所有的教室
            List<Z_Room> rooms = roomService.GetEntity(r => r.F_RoomType == "ClassRoom").ToList();

            foreach (var room in rooms)
            {
                Process(room.F_RoomNo, "00");
            }
        }
        private void Process(string classroom, string nodeAdd)
        {
            try
            {
                byte fun = (byte)Convert.ToInt32(AppSettingUtils.GetValue("Clock"));

                byte[] classAddr =classroom.StrToHexByte();
                byte[] nodeAddr = nodeAdd.StrToHexByte();
                DateTime currentTime = DateTime.Now;                //获取当前时间
                                                                    //转换时间格式
                string year = (currentTime.Year % 100).ToString();
                string month = currentTime.Month < 10 ? "0" + currentTime.Month : currentTime.Month.ToString();
                string day = currentTime.Day < 10 ? "0" + currentTime.Day : currentTime.Day.ToString();
                string hour = currentTime.Hour < 10 ? "0" + currentTime.Hour : currentTime.Hour.ToString();
                string minute = currentTime.Minute < 10 ? "0" + currentTime.Minute : currentTime.Minute.ToString();
                string second = currentTime.Second < 10 ? "0" + currentTime.Second : currentTime.Second.ToString();
                string date = $"{year} {month} {day}";      //日期部分
                byte[] yMd = date.StrToHexByte();   //将日期部分转为byte[]类型
                string time = $"{hour} {minute} {second}";  //时间部分
                byte[] hms = time.StrToHexByte();   //将时间部分转为byte[]类型
                byte week = (byte)(Convert.ToInt32(currentTime.DayOfWeek.ToString("d")));

                byte[] cmd = { 0x55, 0x02, 0, 0, fun, 0, 0x0D, 0, 0, 0, week, 0, 0, 0, 0x23, 0, 0, 0x70, 0, 0 };
                classAddr.CopyTo(cmd, 2);
                nodeAddr.CopyTo(cmd, 5);
                yMd.CopyTo(cmd, 7);
                hms.CopyTo(cmd, 11);
                cmd = cmd.ActuatorCommand();
                PortService.SendCmd(cmd);
            }
            catch (Exception exception)
            {
                ExceptionHelper.AddException(exception);
            }
        }
    }
}