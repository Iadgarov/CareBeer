using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CareBeer.Tests
{
	class GaitTest : DrunkTest
	{
		private readonly Type TEST_PAGE = typeof(AccelerometerPage);

		private bool _result;
		public override bool Result => _result;

		public override event EventHandler TestFinishedEvent;

		public AccelerometerViewModel AccVm;

		public override void RunTest()
		{
			((Frame)Window.Current.Content).Navigate(TEST_PAGE, this);
		}


		public void Finished(bool skipped)
		{
            if (!skipped)
            {
                CloudServices.replaceIneEntity(EntryPage.user); // should await?
            }
			
			_result = false; // DUMMY


            TestFinishedEvent(this, new EventArgs());

		}
	}
}
