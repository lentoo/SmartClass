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

            //#region 查询楼栋设备Job
            ////创建一个任务
            //IJobDetail job1 = JobBuilder.Create<SearchBuildingAllRoomEquipmentJob>().Build();
            ////触发时间  每10分钟
            //ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()//.WithCronSchedule("0 0 ")
            //    .WithSimpleSchedule(o => o.WithIntervalInMinutes(10).WithRepeatCount(int.MaxValue))
            //    .Build();
            ////添加到任务管理者
            //sched.ScheduleJob(job1, trigger);
            //#endregion

            #region 处理异常信息Job
            IJobDetail exceptionJob = JobBuilder.Create<ProcessExceptionJob>().Build();
            //每10s处理一次
            ISimpleTrigger triggerExceptionJob = (ISimpleTrigger)TriggerBuilder.Create().WithSimpleSchedule(o => o.WithIntervalInSeconds(10).WithRepeatCount(int.MaxValue)).Build();
            sched.ScheduleJob(exceptionJob, triggerExceptionJob);
            #endregion

            #region 同步电子钟Job
            IJobDetail electronicClockJob = JobBuilder.Create<SynchronizeElectronicClockTimeJob>().Build();
            //每周六早上8点同步一次
            ICronTrigger clockTrigger = (ICronTrigger)TriggerBuilder.Create().WithCronSchedule("0 0 8 ? * 6 *").StartNow().Build();
            //ISimpleTrigger t = (ISimpleTrigger)TriggerBuilder.Create().WithSimpleSchedule(o => o.WithIntervalInSeconds(20)).StartNow().Build();
            sched.ScheduleJob(electronicClockJob, clockTrigger);
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