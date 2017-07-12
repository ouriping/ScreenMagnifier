using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;

namespace ScreenMagnifier
{
    class SKHook
    {
        #region ˽�г���

        /// <summary>
        /// ����״̬����
        /// </summary>
        private readonly byte[] m_KeyState = new byte[256];

        #endregion ˽�г���

        #region ˽�б���

        /// <summary>
        /// ��깳�Ӿ��
        /// </summary>
        private IntPtr m_pMouseHook = IntPtr.Zero;

        /// <summary>
        /// ���̹��Ӿ��
        /// </summary>
        private IntPtr m_pKeyboardHook = IntPtr.Zero;

        /// <summary>
        /// ��깳��ί��ʵ��
        /// </summary>
        /// <remarks>
        /// ��Ҫ��ͼʡ�Դ˱���,���򽫻ᵼ��
        /// ���� CallbackOnCollectedDelegate �йܵ������� (MDA)�� 
        /// ��ϸ��μ�MSDN�й��� CallbackOnCollectedDelegate ������
        /// </remarks>
        private HookProc m_MouseHookProcedure;

        /// <summary>
        /// ���̹���ί��ʵ��
        /// </summary>
        /// <remarks>
        /// ��Ҫ��ͼʡ�Դ˱���,���򽫻ᵼ��
        /// ���� CallbackOnCollectedDelegate �йܵ������� (MDA)�� 
        /// ��ϸ��μ�MSDN�й��� CallbackOnCollectedDelegate ������
        /// </remarks>
        private HookProc m_KeyboardHookProcedure;


        #endregion ˽�б���

        #region �¼�����



        /// <summary>
        /// ������¼�
        /// </summary>
        public event MouseEventHandler OnMouseActivity;

        /// <summary>
        /// �������¼�
        /// </summary>
        /// <remarks>������ƶ����߹��ֹ���ʱ����</remarks>
        public event MouseUpdateEventHandler OnMouseUpdate;

        /// <summary>
        /// ���������¼�
        /// </summary>
        public event KeyEventHandler OnKeyDown;

        /// <summary>
        /// �������²��ͷ��¼�
        /// </summary>
        public event KeyPressEventHandler OnKeyPress;

        /// <summary>
        /// �����ͷ��¼�
        /// </summary>
        public event KeyEventHandler OnKeyUp;

        #endregion �¼�����

        #region ˽�з���

        /// <summary>
        /// ��깳�Ӵ�����
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private int MouseHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            //����ƶ��¼�
            if ((nCode >= 0) && (this.OnMouseUpdate != null) && (wParam == (int)WM_MOUSE.WM_MOUSEMOVE || wParam == (int)WM_MOUSE.WM_MOUSEWHEEL))
            {
                MouseHookStruct MouseInfo = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
                this.OnMouseUpdate(MouseInfo.Point.X, MouseInfo.Point.Y);
            }

            //������¼�
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

            ////����������в����û�Ҫ����������Ϣ  
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
            //    //�ӻص������еõ�������Ϣ  
            //    MouseHookStruct MyMouseHookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
            //    MouseEventArgs e = new MouseEventArgs(button, clickCount, MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y, 0);
            //    //if(e.X>700)return 1;//�����Ҫ�����������Ļ�е��ƶ���������ڴ˴�����  
            //    OnMouseActivity(this, e);
            //}



            return Win32API.CallNextHookEx(this.m_pMouseHook, nCode, wParam, lParam);
        }

        /// <summary>
        /// ���̹��Ӵ�����
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        /// <remarks>�˰汾�ļ����¼������Ǻܺ�,���д�����.</remarks>
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
                     * ��ToAscii����1���ַ���ʾΪ������
                     * Ϊ0��ʾת��ʧ��
                     * Ϊ2��ʾת����2���ַ�����KeyPressEventArgs��ֻ��һ��Char��Ϣ�����Դ�����������ԡ�
                     * һ��������������루��������ȵ�ע����ʱ������
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

        #endregion ˽�з���

        #region ��������

        /// <summary>
        /// ��װ����
        /// </summary>
        /// <returns></returns>
        public bool InstallHook()
        {
            IntPtr pInstance = Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().ManifestModule);

            // ����û�а�װ��깳��
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
        /// ж�ع���
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

        #endregion ��������

        #region ���캯��

        /// <summary>
        /// ������
        /// </summary>
        /// <remarks>���������ʵ���� WH_KEYBOARD_LL �Լ� WH_MOUSE_LL </remarks>
        public SKHook()
        {
            Win32API.GetKeyboardState(this.m_KeyState);
        }

        #endregion ���캯��
    }
}
