using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinEcho.NET
{
	public class DeviceEnumerator : IDisposable
	{
		private bool disposedValue;

		internal IntPtr Handle { get; set; }
		public DeviceEnumerator(DataFlow flow){
			Handle = Library.genDeviceEnumerator((int)flow);
		}

		public int DeviceCount => (int)Library.deviceEnumeratorGetDeviceCount(Handle).ToInt64();

		public Device GetDeviceByIndex(int index){
			IntPtr ptr = Library.deviceEnumeratorGetDeviceByIndex(Handle, new IntPtr(index));
			if (ptr == IntPtr.Zero)
				return null;
			return new Device(ptr);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				Library.releaseDeviceEnumerator(Handle);
				Handle = IntPtr.Zero;
				disposedValue = true;
			}
		}

		~DeviceEnumerator()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}

	public enum DataFlow : int {
		Render = 0,
		Capture = 1,
		All = 2
	}
}
