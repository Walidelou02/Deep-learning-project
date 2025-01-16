using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deep_Learning.Models
{
    public class PredictionResponse
    {
        public string PredictedClass { get; set; }
        public float[] PredictionProbabilities { get; set; }
    }
}
