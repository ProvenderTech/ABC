using OpenTK.Graphics.OpenGL;

namespace ABC
{
    public class CWorldGrid
    {
        private readonly FormGPS mf;

        //Z
        public double northingMax;

        public double northingMin;

        //X
        public double eastingMax;

        public double eastingMin;

        private double texZoomE = 20, texZoomN = 20;

        /// <summary>
        /// Constructor for the grid on the application.
        /// </summary>
        /// <param name="f">FormGPS struct</param>
        public CWorldGrid(FormGPS f)
        {
            mf = f;
        }

        /// <summary>
        /// This function will draw the "floor" of the application.
        /// </summary>
        public void DrawFieldSurface()
        {
            GL.Enable(EnableCap.Texture2D);
            GL.Color3(mf.redField, mf.grnField, mf.bluField);
            //texture[1] is the Floor.png in the dependencies folder
            GL.BindTexture(TextureTarget.Texture2D, mf.texture[1]);
            GL.Begin(PrimitiveType.TriangleStrip);
            GL.TexCoord2(0, 0);
            GL.Vertex3(eastingMin, northingMax, 0.0);
            GL.TexCoord2(texZoomE, 0.0);
            GL.Vertex3(eastingMax, northingMax, 0.0);
            GL.TexCoord2(0.0, texZoomN);
            GL.Vertex3(eastingMin, northingMin, 0.0);
            GL.TexCoord2(texZoomE, texZoomN);
            GL.Vertex3(eastingMax, northingMin, 0.0);
            GL.End();
            GL.Disable(EnableCap.Texture2D);
        }                                                                                          
        /// <summary>
        /// This function draws the vertical and horizontal lines for the grid.
        /// It takes into account the zoom level on the application.
        /// </summary>
        /// <param name="gridZoom">used to determine how zoomed in the application is</param>
        public void DrawWorldGrid(double gridZoom)
        {
            GL.Color3(.9, .5, .2);
            //GL.LineWidth(1);
            GL.Begin(PrimitiveType.Lines);
            // Vertical lines 
            for (double num = eastingMin; num < eastingMax; num += gridZoom)
            {
                GL.Vertex3(num, northingMax, 0.1);
                GL.Vertex3(num, northingMin, 0.1);
            }
            // Horizontal Lines
            for (double num2 = northingMin; num2 < northingMax; num2 += gridZoom)
            {
                GL.Vertex3(eastingMax, num2, 0.1);
                GL.Vertex3(eastingMin, num2, 0.1);
            }
            GL.End();
        }

        /// <summary>
        /// Defines the max and min variables for the grid based off of the GPS coordinates
        /// </summary>
        /// <param name="northing">The current North Geographic Cartesian Coordinates (GPS)</param>
        /// <param name="easting">The current East Geographic Cartesian Coordinates (GPS)</param>
        public void CreateWorldGrid(double northing, double easting)
        {
            northingMax = northing + 16000.0;
            northingMin = northing - 16000.0;
            eastingMax = easting + 16000.0;
            eastingMin = easting - 16000.0;
        }

        /// <summary>
        /// This function checks the current location and determines the grid mid and max
        /// based off of the northing and easting values.
        /// </summary>
        /// <param name="northing">The current North Geographic Cartesian Coordinates (GPS)</param>
        /// <param name="easting">The current East Geographic Cartesian Coordinates (GPS)</param>
        public void CheckZoomWorldGrid(double northing, double easting)
        {
            if (northingMax - northing < 1000.0)
            {
                northingMax = northing + 2000.0;
                texZoomN = (int)((northingMax - northingMin) / 500.0);
            }
            if (northing - northingMin < 1000.0)
            {
                northingMin = northing - 2000.0;
                texZoomN = (int)((northingMax - northingMin) / 500.0);
            }
            if (eastingMax - easting < 1000.0)
            {
                eastingMax = easting + 2000.0;
                texZoomE = (int)((eastingMax - eastingMin) / 500.0);
            }
            if (easting - eastingMin < 1000.0)
            {
                eastingMin = easting - 2000.0;
                texZoomE = (int)((eastingMax - eastingMin) / 500.0);
            }
        }
    }
}