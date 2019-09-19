using System;
using System.Collections.Generic;
using System.Text;

namespace FastRacoonFrequencyFunction
{
    public class InputRecord
    {
        public class InputRecordData
        {
            public string text { get; set; }
        }

        public string RecordId { get; set; }
        public InputRecordData Data { get; set; }
    }

}
