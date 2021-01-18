using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinEcho.NET
{
	public class StreamInstance : IDisposable
	{
		private bool disposedValue;

		internal IntPtr Handle { get; set; }
		public StreamInstance(Device captureDevice, Device targetDevice, int bufferSize, uint delayMs){
			Handle = Library.genStreamInstance(captureDevice.Handle, targetDevice.Handle, new IntPtr(bufferSize), delayMs);
			if(Handle == IntPtr.Zero){
				throw new ApplicationException("Failed to generate StreamInstance");
			}
		}

		public bool IsRunning => Library.streamInstanceIsRunning(Handle);

		public bool Start(){
			return Library.streamInstanceStart(Handle);
		}

		public void Stop(bool wait = true){
			Library.streamInstanceStop(Handle, wait);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				disposedValue = true;
			}

			Library.releaseStreamInstance(Handle);
			Handle = IntPtr.Zero;
		}

		
		~StreamInstance()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
