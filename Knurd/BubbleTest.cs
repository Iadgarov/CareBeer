using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CareBeer.Tests
{
	class BubbleTest : DrunkTest
	{
		private readonly Type TEST_PAGE = typeof(BubblePage);

		public List<double> accelEnergy;
		public List<double> gyroEnergy;

		private bool _result;
		public override bool Result => _result;

		public override event EventHandler TestFinishedEvent;

		public override void RunTest()
		{
			accelEnergy = new List<double>();
			gyroEnergy = new List<double>();
			((Frame)Window.Current.Content).Navigate(TEST_PAGE, this);
		}


		public void Finished()
		{
			updateUser();
			CloudServices.replaceIneEntity(EntryPage.user); // should await?

			_result = false; // DUMMY

			TestFinishedEvent(this, new EventArgs());

		}


		private void updateUser()
		{
			User u = EntryPage.user;
			if (u.bubble_baslineExists)
			{
				u.acc_bubble_energy = User.listToString(accelEnergy);
				u.gyr_bubble_energy = User.listToString(gyroEnergy);

			}
			else
			{
				u.B_acc_bubble_energy = User.listToString(accelEnergy);
				u.B_gyr_bubble_energy = User.listToString(gyroEnergy);

				u.bubble_baslineExists = true;
			}
		}
	}
}
