using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CareBeer.Tests.ReactionTime
{
	class ReactionTimeTest : DrunkTest
	{
		private readonly Type TEST_PAGE_SINGLE = typeof(ReactionPageSingle);
		private readonly Type TEST_PAGE_MULTIPLE = typeof(ReactionPage);

		private bool single;

		private double threshold;

		public double ReactionTimeMean { get; private set; }
		public double ReactionTimeVar { get; private set; }
		public int Mistakes { get; private set; }

		private ResultValue _result;
		public override ResultValue Result => _result;
		public override event EventHandler TestFinishedEvent;

		public List<ReactionData> Data;


		public ReactionTimeTest(bool s) { single = s; }

		public override void RunTest()
		{
			Data = new List<ReactionData>();

			if (single)
				((Frame)Window.Current.Content).Navigate(TEST_PAGE_SINGLE, this);

			else ((Frame)Window.Current.Content).Navigate(TEST_PAGE_MULTIPLE, this);
		}

		public void Finished()
		{
			if (single)
				updateUserSingle();
			else updateUserMultiple();

			CloudServices.replaceIneEntity(EntryPage.user); // should await?
            

            _result = ResultValue.PASS; // DUMMY
            if (single)
            {
                TestManager.Instance.Results.SingleReactionResult = _result;
            }
            else
            {
                TestManager.Instance.Results.ReactionResult = _result;
            }

            TestFinishedEvent(this, new EventArgs());
			
		}


		public void CalculateResult()
		{
			ReactionTimeMean = ReactionData.averageReactionTime(Data);
			ReactionTimeVar = ReactionData.varianceReactionTime(Data);
			if (!single)
			{
				Mistakes = ReactionData.mistakes(Data);
			}

			//_result = ResultValue.PASS; // DUMMY
		}


		private void updateUserSingle()
		{
			User u = EntryPage.user;
			if (u.reactionSingle_baslineExists)
			{
				u.reactionSingle_mean = ReactionTimeMean;
				u.reactionSingle_variance = ReactionTimeVar;
			}
			else
			{
				u.B_reactionSingle_mean = ReactionTimeMean;
				u.B_reactionSingle_variance = ReactionTimeVar;
				u.reactionSingle_baslineExists = true;
			}
		}

		private void updateUserMultiple()
		{
			User u = EntryPage.user;
			if (u.reaction_baslineExists)
			{
				u.reaction_mean = ReactionTimeMean;
				u.reaction_mistakes = Mistakes;
				u.reaction_variance = ReactionTimeVar;
			}
			else
			{
				u.B_reaction_mean = ReactionTimeMean;
				u.B_reaction_mistakes = Mistakes;
				u.B_reaction_variance = ReactionTimeVar;
				u.reaction_baslineExists = true;
			}
		}


	}


	public class ReactionData
	{
		public bool isRight { get; set; }
		public long rTime { get; set; }


		public static double averageReactionTime(List<ReactionData> d)
		{
			double temp = 0.0;
			foreach (var t in d)
			{
				temp += t.rTime;
			}
			temp /= d.Count();

			return temp;
		}

		public static double varianceReactionTime(List<ReactionData> d)
		{
			double temp = 0;
			double mean = averageReactionTime(d);

			foreach (var t in d)
			{
				temp += Math.Pow(t.rTime - mean, 2);
			}
			temp /= d.Count() - 1;

			return temp;
		}

		public static int mistakes(List<ReactionData> d)
		{
			int temp = 0;
			foreach (var t in d)
			{
				if (!t.isRight)
				{
					temp++;
				}
			}
			return temp;
		}

	}
}
