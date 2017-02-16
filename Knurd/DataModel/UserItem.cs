using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using System.Diagnostics;

namespace CareBeer
{
    public class User : TableEntity
    {

        public User() { }

        public User(string userName, string password)
        {
            this.userName = userName;
            this.password = password;

            baselineExists = false;
            this.reaction_baslineExists = false;
            this.step_baslineExists = false;
            this.speech_baslineExists = false;

            this.PartitionKey = password;
            this.RowKey = userName;

        }

        [JsonProperty(PropertyName = "userName")]
        public string userName { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string password { get; set; }

        [JsonProperty(PropertyName = "baselineExists")]
        public bool baselineExists { get; set; }

        [JsonProperty(PropertyName = "reactionSingle_baslineExists")]
        public bool reactionSingle_baslineExists { get; set; }

        [JsonProperty(PropertyName = "reaction_baslineExists")]
        public bool reaction_baslineExists { get; set; }

        [JsonProperty(PropertyName = "step_baslineExists")]
        public bool step_baslineExists { get; set; }

        [JsonProperty(PropertyName = "speech_baslineExists")]
        public bool speech_baslineExists { get; set; }

        [JsonProperty(PropertyName = "bubble_baslineExists")]
        public bool bubble_baslineExists { get; set; }

        /******************************************************************/

        // reactiont time test results:
        [JsonProperty(PropertyName = "reactionSingle_mean")]
        public double reactionSingle_mean { get; set; }

        [JsonProperty(PropertyName = "reactionSingle_variance")]
        public double reactionSingle_variance { get; set; }

        [JsonProperty(PropertyName = "reaction_mean")]
        public double reaction_mean { get; set; }

        [JsonProperty(PropertyName = "reaction_variance")]
        public double reaction_variance { get; set; }

        [JsonProperty(PropertyName = "reaction_mistakes")]
        public int reaction_mistakes { get; set; }

        // basline data for this test
        [JsonProperty(PropertyName = "B_reactionSingle_mean")]
        public double B_reactionSingle_mean { get; set; }

        [JsonProperty(PropertyName = "B_reactionSingle_variance")]
        public double B_reactionSingle_variance { get; set; }

        [JsonProperty(PropertyName = "B_reaction_mean")]
        public double B_reaction_mean { get; set; }

        [JsonProperty(PropertyName = "B_reaction_variance")]
        public double B_reaction_variance { get; set; }

        [JsonProperty(PropertyName = "B_reaction_mistakes")]
        public int B_reaction_mistakes { get; set; }


        /******************************************************************/

        // gait test results:
        [JsonProperty(PropertyName = "acc_energyList")] 
        public string acc_energyList { get; set; } // the raw data

        [JsonProperty(PropertyName = "gyr_energyList")]
        public string gyr_energyList { get; set; } // the raw data

        [JsonProperty(PropertyName = "maxPoints")]
        public string maxPoints { get; set; }

        [JsonProperty(PropertyName = "minPoints")]
        public string minPoints { get; set; }

        [JsonProperty(PropertyName = "strideLength")]
        public string strideLength { get; set; }

        [JsonProperty(PropertyName = "stepAmplitude")]
        public string stepAmplitude { get; set; }

        [JsonProperty(PropertyName = "strideLengthVariance")]
        public double strideLengthVariance { get; set; }

        //Basline for this test:
        [JsonProperty(PropertyName = "B_acc_energyList")]
        public string B_acc_energyList { get; set; } // the acc raw data

        [JsonProperty(PropertyName = "B_gyr_energyList")]
        public string B_gyr_energyList { get; set; } // the acc raw data

        [JsonProperty(PropertyName = "B_maxPoints")]
        public string B_maxPoints { get; set; }

        [JsonProperty(PropertyName = "B_minPoints")]
        public string B_minPoints { get; set; }

        [JsonProperty(PropertyName = "B_strideLength")]
        public string B_strideLength { get; set; }

        [JsonProperty(PropertyName = "B_stepAmplitude")]
        public string B_stepAmplitude { get; set; }

        [JsonProperty(PropertyName = "B_strideLengthVariance")]
        public double B_strideLengthVariance { get; set; }


        /******************************************************************/

        // bubble test results:
        [JsonProperty(PropertyName = "acc_bubble_energyList")]
        public string acc_bubble_energy { get; set; } // the raw data

        [JsonProperty(PropertyName = "gyr_bubble_energyList")]
        public string gyr_bubble_energy { get; set; } // the raw data

        [JsonProperty(PropertyName = "gyr_bubble_energyVariance")]
        public double gyr_bubble_energyVariance { get; set; } // the raw data

        //Basline for this test:
        [JsonProperty(PropertyName = "B_acc_bubble_energyList")]
        public string B_acc_bubble_energy { get; set; } // the acc raw data

        [JsonProperty(PropertyName = "B_gyr_bubble_energyList")]
        public string B_gyr_bubble_energy { get; set; } // the acc raw data

        [JsonProperty(PropertyName = "B_gyr_bubble_energyVariance")]
        public double B_gyr_bubble_energyVariance { get; set; }

        /******************************************************************/


        // speech test results:
        [JsonProperty(PropertyName = "pause_length_mean")]
        public double pause_length_mean { get; set; }

        [JsonProperty(PropertyName = "pause_length_variance")]
        public double pause_length_variance { get; set; }

        [JsonProperty(PropertyName = "speech_length_mean")]
        public double speech_length_mean { get; set; }

        [JsonProperty(PropertyName = "speech_length_variance")]
        public double speech_length_variance { get; set; }

        // baseline for this test:
        [JsonProperty(PropertyName = "B_pause_length_mean")]
        public double B_pause_length_mean { get; set; }

        [JsonProperty(PropertyName = "B_pause_length_variance")]
        public double B_pause_length_variance { get; set; }

        [JsonProperty(PropertyName = "B_speech_length_mean")]
        public double B_speech_length_mean { get; set; }

        [JsonProperty(PropertyName = "B_speech_length_variance")]
        public double B_speech_length_variance { get; set; }


        /******************************************************************/

        // util:

        // turn list into comma deliminated string
        public static string listToString(List<double> l)
        {
            string s = "";
            bool first = true;
            foreach (double item in l)
            {
                s += first ? item.ToString() : "," + item.ToString("F");
                first = false;
            }
            Debug.WriteLine("Size of string: " + System.Text.ASCIIEncoding.ASCII.GetByteCount(s));
            return s;
        }

        // convert comma deliminated string to double list
        public static List<double> stringToList(string s)
        {
            string[] itemArray = s.Split(',');
            List<double> l = new List<double>();
            foreach (string item in itemArray)
            {
                l.Add(double.Parse(item));
            }
            return l;
        }

        public bool isSetUp()
        {
            //return this.reaction_baslineExists && this.speech_baslineExists && this.step_baslineExists;
            return baselineExists;
        }

    }
}
