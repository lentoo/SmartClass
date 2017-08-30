using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Quartz;
using Quartz.Impl;
using System.Web.Mvc;
using SmartClass.IService;
using SmartClass.Service;
using System.Data.Entity;
using Model;

namespace SmartClass.Models.Job
{
  /// <summary>
  /// Quartz定时任务框架配置
  /// </summary>
  public class QuartzConfig
  {
    private static IScheduler sched;

    /// <summary>
    /// 初始化任务
    /// </summary>
    public static void InitJob()
    {
      sched = DependencyResolver.Current.GetService<IScheduler>();
      #region 查询楼栋设备Job
      using (var dbcontext = new NFineBaseEntities())
      {
        //查询教室总数
        int count = dbcontext.Z_Room.Where(u => u.F_RoomType == "ClassRoom").ToList().Count;
        //创建一个任务
        IJobDetail job1 = JobBuilder.Create<SearchBuildingAllRoomEquipmentJob>().Build();
        //触发时间  教室总数乘以10s
        ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()//.WithCronSchedule("0 0 ")
            .WithSimpleSchedule(o => o.WithIntervalInMinutes(count).WithRepeatCount(int.MaxValue))
            .Build();
        //添加到任务管理者
        sched.ScheduleJob(job1, trigger);
      }
      #endregion

      #region 处理异常信息Job           

      IJobDetail exceptionJob = JobBuilder.Create<ProcessExceptionJob>().Build();
      //每10s处理一次
      ISimpleTrigger triggerExceptionJob = (ISimpleTrigger)TriggerBuilder.Create().WithSimpleSchedule(o => o.WithIntervalInSeconds(10).WithRepeatCount(int.MaxValue)).Build();
      sched.ScheduleJob(exceptionJob, triggerExceptionJob);

      #endregion

      #region 同步电子钟Job

      IJobDetail electronicClockJob = JobBuilder.Create<SynchronizeElectronicClockTimeJob>().Build();
      //每周日早上8点同步一次
      ICronTrigger clockTrigger = (ICronTrigger)TriggerBuilder.Create().WithCronSchedule("0 0 8 ? * 7 *").StartNow().Build();
      //ISimpleTrigger t = (ISimpleTrigger)TriggerBuilder.Create().WithSimpleSchedule(o => o.WithIntervalInSeconds(20)).StartNow().Build();
      sched.ScheduleJob(electronicClockJob, clockTrigger);

      #endregion

      #region 处理每日课程
      IJobDetail CourseJob = JobBuilder.Create<TimingProcessDailyCoursesJob>().Build();
      //每周一到周五早上6点触发一次
      ICronTrigger CourseTrigger = (ICronTrigger)TriggerBuilder.Create().WithCronSchedule("0 0 6 ? * 2,3,4,5,6 *").StartNow().Build();
      //ISimpleTrigger CourseTrigger = (ISimpleTrigger)TriggerBuilder.Create().WithSimpleSchedule(o => o.WithIntervalInHours(1)).StartNow().Build();
      sched.ScheduleJob(CourseJob, CourseTrigger);

      #endregion
    }
    /// <summary>
    /// 暂停所有任务计划
    /// </summary>
    public static void PauseAllJob()
    {
      if (sched == null)
      {
        throw new NullReferenceException("IScheduler为Null");
      }
      sched.PauseAll();
    }
    /// <summary>
    /// 开始执行任务计划
    /// </summary>
    public static void StartJob()
    {
      if (sched == null)
      {
        throw new NullReferenceException("IScheduler为Null");
      }
      sched.Start();
    }
    /// <summary>
    /// 停止所有任务计划
    /// </summary>
    public static void StopJob()
    {
      if (sched == null)
      {
        throw new NullReferenceException("IScheduler为Null");
      }
      sched.Shutdown();
    }
  }
}