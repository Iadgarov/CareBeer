using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CareBeer.Tests
{
    class SpeechTest : DrunkTest
    {
        private readonly Type TEST_PAGE = typeof(SpeechRecordingPage);

        private ResultValue _result;
        public override ResultValue Result => _result;

        public override event EventHandler TestFinishedEvent;

        public SpeechActivityAnalyzer vadAnalyzer;


        public override void RunTest()
        {
            ((Frame)Window.Current.Content).Navigate(TEST_PAGE, this);
        }


        public void Finished()
        {
            updateUser();
            CloudServices.replaceIneEntity(EntryPage.user); // should await?

            _result = (vadAnalyzer.PauseLengthMean + vadAnalyzer.PauseLengthVariance >
                1.5 * (EntryPage.user.B_pause_length_mean + EntryPage.user.B_pause_length_variance) ?
                ResultValue.FAIL : ResultValue.PASS);

            TestManager.Instance.Results.SpeechResult = _result;

            TestFinishedEvent(this, new EventArgs());
        }


        private void updateUser()
        {
            User u = EntryPage.user;
            if (u.speech_baslineExists)
            {
                u.pause_length_mean = vadAnalyzer.PauseLengthMean;
                u.pause_length_variance = vadAnalyzer.PauseLengthVariance;
                u.speech_length_mean = vadAnalyzer.SpeechLengthMean;
                u.speech_length_variance = vadAnalyzer.SpeechLengthVariance;
            }
            else
            {
                u.B_pause_length_mean = vadAnalyzer.PauseLengthMean;
                u.B_pause_length_variance = vadAnalyzer.PauseLengthVariance;
                u.B_speech_length_mean = vadAnalyzer.SpeechLengthMean;
                u.B_speech_length_variance = vadAnalyzer.SpeechLengthVariance;
                u.speech_baslineExists = true;
            }
        }
    }
}
