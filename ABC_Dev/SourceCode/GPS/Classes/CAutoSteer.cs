namespace ABC
{
    public class CAutoSteer
    {
        private readonly FormGPS mf;

        //flag for free drive window to control autosteer
        public bool isInFreeDriveMode;

        /// <summary>
        /// When called will default the drive mode to AutoSteer
        /// </summary>
        /// <param name="f">FormGPS Class</param>
        public CAutoSteer(FormGPS f)
        {
            mf = f;
            isInFreeDriveMode = false;
        }
    }
}