/*******************************************************************
 * 声明
 * 
 * 本示例代码仅用于学习交流。
 * 拒绝在未经过本人许可的情况下在任何商业性出版物或商业性网站上使用。
 * 
 *                                                SHARKOO 2006.3.24
 *
 * Mail： sharkoo@msn.com
 * Blog： http://sharkoo.cnblogs.com/
 * 
 *******************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace ScreenMagnifier
{
    /// <summary>
    /// 屏幕放大示例 V1.0
    /// 
    /// 功能说明：
    /// 简单实现了屏幕图像的放大显示。采样点为鼠标点（-25，-25）－（25，25），放大2倍显示。
    /// 
    /// 已知问题：
    /// 1。不支持视频截取
    /// 2。部分ToolTip提示无法显示
    /// 3。可能会造成背景窗口部分显示失效。
    /// 4。SKHook类中，对于键盘事件处理，存在不足。
    /// 
    /// </summary>
    public partial class FrmMain : Form
    {
        #region 私有常量

        private readonly int m_ScreenWidth = 1024;

        private readonly int m_ScreenHeight = 768;

        #endregion 私有常量

        #region 私有变量

        /// <summary>
        /// 用于存在屏幕捕获位图
        /// </summary>
        private Bitmap m_ScreenCapture = new Bitmap(50, 50);

        /// <summary>
        /// 屏幕捕获点X坐标
        /// </summary>
        private int m_CaptureX = 0;

        /// <summary>
        /// 屏幕捕获点Y坐标
        /// </summary>
        private int m_CaptureY = 0;

        /// <summary>
        /// 锁定对象,用于加锁
        /// </summary>
        private object m_LockObj = new object();

        /// <summary>
        /// 钩子管理实例
        /// </summary>
        private SKHook m_HookMain = new SKHook();

        #endregion 私有变量

        #region 私有方法

        /// <summary>
        /// 手动释放资源
        /// </summary>
        private void CustomDispose()
        {
            this.m_ScreenCapture.Dispose();
        }

        /// <summary>
        ///	使用鼠标位置设置捕获位置,同时进行位图捕获及设置窗体显示位置
        /// </summary>
        /// <param name="mousex"></param>
        /// <param name="mousey"></param>
        private void SetCaptureXY(int mousex, int mousey)
        {
            this.GetLoactionOfCapture(ref mousex, ref mousey);
            if (this.m_CaptureX == mousex && this.m_CaptureY == mousey)
                return;
            lock (this.m_LockObj)
            {
                this.m_CaptureX = mousex;
                this.m_CaptureY = mousey;
            }
            this.GetLoactionOfForm(ref mousex, ref mousey);
            Win32API.SetWindowPos(this.Handle, -1, mousex, mousey, 0, 0,
                (int)SetWindowPosFlags.SWP_ASYNCWINDOWPOS | (int)SetWindowPosFlags.SWP_NOSIZE);
            this.CreateScreenCapture();
        }

        /// <summary>
        /// 捕获屏幕图像到位图
        /// </summary>
        private void CreateScreenCapture()
        {
            lock (this.m_LockObj)
            {
                using (Graphics g = Graphics.FromImage(this.m_ScreenCapture))
                {
                    //					g.Clear( Color.White );
                    g.CopyFromScreen(this.m_CaptureX, this.m_CaptureY, 0, 0, new Size(50, 50));
                }
            }
            if (this.InvokeRequired)
            {
                VoidCallback InvalidateCallback = new VoidCallback(this.Invalidate);
                this.Invoke(InvalidateCallback, null);
            }
            else
                this.Invalidate();
        }

        /// <summary>
        /// 获取合理的窗体显示位置
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <remarks>当捕获区域和窗体显示区域重叠时,出现的图像可能会有问题</remarks>
        private void GetLoactionOfForm(ref int x, ref int y)
        {
            x += 25;
            y -= 100;

            if (x > this.m_ScreenWidth - 100)
                x = this.m_ScreenWidth - 100;
            if (y < 0)
                y += 150;
        }

        /// <summary>
        /// 鼠标点转换成捕获位置
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void GetLoactionOfCapture(ref int x, ref int y)
        {
            x -= 25;
            y -= 25;
            if (x < 0)
                x = 0;
            else if (x > this.m_ScreenWidth - 50)
                x = this.m_ScreenWidth - 50;
            if (y < 0)
                y = 0;
            else if (y > this.m_ScreenHeight - 50)
                y = this.m_ScreenHeight - 50;
        }

        #endregion 私有方法

        #region 构造函数

        /// <summary>
        /// 主窗口
        /// </summary>
        public FrmMain()
        {
            InitializeComponent();

            this.m_ScreenWidth = Screen.PrimaryScreen.Bounds.Width;
            this.m_ScreenHeight = Screen.PrimaryScreen.Bounds.Height;

            this.m_HookMain.OnKeyDown += new KeyEventHandler(HookMain_OnKeyDown);
            this.m_HookMain.OnMouseUpdate += new MouseUpdateEventHandler(HookMain_OnMouseUpdate);
            this.m_HookMain.OnMouseActivity += new MouseEventHandler(m_HookMain_OnMouseActivity);
        }

        void m_HookMain_OnMouseActivity(object sender, MouseEventArgs e)
        {
            //throw new NotImplementedException();
            //            this.Text = 
            this.Text = e.X.ToString();
        }



        #endregion 构造函数

        #region 控件事件

        /// <summary>
        /// 窗体绘制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMain_Paint(object sender, PaintEventArgs e)
        {
            lock (this.m_LockObj)
            {
                e.Graphics.DrawImage(this.m_ScreenCapture, new Rectangle(0, 0, 100, 100), 0, 0, 50, 50, GraphicsUnit.Pixel);
            }
            ControlPaint.DrawBorder(e.Graphics, new Rectangle(0, 0, this.Width, this.Height), Color.Black, ButtonBorderStyle.Solid);
        }

        /// <summary>
        /// 窗体加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMain_Load(object sender, EventArgs e)
        {
            POINT mousePoint = new POINT();
            Win32API.GetCursorPos(ref mousePoint);

            this.SetCaptureXY(mousePoint.X, mousePoint.Y);

            this.m_HookMain.InstallHook();

            this.BW_Main.RunWorkerAsync();
        }

        /// <summary>
        /// 窗体已经关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.m_HookMain.UnInstallHook();
        }

        /// <summary>
        /// 窗体正在关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.BW_Main.CancelAsync();
            Thread.Sleep(300);
        }

        /// <summary>
        /// 鼠标钩子更新事件
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void HookMain_OnMouseUpdate(int x, int y)
        {
            this.SetCaptureXY(x, y);
        }

        /// <summary>
        /// 键盘钩子KeyDown事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HookMain_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && Control.ModifierKeys == Keys.Shift)
            {
                this.Close();
            }
        }

        /// <summary>
        /// 后台操作组件，用于刷新。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>当不需要显示动画内容时,可以考虑停用该组件</remarks>
        private void BW_Main_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!this.BW_Main.CancellationPending)
            {
                this.CreateScreenCapture();

                // 一般当达到25fps时,基本已经感觉不出延迟
                Thread.Sleep(40);
            }
        }

        #endregion 控件事件

    }
}