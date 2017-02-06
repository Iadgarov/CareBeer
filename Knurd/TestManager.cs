using CareBeer.Tests.ReactionTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CareBeer.Tests
{
	[Flags]
	public enum TestId
	{
		None = 0,
		Gait = 1,
		Bubble = 2,
		ReactionSingle = 4,
		Reaction = 8,
		//Speech = 16,
		All = 15
	}

	class TestManager
	{
		private static TestManager instance;
		private TestManager() { }
        

		public static TestManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new TestManager();
				}
				return instance;
			}
		}

		//public List<DrunkTest> Tests { get; set; }
		public TestId TestsToRun { get; set; }
		private int curr;
        public Results Results { get; private set; }


        public void Start()
		{
			if (TestsToRun == TestId.None)
			{
				// no tests selected
				return;
			}

            Results = new Results();
            curr = 1;
			Next();
			
		}

		private void Next()
		{
			//currTest++;
			DrunkTest next = getNextTest();
			if (next == null)
			{
				((Frame)Window.Current.Content).Navigate(typeof(ResultsPage), Results);
				return;
			}

			next.TestFinishedEvent += (sender, args) => { Next(); };
			next.RunTest();

		}


		private DrunkTest getNextTest()
		{
			while (!TestsToRun.HasFlag((TestId)curr))
			{
				curr <<= 1;

				if (curr > (int)TestId.All)
				{
					break;
				}
			}

			// turn off flag
			TestsToRun = (TestId)((int)TestsToRun ^ curr);

			switch ((TestId)curr)
			{
				case TestId.Gait:
					return new GaitTest();

				case TestId.Bubble:
					return new BubbleTest();

				case TestId.ReactionSingle:
					return new ReactionTimeTest(true);

				case TestId.Reaction:
					return new ReactionTimeTest(false);

				default:
					return null;
			}
		}
	}
}