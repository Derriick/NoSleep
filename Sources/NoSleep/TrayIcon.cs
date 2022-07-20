using System.Reflection;
using Timer = System.Windows.Forms.Timer;

namespace NoSleep
{
	internal class TrayIcon : ApplicationContext
	{
		/// <summary>
		/// Interval between timer ticks (in ms) to refresh Windows idle timers. Shouldn't be too small to avoid resources consumption. Must be less then Windows screensaver/sleep timer.
		/// Default = 10 000 ms (10 seconds).
		/// </summary>
		const int RefreshInterval = 10000;
		/// <summary>
		/// ExecutionMode defines how blocking is made. See details at https://msdn.microsoft.com/en-us/library/aa373208.aspx?f=255&MSPPError=-2147217396
		/// </summary>
		const EXECUTION_STATE ExecutionMode = EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_AWAYMODE_REQUIRED;

		// PRIVATE VARIABLES
		private NotifyIcon _TrayIcon;
		private readonly Timer _RefreshTimer;

		// CONSTRUCTOR
		public TrayIcon() {
			// Initialize application
			Application.ApplicationExit += OnApplicationExit;
			InitializeComponent();
			if (_TrayIcon == null) {
				throw new Exception("_TrayIcon is null after initialization");
			}
			_TrayIcon.Visible = true;

			// Set timer to tick to refresh idle timers
			_RefreshTimer = new Timer() { Interval = RefreshInterval, Enabled = true };
			_RefreshTimer.Tick += RefreshTimer_Tick;
		}

		private void InitializeComponent() {
			// Initialize Tray icon
			_TrayIcon = new NotifyIcon {
				BalloonTipIcon = ToolTipIcon.Info,
				BalloonTipText = "A tiny C# application to prevent windows screensaver/sleep.",
				BalloonTipTitle = Assembly.GetExecutingAssembly().GetName().Name,
				Text = Assembly.GetExecutingAssembly().GetName().Name,
				Icon = Properties.Resources.TrayIcon
			};
			_TrayIcon.DoubleClick += TrayIcon_DoubleClick;

			// Initialize Close menu item for context menu
			ToolStripMenuItem _CloseMenuItem = new ToolStripMenuItem() { Text = "Close" };
			_CloseMenuItem.Click += CloseMenuItem_Click;

			// Initialize context menu
			_TrayIcon.ContextMenuStrip = new ContextMenuStrip();
			_TrayIcon.ContextMenuStrip.Items.Add(_CloseMenuItem);
		}

		private void OnApplicationExit(object? sender, EventArgs e) {
			// Clean up things on exit
			_TrayIcon.Visible = false;
			_RefreshTimer.Enabled = false;
			// Clean up continuous state, if required
			if (ExecutionMode.HasFlag(EXECUTION_STATE.ES_CONTINUOUS)) WinU.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
		}

		private void TrayIcon_DoubleClick(object? sender, EventArgs e) { _TrayIcon.ShowBalloonTip(10000); }
		private void CloseMenuItem_Click(object? sender, EventArgs e) { Application.Exit(); }
		private void RefreshTimer_Tick(object? sender, EventArgs e) { WinU.SetThreadExecutionState(ExecutionMode); }
	}
}