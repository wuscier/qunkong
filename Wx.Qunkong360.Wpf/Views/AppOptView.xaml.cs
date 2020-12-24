﻿using Cj.EmbeddedAPP.BLL;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using WpfTreeView;
using Wx.Qunkong360.Wpf.Implementation;
using Wx.Qunkong360.Wpf.Tasks;
using Wx.Qunkong360.Wpf.Utils;
using Wx.Qunkong360.Wpf.ViewModels;
using Xzy.EmbeddedApp.Model;
using Xzy.EmbeddedApp.Utils;
using Xzy.EmbeddedApp.WinForm.Tasks;
using Panel = System.Windows.Forms.Panel;

namespace Wx.Qunkong360.Wpf
{
    /// <summary>
    /// AppOptView.xaml 的交互逻辑
    /// </summary>
    public partial class AppOptView
    {
        AppOptViewModel _appOptViewModel;
        public AppOptView(AppOptViewModel appOptViewModel)
        {
            InitializeComponent();
            _appOptViewModel = appOptViewModel;
            if (ConfigVals.Lang == 1)
            {
                Res.Culture.Button = new Res_Zh_Button();
                ResD.CultureD.Button = new ResD_Zh_Button();
            }
            else if (ConfigVals.Lang == 2)
            {
                Res.Culture.Button = new Res_En_Button();
                ResD.CultureD.Button = new ResD_En_Button();
            }

            Title = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Operation", SystemLanguageManager.Instance.CultureInfo);
        }

        /// <summary>
        /// 初始化树结构
        /// </summary>
        /// <param name="wpfTreeView"></param>
        private void InitRunningVmsTreeView(WpfTreeView.WpfTreeView wpfTreeView)
        {
            int runningGroupIndex = VmManager.Instance.RunningGroupIndex;            

            if (runningGroupIndex == -1)
            {
                return;
            }

            int groupEndIndex = VmManager.Instance.VmIndexArray[runningGroupIndex, VmManager.Instance.Column - 1];
            int endNumber = groupEndIndex == -1 ? VmManager.Instance.MaxVmNumber : groupEndIndex + 1;


            //string firstLevelNodeText = $"第{runningGroupIndex + 1}组 {VmManager.Instance.VmIndexArray[runningGroupIndex, 0] + 1}-{endNumber}";
            string firstLevelNodeText = string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Group", SystemLanguageManager.Instance.CultureInfo), runningGroupIndex + 1, VmManager.Instance.VmIndexArray[runningGroupIndex, 0] + 1, endNumber);

            List<WpfTreeViewItem> wpfTreeViewItems = new List<WpfTreeViewItem>();

            WpfTreeViewItem topLevelNode = new WpfTreeViewItem()
            {
                Caption = firstLevelNodeText,
                Id = -1,
                IsExpanded = true,
            };

            wpfTreeViewItems.Add(topLevelNode);

            for (int i = 0; i < VmManager.Instance.Column; i++)
            {
                if (VmManager.Instance.VmIndexArray[runningGroupIndex, i] != -1)
                {
                    WpfTreeViewItem subNode = new WpfTreeViewItem()
                    {
                        Id = VmManager.Instance.VmIndexArray[runningGroupIndex, i] + 1,
                        Caption = string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Phone_Num", SystemLanguageManager.Instance.CultureInfo), VmManager.Instance.VmIndexArray[runningGroupIndex, i] + 1),
                        ParentId = -1,
                    };

                    wpfTreeViewItems.Add(subNode);
                }
            }

            wpfTreeView.SetItemsSourceData(wpfTreeViewItems, item => item.Caption, item => item.IsExpanded, item => item.Id, item => item.ParentId);
        }

        private void btnAddPicture_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Panel picContainer = btn.Tag as Panel;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Title = SystemLanguageManager.Instance.ResourceManager.GetString("Select_Picture", SystemLanguageManager.Instance.CultureInfo);
            openFileDialog.Filter = SystemLanguageManager.Instance.ResourceManager.GetString("Picture_Filter", SystemLanguageManager.Instance.CultureInfo);

            if (openFileDialog.ShowDialog().Value)
            {
                if (picContainer.Controls.Count > 0)
                {
                    picContainer.Controls.Clear();
                    //picContainer.ScrollControlIntoView(picContainer.Controls[0]);
                }

                string[] addedPicPath = openFileDialog.FileNames;

                int startingIndex = picContainer.Controls.Count;
                double total = picContainer.Controls.Count + addedPicPath.Length;
                double columnCapacity = TaskManager.ColumnCapacity;

                int totalRow = (int)Math.Ceiling(total / columnCapacity);

                for (int row = 0; row < totalRow; row++)
                {
                    for (int column = 0; column < TaskManager.ColumnCapacity; column++)
                    {
                        int picturePathIndex = row * TaskManager.ColumnCapacity + column;

                        if (picturePathIndex < total && picturePathIndex >= startingIndex)
                        {

                            Panel panel = new Panel()
                            {
                                Name = $"panel{picturePathIndex}",
                                BackColor = System.Drawing.Color.Gray,
                                Width = 300,
                                Height = 300,
                                Location = new System.Drawing.Point()
                                {
                                    X = 5 + column * 305,
                                    Y = 5 + row * 305,
                                },
                            };

                            System.Windows.Forms.CheckBox checkBox = new System.Windows.Forms.CheckBox()
                            {
                                Checked = false,
                                Location = new System.Drawing.Point()
                                {
                                    X = 0,
                                    Y = 0,
                                },
                                Width = 30,
                                Height = 30
                            };

                            System.Windows.Forms.PictureBox pictureBox = new System.Windows.Forms.PictureBox()
                            {
                                BackColor = System.Drawing.Color.Gray,
                                Width = 300,
                                Height = 300,
                                ImageLocation = addedPicPath[picturePathIndex - startingIndex],
                                Location = new System.Drawing.Point()
                                {
                                    X = 0,
                                    Y = 0,
                                },
                                SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom,
                            };

                            panel.Controls.Add(checkBox);
                            panel.Controls.Add(pictureBox);

                            picContainer.Controls.Add(panel);
                        }
                    }
                }
            }
        }

        private void btnDeletePicture_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Panel picContainer = btn.Tag as Panel;

            var panels = picContainer.Controls.Cast<Panel>().Where(p => p.Controls[0] is System.Windows.Forms.CheckBox && ((System.Windows.Forms.CheckBox)p.Controls[0]).Checked);

            if (panels.Count() == 0)
            {
                return;
            }

            List<string> tobeDeletedKeys = new List<string>();

            foreach (var p in panels)
            {
                tobeDeletedKeys.Add(p.Name);
            }

            foreach (var key in tobeDeletedKeys)
            {
                picContainer.Controls.RemoveByKey(key);
            }

            double total = picContainer.Controls.Count;
            double columnCapacity = TaskManager.ColumnCapacity;

            int totalRow = (int)Math.Ceiling(total / columnCapacity);

            for (int row = 0; row < totalRow; row++)
            {
                for (int column = 0; column < TaskManager.ColumnCapacity; column++)
                {
                    int picturePathIndex = row * TaskManager.ColumnCapacity + column;

                    if (picturePathIndex < total)
                    {
                        Panel panel = picContainer.Controls[picturePathIndex] as Panel;

                        panel.Location = new System.Drawing.Point()
                        {
                            X = 5 + column * 305,
                            Y = 5 + row * 305,
                        };
                    }
                }
            }

        }

        public async Task ProessTask()
        {
            ConfigVals.IsRunning = 1;
            //TasksSchedule tasks = new TasksSchedule();
            await TasksSchedule.ProessTask();
        }

        #region 通讯录导入
        private void tabImportContact_Loaded(object sender, RoutedEventArgs e)
        {
            lblImportContact_MouseLeftButtonDown(null, null);
        }

        private void lblImportContact_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is null))
            {

                lblImportContact.FontWeight = FontWeights.Bold;
                lblSendMsg.FontWeight = FontWeights.Regular;
                lblPostMoment.FontWeight = FontWeights.Regular;
                lblProfile.FontWeight = FontWeights.Regular;
                lblTaskManagement.FontWeight = FontWeights.Regular;
                lblScanning.FontWeight = FontWeights.Regular;
                lblLoginInfo.FontWeight = FontWeights.Regular;
                lblCreateGroup.FontWeight = FontWeights.Regular;
                lblSendGroupMessage.FontWeight = FontWeights.Regular;
            }

            InitRunningVmsTreeView(wpfTreeview1);

            btnClearContact.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Clear_Contact", SystemLanguageManager.Instance.CultureInfo);
            lblImportContact.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Import_Address_List", SystemLanguageManager.Instance.CultureInfo);
            tbEnterPhoneNumsTips.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Enter_Phone_Number", SystemLanguageManager.Instance.CultureInfo);
            btnSelect.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Select_Path", SystemLanguageManager.Instance.CultureInfo);
            btnImport.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Import", SystemLanguageManager.Instance.CultureInfo);

            bool enabled = VmManager.Instance.RunningGroupIndex != -1;

            gridImport.IsEnabled = enabled;
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            string path = string.Empty;
            OpenFileDialog openfile = new OpenFileDialog();
            openfile.Multiselect = false;
            openfile.Title = SystemLanguageManager.Instance.ResourceManager.GetString("Select_Phone_File", SystemLanguageManager.Instance.CultureInfo);
            openfile.Filter = SystemLanguageManager.Instance.ResourceManager.GetString("Text_File", SystemLanguageManager.Instance.CultureInfo);
            if (openfile.ShowDialog().Value)
            {
                tbContactPath.Text = openfile.FileName;
            }

        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            //移动文件到指定的目录

            var targets = from item in wpfTreeview1.ItemsSourceData.FirstOrDefault().Children
                          where item.IsChecked
                          select (int)item.Id - 1;

            if (targets.Count() == 0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            List<int> checkMobiles = targets.ToList();

            List<string> phoneStr = new List<string>();

            TextRange textRange = new TextRange(rtbPhoneNums.Document.ContentStart, rtbPhoneNums.Document.ContentEnd);

            if (!string.IsNullOrEmpty(textRange.Text))
            {
                phoneStr = textRange.Text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.None).ToList();
                phoneStr = phoneStr.Where(s => !string.IsNullOrEmpty(s)).ToList();
            }
            int flag = 0;
            if (tbContactPath.Text != "")
            {
                StreamReader sr = new StreamReader(tbContactPath.Text, Encoding.Default);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    phoneStr.Add(line.ToString());

                    if (phoneStr.Count > 100000)
                    {
                        flag = -1;
                        break;
                    }
                }
            }

            if (flag == -1)
            {
                MessageDialogManager.ShowDialogAsync(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Exceed_Max_Import_Num", SystemLanguageManager.Instance.CultureInfo)));
                return;
            }

            //插入到任务表中
            int sr1 = 21 / 1;
            int onenums = phoneStr.Count / checkMobiles.Count;
            PhonenumBLL phonebll = new PhonenumBLL();
            //TasksBLL taskbll = new TasksBLL();           

            for (int m = 0; m < checkMobiles.Count; m++)
            {
                int res = 0;
                List<string> strIds = new List<string>();
                if (checkMobiles.Count > 1 && m == checkMobiles.Count - 1)
                {
                    onenums = phoneStr.Count;
                }
                for (int i = 0; i < onenums; i++)
                {
                    Phonenum phone = new Phonenum();
                    phone.PhoneNum = phoneStr[i];
                    phone.SimulatorId = checkMobiles[m];
                    phone.Status = 0;   //待导入

                    int nflag = phonebll.InsertPhoneNum(phone);
                    if (nflag > 0)
                    {
                        res++;
                    }
                    strIds.Add(phone.PhoneNum);
                }
                if (strIds != null && strIds.Count > 0)
                {
                    for (int j = 0; j < strIds.Count; j++)
                        phoneStr.Remove(strIds[j]);
                }
                //号码写入文件
                string filepath = CopyPhoneNumsFile(strIds, checkMobiles[m]);

                var lists = new JArray
                {
                };

                if (filepath != "")
                {
                    lists.Add(filepath);
                    var obj = new JObject() { { "tasktype", 1 }, { "txtmsg", "" } };
                    obj.Add("list", lists);
                    //插入任务
                    TaskSch tasks = new TaskSch();
                    tasks.TypeId = 1;
                    tasks.Remotes = checkMobiles[m].ToString();
                    tasks.MobileIndex = checkMobiles[m];
                    tasks.Bodys = obj.ToString(Formatting.None);
                    //tasks.Bodys = JsonConvert.SerializeObject(new string[] { "tasktype:1", "txtmsg:", "filepath:"+ filepath, "nums:"+res}); 
                    tasks.Status = "waiting";
                    tasks.ResultVal = "";
                    tasks.RepeatNums = 1;
                    tasks.RandomMins = 5;
                    tasks.RandomMaxs = 12;
                    tasks.StartTime = TimedTaskManager.Instance.StartTime;

                    TasksBLL.CreateTask(tasks);
                }
            }
            //启动任务列表        
            if (ConfigVals.IsRunning != 1)
            {
                Task.Run(async () =>
                {
                    await ProessTask();
                });
            }

            MessageDialogManager.ShowDialogAsync(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Submitted_Task", SystemLanguageManager.Instance.CultureInfo), checkMobiles.Count));
        }

        private void btnClearContact_Click(object sender, RoutedEventArgs e)
        {
            var targets = from item in wpfTreeview1.ItemsSourceData.FirstOrDefault().Children
                          where item.IsChecked
                          select (int)item.Id - 1;

            if (targets.Count() == 0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            List<int> checkMobiles = targets.ToList();

            foreach (int mobileIndex in checkMobiles)
            {
                string jsonMsg2 = "{" + "\"tasktype\":13, \"txtmsg\":\"\", \"list\":[]" + "}";

                TaskSch taskSch = new TaskSch()
                {
                    Bodys = jsonMsg2,
                    MobileIndex = mobileIndex,
                    TypeId = (int)TaskType.ClearContact,
                    Status = "waiting",
                    RepeatNums = 1,
                    RandomMins = 1,
                    RandomMaxs = 2,
                    StartTime = TimedTaskManager.Instance.StartTime,
                };

                TasksBLL.CreateTask(taskSch);
            }

            //启动任务
            Task.Run(async () =>
            {
                await ProessTask();
            });

            MessageDialogManager.ShowDialogAsync(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Submitted_Task", SystemLanguageManager.Instance.CultureInfo), checkMobiles.Count));
        }


        /// <summary>
        /// 拷贝号码文件到模拟器
        /// </summary>
        /// <returns></returns>
        public string CopyPhoneNumsFile(List<string> strIds, int mobileIndex)
        {
            string res = "";
            if (strIds != null && strIds.Count > 0)
            {
                string filename = DateTime.Now.ToString("yyyyMMddHHmmssffff");

                //判断目录是否存在
                string filepath = $"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}/PhoneFiles/";
                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }

                string path = $"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}/PhoneFiles/{filename}.txt";
                FileStream fs = new FileStream(path, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);

                for (int i = 0; i < strIds.Count; i++)
                {
                    sw.Write(strIds[i] + "\r\n");
                }
                //清空缓冲区
                sw.Flush();
                //关闭流
                sw.Close();
                fs.Close();
                if (File.Exists(path))
                {
                    //移动到sd卡
                    string target = $"/sdcard/qunkong/txt/{filename}.txt";
                    //string target = $"/sdcard/qunkong/txt/5201314.txt";
                    string mobileId = DeviceConnectionManager.Instance.GetDeviceNameByMobileIndex(mobileIndex);
                    ProcessUtils.PushFileToVm(mobileId, path, target);

                    res = target;
                }
            }

            return res;
        }

        #endregion

        #region 发送消息
        private void tabSendMsg_Loaded(object sender, RoutedEventArgs e)
        {
            lblSendMsg_MouseLeftButtonDown(null, null);
        }

        private void lblSendMsg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is null))
            {

                lblImportContact.FontWeight = FontWeights.Regular;
                lblSendMsg.FontWeight = FontWeights.Bold;
                lblPostMoment.FontWeight = FontWeights.Regular;
                lblProfile.FontWeight = FontWeights.Regular;
                lblTaskManagement.FontWeight = FontWeights.Regular;
                lblScanning.FontWeight = FontWeights.Regular;
                lblLoginInfo.FontWeight = FontWeights.Regular;
                lblCreateGroup.FontWeight = FontWeights.Regular;
                lblSendGroupMessage.FontWeight = FontWeights.Regular;
            }

            btnAddPicture.Tag = panelPics;
            btnDeletePicture.Tag = panelPics;

            SetSendMessagesPageStatus();

            InitRunningVmsTreeView(wpfTreeview2);

            InitSendMessageTypes();

            Localize();
        }

        private void SetSendMessagesPageStatus()
        {
            bool enabled = VmManager.Instance.RunningGroupIndex != -1;

            gridSendMsg.IsEnabled = enabled;
        }

        private void InitSendMessageTypes()
        {
            cbMsgTypes.Items.Clear();
            cbMsgTypes.Items.Add(new MessageType() { Display = SystemLanguageManager.Instance.ResourceManager.GetString("MsgType_Text", SystemLanguageManager.Instance.CultureInfo), Value = (int)TaskType.SendMessage });
            cbMsgTypes.Items.Add(new MessageType() { Display = SystemLanguageManager.Instance.ResourceManager.GetString("MsgType_Picture", SystemLanguageManager.Instance.CultureInfo), Value = (int)TaskType.SendPicture });
            cbMsgTypes.Items.Add(new MessageType() { Display = SystemLanguageManager.Instance.ResourceManager.GetString("MsgType_Picture_Text", SystemLanguageManager.Instance.CultureInfo), Value = (int)TaskType.SendMessageAndPicture });
            cbMsgTypes.Items.Add(new MessageType() { Display = SystemLanguageManager.Instance.ResourceManager.GetString("MsgType_Video", SystemLanguageManager.Instance.CultureInfo), Value = (int)TaskType.SendVideo });
            cbMsgTypes.Items.Add(new MessageType() { Display = SystemLanguageManager.Instance.ResourceManager.GetString("MsgType_Video_Text", SystemLanguageManager.Instance.CultureInfo), Value = (int)TaskType.SendMessageAndVideo });
            //cbMsgTypes.Items.Add(new MessageType() { Display = "音频消息", Value = (int)TaskType.SendAudio });

            cbMsgTypes.DisplayMemberPath = "Display";
            cbMsgTypes.SelectedValuePath = "Value";
            cbMsgTypes.SelectedIndex = 0;
        }

        private void Localize()
        {
            lblSendMsg.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Group_Messages", SystemLanguageManager.Instance.CultureInfo);
            tbSelectMsgType.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Select_Message_Type", SystemLanguageManager.Instance.CultureInfo);
            gpText.Header = SystemLanguageManager.Instance.ResourceManager.GetString("Text", SystemLanguageManager.Instance.CultureInfo);
            gpPicture.Header = SystemLanguageManager.Instance.ResourceManager.GetString("Picture", SystemLanguageManager.Instance.CultureInfo);
            gpVideo.Header = SystemLanguageManager.Instance.ResourceManager.GetString("Video", SystemLanguageManager.Instance.CultureInfo);
            btnClearMessage.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Clear_Text", SystemLanguageManager.Instance.CultureInfo);
            btnAddPicture.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Add_Picture", SystemLanguageManager.Instance.CultureInfo);
            btnDeletePicture.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Delete_Picture", SystemLanguageManager.Instance.CultureInfo);
            btnAddVideo.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Add_Video", SystemLanguageManager.Instance.CultureInfo);
            btnDeleteVideo.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Delete_Video", SystemLanguageManager.Instance.CultureInfo);
            lblSendMsgTimes.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Send_Message_Times", SystemLanguageManager.Instance.CultureInfo);
            lblIntervalUnit.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Second", SystemLanguageManager.Instance.CultureInfo);
            lblMsgTaskInterval.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Send_Message_Interval", SystemLanguageManager.Instance.CultureInfo);
            btnSubmitMsgTask.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Submit_Task", SystemLanguageManager.Instance.CultureInfo);
        }

        class MessageType
        {
            public string Display { get; set; }
            public int Value { get; set; }
        }

        private void cbMsgTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageType messageType = cbMsgTypes.SelectedItem as MessageType;

            if (messageType == null || VmManager.Instance.RunningGroupIndex == -1)
            {
                return;
            }

            TaskType taskType = (TaskType)messageType.Value;

            switch (taskType)
            {
                case TaskType.ImportContacts:
                    break;
                case TaskType.PostMessage:
                    break;
                case TaskType.PostPicture:
                    break;
                case TaskType.PostMessageAndPicture:
                    break;
                case TaskType.SendMessage:

                    rtbMessage.IsEnabled = true;
                    btnClearMessage.IsEnabled = true;
                    //btnSendMsg.IsEnabled = true;

                    panelPics.Enabled = false;
                    panelVideos.Enabled = false;


                    btnAddPicture.IsEnabled = false;
                    btnDeletePicture.IsEnabled = false;
                    btnAddVideo.IsEnabled = false;
                    btnDeleteVideo.IsEnabled = false;




                    break;
                case TaskType.SendPicture:

                    rtbMessage.IsEnabled = false;
                    btnClearMessage.IsEnabled = false;
                    //btnSendMsg.IsEnabled = true;


                    panelPics.Enabled = true;
                    panelVideos.Enabled = false;


                    btnAddPicture.IsEnabled = true;
                    btnDeletePicture.IsEnabled = true;
                    btnAddVideo.IsEnabled = false;
                    btnDeleteVideo.IsEnabled = false;



                    break;
                case TaskType.SendMessageAndPicture:

                    rtbMessage.IsEnabled = true;
                    btnClearMessage.IsEnabled = true;
                    //btnSendMsg.IsEnabled = true;


                    panelPics.Enabled = true;
                    panelVideos.Enabled = false;


                    btnAddPicture.IsEnabled = true;
                    btnDeletePicture.IsEnabled = true;
                    btnAddVideo.IsEnabled = false;
                    btnDeleteVideo.IsEnabled = false;



                    break;
                case TaskType.SendVideo:

                    rtbMessage.IsEnabled = false;
                    btnClearMessage.IsEnabled = false;
                    //btnSendMsg.IsEnabled = true;


                    panelPics.Enabled = false;
                    panelVideos.Enabled = true;


                    btnAddPicture.IsEnabled = false;
                    btnDeletePicture.IsEnabled = false;
                    btnAddVideo.IsEnabled = true;
                    btnDeleteVideo.IsEnabled = true;



                    break;
                case TaskType.SendMessageAndVideo:

                    rtbMessage.IsEnabled = true;
                    btnClearMessage.IsEnabled = true;
                    //btnSendMsg.IsEnabled = true;


                    panelPics.Enabled = false;
                    panelVideos.Enabled = true;


                    btnAddPicture.IsEnabled = false;
                    btnDeletePicture.IsEnabled = false;
                    btnAddVideo.IsEnabled = true;
                    btnDeleteVideo.IsEnabled = true;



                    break;
                default:
                    break;
            }
        }

        private void btnClearMessage_Click(object sender, RoutedEventArgs e)
        {
            rtbMessage.Document.Blocks.Clear();
        }

        private void btnAddVideo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Title = SystemLanguageManager.Instance.ResourceManager.GetString("Select_Video", SystemLanguageManager.Instance.CultureInfo);
            openFileDialog.Filter = SystemLanguageManager.Instance.ResourceManager.GetString("Video_Filter", SystemLanguageManager.Instance.CultureInfo);

            if (openFileDialog.ShowDialog().Value)
            {
                if (panelVideos.Controls.Count > 0)
                {
                    panelVideos.ScrollControlIntoView(panelVideos.Controls[0]);
                }

                string[] addedVideoPath = openFileDialog.FileNames;

                int startingIndex = panelVideos.Controls.Count;
                double total = panelVideos.Controls.Count + addedVideoPath.Length;
                double columnCapacity = TaskManager.ColumnCapacity;

                int totalRow = (int)Math.Ceiling(total / columnCapacity);

                for (int row = 0; row < totalRow; row++)
                {
                    for (int column = 0; column < TaskManager.ColumnCapacity; column++)
                    {
                        int picturePathIndex = row * TaskManager.ColumnCapacity + column;

                        if (picturePathIndex < total && picturePathIndex >= startingIndex)
                        {
                            ElementHost host = new ElementHost()
                            {
                                Name = $"host{picturePathIndex}",
                                BackColor = System.Drawing.Color.Gray,
                                Width = 300,
                                Height = 300,
                                //ImageLocation = addedPicPath[picturePathIndex - startingIndex],
                                Location = new System.Drawing.Point()
                                {
                                    X = 5 + column * 305,
                                    Y = 5 + row * 305,
                                },
                                //SizeMode = PictureBoxSizeMode.Zoom,
                            };

                            System.Windows.Controls.Grid grid = new System.Windows.Controls.Grid();

                            System.Windows.Controls.CheckBox checkBox = new System.Windows.Controls.CheckBox()
                            {
                                Width = 30,
                                Height = 30,
                                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                            };


                            System.Windows.Controls.MediaElement mediaElement = new MediaElement()
                            {
                                Width = 300,
                                Height = 300,
                                //VerticalAlignment = System.Windows.VerticalAlignment.Bottom,
                                //HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                                Source = new Uri(addedVideoPath[picturePathIndex - startingIndex]),
                            };
                            grid.Children.Add(checkBox);
                            grid.Children.Add(mediaElement);

                            host.Child = grid;

                            panelVideos.Controls.Add(host);

                        }
                    }
                }
            }

        }

        private void btnDeleteVideo_Click(object sender, RoutedEventArgs e)
        {
            var hosts = from host in panelVideos.Controls.Cast<ElementHost>()
                        where host.Child is System.Windows.Controls.Grid &&
                        ((System.Windows.Controls.Grid)host.Child).Children[0] is System.Windows.Controls.CheckBox &&
                      ((System.Windows.Controls.CheckBox)((System.Windows.Controls.Grid)host.Child).Children[0]).IsChecked.Value
                        select host;

            if (hosts.Count() == 0)
            {
                return;
            }

            List<string> tobeDeletedKeys = new List<string>();

            foreach (var h in hosts)
            {
                tobeDeletedKeys.Add(h.Name);
            }

            foreach (var key in tobeDeletedKeys)
            {
                panelVideos.Controls.RemoveByKey(key);
            }

            double total = panelVideos.Controls.Count;
            double columnCapacity = TaskManager.ColumnCapacity;

            int totalRow = (int)Math.Ceiling(total / columnCapacity);

            for (int row = 0; row < totalRow; row++)
            {
                for (int column = 0; column < TaskManager.ColumnCapacity; column++)
                {
                    int picturePathIndex = row * TaskManager.ColumnCapacity + column;

                    if (picturePathIndex < total)
                    {
                        ElementHost host = panelVideos.Controls[picturePathIndex] as ElementHost;

                        host.Location = new System.Drawing.Point()
                        {
                            X = 5 + column * 305,
                            Y = 5 + row * 305,
                        };
                    }
                }
            }

        }

        private void btnSubmitMsgTask_Click(object sender, RoutedEventArgs e)
        {
            btnSubmitMsgTask.IsEnabled = false;
            try
            {
                if (!IsValidInput())
                {
                    btnSubmitMsgTask.IsEnabled = true;
                    return;
                }

                string[] paths = null;

                var targets = from item in wpfTreeview2.ItemsSourceData.FirstOrDefault().Children
                              where item.IsChecked
                              select (int)item.Id - 1;


                if (targets.Count() == 0)
                {
                    btnSubmitMsgTask.IsEnabled = true;

                    MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
                    return;
                }

                MessageType messageType = (MessageType)cbMsgTypes.SelectedItem;

                string folderName = string.Empty;

                switch ((TaskType)messageType.Value)
                {
                    case TaskType.ImportContacts:
                        break;
                    case TaskType.PostMessage:
                        break;
                    case TaskType.PostPicture:
                        break;
                    case TaskType.PostMessageAndPicture:
                        break;
                    case TaskType.SendMessage:
                        break;
                    case TaskType.SendPicture:
                    case TaskType.SendMessageAndPicture:

                        folderName = "image";

                        var pictures = from pic in panelPics.Controls.Cast<Panel>()
                                       select ((System.Windows.Forms.PictureBox)pic.Controls[1]).ImageLocation;

                        paths = pictures.ToArray();

                        break;
                    case TaskType.SendVideo:
                    case TaskType.SendMessageAndVideo:

                        folderName = "video";

                        var videos = from host in panelVideos.Controls.Cast<ElementHost>()
                                     select ((MediaElement)(((System.Windows.Controls.Grid)host.Child).Children[1])).Source.OriginalString;

                        paths = videos.ToArray();

                        break;
                    case TaskType.SendAudio:

                        folderName = "audio";


                        break;
                    default:
                        break;
                }

                TextRange textRange = new TextRange(rtbMessage.Document.ContentStart, rtbMessage.Document.ContentEnd);
                TaskManager taskManager = new TaskManager((TaskType)messageType.Value, textRange.Text, paths, targets.ToArray());


                string dir = DateTime.Now.ToString("yyyyMMddHHmmssffff");

                foreach (int mobileIndex in taskManager.MobileIndexs)
                {
                    var lists = new JArray
                    {

                    };


                    for (int i = 0; i < taskManager.Paths?.Length; i++)
                    {
                        string target = $"/sdcard/qunkong/{folderName}/{dir}/{i + 1}{System.IO.Path.GetExtension(taskManager.Paths[i])}";
                        string mobileId = DeviceConnectionManager.Instance.GetDeviceNameByMobileIndex(mobileIndex);

                        ProcessUtils.PushFileToVm(mobileId, taskManager.Paths[i], target);

                        lists.Add(target);
                    }

                    var obj = new JObject() { { "tasktype", (int)taskManager.TaskType }, { "txtmsg", taskManager.Message } };

                    obj.Add("list", lists);


                    TaskSch taskSch = new TaskSch()
                    {
                        Bodys = obj.ToString(Formatting.None),
                        MobileIndex = mobileIndex,
                        TypeId = (int)taskManager.TaskType,
                        Status = "waiting",
                        RepeatNums = int.Parse(tbSendMsgTimes.Text.Trim()),
                        RandomMins = int.Parse(tbMsgMinInterval.Text.Trim()),
                        RandomMaxs = int.Parse(tbMsgMaxInterval.Text.Trim()),
                        StartTime = TimedTaskManager.Instance.StartTime,
                    };

                    TasksBLL.CreateTask(taskSch);
                }

                Task.Run(async () =>
                {
                    await ProessTask();
                });

                MessageDialogManager.ShowDialogAsync(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Submitted_Task", SystemLanguageManager.Instance.CultureInfo), taskManager.MobileIndexs.Length));

                btnSubmitMsgTask.IsEnabled = true;
            }
            catch (Exception ex)
            {
                btnSubmitMsgTask.IsEnabled = true;
                LogUtils.Error($"{ex}");
                MessageDialogManager.ShowDialogAsync(ex.Message);
            }

        }

        private bool IsValidInput()
        {
            string sRepeatTimes = tbSendMsgTimes.Text.Trim();
            string sMinTimeSpan = tbMsgMinInterval.Text.Trim();
            string sMaxTimeSpan = tbMsgMaxInterval.Text.Trim();

            int intRepeatTimes, intMinTimeSpan, intMaxTimeSpan;

            if (!int.TryParse(sRepeatTimes, out intRepeatTimes))
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Invalid_Repeat_Times", SystemLanguageManager.Instance.CultureInfo));
                return false;
            }
            else
            {
                if (intRepeatTimes < 1)
                {
                    MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Repeat_Time_Less_Than_One", SystemLanguageManager.Instance.CultureInfo));
                    return false;
                }
            }

            if (!int.TryParse(sMinTimeSpan, out intMinTimeSpan))
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Invalid_Interval", SystemLanguageManager.Instance.CultureInfo));
                return false;
            }
            else
            {
                if (intMinTimeSpan < 1)
                {
                    MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Starting_Interval_Less_Than_One", SystemLanguageManager.Instance.CultureInfo));
                    return false;
                }
            }

            if (!int.TryParse(sMaxTimeSpan, out intMaxTimeSpan))
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Invalid_Interval", SystemLanguageManager.Instance.CultureInfo));
                return false;
            }
            else
            {
                if (intMaxTimeSpan < 2)
                {
                    MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Ending_Interval_Less_Than_Two", SystemLanguageManager.Instance.CultureInfo));
                    return false;
                }
            }

            if (intMinTimeSpan >= intMaxTimeSpan)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Starting_Interval_Greater_Than_Ending", SystemLanguageManager.Instance.CultureInfo));
                return false;
            }

            return true;
        }


        #endregion

        #region 发送动态
        private void tabPostMoment_Loaded(object sender, RoutedEventArgs e)
        {
            lblPostMoment_MouseLeftButtonDown(null, null);
        }

        private void lblPostMoment_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is null))
            {

                lblImportContact.FontWeight = FontWeights.Regular;
                lblSendMsg.FontWeight = FontWeights.Regular;
                lblPostMoment.FontWeight = FontWeights.Bold;
                lblProfile.FontWeight = FontWeights.Regular;
                lblTaskManagement.FontWeight = FontWeights.Regular;
                lblScanning.FontWeight = FontWeights.Regular;
                lblLoginInfo.FontWeight = FontWeights.Regular;
                lblCreateGroup.FontWeight = FontWeights.Regular;
                lblSendGroupMessage.FontWeight = FontWeights.Regular;
            }

            btnAddPicture2.Tag = panelPics2;
            btnDeletePicture2.Tag = panelPics2;

            InitRunningVmsTreeView(wpfTreeview3);

            SetSendMessagesPageStatus2();

            Localize2();
        }

        private void Localize2()
        {
            lblPostMoment.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Post_Moments", SystemLanguageManager.Instance.CultureInfo);

            gpText2.Header = SystemLanguageManager.Instance.ResourceManager.GetString("Text", SystemLanguageManager.Instance.CultureInfo);
            gpPicture2.Header = SystemLanguageManager.Instance.ResourceManager.GetString("Picture", SystemLanguageManager.Instance.CultureInfo);

            btnClearMessage2.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Clear_Text", SystemLanguageManager.Instance.CultureInfo);
            btnAddPicture2.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Add_Picture", SystemLanguageManager.Instance.CultureInfo);
            btnDeletePicture2.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Delete_Picture", SystemLanguageManager.Instance.CultureInfo);

            btnPostMoment.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Post_Moments", SystemLanguageManager.Instance.CultureInfo);
        }

        private void btnClearMessage2_Click(object sender, RoutedEventArgs e)
        {
            rtbMessage2.Document.Blocks.Clear();
        }

        private void btnPostMoment_Click(object sender, RoutedEventArgs e)
        {
            btnPostMoment.IsEnabled = false;

            try
            {
                var targets = from item in wpfTreeview3.ItemsSourceData.FirstOrDefault().Children
                              where item.IsChecked
                              select (int)item.Id - 1;


                var pictures = from pic in panelPics2.Controls.Cast<Panel>()
                               select ((System.Windows.Forms.PictureBox)pic.Controls[1]).ImageLocation;

                string[] paths = pictures.ToArray();
                TextRange textRange = new TextRange(rtbMessage2.Document.ContentStart, rtbMessage2.Document.ContentEnd);
                string message = textRange.Text;

                TaskType taskType = TaskType.PostMessage;
                string folderName = "image";

                if (paths != null && paths.Length > 0 && !string.IsNullOrEmpty(message))
                {
                    taskType = TaskType.PostMessageAndPicture;
                }

                if (paths != null && paths.Length > 0 && string.IsNullOrEmpty(message))
                {
                    taskType = TaskType.PostPicture;
                }

                if ((paths == null || paths.Length == 0) && !string.IsNullOrEmpty(message))
                {
                    taskType = TaskType.PostMessage;
                }

                if (!targets.Any())
                {
                    btnPostMoment.IsEnabled = true;
                    MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
                    return;
                }

                TaskManager taskManager = new TaskManager(taskType, message, paths, targets.ToArray());


                string dir = DateTime.Now.ToString("yyyyMMddHHmmssffff");

                foreach (int mobileIndex in taskManager.MobileIndexs)
                {
                    var lists = new JArray
                    {

                    };


                    for (int i = 0; i < taskManager.Paths?.Length; i++)
                    {
                        string target = $"/sdcard/qunkong/{folderName}/{dir}/{i + 1}{System.IO.Path.GetExtension(taskManager.Paths[i])}";
                        string mobileId = DeviceConnectionManager.Instance.GetDeviceNameByMobileIndex(mobileIndex);
                        ProcessUtils.PushFileToVm(mobileId, taskManager.Paths[i], target);

                        lists.Add(target);
                    }

                    var obj = new JObject() { { "tasktype", (int)taskManager.TaskType }, { "txtmsg", taskManager.Message } };

                    obj.Add("list", lists);


                    TaskSch taskSch = new TaskSch()
                    {
                        Bodys = obj.ToString(Formatting.None),
                        MobileIndex = mobileIndex,
                        TypeId = (int)taskManager.TaskType,
                        Status = "waiting",
                        RepeatNums = 1,
                        RandomMins = 1,
                        RandomMaxs = 2,
                        StartTime = TimedTaskManager.Instance.StartTime,
                    };

                    TasksBLL.CreateTask(taskSch);
                }

                //if (ConfigVals.IsRunning != 1)
                //{
                Task.Run(async () =>
                {
                    await ProessTask();
                });
                //}

                btnPostMoment.IsEnabled = true;

                MessageDialogManager.ShowDialogAsync(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Submitted_Task", SystemLanguageManager.Instance.CultureInfo), taskManager.MobileIndexs.Length));
            }
            catch (Exception ex)
            {
                btnPostMoment.IsEnabled = true;
                LogUtils.Error($"{ex}");
                MessageDialogManager.ShowDialogAsync(ex.Message);
            }
        }

        private void SetSendMessagesPageStatus2()
        {
            bool enabled = VmManager.Instance.RunningGroupIndex != -1;

            gridPostMoment.IsEnabled = enabled;
        }

        #endregion


        #region 创建群组

        private void tabCreateGroup_Loaded(object sender, RoutedEventArgs e)
        {
            lblCreateGroup_MouseLeftButtonDown(null, null);
        }

        private void lblCreateGroup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is null))
            {

                lblImportContact.FontWeight = FontWeights.Regular;
                lblSendMsg.FontWeight = FontWeights.Regular;
                lblPostMoment.FontWeight = FontWeights.Regular;
                lblProfile.FontWeight = FontWeights.Regular;
                lblTaskManagement.FontWeight = FontWeights.Regular;
                lblScanning.FontWeight = FontWeights.Regular;
                lblLoginInfo.FontWeight = FontWeights.Regular;
                lblCreateGroup.FontWeight = FontWeights.Bold;
                lblSendGroupMessage.FontWeight = FontWeights.Regular;
            }

            InitRunningVmsTreeView(wpfTreeview5);

            SetSendMessagesPageStatus_CreateGroup();

            Localize_CreateGroup();
        }


        private void SetSendMessagesPageStatus_CreateGroup()
        {
            bool enabled = VmManager.Instance.RunningGroupIndex != -1;

            gridCreateGroup.IsEnabled = enabled;
        }

        private void Localize_CreateGroup()
        {
            lblCreateGroup.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Create_Group", SystemLanguageManager.Instance.CultureInfo);

            lblGroupName.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Group_Name", SystemLanguageManager.Instance.CultureInfo);
            lblGroupMemberRange.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Group_Member_Range", SystemLanguageManager.Instance.CultureInfo);


            lblGroupMemberExplanation.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Group_Member_Explanation", SystemLanguageManager.Instance.CultureInfo);

            btnSumitCreateGroup.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Create_Group", SystemLanguageManager.Instance.CultureInfo);
        }


        private void btnSumitCreateGroup_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion


        #region 个人信息
        private void tabProfile_Loaded(object sender, RoutedEventArgs e)
        {
            lblProfile_MouseLeftButtonDown(null, null);
        }

        private void lblProfile_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is null))
            {

                lblSendMsg.FontWeight = FontWeights.Regular;
                lblPostMoment.FontWeight = FontWeights.Regular;
                lblProfile.FontWeight = FontWeights.Bold;
                lblTaskManagement.FontWeight = FontWeights.Regular;
                lblImportContact.FontWeight = FontWeights.Regular;
                lblScanning.FontWeight = FontWeights.Regular;
                lblLoginInfo.FontWeight = FontWeights.Regular;
                lblCreateGroup.FontWeight = FontWeights.Regular;
                lblSendGroupMessage.FontWeight = FontWeights.Regular;
            }

            btnAddPicture3.Tag = panelPics3;
            btnDeletePicture3.Tag = panelPics3;

            InitRunningVmsTreeView(wpfTreeview4);

            SetSendMessagesPageStatus3();

            Localize3();
        }

        private void Localize3()
        {
            lblProfile.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Profile", SystemLanguageManager.Instance.CultureInfo);

            lblNickName.Text = SystemLanguageManager.Instance.ResourceManager.GetString("NickName", SystemLanguageManager.Instance.CultureInfo);
            lblDescription.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Description", SystemLanguageManager.Instance.CultureInfo);

            gpText3.Header = SystemLanguageManager.Instance.ResourceManager.GetString("Text", SystemLanguageManager.Instance.CultureInfo);
            gpPicture3.Header = SystemLanguageManager.Instance.ResourceManager.GetString("Picture", SystemLanguageManager.Instance.CultureInfo);

            btnClearMessage3.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Clear_Text", SystemLanguageManager.Instance.CultureInfo);
            btnAddPicture3.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Add_Picture", SystemLanguageManager.Instance.CultureInfo);
            btnDeletePicture3.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Delete_Picture", SystemLanguageManager.Instance.CultureInfo);

            btnSaveProfile.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Save_Profile", SystemLanguageManager.Instance.CultureInfo);
        }

        private void SetSendMessagesPageStatus3()
        {
            bool enabled = VmManager.Instance.RunningGroupIndex != -1;

            gridProfile.IsEnabled = enabled;
        }

        private void btnClearMessage3_Click(object sender, RoutedEventArgs e)
        {
            tbNickName.Text = string.Empty;
            rtbDescription.Document.Blocks.Clear();
        }

        private void btnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            string nickname = tbNickName.Text;
            TextRange textRange = new TextRange(rtbDescription.Document.ContentStart, rtbDescription.Document.ContentEnd);
            string intro = textRange.Text;

            var lists = new JArray
            {
            };
            var obj = new JObject() { { "tasktype", 11 }, { "txtmsg", string.Format("{0}|{1}", nickname, intro) } };
            obj.Add("list", lists);

            var targets = from item in wpfTreeview4.ItemsSourceData.FirstOrDefault().Children
                          where item.IsChecked
                          select (int)item.Id - 1;

            var pictures = from pic in panelPics3.Controls.Cast<Panel>()
                           select ((System.Windows.Forms.PictureBox)pic.Controls[1]).ImageLocation;

            string[] paths = pictures.ToArray();
            if (nickname == "" && intro == "" && paths.Count() == 0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("WhatsApp_Op_UpdateNickName", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            string dir = DateTime.Now.ToString("yyyyMMddHHmmssffff");

            var MobileIndexArr = targets.ToArray();
            int successnums = 0;
            if (MobileIndexArr.Count() > 0)
            {
                foreach (int mobile in MobileIndexArr)
                {
                    lists.Clear();
                    if (paths != null && paths.Count() > 0)
                    {
                        string target = $"/sdcard/qunkong/image/{dir}/{System.IO.Path.GetFileName(paths[0])}";
                        string mobileId = DeviceConnectionManager.Instance.GetDeviceNameByMobileIndex(mobile);

                        ProcessUtils.PushFileToVm(mobileId, paths[0], target);

                        lists.Add(target);
                    }

                    //插入任务
                    TaskSch tasks = new TaskSch();
                    tasks.TypeId = 11;
                    tasks.Remotes = mobile.ToString();
                    tasks.MobileIndex = mobile;
                    tasks.Bodys = obj.ToString(Formatting.None);
                    //tasks.Bodys = JsonConvert.SerializeObject(new string[] { "tasktype:1", "txtmsg:", "filepath:"+ filepath, "nums:"+res}); 
                    tasks.Status = "waiting";
                    tasks.ResultVal = "";
                    tasks.RepeatNums = 1;
                    tasks.RandomMins = 5;
                    tasks.RandomMaxs = 10;

                    tasks.StartTime = TimedTaskManager.Instance.StartTime;

                    TasksBLL.CreateTask(tasks);

                    successnums++;
                }

                if (ConfigVals.IsRunning != 1)
                {
                    Task.Run(async () =>
                    {
                        await ProessTask();
                    });
                }

                MessageDialogManager.ShowDialogAsync(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Submitted_Task", SystemLanguageManager.Instance.CultureInfo), successnums));

            }
            else
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
            }

        }
        #endregion

        private void tabTaskManagement_Loaded(object sender, RoutedEventArgs e)
        {
            lblTaskManagement_MouseLeftButtonDown(null, null);
        }

        private void lblTaskManagement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is null))
            {

                lblImportContact.FontWeight = FontWeights.Regular;
                lblSendMsg.FontWeight = FontWeights.Regular;
                lblPostMoment.FontWeight = FontWeights.Regular;
                lblProfile.FontWeight = FontWeights.Regular;
                lblTaskManagement.FontWeight = FontWeights.Bold;
                lblScanning.FontWeight = FontWeights.Regular;
                lblLoginInfo.FontWeight = FontWeights.Regular;
                lblCreateGroup.FontWeight = FontWeights.Regular;
                lblSendGroupMessage.FontWeight = FontWeights.Regular;
            }

            SetSendMessagesPageStatus4();

            Localize4();

        }

        private void Localize4()
        {
            lblTaskManagement.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Task_Management", SystemLanguageManager.Instance.CultureInfo);
            btnSearchTask.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Search_Task", SystemLanguageManager.Instance.CultureInfo);
            btnRefreshTask.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Refresh_Task", SystemLanguageManager.Instance.CultureInfo);
            btnDeleteTask.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Delete_Task", SystemLanguageManager.Instance.CultureInfo);
            btnClearTask.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Clear_Task", SystemLanguageManager.Instance.CultureInfo);
        }

        private void SetSendMessagesPageStatus4()
        {
            bool enabled = VmManager.Instance.RunningGroupIndex != -1;

            gridTaskManagement.IsEnabled = enabled;

        }

        /// <summary>
        /// 查询任务
        /// </summary>
        public void getTasksList()
        {
            colNo.Header = SystemLanguageManager.Instance.ResourceManager.GetString("Col_Id", SystemLanguageManager.Instance.CultureInfo);
            colTaskType.Header = SystemLanguageManager.Instance.ResourceManager.GetString("Col_TaskType", SystemLanguageManager.Instance.CultureInfo);
            colPhone.Header = SystemLanguageManager.Instance.ResourceManager.GetString("Col_Phone", SystemLanguageManager.Instance.CultureInfo);
            colParameter.Header = SystemLanguageManager.Instance.ResourceManager.GetString("Col_Parameter", SystemLanguageManager.Instance.CultureInfo);
            colStatus.Header = SystemLanguageManager.Instance.ResourceManager.GetString("Col_Status", SystemLanguageManager.Instance.CultureInfo);
            colResult.Header = SystemLanguageManager.Instance.ResourceManager.GetString("Col_Result", SystemLanguageManager.Instance.CultureInfo);
            colTime.Header = SystemLanguageManager.Instance.ResourceManager.GetString("Col_Time", SystemLanguageManager.Instance.CultureInfo);

            List<TaskSch> list = TasksBLL.GetTasksList("-1", 2, 1000);

            if (list != null && list.Count() > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].TypeId == 1)
                    {
                        list[i].TypeDescripton = SystemLanguageManager.Instance.ResourceManager.GetString("Import_Address_List", SystemLanguageManager.Instance.CultureInfo);
                    }
                    else if (list[i].TypeId == 2)
                    {
                        list[i].TypeDescripton = SystemLanguageManager.Instance.ResourceManager.GetString("MomentType_Text", SystemLanguageManager.Instance.CultureInfo);
                    }
                    else if (list[i].TypeId == 3)
                    {
                        list[i].TypeDescripton = SystemLanguageManager.Instance.ResourceManager.GetString("MomentType_Picture", SystemLanguageManager.Instance.CultureInfo);
                    }
                    else if (list[i].TypeId == 4)
                    {
                        list[i].TypeDescripton = SystemLanguageManager.Instance.ResourceManager.GetString("MomentType_Picture_Text", SystemLanguageManager.Instance.CultureInfo);
                    }
                    else if (list[i].TypeId == 5)
                    {
                        list[i].TypeDescripton = SystemLanguageManager.Instance.ResourceManager.GetString("MsgType_Text", SystemLanguageManager.Instance.CultureInfo);
                    }
                    else if (list[i].TypeId == 6)
                    {
                        list[i].TypeDescripton = SystemLanguageManager.Instance.ResourceManager.GetString("MsgType_Picture", SystemLanguageManager.Instance.CultureInfo);
                    }
                    else if (list[i].TypeId == 7)
                    {
                        list[i].TypeDescripton = SystemLanguageManager.Instance.ResourceManager.GetString("MsgType_Picture_Text", SystemLanguageManager.Instance.CultureInfo);
                    }
                    else if (list[i].TypeId == 8)
                    {
                        list[i].TypeDescripton = SystemLanguageManager.Instance.ResourceManager.GetString("MsgType_Video", SystemLanguageManager.Instance.CultureInfo);
                    }
                    else if (list[i].TypeId == 9)
                    {
                        list[i].TypeDescripton = SystemLanguageManager.Instance.ResourceManager.GetString("MsgType_Video_Text", SystemLanguageManager.Instance.CultureInfo);
                    }
                    else if (list[i].TypeId == 11)
                    {
                        list[i].TypeDescripton = SystemLanguageManager.Instance.ResourceManager.GetString("WhatsApp_Op_TaskType_UpNickName", SystemLanguageManager.Instance.CultureInfo);
                    }
                }
            }
            datagrid.ItemsSource = list;
        }


        private void btnSearchTask_Click(object sender, RoutedEventArgs e)
        {
            getTasksList();
        }

        private void btnRefreshTask_Click(object sender, RoutedEventArgs e)
        {
            getTasksList();
            if (ConfigVals.IsRunning != 1)
            {
                Task.Run(async () =>
                {
                    await ProessTask();
                });
            }
        }

        private void btnDeleteTask_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnClearTask_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show(SystemLanguageManager.Instance.ResourceManager.GetString("Delete_Task_Tips", SystemLanguageManager.Instance.CultureInfo), SystemLanguageManager.Instance.ResourceManager.GetString("Delete_Task_Confirmation", SystemLanguageManager.Instance.CultureInfo), System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                TasksBLL.DeleteTasks(-1);
                getTasksList();
            }
        }


        #region 登录信息
        private void tabLoginInfo_Loaded(object sender, RoutedEventArgs e)
        {
            lblScanning.FontWeight = FontWeights.Bold;
            lblImportContact.FontWeight = FontWeights.Regular;
            SetSendMessagesPageStatus5();
            Localize5();
        }
        private void lblLoginInfo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is null))
            {

                lblImportContact.FontWeight = FontWeights.Regular;
                lblSendMsg.FontWeight = FontWeights.Regular;
                lblPostMoment.FontWeight = FontWeights.Regular;
                lblProfile.FontWeight = FontWeights.Regular;
                lblTaskManagement.FontWeight = FontWeights.Regular;
                lblScanning.FontWeight = FontWeights.Regular;
                lblLoginInfo.FontWeight = FontWeights.Bold;
                lblCreateGroup.FontWeight = FontWeights.Regular;
                lblSendGroupMessage.FontWeight = FontWeights.Regular;
            }

            SetSendMessagesPageStatus5();
            Localize5();
        }

        private void Localize5()
        {
            lblLoginInfo.Content = SystemLanguageManager.Instance.ResourceManager.GetString("login_Info", SystemLanguageManager.Instance.CultureInfo);
            phoneId.Header = SystemLanguageManager.Instance.ResourceManager.GetString("ID", SystemLanguageManager.Instance.CultureInfo);
            phonenum.Header = SystemLanguageManager.Instance.ResourceManager.GetString("Phone_Number", SystemLanguageManager.Instance.CultureInfo);
            Explain.Header = SystemLanguageManager.Instance.ResourceManager.GetString("Explain", SystemLanguageManager.Instance.CultureInfo);
            ProxyIP.Header = SystemLanguageManager.Instance.ResourceManager.GetString("Proxy_IP", SystemLanguageManager.Instance.CultureInfo);
            scanport.Header = SystemLanguageManager.Instance.ResourceManager.GetString("Proxy_Port", SystemLanguageManager.Instance.CultureInfo);

            this.DataContext = _appOptViewModel;
            List<Simulators> list = SimulatorsBLL.GetSimulatorsList();
            int listcount = list.Count;
            int idCount = _appOptViewModel.simulators.Count + listcount;
            for (int i = idCount + 1; i < ConfigVals.MaxNums; i++)
            {
                _appOptViewModel.simulators.Add(new Simulators()
                {
                    id = i
                });
            }
            dgPhone.ItemsSource = _appOptViewModel.simulators;
            //List<Simulators> list = SimulatorsBLL.GetSimulatorsList();
            //int listcount = list.Count;
            //for (int i = listcount+1; i < ConfigVals.MaxNums + listcount; i++)
            //{
            //    list.Add(new Simulators()
            //    {
            //        id = i  
            //    });
            //}
            //dgPhone.ItemsSource = list;
        }

        private void SetSendMessagesPageStatus5()
        {
            bool enabled = VmManager.Instance.RunningGroupIndex != -1;
            dgPhone.IsEnabled = enabled;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var a = this.dgPhone.SelectedItem as Simulators;
            Simulators objSimulators = new Simulators()
            {
                id = a.id,
                phonenum = a.phonenum,
                androidid = a.androidid,
                created = a.created,
                imei = a.imei
            };
            SimulatorsBLL objSimulatorsBLL = new SimulatorsBLL();
            objSimulatorsBLL.UpdateSimulators(objSimulators);
            MessageDialogManager.ShowDialogAsync(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Save_Success", SystemLanguageManager.Instance.CultureInfo)));
        }

        #endregion

        #region 扫号
        private void tabScanning_Loaded(object sender, RoutedEventArgs e)
        {

            lblScanning_MouseLeftButtonDown(null, null);

        }
        private void lblScanning_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (!(sender is null))
            {

                lblScanning.FontWeight = FontWeights.Bold;
                lblImportContact.FontWeight = FontWeights.Regular;
                lblSendMsg.FontWeight = FontWeights.Regular;
                lblPostMoment.FontWeight = FontWeights.Regular;
                lblProfile.FontWeight = FontWeights.Regular;
                lblTaskManagement.FontWeight = FontWeights.Regular;
                lblLoginInfo.FontWeight = FontWeights.Regular;
                lblCreateGroup.FontWeight = FontWeights.Regular;
                lblSendGroupMessage.FontWeight = FontWeights.Regular;
            }

            SetSendMessagesPageStatus1();

            Localize1();
        }
        private void SetSendMessagesPageStatus1()
        {
            bool enabled = VmManager.Instance.RunningGroupIndex != -1;
            dgPhone.IsEnabled = enabled;
        }
        private void Localize1()
        {
            lblScanning.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Scanning_Number", SystemLanguageManager.Instance.CultureInfo);

            lblTableBegin.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Table_Beginning", SystemLanguageManager.Instance.CultureInfo);
            lblInternationalNum.Content = SystemLanguageManager.Instance.ResourceManager.GetString("International_Number", SystemLanguageManager.Instance.CultureInfo);
            btnGenerate.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Generate", SystemLanguageManager.Instance.CultureInfo);
        }
        /// <summary>
        /// 验证正整数
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsInteger(string txt)
        {
            Regex objReg = new Regex(@"^[1-9]\d*$");
            return objReg.IsMatch(txt);
        }
        /// <summary>
        /// 验证电话号码
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsPhoneNumber(string txt)
        {
            Regex objReg = new Regex(@"^(13[0-9]|14[579]|15[0-3,5-9]|16[6]|17[0135678]|18[0-9]|19[89])\\d{8}$");
            return objReg.IsMatch(txt);
        }
        /// <summary>
        /// 生成国际号码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            uint num; string[] tmp;
            tbNumber.Text = "";
            this.tbNumber.Visibility = Visibility.Visible;

            if (!IsPhoneNumber(txtTableBegin.Text.Trim()) && !IsInteger(this.txtTableBegin.Text.Trim()))
            {
                if (txtTableBegin.Text.Trim() != "")
                {

                    MessageDialogManager.ShowDialogAsync("输入的表始号码不符合要求！");
                    return;
                }
                this.txtTableBegin.Focus();
                this.txtTableBegin.SelectAll();
                //this.popupl.IsOpen = true;

                return;
            }
            else
            {
                try
                {
                    num = uint.Parse(txtGenerate.Text);
                }
                catch (Exception)
                {
                    MessageDialogManager.ShowDialogAsync("输入的表始不符号要求！\n Error:Generate");
                    return;
                }

                if (txtTableBegin.Text.Trim().Length != 11)
                {
                    MessageDialogManager.ShowDialogAsync("输入的表始号码不正确");
                    return;
                }
                tmp = GetNumber(txtTableBegin.Text.Trim(), num);
                foreach (string item in tmp)
                {
                    tbNumber.Text += item + "\n";
                }

            }


            if (!IsPhoneNumber(txtInternationalNum.Text.Trim()) && !IsInteger(this.txtInternationalNum.Text.Trim()))
            {
                if (txtInternationalNum.Text.Trim() != "")
                {
                    //MessageDialogManager.ShowDialogAsync("输入的国际号码不符合要求！", MessageBoxImage.Warning.ConvertToString());
                    MessageBox.Show("输入的国际号码不符合要求！", MessageBoxImage.Warning.ConvertToString());

                }
                this.txtInternationalNum.Focus();
                this.txtInternationalNum.SelectAll();
                return;
            }
            else
            {
                long tmpInum = long.Parse(txtInternationalNum.Text.Trim());
                if (tmpInum < 20000)
                {

                    MessageDialogManager.ShowDialogAsync("输入的国际号码长度不符合要求！");
                    return;
                }
                tmp = GetNumber(txtInternationalNum.Text.Trim(), num);

                foreach (string item in tmp)
                {
                    tbNumber.Text += item + "\n";
                }
            }
        }

        public string[] GetNumber(string InputNumber, uint OutputNumber = 100, int minRand = -9999, int maxRand = 9999)
        {
            if (OutputNumber == 0) throw new Exception("OutputNumber must large than 0");
            if (maxRand - minRand < OutputNumber)
            {
                throw new Exception("Rand set Error");
            }

            string[] ret = new string[OutputNumber];

            Random rnd = new Random();

            long num = long.Parse(InputNumber);

            long tmpNumber;

            for (int i = 0; i < OutputNumber; i++)
            {
                tmpNumber = num + (long)rnd.Next(minRand, maxRand);

                if (ret.Contains(tmpNumber.ToString()))
                {
                    tmpNumber = num + (long)rnd.Next(minRand, maxRand);
                }
                ret[i] = tmpNumber.ToString();
            }
            return ret;
        }

        #endregion


        private void tabSendGroupMessage_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void lblSendGroupMessage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }

    #region 保存中英文转换
    public static class Res
    {
        public static ResCulture Culture = new ResCulture();
    }

    public class ResCulture : INotifyPropertyChanged
    {
        private IRes_Button _Button;
        public IRes_Button Button
        {
            get
            {
                return _Button;
            }
            set
            {
                _Button = value;
                OnPropertyChanged(nameof(Button));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    public interface IRes_Button
    {
        string btnSave { get; }
    }

    public class Res_En_Button : IRes_Button
    {
        public string btnSave
        {
            get
            {
                return "Save";
            }
        }
    }

    public class Res_Zh_Button : IRes_Button
    {
        public string btnSave
        {
            get
            {
                return "保存";
            }
        }
    }
    #endregion

    #region 登录中英文转换
    public static class ResD
    {
        public static ResCultureD CultureD = new ResCultureD();
    }

    public class ResCultureD : INotifyPropertyChanged
    {
        private IResD_Button _Button;
        public IResD_Button Button
        {
            get
            {
                return _Button;
            }
            set
            {
                _Button = value;
                OnPropertyChanged(nameof(Button));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    public interface IResD_Button
    {
        string btnLogin { get; }
    }

    public class ResD_En_Button : IResD_Button
    {
        public string btnLogin
        {
            get
            {
                return "Login";
            }
        }
    }

    public class ResD_Zh_Button : IResD_Button
    {
        public string btnLogin
        {
            get
            {
                return "登录";
            }
        }
    }
    #endregion
}
