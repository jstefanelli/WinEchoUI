using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinEcho.NET;

namespace WinEchoUI
{
	/// <summary>
	/// Interaction logic for StreamControl.xaml
	/// </summary>
	public partial class StreamControl : UserControl
	{
		public static DependencyProperty DevicesProperty = DependencyProperty.Register("Devices", typeof(IList<Device>), typeof(StreamControl));
		public static RoutedEvent DeletedEvent = EventManager.RegisterRoutedEvent("Deleted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(StreamControl));
		public StreamInstance RunningStream { get; protected set; } = null;

		public event RoutedEventHandler Deleted {
			add { AddHandler(DeletedEvent, value); }
			remove { RemoveHandler(DeletedEvent, value); }
		}

		public IList<Device> Devices {
			get => (IList<Device>)GetValue(DevicesProperty);
			set => SetValue(DevicesProperty, value);
		}

		public Device CaptureDevice { 
			get => Devices[cbxSource.SelectedIndex];
			set
			{
				if (!Devices.Contains(value))
					return;

				cbxSource.SelectedIndex = Devices.IndexOf(value);
			}
		}
		public Device RenderDevice { 
			get => Devices[cbxTarget.SelectedIndex];
			set
			{
				if (!Devices.Contains(value))
					return;

				cbxTarget.SelectedIndex = Devices.IndexOf(value);
			}
		}

		public StreamControl()
		{
			InitializeComponent();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			cbxSource.ItemsSource = Devices;
			cbxTarget.ItemsSource = Devices;

			if(cbxSource.SelectedIndex < 0 || cbxSource.SelectedIndex >= Devices.Count)
			{
				cbxSource.SelectedIndex = 0;
			}

			if (cbxTarget.SelectedIndex < 0 || cbxTarget.SelectedIndex >= Devices.Count)
			{
				cbxTarget.SelectedIndex = 0;
			}

			btnPlay.IsEnabled = true;
		}


		private void btnPlay_Click(object sender, RoutedEventArgs e)
		{
			Device source = Devices[cbxSource.SelectedIndex];
			Device target = Devices[cbxTarget.SelectedIndex];

			if (source == target)
			{
				return;
			}

			Resources["playColor"] = new SolidColorBrush(Colors.DarkGreen);
			btnPlay.IsEnabled = false;
			cbxSource.IsEnabled = false;
			cbxTarget.IsEnabled = false;

			RunningStream = new StreamInstance(source, target, 1024 * 512, 5);
			RunningStream.Start();

			btnStop.IsEnabled = true;
			Resources["stopColor"] = new SolidColorBrush(Colors.Red);
		}

		private void btnStop_Click(object sender, RoutedEventArgs e)
		{
			if (RunningStream == null || !RunningStream.IsRunning)
				return;

			Resources["stopColor"] = new SolidColorBrush(Colors.DarkRed);
			btnStop.IsEnabled = false;

			RunningStream.Stop();
			RunningStream = null;

			cbxSource.IsEnabled = true;
			cbxTarget.IsEnabled = true;
			btnPlay.IsEnabled = true;
			Resources["playColor"] = new SolidColorBrush(Colors.Green);
		}

		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			DisposeStream();
			RaiseEvent(new RoutedEventArgs(DeletedEvent, this));
		}

		public void DisposeStream(){

			if (RunningStream != null)
			{
				if (RunningStream.IsRunning)
					RunningStream.Stop(true);
				RunningStream.Dispose();
				RunningStream = null;
			}
		}
	}
}
