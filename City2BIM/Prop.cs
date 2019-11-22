﻿namespace City2BIM
{
    public static class Prop
    {
        //public static readonly double Distolsq = 0.01; //10cm^2!

        //public static readonly double Distolsq = 0.0009; //3cm^2!

        //public static readonly double Distolsq = 0.0001; //1cm^2!

        /// <summary>
        /// Assumption tolerance for same points
        /// </summary>
        public static readonly double Distolsq = 0.000001; //1mm^2!

        /// <summary>
        /// Tolerance for valid determinant
        /// </summary>
        //public static readonly double Determinanttol = 1.0E-4;
        public static readonly double Determinanttol = 1.0E-8;

        public const double radToDeg = 180 / System.Math.PI;
        public const double feetToM = 0.3048;

    }
}
