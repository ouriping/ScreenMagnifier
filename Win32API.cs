using System;
using System.Runtime.InteropServices;

namespace ScreenMagnifier
{
	class Win32API
	{
		#region DLL����

		/// <summary>
		/// �������ô���
		/// </summary>
		/// <param name="hWnd"></param>
		/// <param name="hWndInsertAfter"></param>
		/// <param name="X"></param>
		/// <param name="Y"></param>
		/// <param name="cx"></param>
		/// <param name="cy"></param>
		/// <param name="uFlags"></param>
		/// <returns></returns>
		[DllImport( "user32.dll", CallingConvention = CallingConvention.StdCall )]
		public static extern bool SetWindowPos ( IntPtr hWnd, int hWndInsertAfter, 
			int X, int Y, int cx, int cy, int uFlags );

		/// <summary>
		/// ��װ����
		/// </summary>
		/// <param name="idHook"></param>
		/// <param name="lpfn"></param>
		/// <param name="hInstance"></param>
		/// <param name="threadId"></param>
		/// <returns></returns>
		[DllImport( "user32.dll", CallingConvention = CallingConvention.StdCall )]
		public static extern IntPtr SetWindowsHookEx ( WH_Codes idHook, HookProc lpfn,
			IntPtr pInstance, int threadId );

		/// <summary>
		/// ж�ع���
		/// </summary>
		/// <param name="idHook"></param>
		/// <returns></returns>
		[DllImport( "user32.dll", CallingConvention = CallingConvention.StdCall )]
		public static extern bool UnhookWindowsHookEx ( IntPtr pHookHandle );

		/// <summary>
		/// ���ݹ���
		/// </summary>
		/// <param name="idHook"></param>
		/// <param name="nCode"></param>
		/// <param name="wParam"></param>
		/// <param name="lParam"></param>
		/// <returns></returns>
		[DllImport( "user32.dll", CallingConvention = CallingConvention.StdCall )]
		public static extern int CallNextHookEx ( IntPtr pHookHandle, int nCode,
			Int32 wParam, IntPtr lParam );

		/// <summary>
		/// ת����ǰ������Ϣ
		/// </summary>
		/// <param name="uVirtKey"></param>
		/// <param name="uScanCode"></param>
		/// <param name="lpbKeyState"></param>
		/// <param name="lpwTransKey"></param>
		/// <param name="fuState"></param>
		/// <returns></returns>
		[DllImport( "user32.dll" )]
		public static extern int ToAscii( UInt32 uVirtKey, UInt32 uScanCode,
			byte[] lpbKeyState, byte[] lpwTransKey, UInt32 fuState );

		/// <summary>
		/// ��ȡ����״̬
		/// </summary>
		/// <param name="pbKeyState"></param>
		/// <returns>��0��ʾ�ɹ�</returns>
		[DllImport( "user32.dll" )]
		public static extern int GetKeyboardState( byte[] pbKeyState );

		/// <summary>
		/// ��ȡ��ǰ���λ��
		/// </summary>
		/// <param name="lpPoint"></param>
		/// <returns></returns>
		[DllImport("user32.dll")]
		public extern static int GetCursorPos( ref POINT lpPoint ); 


		#endregion DLL����
	}
}
