using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABC
{
    public static class GlobalConstants
    {
        public struct OpenGLColor
        {
            double red;
            double green;
            double blue;

            public OpenGLColor(double red, double green, double blue)
            {
                this.red = red;
                this.green = green;
                this.blue = blue;
            }
        }
    }
}
