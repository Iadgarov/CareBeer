using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareBeer.Tests
{
    public enum ResultValue { PASS, FAIL, SKIP }

    class Results
    {
        public ResultValue GaitResult { get; set; }
        public ResultValue BubbleResult { get; set; }
        public ResultValue SingleReactionResult { get; set; }
        public ResultValue ReactionResult { get; set; }
        public ResultValue SpeechResult { get; set; }

        public Results()
        {
            // all default values are "skipped"
            GaitResult = ResultValue.SKIP;
            BubbleResult = ResultValue.SKIP;
            SingleReactionResult = ResultValue.SKIP;
            ReactionResult = ResultValue.SKIP;
            SpeechResult = ResultValue.SKIP;
        }
    }
}
