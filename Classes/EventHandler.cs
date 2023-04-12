using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if !NET47
namespace Extensions
{
	public delegate void EventHandler<T>(object sender, T e);
}
#endif