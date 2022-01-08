using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WinEcho.NET;

namespace WinEchoUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public static readonly string DefaultConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WinEchoUIConfig.json");

		protected DeviceEnumerator Enumerator { get; set; }

		public static DependencyProperty DevicesProperty = DependencyProperty.Register("Devices", typeof(IList<Device>), typeof(MainWindow));

		protected List<StreamControl> Controls { get; set; } = new List<StreamControl>();

		protected IList<Device> Devices
		{
			get => (IList<Device>)GetValue(DevicesProperty);
			set => SetValue(DevicesProperty, value);
		}
		public MainWindow()
		{
			InitializeComponent();
			Version v = Assembly.GetExecutingAssembly().GetName().Version;
			Title = $"WinEchuUI {v.Major}.{v.Minor}";
			
		}

		private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Enumerator = new DeviceEnumerator(DataFlow.Render);
			Devices = new List<Device>();
			for(int i = 0; i < Enumerator.DeviceCount; i++){
				Devices.Add(Enumerator.GetDeviceByIndex(i));
			}

			if(File.Exists(DefaultConfigPath)){
				Config defaultConfig = await Config.Load(DefaultConfigPath);
				if (defaultConfig != null)
				{
					if(!ApplyConfig(defaultConfig)){
						MessageBox.Show("Failed to apply default config. Some audio devices weere not found", "WinEchoUI", MessageBoxButton.OK, MessageBoxImage.Warning);
					}
				}
			}
		}

		private bool ApplyConfig(Config c){
			if (c == null)
				return false;

			foreach(StreamControl control in Controls){
				control.DisposeStream();
			}

			Controls.Clear();
			controlsPanel.Children.Clear();

			bool allOk = true;
			foreach(var e in c.Entries){
				Device captureDevice = Devices.Where((dev) =>
				{
					return dev.FullName == e.CaptureName;
				}).FirstOrDefault();

				Device renderDevice = Devices.Where((dev) =>
				{
					return dev.FullName == e.RenderName;
				}).FirstOrDefault();

				if (captureDevice == null || renderDevice == null)
				{
					allOk = false;
					continue;
				}

				StreamControl control = AddControl();
				control.CaptureDevice = captureDevice;
				control.RenderDevice = renderDevice;
			}

			return allOk;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			foreach(StreamControl c in Controls){
				c.DisposeStream();
			}	
		}

		private StreamControl AddControl(){
			StreamControl c = new StreamControl();
			Binding b = new Binding();
			b.Mode = BindingMode.OneWay;
			b.Source = Devices;
			c.SetBinding(StreamControl.DevicesProperty, b);
			c.Deleted += StreamControl_Deleted;
            c.Errors += StreamControl_Errors;
			Controls.Add(c);
			controlsPanel.Children.Add(c);

			return c;
		}

        private void StreamControl_Errors(object sender, StreamControl.ErrorsEventArgs e)
        {
			if (sender is not StreamControl ctrl)
            {
				return;
            }

			string noDeviceText = "{No Device}";
			string errorCaption = $"Erros from stream:";
			string errorText = $"Stream:\n{(ctrl.CaptureDevice != null ? ctrl.CaptureDevice.FullName : noDeviceText)}\nto\n{(ctrl.RenderDevice != null ? ctrl.RenderDevice.FullName : noDeviceText)}\n\nErrors:";
			foreach(StreamInstance.Error err in e.Errors)
            {
				errorText += $"\n{err}";
            }
			MessageBox.Show(errorText, errorCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void btnAddStream_Click(object sender, RoutedEventArgs e)
		{
			AddControl();
		}

		private void StreamControl_Deleted(object sender, RoutedEventArgs e)
		{
			if(sender is not StreamControl ctrl){
				return;
			}

			Controls.Remove(ctrl);
			controlsPanel.Children.Remove(ctrl);
		}

		private Config GrabCurrentConfig(){
			if (Controls.Count == 0)
				return null;

			Config config = new Config();
			foreach (StreamControl c in Controls)
			{
				Device capture = c.CaptureDevice;
				Device render = c.RenderDevice;

				if(capture == null || render == null){
					continue;	
				}

				config.Entries.Add(new ConfigEntry()
				{
					CaptureName = capture.FullName,
					RenderName = render.FullName
				});
			}
			return config;
		}

		private async void mniSaveConfigAs_Click(object sender, RoutedEventArgs e)
		{
			Config config = GrabCurrentConfig();

			if(config == null || config.Entries.Count == 0){
				MessageBox.Show("Can't savge configuration. Add more streams first", "WinEchoUI", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}

			SaveFileDialog dialog = new SaveFileDialog();
			dialog.Title = "Save current configuration: ";
			dialog.FileName = "WinEchoConfig.json";
			dialog.DefaultExt = ".json";
			dialog.Filter = "Json Config Files (.json)|*.json";
			
			if(dialog.ShowDialog() == true){
				await config.Save(dialog.FileName);
			}
		}

		private async void mnsiSaveDefaultConfig_Click(object sender, RoutedEventArgs e)
		{
			Config config = GrabCurrentConfig();

			if (config == null || config.Entries.Count == 0)
			{
				MessageBox.Show("Can't savge configuration. Add more streams first", "WinEchoUI", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}

			await config.Save(DefaultConfigPath);
		}

		private async void mniLoadconfig_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Title = "Open WinEcho configuration file: ";
			dialog.Filter = "Json Config File (.json)|*.json";

			if(dialog.ShowDialog() == true){
				Config c = await Config.Load(dialog.FileName);
				if(c == null){
					MessageBox.Show("Failed to load configuration.", "WinEchoUI", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				ApplyConfig(c);
			}
		}
	}
}
