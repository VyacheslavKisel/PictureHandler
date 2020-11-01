using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureHandler
{
    public class ServiceInfoMedianFiltering
    {
        public int InitialHeight { get; }
        public int FiniteHeight { get; }

        public ServiceInfoMedianFiltering(int initialHeight, int finiteHeight)
        {
            InitialHeight = initialHeight;
            FiniteHeight = finiteHeight;
        }
    }
}
