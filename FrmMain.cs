/*******************************************************************
 * ����
 * 
 * ��ʾ�����������ѧϰ������
 * �ܾ���δ����������ɵ���������κ���ҵ�Գ��������ҵ����վ��ʹ�á�
 * 
 *                                                SHARKOO 2006.3.24
 *
 * Mail�� sharkoo@msn.com
 * Blog�� http://sharkoo.cnblogs.com/
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
    /// ��Ļ�Ŵ�ʾ�� V1.0
    /// 
    /// ����˵����
    /// ��ʵ������Ļͼ��ķŴ���ʾ��������Ϊ���㣨-25��-25������25��25�����Ŵ�2����ʾ��
    /// 
    /// ��֪���⣺
    /// 1����֧����Ƶ��ȡ
    /// 2������ToolTip��ʾ�޷���ʾ
    /// 3�����ܻ���ɱ������ڲ�����ʾʧЧ��
    /// 4��SKHook���У����ڼ����¼��������ڲ��㡣
    /// 
    /// </summary>
    public partial class FrmMain : Form
    {
        #region ˽�г���

        private readonly int m_ScreenWidth = 1024;

        private readonly int m_ScreenHeight = 768;

        #endregion ˽�г���

        #region ˽�б���

        /// <summary>
        /// ���ڴ�����Ļ����λͼ
        /// </summary>
        private Bitmap m_ScreenCapture = new Bitmap(50, 50);

        /// <summary>
        /// ��Ļ�����X����
        /// </summary>
        private int m_CaptureX = 0;

        /// <summary>
        /// ��Ļ�����Y����
        /// </summary>
        private int m_CaptureY = 0;

        /// <summary>
        /// ��������,���ڼ���
        /// </summary>
        private object m_LockObj = new object();

        /// <summary>
        /// ���ӹ���ʵ��
        /// </summary>
        private SKHook m_HookMain = new SKHook();

        #endregion ˽�б���

        #region ˽�з���

        /// <summary>
        /// �ֶ��ͷ���Դ
        /// </summary>
        private void CustomDispose()
        {
            this.m_ScreenCapture.Dispose();
        }

        /// <summary>
        ///	ʹ�����λ�����ò���λ��,ͬʱ����λͼ�������ô�����ʾλ��
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
        /// ������Ļͼ��λͼ
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
        /// ��ȡ����Ĵ�����ʾλ��
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <remarks>����������ʹ�����ʾ�����ص�ʱ,���ֵ�ͼ����ܻ�������</remarks>
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
        /// ����ת���ɲ���λ��
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

        #endregion ˽�з���

        #region ���캯��

        /// <summary>
        /// ������
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



        #endregion ���캯��

        #region �ؼ��¼�

        /// <summary>
        /// �������
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
        /// �������
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
        /// �����Ѿ��ر�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.m_HookMain.UnInstallHook();
        }

        /// <summary>
        /// �������ڹر�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.BW_Main.CancelAsync();
            Thread.Sleep(300);
        }

        /// <summary>
        /// ��깳�Ӹ����¼�
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void HookMain_OnMouseUpdate(int x, int y)
        {
            this.SetCaptureXY(x, y);
        }

        /// <summary>
        /// ���̹���KeyDown�¼�
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
        /// ��̨�������������ˢ�¡�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>������Ҫ��ʾ��������ʱ,���Կ���ͣ�ø����</remarks>
        private void BW_Main_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!this.BW_Main.CancellationPending)
            {
                this.CreateScreenCapture();

                // һ�㵱�ﵽ25fpsʱ,�����Ѿ��о������ӳ�
                Thread.Sleep(40);
            }
        }

        #endregion �ؼ��¼�

    }
}