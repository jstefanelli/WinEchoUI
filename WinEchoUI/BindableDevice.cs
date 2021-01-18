using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinEcho.NET;

namespace WinEchoUI
{
	class BindableDevice
	{
		public Device Device { get; set; }
		public BindableDevice(Device dev){
			Device = dev;
		}


	}
}
