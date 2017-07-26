using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using Quartz.Impl;
using System.Web.Mvc;

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

            IJobDetail job1 = JobBuilder.Create<SearchBuildingAllRoomEquipmentJob>().Build();
            ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create().WithSimpleSchedule(o => o.WithIntervalInMinutes(10).WithRepeatCount(int.MaxValue)).Build();
            sched.ScheduleJob(job1, trigger);

            IJobDetail exceptionJob = JobBuilder.Create<ProcessExceptionJob>().Build();
            ISimpleTrigger triggerExceptionJob = (ISimpleTrigger)TriggerBuilder.Create().WithSimpleSchedule(o => o.WithIntervalInSeconds(10).WithRepeatCount(int.MaxValue)).Build();
            sched.ScheduleJob(exceptionJob, triggerExceptionJob);
           
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