using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mp3AlbumCoverUpdater
{
    public partial class frmWaitingBox : Form
    {
        #region Properties
        private int _MaxWaitTime;
        private int _WaitTime;
        private bool _CancelEnable;
        private IAsyncResult _AsyncResult;
        private EventHandler<EventArgs> _Method;
        private bool _IsShown = true;
        private readonly int _EffectCount = 10;
        private readonly int _EffectTime = 500;
        /// <summary>
        /// 控制界面显示的特性
        /// </summary>
        private Timer _Timer;
        public string Message = "";
        public int TimeSpan = 0;
        public bool FormEffectEnable = false;
        #endregion

        #region frmWaitingBox
        public frmWaitingBox(EventHandler<EventArgs> method,int maxWaitTime,string waitMessage,bool cancelEnable,bool timerVisable)
        {
            maxWaitTime *= 1000;
            Initialize(method, maxWaitTime,waitMessage, cancelEnable, timerVisable);
        }
        #endregion

        #region Initialize
        private void Initialize(EventHandler<EventArgs> method, int maxWaitTime,string waitMessage,bool cancelEnable, bool timerVisable)
        {
            InitializeComponent();            
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;
            this.labMessage.Text = waitMessage;
            _Timer = new Timer();
            _Timer.Interval = _EffectTime/_EffectCount;
            _Timer.Tick += _Timer_Tick;
            this.Opacity = 0;
            FormEffectEnable = true;
            //para
            TimeSpan = 500;
            Message = string.Empty;
            _CancelEnable = cancelEnable;
            _MaxWaitTime = maxWaitTime;
            _WaitTime = 0;
            _Method = method;
            this.btnCancel.Visible = _CancelEnable;
            this.labTimer.Visible = timerVisable;
            this.timer1.Interval = TimeSpan;
            this.timer1.Start();
        }
        #endregion    

        #region Events
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Message = "您结束了当前操作！";
            this.DialogResult = DialogResult.OK;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            _WaitTime += TimeSpan;
            this.labTimer.Text = string.Format("{0}ms", _WaitTime);
            if (!this._AsyncResult.IsCompleted)
            {
                if (_WaitTime > _MaxWaitTime)
                {
                    Message = string.Format("处理数据超时{0}ms，结束当前操作！", _MaxWaitTime);
                    this.DialogResult = DialogResult.OK;
                }
            }
            else
            {
                this.Message = string.Empty;
                this.DialogResult = DialogResult.OK;
            }
            
        }

        private void frmWaitingBox_Shown(object sender, EventArgs e)
        {
            _AsyncResult = _Method.BeginInvoke(null, null, null, null);
            //Effect
            if (FormEffectEnable)
            {
                _Timer.Start();
            }
            else
                this.Opacity = 1;
        }
        private void frmWaitingBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (FormEffectEnable)
            {
                if(this.Opacity>=1)
                    e.Cancel = true;
                _Timer.Start();
            }
        }
        private void _Timer_Tick(object sender, EventArgs e)
        {
            if (_IsShown)
            {
                if (this.Opacity >= 1)
                {
                    _Timer.Stop();
                    _IsShown = false;
                }
                this.Opacity += 1.00 / _EffectCount;
            }
            else
            {
                if (this.Opacity <= 0)
                {
                    _Timer.Stop();
                    _IsShown = true;
                    this.DialogResult = DialogResult.OK;
                }
                this.Opacity -= 1.00 / _EffectCount;
            }
        }
        #endregion

        
    }
}
