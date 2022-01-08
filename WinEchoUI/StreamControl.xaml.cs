using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		public static RoutedEvent ErrorsEvent = EventManager.RegisterRoutedEvent("Errors", RoutingStrategy.Bubble, typeof(ErrorsEventHandler), typeof(StreamControl));
		public delegate void ErrorsEventHandler(object sender, ErrorsEventArgs e);
		public class ErrorsEventArgs : RoutedEventArgs
        {
			public List<StreamInstance.Error> Errors;

			public ErrorsEventArgs(RoutedEvent ev, object sender, List<StreamInstance.Error> errors) : base(ev, sender)
            {
				Errors = errors;
            }
        }

		public StreamInstance RunningStream { get; protected set; } = null;
		protected bool Playing = false;

		public event RoutedEventHandler Deleted {
			add { AddHandler(DeletedEvent, value); }
			remove { RemoveHandler(DeletedEvent, value); }
		}

		public event ErrorsEventHandler Errors
        {
			add { AddHandler(ErrorsEvent, value); }
			remove { RemoveHandler(ErrorsEvent, value); }
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

			Playing = false;
			Update();
		}


		private void Update(bool checkNative = false)
        {
			if (checkNative)
            {
				Playing = RunningStream == null ? false : RunningStream.IsRunning;
            }

			if (Playing)
            {
				btnPlay.IsEnabled = false;
				cbxSource.IsEnabled = false;
				cbxTarget.IsEnabled = false;
				Resources["playColor"] = new SolidColorBrush(Colors.DarkGreen);

				btnStop.IsEnabled = true;
				Resources["stopColor"] = new SolidColorBrush(Colors.Red);
            } else
            {
				Resources["stopColor"] = new SolidColorBrush(Colors.DarkRed);
				btnStop.IsEnabled = false;

				cbxSource.IsEnabled = true;
				cbxTarget.IsEnabled = true;
				btnPlay.IsEnabled = true;
				Resources["playColor"] = new SolidColorBrush(Colors.Green);
			}
        }

		private void btnPlay_Click(object sender, RoutedEventArgs e)
		{
			Device source = Devices[cbxSource.SelectedIndex];
			Device target = Devices[cbxTarget.SelectedIndex];

			if (source == target)
			{
				return;
			}

			if (RunningStream != null)
            {
				DisposeStream();
            }

			RunningStream = new StreamInstance(source, target, 1024 * 512, 5);
			RunningStream.Start();
			Playing = true;

			Update();

			Dispatcher.InvokeAsync(async () =>
			{
				await Task.Delay(1500);
				if (Playing)
					Update(true);

				if (RunningStream != null)
				{
					List<StreamInstance.Error> errors = new List<StreamInstance.Error>();
					StreamInstance.Error error;
					do
					{
						error = RunningStream.GetError();
						if (error != StreamInstance.Error.Ok)
                        {
							errors.Add(error);
							Debug.WriteLine("[StreamInstance] Error: " + error);
                        }
					} while (error != StreamInstance.Error.Ok);

					if (errors.Count > 0)
                    {
						RaiseEvent(new ErrorsEventArgs(ErrorsEvent, this, errors));
                    }
				}
			});
		}

		private void btnStop_Click(object sender, RoutedEventArgs e)
		{
			if (RunningStream == null || !RunningStream.IsRunning)
				return;

			RunningStream.Stop();
			RunningStream = null;
			Playing = false;

			Update();
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
