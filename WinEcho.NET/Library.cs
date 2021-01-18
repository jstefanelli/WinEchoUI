using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace WinEcho.NET
{
	internal static class Library
	{
		internal const string LibName = "WinEcho.dll";
		internal const string LibName_x86 = "nativeì\\x86\\WinEcho.dll";
		internal const string LibName_x64 = "native\\x64\\WinEcho.dll";

		static Library() {
			NativeLibrary.SetDllImportResolver(typeof(Library).Assembly, (name, assembly, path) =>
			{
				Debug.WriteLine($"[DLL] Looking for {name}");
				if(name != LibName) {
					return IntPtr.Zero;
				}

				string target = Environment.Is64BitProcess ? LibName_x64 : LibName_x86;
				Debug.WriteLine($"[DLL] Target library: {target}");
				if(NativeLibrary.TryLoad(target, assembly, DllImportSearchPath.AssemblyDirectory, out IntPtr handle)) {
					return handle;
				}

				return IntPtr.Zero;
			});
		}

		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr genDeviceEnumerator(int dataFlow);

		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void releaseDeviceEnumerator(IntPtr deviceEnumerator);

		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr deviceEnumeratorGetDeviceCount(IntPtr deviceEnumerator);

		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr deviceEnumeratorGetDeviceByIndex(IntPtr deviceEnumerator, IntPtr index);

		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr deviceEnumeratorGetDeviceByName(IntPtr deviceEnumerator, [MarshalAs(UnmanagedType.LPWStr)] string name);


		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void releaseDeviceInstance(IntPtr device);

		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void deviceId(IntPtr device, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder idStr, ref uint length);

		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void deviceName(IntPtr device, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder nameStr, ref uint length);

		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void deviceDescription(IntPtr device, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder descStr, ref uint length);


		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr genStreamInstance(IntPtr sourceDevice, IntPtr targetDevice, IntPtr bufferSize, uint desiredLatencyMs);

		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void releaseStreamInstance(IntPtr streamInstance);

		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool streamInstanceIsRunning(IntPtr streamInstance);

		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool streamInstanceStart(IntPtr streamInstance);

		[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void streamInstanceStop(IntPtr streamInstance, bool wait);
	}
}
