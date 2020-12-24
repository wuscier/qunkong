using Cj.EmbeddedAPP.BLL;
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
    public class GlobalTaskManager
    {
        private GlobalTaskManager()
        {
            _globalTaskCheckTimer = new Timer(CheckGlobalTask, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void StartCheckingGlobalTasks()
        {
            if (IsGlobalTaskRunning)
            {
                return;
            }
            else
            {
                _globalTaskCheckTimer.Change(2000, Timeout.Infinite);
            }
        }

        public void StopCheckingGlobalTasks()
        {
            if (IsGlobalTaskRunning)
            {
                IsGlobalTaskRunning = false;
            }
        }

        public static readonly GlobalTaskManager Instance = new GlobalTaskManager();

        public bool IsGlobalTaskRunning { get; private set; } = false;

        private Timer _globalTaskCheckTimer;
        private bool _previousGlobalTaskByGroupFinished;
        private Timer _checkLaunchedVmsTimer;
        private int _recheckTimes;
        private int _launchVmsFaildTimes;

        private async void CheckGlobalTask(object state)
        {
            List<TaskSch> globalTasks = TasksBLL.GetGlobalTaskList("waiting");

            if (globalTasks.Count == 0)
            {
                IsGlobalTaskRunning = false;
            }
            else
            {
                IsGlobalTaskRunning = true;

                var earlistStartTime = globalTasks.Min(task => task.Created);

                var earlistGlobalTasks = globalTasks.Where(task => task.Created == earlistStartTime);

               await ExecuteGlobalTasks(earlistGlobalTasks.ToList());

                _globalTaskCheckTimer.Change(2000, Timeout.Infinite);
            }
        }

        private async Task ExecuteGlobalTasks(List<TaskSch> list)
        {
            List<TaskSch> tobeRunGroup = new List<TaskSch>();

            for (int i = 0; i < VmManager.Instance.Row; i++)
            {
                tobeRunGroup.Clear();
                _previousGlobalTaskByGroupFinished = false;

                for (int j = 0; j < VmManager.Instance.Column; j++)
                {
                    int mobileIndex = VmManager.Instance.VmIndexArray[i, j];

                    if (mobileIndex!=-1)
                    {
                        var targetTask = list.FirstOrDefault(task => task.MobileIndex == mobileIndex);

                        if (targetTask!=null)
                        {
                            tobeRunGroup.Add(targetTask);
                        }
                    }
                }

                if (tobeRunGroup.Count>0)
                {
                    ExecuteGlobalTasksByGroup(tobeRunGroup, i);

                    while (true)
                    {
                        if (_previousGlobalTaskByGroupFinished)
                        {
                            break;
                        }
                        else
                        {
                            await Task.Delay(2000);
                        }
                    }
                }
            }
        }

        private async void ExecuteGlobalTasksByGroup(List<TaskSch> tobeRunGroup, int groupIndex)
        {
            if (TimedTaskManager.Instance.IsTimedTaskRunning)
            {
                _previousGlobalTaskByGroupFinished = true;
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

        private async Task ProcessTasks(List<TaskSch> tobeRunGroup)
        {
            List<Task> taskList = new List<Task>();


            if (tobeRunGroup != null)
            {
                foreach (var task in tobeRunGroup)
                {
                    ProcessUtils.AdbOpenApps(task.MobileIndex, "com.facebook.katana");
                    taskList.Add(TasksSchedule.ProcessSingleTask(task));
                }

                await Task.Delay(8000);

                try
                {
                    LogUtils.Information($"count of tobeRunGroup:{taskList.Count}");
                    await Task.WhenAll(taskList);
                }
                catch (Exception ex)
                {
                    _previousGlobalTaskByGroupFinished = true;

                    LogUtils.Error($"{ex}");
                }
            }

            _previousGlobalTaskByGroupFinished = true;

        }

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
                    _previousGlobalTaskByGroupFinished = true;

                    return;
                }
                else
                {
                    _checkLaunchedVmsTimer.Change(6000, Timeout.Infinite);
                }
            }
        }
    }
}
