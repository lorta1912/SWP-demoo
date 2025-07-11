using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Application.DTOs

{
    public class PredictCycle
    {
        public DateOnly ovulationDay { get; set; }
        public DateOnly fertileStart { get; set; }
        public DateOnly fertileEnd { get; set; }
        public DateOnly nextPeriod { get; set; }

        public DateOnly endNextPeriod { get; set; }
    }
}

