using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinEcho.NET
{
	public class StreamInstance : IDisposable
	{
		public enum Error : int
		{
			Ok = 0,
			ComInitFailure = 1,
			NoCaptureDevice = 2,
			NoRenderDevice = 3,
			CaptureActivationFailed = 4,
			RenderActivationFailed = 5,
			CaptureFormatMatchFailed = 6,
			RenderFormatMatchFailed = 7,
			CaptureInitFailed = 8,
			RenderInitFailed = 9,
			CaptureEventFailed = 10,
			RenderEventFailed = 11,
			CaptureSetEventFailed = 12,
			RenderSetEventFailed = 13,
			CaptureGetClientFailed = 14,
			RenderGetClientFailed = 15,
			CaptureClientStartFailed = 16,
			RenderClientStartFailed = 17,
			CaptureGetPacketSizeFailed = 18,
			CaptureGetBufferFailed = 19,
			CaptureReleaseBufferFailed = 20,
			RenderGetBufferFailed = 21,
			RenderReleaseBufferFailed = 22,
		}

		private bool disposedValue;

		internal IntPtr Handle { get; set; }
		public StreamInstance(Device captureDevice, Device targetDevice, int bufferSize, uint delayMs)
		{
			Handle = Library.genStreamInstance(captureDevice.Handle, targetDevice.Handle, new IntPtr(bufferSize), delayMs);
			if (Handle == IntPtr.Zero)
			{
				throw new ApplicationException("Failed to generate StreamInstance");
			}
		}

		public bool IsRunning => Library.streamInstanceIsRunning(Handle);

		public bool Start()
		{
			return Library.streamInstanceStart(Handle);
		}

		public void Stop(bool wait = true)
		{
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

		public Error GetError()
		{
			int err = Library.streamInstanceGetError(Handle);
			return (Error)err;
		}
	}
}
