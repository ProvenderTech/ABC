using OpenTK.Graphics.OpenGL;
using System;

namespace ABC
{
    public class CCamera
    {
        private double camPosX;
        private double camPosY;
        private readonly double camPosZ;

        private double fixHeading;
        private double camYaw;

        public double camPitch;
        public double offset;
        public double camSetDistance = -75;

        public double gridZoom;

        public double zoomValue = 15;
        public double previousZoom = 25;

        public bool camFollowing;

        //private double camDelta = 0;

        public CCamera()
        {
            //get the pitch of camera from settings
            camPitch = Properties.Settings.Default.setCam_pitch;
            camPosZ = 0.0;
            camFollowing = true;
        }

        /// <summary>
        /// This will set the position for the world camera
        /// </summary>
        /// <param name="fixPosX">The X position for the camera</param>
        /// <param name="fixPosY">The Y position for the camera</param>
        /// <param name="fixedHeading">The heading position for the camera</param>
        public void SetWorldCam(double fixPosX, double fixPosY, double fixedHeading)
        {
            //initializing variables
            const double fixedOffset = 0.02;
            camPosX = fixPosX;
            camPosY = fixPosY;
            fixHeading = fixedHeading;
            camYaw = fixedHeading;
            var sinHeading = Math.Sin(glm.toRadians(fixHeading));
            var cosHeading = Math.Cos(glm.toRadians(fixHeading));

            //back the camera up
            GL.Translate(0.0, 0.0, camSetDistance * 0.5);
            //rotate the camera down to look at fix
            GL.Rotate(camPitch, 1.0, 0.0, 0.0);

            //following game style or N fixed cam
            if (camFollowing)
            {
                GL.Rotate(camYaw, 0.0, 0.0, 1.0);

                if (camPitch > -45)
                {
                    offset = (45.0 + camPitch) / 45.0;

                    offset = (offset * offset * offset * offset * 0.015) + 0.02;

                    GL.Translate(
                        -camPosX + (offset * camSetDistance * sinHeading),
                        -camPosY + (offset * camSetDistance * cosHeading),
                        -camPosZ);
                }
                else
                {
                    //fixed offset of .02
                    GL.Translate(
                        -camPosX + (fixedOffset * camSetDistance * sinHeading),
                        -camPosY + (fixedOffset * camSetDistance * cosHeading),
                        -camPosZ);
                }
            }
            else
            {
                GL.Translate(-camPosX, -camPosY, -camPosZ);
            }
        }
    }
}