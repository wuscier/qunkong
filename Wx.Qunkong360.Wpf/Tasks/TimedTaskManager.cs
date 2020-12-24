using Cj.EmbeddedAPP.BLL;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wx.Qunkong360.Wpf.Events;
using Wx.Qunkong360.Wpf.Implementation;
using Xzy.EmbeddedApp.Model;
using Xzy.EmbeddedApp.Utils;
using Xzy.EmbeddedApp.WinForm.Socket;
using Xzy.EmbeddedApp.WinForm.Tasks;

namespace Wx.Qunkong360.Wpf.Tasks
{
    public class TimedTaskManager
    {
        private TimedTaskManager()
        {
            _timedTaskCheckTimer = new Timer(CheckTimedTask, null, Timeout.Infinite, Timeout.Infinite);
            //TimedTaskCheckTimer.Change(5000, Timeout.Infinite);
        }

        public void StartCheckingTimedTasks()
        {
            _timedTaskCheckTimer.Change(5000, Timeout.Infinite);
        }

        private async void CheckTimedTask(object state)
        {
            List<TaskSch> timedTasks = TasksBLL.GetTimedTaskList();

            var earliestStartTime = timedTasks.Min(task => task.StartTime);

            var now = DateTime.Now;

            var currentTime = new DateTime(1970, 1, 1, now.Hour, now.Minute, now.Second);

            //var leftBoundaryTime = currentTime.AddMinutes(-5);
            var rightBoundaryTime = currentTime.AddMinutes(5);

            if (earliestStartTime <= rightBoundaryTime)
            {
                var earliestTimedTasks = timedTasks.Where(task => task.StartTime == earliestStartTime);
                await ExecuteTimedTasks(earliestTimedTasks.ToList());

                _timedTaskCheckTimer.Change(5 * 60000, Timeout.Infinite);
            }
            else
            {
                _timedTaskCheckTimer.Change(5 * 60000, Timeout.Infinite);
            }
        }

        private async Task ExecuteTimedTasks(List<TaskSch> list)
        {
            List<TaskSch> tobeRunGroup = new List<TaskSch>();

            for (int i = 0; i < VmManager.Instance.Row; i++)
            {
                tobeRunGroup.Clear();

                _previousTimedTaskByGroupFinished = false;

                for (int j = 0; j < VmManager.Instance.Column; j++)
                {
                    int mobileIndex = VmManager.Instance.VmIndexArray[i, j];

                    if (mobileIndex != -1)
                    {
                        var targetTask = list.FirstOrDefault(task => task.MobileIndex == mobileIndex);
                        if (targetTask != null)
                        {
                            tobeRunGroup.Add(targetTask);
                        }
                    }
                }

                if (tobeRunGroup.Count > 0)
                {
                   await ExecuteTimedTaskByGroup(tobeRunGroup, i);
                    while (!_previousTimedTaskByGroupFinished)
                    {

                    }
                }
            }
        }

        private bool _previousTimedTaskByGroupFinished;

        private Timer _checkLaunchedVmsTimer;

        private async Task ExecuteTimedTaskByGroup(List<TaskSch> tobeRunGroup, int groupIndex)
        {
            if (GlobalTaskManager.Instance.IsGlobalTaskRunning)
            {
                // shutdown vm and wait unitl all running global tasks are done.

                if (VmManager.Instance.RunningGroupIndex !=-1)
                {
                    EventAggregatorManager.Instance.EventAggregator.GetEvent<GroupVmsTriggerEvent>().Publish(VmManager.Instance.RunningGroupIndex);

                    GlobalTaskManager.Instance.StopCheckingGlobalTasks();

                    _checkLaunchedVmsTimer?.Dispose();
                    _checkLaunchedVmsTimer = null;

                    _checkLaunchedVmsTimer = new Timer(CheckLaunchedVms, tobeRunGroup, Timeout.Infinite, Timeout.Infinite);
                    _checkLaunchedVmsTimer.Change(6000, Timeout.Infinite);
                }
            }
            else
            {
                if (VmManager.Instance.RunningGroupIndex == -1)
                {
                    EventAggregatorManager.Instance.EventAggregator.GetEvent<GroupVmsTriggerEvent>().Publish(groupIndex);

                    _checkLaunchedVmsTimer?.Dispose();
                    _checkLaunchedVmsTimer = null;

                    _recheckTimes = 0;
                    _launchVmsFaildTimes = 0;

                    _checkLaunchedVmsTimer = new Timer(CheckLaunchedVms, tobeRunGroup, Timeout.Infinite, Timeout.Infinite);
                    _checkLaunchedVmsTimer.Change(6000, Timeout.Infinite);

                }
                else if (VmManager.Instance.RunningGroupIndex != groupIndex)
                {
                    EventAggregatorManager.Instance.EventAggregator.GetEvent<GroupVmsTriggerEvent>().Publish(VmManager.Instance.RunningGroupIndex);

                    await Task.Delay(5000);

                    EventAggregatorManager.Instance.EventAggregator.GetEvent<GroupVmsTriggerEvent>().Publish(groupIndex);

                    _checkLaunchedVmsTimer?.Dispose();
                    _checkLaunchedVmsTimer = null;

                    _recheckTimes = 0;
                    _launchVmsFaildTimes = 0;

                    _checkLaunchedVmsTimer = new Timer(CheckLaunchedVms, tobeRunGroup, Timeout.Infinite, Timeout.Infinite);
                    _checkLaunchedVmsTimer.Change(6000, Timeout.Infinite);
                }
                else if (VmManager.Instance.RunningGroupIndex == groupIndex)
                {
                    await ProcessTasks(tobeRunGroup);
                }
            }
        }


        private int _launchVmsFaildTimes = 0;
        private int _recheckTimes = 0;


        private async void CheckLaunchedVms(object state)
        {
            if (SocketServer.AllConnectionKey.Values.Count > 0 && VmManager.Instance.Column - SocketServer.AllConnectionKey.Values.Count <= 3)
            {
                _recheckTimes++;

                if (_recheckTimes > 2)
                {
                    List<TaskSch> tasks = state as List<TaskSch>;
                    await ProcessTasks(tasks);
                }
                else
                {
                    _checkLaunchedVmsTimer.Change(6000, Timeout.Infinite);
                }
            }
            else
            {
                _launchVmsFaildTimes++;

                if (_launchVmsFaildTimes >= 50)
                {
                    _previousTimedTaskByGroupFinished = true;

                    return;
                }
                else
                {
                    _checkLaunchedVmsTimer.Change(6000, Timeout.Infinite);
                }
            }
        }

        private async Task ProcessTasks(List<TaskSch> tasks)
        {
            List<Task> taskList = new List<Task>();


            if (tasks != null)
            {
                foreach (var task in tasks)
                {
                    ProcessUtils.AdbOpenApps(task.MobileIndex, "com.facebook.katana");
                    taskList.Add(TasksSchedule.ProcessSingleTask(task));
                }

                await Task.Delay(5000);

                try
                {
                    await Task.WhenAll(taskList);

                    foreach (var task in tasks)
                    {
                        TasksBLL.UpdateTimedTaskExecuteTime(task.Id);
                    }
                }
                catch (Exception ex)
                {
                    LogUtils.Error($"{ex}");

                    foreach (var task in tasks)
                    {
                        TasksBLL.UpdateTimedTaskExecuteTime(task.Id);
                    }

                    _previousTimedTaskByGroupFinished = true;

                }
            }

            _previousTimedTaskByGroupFinished = true;

        }




        public static readonly TimedTaskManager Instance = new TimedTaskManager();

        public Timer _timedTaskCheckTimer;

        public bool IsTimedTaskEnabled { get; set; } = false;

        public bool IsTimedTaskRunning { get; set; }

        public DateTime? StartTime
        {
            get
            {
                if (!IsTimedTaskEnabled || _timePicker == null || !_timePicker.SelectedTime.HasValue)
                {
                    return null;
                }
                else
                {
                    var selectedDateTime = _timePicker.SelectedTime.Value;
                    return new DateTime(1970, 1, 1, selectedDateTime.Hour, selectedDateTime.Minute, selectedDateTime.Second);
                }
            }
        }

        public void SetTimePicker(TimePicker timePicker)
        {
            _timePicker = timePicker;
        }

        private TimePicker _timePicker;
    }
}
