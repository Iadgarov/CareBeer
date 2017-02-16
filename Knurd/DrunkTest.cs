using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareBeer.Tests
{
	abstract class DrunkTest
	{
		abstract public ResultValue Result { get; }
		abstract public void RunTest();

		//public delegate void EventHandler(object sender, EventArgs args);
		//public event EventHandler TestFinishedEvent = delegate { };
		abstract public event EventHandler TestFinishedEvent;
	}
}
