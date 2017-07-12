namespace ScreenMagnifier
{
	partial class FrmMain
	{
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		/// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
		protected override void Dispose ( bool disposing )
		{
			if ( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			this.CustomDispose();
			base.Dispose( disposing );
		}

		#region Windows 窗体设计器生成的代码

		/// <summary>
		/// 设计器支持所需的方法 - 不要
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent ()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( FrmMain ) );
			this.BW_Main = new System.ComponentModel.BackgroundWorker();
			this.SuspendLayout();
			// 
			// BW_Main
			// 
			this.BW_Main.WorkerSupportsCancellation = true;
			this.BW_Main.DoWork += new System.ComponentModel.DoWorkEventHandler( this.BW_Main_DoWork );
			// 
			// FrmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 12F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size( 100, 100 );
			this.ControlBox = false;
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ( ( System.Drawing.Icon )( resources.GetObject( "$this.Icon" ) ) );
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size( 100, 100 );
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size( 100, 100 );
			this.Name = "FrmMain";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler( this.FrmMain_FormClosed );
			this.Paint += new System.Windows.Forms.PaintEventHandler( this.FrmMain_Paint );
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler( this.FrmMain_FormClosing );
			this.Load += new System.EventHandler( this.FrmMain_Load );
			this.ResumeLayout( false );

		}

		#endregion

		private System.ComponentModel.BackgroundWorker BW_Main;
	}
}

