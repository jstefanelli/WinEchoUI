using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinEcho.NET
{
	public class Device : IDisposable
	{
		private bool disposedValue;
		internal IntPtr Handle { get; set; }
		internal Device(IntPtr handle){
			Handle = handle;
		}

		public string Id() {
			uint size = 0;

			Library.deviceId(Handle, null, ref size);

			StringBuilder sb = new StringBuilder((int)size);

			Library.deviceId(Handle, sb, ref size);

			return sb.ToString();
		}

		public string Name()
		{
			uint size = 0;

			Library.deviceName(Handle, null, ref size);

			StringBuilder sb = new StringBuilder((int)size);

			Library.deviceName(Handle, sb, ref size);

			return sb.ToString();
		}

		public string Description()
		{
			uint size = 0;

			Library.deviceDescription(Handle, null, ref size);

			StringBuilder sb = new StringBuilder((int)size);

			Library.deviceDescription(Handle, sb, ref size);

			return sb.ToString();
		}

		public string FullName => $"{Description()} ({Name()})";

		public override bool Equals(object obj)
		{
			if(obj is Device dev){
				return Id() == dev.Id();
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Id().GetHashCode();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				Library.releaseDeviceInstance(Handle);
				disposedValue = true;
			}
		}

		~Device()
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
