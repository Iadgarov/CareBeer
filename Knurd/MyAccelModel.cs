namespace Knurd
{
    public class MyAccelModel
    {
        private System.DateTime timeStamp;
        private double accel;

        public System.DateTime TimeStamp
        {
            get
            {
                return timeStamp;
            }
            set
            {
                timeStamp = value;
            }
        }

        public double Accel
        {
            get
            {
                return accel;
            }
            set
            {
                accel = value;
            }
        }


        public MyAccelModel(System.DateTime? TimeStamp = null, double Accel = 0)
        {
            if (TimeStamp == null)
                this.TimeStamp = System.DateTime.Now;
            else
            {
                this.TimeStamp = (System.DateTime)TimeStamp;
            }
            this.Accel = Accel;
        }
    }
}