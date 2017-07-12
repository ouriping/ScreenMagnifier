using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;

namespace ScreenMagnifier
{
    class SKHook
    {
        #region 私有常量

        /// <summary>
        /// 按键状态数组
        /// </summary>
        private readonly byte[] m_KeyState = new byte[256];

        #endregion 私有常量

        #region 私有变量

        /// <summary>
        /// 鼠标钩子句柄
        /// </summary>
        private IntPtr m_pMouseHook = IntPtr.Zero;

        /// <summary>
        /// 键盘钩子句柄
        /// </summary>
        private IntPtr m_pKeyboardHook = IntPtr.Zero;

        /// <summary>
        /// 鼠标钩子委托实例
        /// </summary>
        /// <remarks>
        /// 不要试图省略此变量,否则将会导致
        /// 激活 CallbackOnCollectedDelegate 托管调试助手 (MDA)。 
        /// 详细请参见MSDN中关于 CallbackOnCollectedDelegate 的描述
        /// </remarks>
        private HookProc m_MouseHookProcedure;

        /// <summary>
        /// 键盘钩子委托实例
        /// </summary>
        /// <remarks>
        /// 不要试图省略此变量,否则将会导致
        /// 激活 CallbackOnCollectedDelegate 托管调试助手 (MDA)。 
        /// 详细请参见MSDN中关于 CallbackOnCollectedDelegate 的描述
        /// </remarks>
        private HookProc m_KeyboardHookProcedure;


        #endregion 私有变量

        #region 事件定义



        /// <summary>
        /// 鼠标点击事件
        /// </summary>
        public event MouseEventHandler OnMouseActivity;

        /// <summary>
        /// 鼠标更新事件
        /// </summary>
        /// <remarks>当鼠标移动或者滚轮滚动时触发</remarks>
        public event MouseUpdateEventHandler OnMouseUpdate;

        /// <summary>
        /// 按键按下事件
        /// </summary>
        public event KeyEventHandler OnKeyDown;

        /// <summary>
        /// 按键按下并释放事件
        /// </summary>
        public event KeyPressEventHandler OnKeyPress;

        /// <summary>
        /// 按键释放事件
        /// </summary>
        public event KeyEventHandler OnKeyUp;

        #endregion 事件定义

        #region 私有方法

        /// <summary>
        /// 鼠标钩子处理函数
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private int MouseHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            //鼠标移动事件
            if ((nCode >= 0) && (this.OnMouseUpdate != null) && (wParam == (int)WM_MOUSE.WM_MOUSEMOVE || wParam == (int)WM_MOUSE.WM_MOUSEWHEEL))
            {
                MouseHookStruct MouseInfo = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
                this.OnMouseUpdate(MouseInfo.Point.X, MouseInfo.Point.Y);
            }

            //鼠标点击事件
            if ((nCode >= 0) && (this.OnMouseActivity != null) && (wParam == (int)WM_MOUSE.WM_RBUTTONDOWN || wParam == (int)WM_MOUSE.WM_MBUTTONDOWN || wParam == (int)WM_MOUSE.WM_LBUTTONDOWN))
            {
                MouseButtons button = MouseButtons.None;
                int clickCount = 0;

                switch (wParam)
                {
                    case (int)WM_MOUSE.WM_MBUTTONDOWN:
                        button = MouseButtons.Middle;
                        clickCount = 1;
                        break;
                    case (int)WM_MOUSE.WM_MBUTTONUP:
                        button = MouseButtons.Middle;
                        clickCount = 0;
                        break;
                }

                MouseHookStruct MouseInfo = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
                MouseEventArgs mouseEvent = new MouseEventArgs(button, clickCount, MouseInfo.Point.X, MouseInfo.Point.Y, 0);
                this.OnMouseActivity(this, mouseEvent);
            }

            ////如果正常运行并且用户要监听鼠标的消息  
            //if ((nCode >= 0) && (OnMouseActivity != null))
            //{
            //    MouseButtons button = MouseButtons.None;
            //    int clickCount = 0;
            //
            //    switch (wParam)
            //    {
            //        case WM_MOUSE.WM_MBUTTONDOWN:
            //            button = MouseButtons.Middle;
            //            clickCount = 1;
            //            break;
            //        case WM_MOUSE.WM_MBUTTONUP:
            //            button = MouseButtons.Middle;
            //            clickCount = 0;
            //            break;
            //    }
            //
            //    //从回调函数中得到鼠标的信息  
            //    MouseHookStruct MyMouseHookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
            //    MouseEventArgs e = new MouseEventArgs(button, clickCount, MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y, 0);
            //    //if(e.X>700)return 1;//如果想要限制鼠标在屏幕中的移动区域可以在此处设置  
            //    OnMouseActivity(this, e);
            //}



            return Win32API.CallNextHookEx(this.m_pMouseHook, nCode, wParam, lParam);
        }

        /// <summary>
        /// 键盘钩子处理函数
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        /// <remarks>此版本的键盘事件处理不是很好,还有待修正.</remarks>
        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            if ((nCode >= 0) && (this.OnKeyDown != null || this.OnKeyPress != null || this.OnKeyUp != null))
            {
                KeyboardHookStruct KeyboardInfo = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));

                if (this.OnKeyDown != null && (wParam == (Int32)WM_KEYBOARD.WM_KEYDOWN || wParam == (Int32)WM_KEYBOARD.WM_SYSKEYDOWN))
                {
                    Keys keyData = (Keys)KeyboardInfo.VKCode;
                    KeyEventArgs keyEvent = new KeyEventArgs(keyData);
                    this.OnKeyDown(this, keyEvent);
                }

                if (this.OnKeyPress != null && wParam == (Int32)WM_KEYBOARD.WM_KEYUP)
                {
                    byte[] inBuffer = new byte[2];

                    /*
                     * 当ToAscii返回1个字符表示为按键，
                     * 为0表示转换失败
                     * 为2表示转换了2个字符，在KeyPressEventArgs中只有一个Char信息，所以此中情况将忽略。
                     * 一般在特殊键盘输入（如德语、法语等的注音）时发生。
                     */
                    if (Win32API.ToAscii(KeyboardInfo.VKCode,
                            KeyboardInfo.ScanCode,
                            this.m_KeyState,
                            inBuffer,
                            KeyboardInfo.Flags) == 1)
                    {
                        KeyPressEventArgs keyPressEvent = new KeyPressEventArgs((char)inBuffer[0]);
                        this.OnKeyPress(this, keyPressEvent);
                    }
                }

                if (this.OnKeyUp != null && (wParam == (Int32)WM_KEYBOARD.WM_KEYUP || wParam == (Int32)WM_KEYBOARD.WM_SYSKEYUP))
                {
                    Keys keyData = (Keys)KeyboardInfo.VKCode;
                    KeyEventArgs keyEvent = new KeyEventArgs(keyData);
                    this.OnKeyUp(this, keyEvent);
                }

            }

            return Win32API.CallNextHookEx(this.m_pKeyboardHook, nCode, wParam, lParam);
        }

        #endregion 私有方法

        #region 公共方法

        /// <summary>
        /// 安装钩子
        /// </summary>
        /// <returns></returns>
        public bool InstallHook()
        {
            IntPtr pInstance = Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().ManifestModule);

            // 假如没有安装鼠标钩子
            if (this.m_pMouseHook == IntPtr.Zero)
            {
                this.m_MouseHookProcedure = new HookProc(this.MouseHookProc);
                this.m_pMouseHook = Win32API.SetWindowsHookEx(WH_Codes.WH_MOUSE_LL,
                    this.m_MouseHookProcedure, pInstance, 0);
                if (this.m_pMouseHook == IntPtr.Zero)
                {
                    this.UnInstallHook();
                    return false;
                }
            }
            if (this.m_pKeyboardHook == IntPtr.Zero)
            {
                this.m_KeyboardHookProcedure = new HookProc(this.KeyboardHookProc);
                this.m_pKeyboardHook = Win32API.SetWindowsHookEx(WH_Codes.WH_KEYBOARD_LL,
                    this.m_KeyboardHookProcedure, pInstance, 0);
                if (this.m_pKeyboardHook == IntPtr.Zero)
                {
                    this.UnInstallHook();
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 卸载钩子
        /// </summary>
        /// <returns></returns>
        public bool UnInstallHook()
        {
            bool result = true;
            if (this.m_pMouseHook != IntPtr.Zero)
            {
                result = (Win32API.UnhookWindowsHookEx(this.m_pMouseHook) && result);
                this.m_pMouseHook = IntPtr.Zero;
            }
            if (this.m_pKeyboardHook != IntPtr.Zero)
            {
                result = (Win32API.UnhookWindowsHookEx(this.m_pKeyboardHook) && result);
                this.m_pKeyboardHook = IntPtr.Zero;
            }

            return result;
        }

        #endregion 公共方法

        #region 构造函数

        /// <summary>
        /// 钩子类
        /// </summary>
        /// <remarks>本类仅仅简单实现了 WH_KEYBOARD_LL 以及 WH_MOUSE_LL </remarks>
        public SKHook()
        {
            Win32API.GetKeyboardState(this.m_KeyState);
        }

        #endregion 构造函数
    }
}
