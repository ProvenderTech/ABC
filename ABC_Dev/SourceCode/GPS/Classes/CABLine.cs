using OpenTK.Graphics.OpenGL;
using System;

namespace ABC
{
    // appears to be for the camera, but not sure
    public class CQuicks
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double heading { get; set; }
        public string fieldName { get; set; }

        // constructor
        public CQuicks(string varFieldName = "North South", 
                       double varHeading = 0, double varX = 0, double varY = 0)
        {
            fieldName = varFieldName;
            heading = varHeading;
            X = varX;
            Y = varY;
        }
    }

    /// <summary>
    /// Class for making the AB line
    /// </summary>
    public class CABLine
    {
        public double abHeading;
        public double abFixHeadingDelta;

        public bool isLineSameAsVehicleHeading = true;
        public bool isOnRightSideCurrentLine = true;

        public double refLineSide = 1.0;

        public double distanceFromRefLine;
        public double distanceFromCurrentLine;
        public double snapDistance;

        public bool isLineSet;
        public bool isLineBeingSet;
        public bool isBtnLineOn;
        public double passNumber;

        public double howManyPathsAway;

        // tramlines
        // Color tramColor = Color.YellowGreen;
        public int tramPassEvery;
        public bool isOnTramLine;

        public int passBasedOn;

        // pointers to mainform controls
        private readonly FormGPS mainForm;

        // A point
        public vec2 refPoint1 = new vec2(0.2, 0.2);
        // B point
        public vec2 refPoint2 = new vec2(0.3, 0.3);

        // the reference line endpoints
        public vec2 refLineP1 = new vec2(0.0, 0.0);

        public vec2 refLineP2 = new vec2(0.0, 1.0);

        // the current AB guidance line
        public vec2 currentLineP1 = new vec2(0.0, 0.0);

        public vec2 currentLineP2 = new vec2(0.0, 1.0);

        // pure pursuit values
        public vec2 goalPoint = new vec2(0, 0);

        public vec2 radiusPoint = new vec2(0, 0);
        public double steerAngle;
        public double rEast, rNorth;
        public double ppRadius;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="f">main form used</param>
        public CABLine(FormGPS f)
        {
            // constructor
            mainForm = f;
            // sets tram line on
            isOnTramLine = true;
        }

        /// <summary>
        /// Deletes an AB line from the AB list
        /// </summary>
        public void DeleteAB()
        {
            // clears values for the saved AB line
            refPoint1 = new vec2(0.0, 0.0);
            refPoint2 = new vec2(0.0, 1.0);

            refLineP1 = new vec2(0.0, 0.0);
            refLineP2 = new vec2(0.0, 1.0);

            currentLineP1 = new vec2(0.0, 0.0);
            currentLineP2 = new vec2(0.0, 1.0);

            abHeading = 0.0;
            passNumber = 0.0;
            howManyPathsAway = 0.0;
            // declares the line is not set due to the current line being deleted.
            isLineSet = false;
        }

        /// <summary>
        /// Sets the AB line when the B point is declared
        /// </summary>
        public void SetABLineByBPoint()
        {
            // grabs the current east and north coordinates from the NEMA code from "pn" object
            refPoint2.easting = mainForm.pn.fix.easting;
            refPoint2.northing = mainForm.pn.fix.northing;

            // calculate the AB Heading
            abHeading = Math.Atan2(refPoint2.easting - refPoint1.easting, 
                                   refPoint2.northing - refPoint1.northing);

            // sets the heading to a positive radian
            if (abHeading < 0) abHeading += glm.twoPI;

            // sin x cos z for endpoints, opposite for additional lines
            // STILL UNSURE ABOUT THE 4000... I think this just defines the edges, but not sure why 400 - Nick
            refLineP1.easting = refPoint1.easting - (Math.Sin(abHeading) * 4000.0);
            refLineP1.northing = refPoint1.northing - (Math.Cos(abHeading) * 4000.0);

            // STILL UNSURE ABOUT THE 4000... I think this just defines the edges, but not sure why 400 - Nick
            refLineP2.easting = refPoint1.easting + (Math.Sin(abHeading) * 4000.0);
            refLineP2.northing = refPoint1.northing + (Math.Cos(abHeading) * 4000.0);

            isLineSet = true;
        }

        /// <summary>
        /// Set the reference lines if the heading is already found
        /// </summary>
        public void SetABLineByHeading()
        {
            // heading is set in the AB Form
            // STILL UNSURE ABOUT THE 4000... I think this just defines the edges, but not sure why 400 - Nick
            refLineP1.easting = refPoint1.easting - (Math.Sin(abHeading) * 4000.0);
            refLineP1.northing = refPoint1.northing - (Math.Cos(abHeading) * 4000.0);

            // STILL UNSURE ABOUT THE 4000... I think this just defines the edges, but not sure why 400 - Nick
            refLineP2.easting = refPoint1.easting + (Math.Sin(abHeading) * 4000.0);
            refLineP2.northing = refPoint1.northing + (Math.Cos(abHeading) * 4000.0);

            refPoint2.easting = refLineP2.easting;
            refPoint2.northing = refLineP2.northing;

            isLineSet = true;

        }

        /// <summary>
        ///  Most likely choosing which AB line to snap to.
        /// </summary>
        public void SnapABLine()
        {
            double headingCalc;
            //calculate the heading 90 degrees to ref ABLine heading
            if (isOnRightSideCurrentLine) headingCalc = abHeading + glm.PIBy2;
            else headingCalc = abHeading - glm.PIBy2;

            // calculate the new points for the reference line and points
            refPoint1.easting = (Math.Sin(headingCalc) * Math.Abs(distanceFromCurrentLine) * 0.001) + refPoint1.easting;
            refPoint1.northing = (Math.Cos(headingCalc) * Math.Abs(distanceFromCurrentLine) * 0.001) + refPoint1.northing;

            // STILL UNSURE ABOUT THE 4000... I think this just defines the edges, but not sure why 400 - Nick
            refLineP1.easting = refPoint1.easting - (Math.Sin(abHeading) * 4000.0);
            refLineP1.northing = refPoint1.northing - (Math.Cos(abHeading) * 4000.0);

            // STILL UNSURE ABOUT THE 4000... I think this just defines the edges, but not sure why 400 - Nick
            refLineP2.easting = refPoint1.easting + (Math.Sin(abHeading) * 4000.0);
            refLineP2.northing = refPoint1.northing + (Math.Cos(abHeading) * 4000.0);

            refPoint2.easting = refLineP2.easting;
            refPoint2.northing = refLineP2.northing;
        }

        /// <summary>
        /// Move the AB line dependent of the distance passed in
        /// </summary>
        /// <param name="dist">distance value for knowing how far to move the AB Line</param>
        public void MoveABLine(double dist)
        {
            // calculate the heading 90 degrees to ref ABLine heading
            double headingCalc = isLineSameAsVehicleHeading ? abHeading + glm.PIBy2 : abHeading - glm.PIBy2;

            // calculate the new points for the reference line and points
            refPoint1.easting = (Math.Sin(headingCalc) * dist) + refPoint1.easting;
            refPoint1.northing = (Math.Cos(headingCalc) * dist) + refPoint1.northing;

            refLineP1.easting = refPoint1.easting - (Math.Sin(abHeading) * 4000.0);
            refLineP1.northing = refPoint1.northing - (Math.Cos(abHeading) * 4000.0);

            refLineP2.easting = refPoint1.easting + (Math.Sin(abHeading) * 4000.0);
            refLineP2.northing = refPoint1.northing + (Math.Cos(abHeading) * 4000.0);

            refPoint2.easting = refLineP2.easting;
            refPoint2.northing = refLineP2.northing;
        }
        // angle velocity
        public double angVel;

        /// <summary>
        /// Getting the current AB Line the tractor is on.
        /// </summary>
        /// <param name="pivot">The difference from the GPS location to the pivot point
        ///                     which is the back wheel</param>
        /// <param name="steer">The difference from the back wheel to the front wheel </param>
        public void GetCurrentABLine(Vec3 pivot, Vec3 steer)
        {
            if (mainForm.isStanleyUsed)
            {
                // move the ABLine over based on the overlap amount set in vehicle
                double widthMinusOverlap = mainForm.vehicle.toolWidth - mainForm.vehicle.toolOverlap;

                // x2-x1
                double dx = refLineP2.easting - refLineP1.easting;
                // z2-z1
                double dy = refLineP2.northing - refLineP1.northing;

                // how far are we away from the reference line at 90 degrees
                distanceFromRefLine = ((dy * pivot.easting) - (dx * pivot.northing) + (refLineP2.easting
                                        * refLineP1.northing) - (refLineP2.northing * refLineP1.easting))
                                            / Math.Sqrt((dy * dy) + (dx * dx));

                // figures out which side of the line were on. 
                // refLineSide = 1  // Right of Line 
                // refLineSide = -1 // Left of Line 
                if (distanceFromRefLine > 0) refLineSide = 1;
                else refLineSide = -1;

                //absolute the distance
                distanceFromRefLine = Math.Abs(distanceFromRefLine);

                //Which ABLine is the vehicle on, negative is left and positive is right side
                howManyPathsAway = Math.Round(distanceFromRefLine / widthMinusOverlap, 0, MidpointRounding.AwayFromZero);

                //generate that pass number as signed integer
                passNumber = Convert.ToInt32(refLineSide * howManyPathsAway);

                //calculate the new point that is number of implement widths over
                double toolOffset = mainForm.vehicle.toolOffset;
                vec2 point1;

                //depending which way you are going, the offset can be either side
                if (isLineSameAsVehicleHeading)
                {
                    point1 = new vec2((Math.Cos(-abHeading) * ((widthMinusOverlap * howManyPathsAway * refLineSide) - toolOffset)) + refPoint1.easting,
                    (Math.Sin(-abHeading) * ((widthMinusOverlap * howManyPathsAway * refLineSide) - toolOffset)) + refPoint1.northing);
                }
                else
                {
                    point1 = new vec2((Math.Cos(-abHeading) * ((widthMinusOverlap * howManyPathsAway * refLineSide) + toolOffset)) + refPoint1.easting,
                        (Math.Sin(-abHeading) * ((widthMinusOverlap * howManyPathsAway * refLineSide) + toolOffset)) + refPoint1.northing);
                }

                //create the new line extent points for current ABLine based on original heading of AB line
                currentLineP1.easting = point1.easting - (Math.Sin(abHeading) * 40000.0);
                currentLineP1.northing = point1.northing - (Math.Cos(abHeading) * 40000.0);

                currentLineP2.easting = point1.easting + (Math.Sin(abHeading) * 40000.0);
                currentLineP2.northing = point1.northing + (Math.Cos(abHeading) * 40000.0);

                //get the distance from currently active AB line
                //x2-x1
                dx = currentLineP2.easting - currentLineP1.easting;
                //z2-z1
                dy = currentLineP2.northing - currentLineP1.northing;

                //how far from current AB Line is fix
                distanceFromCurrentLine = ((dy * steer.easting) - (dx * steer.northing) + (currentLineP2.easting
                            * currentLineP1.northing) - (currentLineP2.northing * currentLineP1.easting))
                            / Math.Sqrt((dy * dy) + (dx * dx));

                //are we on the right side or not
                isOnRightSideCurrentLine = distanceFromCurrentLine > 0;

                //absolute the distance
                distanceFromCurrentLine = Math.Abs(distanceFromCurrentLine);

                //Subtract the two headings, if > 1.57 its going the opposite heading as refAB
                abFixHeadingDelta = (Math.Abs(mainForm.fixHeading - abHeading));
                if (abFixHeadingDelta >= Math.PI) abFixHeadingDelta = Math.Abs(abFixHeadingDelta - glm.twoPI);

                isLineSameAsVehicleHeading = abFixHeadingDelta < glm.PIBy2;

                // **Stanley Point ** - calc point on ABLine closest to current steer position
                double U = (((steer.easting - currentLineP1.easting) * dx)
                            + ((steer.northing - currentLineP1.northing) * dy))
                            / ((dx * dx) + (dy * dy));

                //point on AB line closest to pivot axle point
                rEast = currentLineP1.easting + (U * dx);
                rNorth = currentLineP1.northing + (U * dy);

                //distance is negative if on left, positive if on right
                if (isLineSameAsVehicleHeading)
                {
                    if (!isOnRightSideCurrentLine)
                    {
                        distanceFromCurrentLine *= -1.0;
                    }
                    abFixHeadingDelta = (steer.heading - abHeading);
                }

                //opposite way so right is left
                else
                {
                    if (isOnRightSideCurrentLine)
                    {
                        distanceFromCurrentLine *= -1.0;
                    }
                    abFixHeadingDelta = (steer.heading - abHeading + Math.PI);
                }

                //Fix the circular error
                if (abFixHeadingDelta > Math.PI) abFixHeadingDelta -= Math.PI;
                else if (abFixHeadingDelta < Math.PI) abFixHeadingDelta += Math.PI;

                if (abFixHeadingDelta > glm.PIBy2) abFixHeadingDelta -= Math.PI;
                else if (abFixHeadingDelta < -glm.PIBy2) abFixHeadingDelta += Math.PI;

                abFixHeadingDelta *= mainForm.vehicle.stanleyHeadingErrorGain;
                if (abFixHeadingDelta > 0.4) abFixHeadingDelta = 0.4;
                if (abFixHeadingDelta < -0.4) abFixHeadingDelta = -0.4;

                steerAngle = Math.Atan((distanceFromCurrentLine * mainForm.vehicle.stanleyGain) / ((mainForm.pn.speed * 0.277777) + 1));

                if (steerAngle > 0.4) steerAngle = 0.4;
                if (steerAngle < -0.4) steerAngle = -0.4;

                steerAngle = glm.toDegrees((steerAngle + abFixHeadingDelta) * -1.0);

                if (steerAngle < -mainForm.vehicle.maxSteerAngle) steerAngle = -mainForm.vehicle.maxSteerAngle;
                if (steerAngle > mainForm.vehicle.maxSteerAngle) steerAngle = mainForm.vehicle.maxSteerAngle;

                //Convert to millimeters
                distanceFromCurrentLine = Math.Round(distanceFromCurrentLine * 1000.0, MidpointRounding.AwayFromZero);
            }
            else
            {
                //move the ABLine over based on the overlap amount set in vehicle
                double widthMinusOverlap = mainForm.vehicle.toolWidth - mainForm.vehicle.toolOverlap;

                //x2-x1
                double dx = refLineP2.easting - refLineP1.easting;
                //z2-z1
                double dy = refLineP2.northing - refLineP1.northing;

                //how far are we away from the reference line at 90 degrees
                distanceFromRefLine = ((dy * pivot.easting) - (dx * pivot.northing) + (refLineP2.easting
                                        * refLineP1.northing) - (refLineP2.northing * refLineP1.easting))
                                            / Math.Sqrt((dy * dy) + (dx * dx));

                //sign of distance determines which side of line we are on
                if (distanceFromRefLine > 0) refLineSide = 1;
                else refLineSide = -1;

                //absolute the distance
                distanceFromRefLine = Math.Abs(distanceFromRefLine);

                //Which ABLine is the vehicle on, negative is left and positive is right side
                howManyPathsAway = Math.Round(distanceFromRefLine / widthMinusOverlap, 0, MidpointRounding.AwayFromZero);

                //generate that pass number as signed integer
                passNumber = Convert.ToInt32(refLineSide * howManyPathsAway);

                //calculate the new point that is number of implement widths over
                double toolOffset = mainForm.vehicle.toolOffset;
                vec2 point1;

                //depending which way you are going, the offset can be either side
                if (isLineSameAsVehicleHeading)
                {
                    point1 = new vec2((Math.Cos(-abHeading) * ((widthMinusOverlap * howManyPathsAway * refLineSide) - toolOffset)) + refPoint1.easting,
                    (Math.Sin(-abHeading) * ((widthMinusOverlap * howManyPathsAway * refLineSide) - toolOffset)) + refPoint1.northing);
                }
                else
                {
                    point1 = new vec2((Math.Cos(-abHeading) * ((widthMinusOverlap * howManyPathsAway * refLineSide) + toolOffset)) + refPoint1.easting,
                        (Math.Sin(-abHeading) * ((widthMinusOverlap * howManyPathsAway * refLineSide) + toolOffset)) + refPoint1.northing);
                }

                //create the new line extent points for current ABLine based on original heading of AB line
                currentLineP1.easting = point1.easting - (Math.Sin(abHeading) * 40000.0);
                currentLineP1.northing = point1.northing - (Math.Cos(abHeading) * 40000.0);

                currentLineP2.easting = point1.easting + (Math.Sin(abHeading) * 40000.0);
                currentLineP2.northing = point1.northing + (Math.Cos(abHeading) * 40000.0);

                //get the distance from currently active AB line
                //x2-x1
                dx = currentLineP2.easting - currentLineP1.easting;
                //z2-z1
                dy = currentLineP2.northing - currentLineP1.northing;

                //how far from current AB Line is fix
                distanceFromCurrentLine = ((dy * pivot.easting) - (dx * pivot.northing) + (currentLineP2.easting
                            * currentLineP1.northing) - (currentLineP2.northing * currentLineP1.easting))
                            / Math.Sqrt((dy * dy) + (dx * dx));

                //are we on the right side or not
                isOnRightSideCurrentLine = distanceFromCurrentLine > 0;

                //absolute the distance
                distanceFromCurrentLine = Math.Abs(distanceFromCurrentLine);

                //update base on autosteer settings and distance from line
                double goalPointDistance = mainForm.vehicle.UpdateGoalPointDistance(distanceFromCurrentLine);
                mainForm.lookaheadActual = goalPointDistance;

                //Subtract the two headings, if > 1.57 its going the opposite heading as refAB
                abFixHeadingDelta = (Math.Abs(mainForm.fixHeading - abHeading));
                if (abFixHeadingDelta >= Math.PI) abFixHeadingDelta = Math.Abs(abFixHeadingDelta - glm.twoPI);

                // ** Pure pursuit ** - calc point on ABLine closest to current position
                double U = (((pivot.easting - currentLineP1.easting) * dx)
                            + ((pivot.northing - currentLineP1.northing) * dy))
                            / ((dx * dx) + (dy * dy));

                //point on AB line closest to pivot axle point
                rEast = currentLineP1.easting + (U * dx);
                rNorth = currentLineP1.northing + (U * dy);

                if (abFixHeadingDelta >= glm.PIBy2)
                {
                    isLineSameAsVehicleHeading = false;
                    goalPoint.easting = rEast - (Math.Sin(abHeading) * goalPointDistance);
                    goalPoint.northing = rNorth - (Math.Cos(abHeading) * goalPointDistance);
                }
                else
                {
                    isLineSameAsVehicleHeading = true;
                    goalPoint.easting = rEast + (Math.Sin(abHeading) * goalPointDistance);
                    goalPoint.northing = rNorth + (Math.Cos(abHeading) * goalPointDistance);
                }

                //calc "D" the distance from pivot axle to lookahead point
                double goalPointDistanceDSquared
                    = glm.DistanceSquared(goalPoint.northing, goalPoint.easting, pivot.northing, pivot.easting);

                //calculate the the new x in local coordinates and steering angle degrees based on wheelbase
                double localHeading = glm.twoPI - mainForm.fixHeading;
                ppRadius = goalPointDistanceDSquared / (2 * (((goalPoint.easting - pivot.easting) * Math.Cos(localHeading))
                    + ((goalPoint.northing - pivot.northing) * Math.Sin(localHeading))));

                steerAngle = glm.toDegrees(Math.Atan(2 * (((goalPoint.easting - pivot.easting) * Math.Cos(localHeading))
                    + ((goalPoint.northing - pivot.northing) * Math.Sin(localHeading))) * mainForm.vehicle.wheelbase
                    / goalPointDistanceDSquared));
                if (steerAngle < -mainForm.vehicle.maxSteerAngle) steerAngle = -mainForm.vehicle.maxSteerAngle;
                if (steerAngle > mainForm.vehicle.maxSteerAngle) steerAngle = mainForm.vehicle.maxSteerAngle;

                //limit circle size for display purpose
                if (ppRadius < -500) ppRadius = -500;
                if (ppRadius > 500) ppRadius = 500;

                radiusPoint.easting = pivot.easting + (ppRadius * Math.Cos(localHeading));
                radiusPoint.northing = pivot.northing + (ppRadius * Math.Sin(localHeading));

                //Convert to millimeters
                distanceFromCurrentLine = Math.Round(distanceFromCurrentLine * 1000.0, MidpointRounding.AwayFromZero);

                //angular velocity in rads/sec  = 2PI * m/sec * radians/meters
                angVel = glm.twoPI * 0.277777 * mainForm.pn.speed * (Math.Tan(glm.toRadians(steerAngle))) / mainForm.vehicle.wheelbase;

                //clamp the steering angle to not exceed safe angular velocity
                if (Math.Abs(angVel) > mainForm.vehicle.maxAngularVelocity)
                {
                    steerAngle = glm.toDegrees(steerAngle > 0 ? (Math.Atan((mainForm.vehicle.wheelbase * mainForm.vehicle.maxAngularVelocity)
                        / (glm.twoPI * mainForm.pn.speed * 0.277777)))
                        : (Math.Atan((mainForm.vehicle.wheelbase * -mainForm.vehicle.maxAngularVelocity) / (glm.twoPI * mainForm.pn.speed * 0.277777))));
                }

                //distance is negative if on left, positive if on right
                if (isLineSameAsVehicleHeading)
                {
                    if (!isOnRightSideCurrentLine) distanceFromCurrentLine *= -1.0;
                }

                //opposite way so right is left
                else
                {
                    if (isOnRightSideCurrentLine) distanceFromCurrentLine *= -1.0;
                }
            }

            mainForm.guidanceLineDistanceOff = (Int16)distanceFromCurrentLine;
            mainForm.guidanceLineSteerAngle = (Int16)(steerAngle * 100);
        }

        /// <summary>
        /// draws the current AB line and ref line. In addition draws the tram lines.
        /// </summary>
        public void DrawABLines()
        {
            //Draw AB Points
            GL.PointSize(8.0f);
            GL.Begin(PrimitiveType.Points);

            // *** COLOR FOR POINT A ****
            GL.Color3(0.0f, 0.0f, 1.0f);
            // **************************
            GL.Vertex3(refPoint1.easting, refPoint1.northing, 0.0);
            // *** COLOR FOR POINT B ****
            GL.Color3(1.0f, 0.0f, 0.0f);
            // **************************
            GL.Vertex3(refPoint2.easting, refPoint2.northing, 0.0);
            GL.End();
            GL.PointSize(1.0f);

            if (isLineSet)
            {
                //Draw reference AB line
                GL.LineWidth(2);
                GL.Enable(EnableCap.LineStipple);
                GL.LineStipple(1, 0x07F0);
                GL.Begin(PrimitiveType.Lines);
                // *** COLOR FOR AB REFERENCE LINE ****
                GL.Color3(0.30f, 1.0f, 0.5f);
                // ************************************
                GL.Vertex3(refLineP1.easting, refLineP1.northing, 0);
                GL.Vertex3(refLineP2.easting, refLineP2.northing, 0);

                GL.End();
                GL.Disable(EnableCap.LineStipple);

                //draw current AB Line
                GL.LineWidth(3);
                GL.Begin(PrimitiveType.Lines);
                // *** COLOR FOR ACTUAL AB LINE ****
                GL.Color3(1.0f, 0.1f, 0.1f);
                // *********************************

                //calculate if tram line is here
                isOnTramLine = true;
                if (tramPassEvery != 0)
                {
                    int pass = (int)passNumber + (tramPassEvery * 300) - passBasedOn;
                    if (pass % tramPassEvery != 0)
                    {
                        GL.Color3(0.9f, 0.0f, 0.0f);
                        isOnTramLine = false;
                    }
                    else
                    {
                        GL.Color3(0, 0.9, 0);
                        isOnTramLine = true;
                    }

                    if (isOnTramLine) mainForm.mc.relayRateData[mainForm.mc.rdTramLine] = 1;
                    else mainForm.mc.relayRateData[mainForm.mc.rdTramLine] = 0;
                }

                //based on line pass
                if (Math.Abs(passBasedOn - (int)passNumber) <= 0 && tramPassEvery != 0) GL.Color3(0.990f, 0.190f, 0.990f);

                GL.Vertex3(currentLineP1.easting, currentLineP1.northing, 0.0);
                GL.Vertex3(currentLineP2.easting, currentLineP2.northing, 0.0);
                GL.End();

                if (mainForm.isSideGuideLines)
                {
                    //get the tool offset and width
                    double toolOffset = mainForm.vehicle.toolOffset * 2;
                    double toolWidth = mainForm.vehicle.toolWidth - mainForm.vehicle.toolOverlap;

                    GL.Color3(0.0f, 0.90f, 0.50f);
                    GL.LineWidth(1);
                    GL.Begin(PrimitiveType.Lines);

                    //precalculate sin cos
                    double cosHeading = Math.Cos(-abHeading);
                    double sinHeading = Math.Sin(-abHeading);

                    if (isLineSameAsVehicleHeading)
                    {
                        GL.Vertex3((cosHeading * (toolWidth + toolOffset)) + currentLineP1.easting, (sinHeading * (toolWidth + toolOffset)) + currentLineP1.northing, 0);
                        GL.Vertex3((cosHeading * (toolWidth + toolOffset)) + currentLineP2.easting, (sinHeading * (toolWidth + toolOffset)) + currentLineP2.northing, 0);
                        GL.Vertex3((cosHeading * (-toolWidth + toolOffset)) + currentLineP1.easting, (sinHeading * (-toolWidth + toolOffset)) + currentLineP1.northing, 0);
                        GL.Vertex3((cosHeading * (-toolWidth + toolOffset)) + currentLineP2.easting, (sinHeading * (-toolWidth + toolOffset)) + currentLineP2.northing, 0);

                        toolWidth *= 2;
                        GL.Vertex3((cosHeading * toolWidth) + currentLineP1.easting, (sinHeading * toolWidth) + currentLineP1.northing, 0);
                        GL.Vertex3((cosHeading * toolWidth) + currentLineP2.easting, (sinHeading * toolWidth) + currentLineP2.northing, 0);
                        GL.Vertex3((cosHeading * (-toolWidth)) + currentLineP1.easting, (sinHeading * (-toolWidth)) + currentLineP1.northing, 0);
                        GL.Vertex3((cosHeading * (-toolWidth)) + currentLineP2.easting, (sinHeading * (-toolWidth)) + currentLineP2.northing, 0);
                    }
                    else
                    {
                        GL.Vertex3((cosHeading * (toolWidth - toolOffset)) + currentLineP1.easting, (sinHeading * (toolWidth - toolOffset)) + currentLineP1.northing, 0);
                        GL.Vertex3((cosHeading * (toolWidth - toolOffset)) + currentLineP2.easting, (sinHeading * (toolWidth - toolOffset)) + currentLineP2.northing, 0);
                        GL.Vertex3((cosHeading * (-toolWidth - toolOffset)) + currentLineP1.easting, (sinHeading * (-toolWidth - toolOffset)) + currentLineP1.northing, 0);
                        GL.Vertex3((cosHeading * (-toolWidth - toolOffset)) + currentLineP2.easting, (sinHeading * (-toolWidth - toolOffset)) + currentLineP2.northing, 0);

                        toolWidth *= 2;
                        GL.Vertex3((cosHeading * toolWidth) + currentLineP1.easting, (sinHeading * toolWidth) + currentLineP1.northing, 0);
                        GL.Vertex3((cosHeading * toolWidth) + currentLineP2.easting, (sinHeading * toolWidth) + currentLineP2.northing, 0);
                        GL.Vertex3((cosHeading * (-toolWidth)) + currentLineP1.easting, (sinHeading * (-toolWidth)) + currentLineP1.northing, 0);
                        GL.Vertex3((cosHeading * (-toolWidth)) + currentLineP2.easting, (sinHeading * (-toolWidth)) + currentLineP2.northing, 0);
                    }
                    GL.End();
                }

                if (mainForm.isPureDisplayOn && !mainForm.isStanleyUsed)
                {
                    //draw the guidance circle
                    const int numSegments = 100;
                    {
                        if (ppRadius < 50 && ppRadius > -50 && mainForm.isPureDisplayOn)
                        {
                            GL.Color3(0.95f, 0.30f, 0.950f);
                            double theta = glm.twoPI / numSegments;
                            double c = Math.Cos(theta);//precalculate the sine and cosine
                            double s = Math.Sin(theta);

                            double x = ppRadius;//we start at angle = 0
                            double y = 0;
                            GL.LineWidth(1);
                            GL.Begin(PrimitiveType.LineLoop);
                            for (int ii = 0; ii < numSegments; ii++)
                            {
                                //output vertex
                                GL.Vertex3(x + radiusPoint.easting, y + radiusPoint.northing, 0.0);

                                //apply the rotation matrix
                                double t = x;
                                x = (c * x) - (s * y);
                                y = (s * t) + (c * y);
                            }
                            GL.End();
                        }
                    }

                    //Draw lookahead Point
                    GL.PointSize(8.0f);
                    GL.Begin(PrimitiveType.Points);
                    GL.Color3(0f, 0f, 0.60f);
                    GL.Vertex3(goalPoint.easting, goalPoint.northing, 0.0);
                    //GL.Vertex3(rEast, rNorth, 0.0);
                    GL.End();
                    GL.PointSize(1.0f);
                }

                GL.PointSize(1.0f);
                GL.LineWidth(1);
            }
        }

        /// <summary>
        /// Function used to clear the current data in the AB object members.
        /// </summary>
        public void ResetABLine()
        {
            refPoint1 = new vec2(0.2, 0.2);
            refPoint2 = new vec2(0.3, 0.3);

            refLineP1 = new vec2(0.0, 0.0);
            refLineP2 = new vec2(0.0, 1.0);

            currentLineP1 = new vec2(0.0, 0.0);
            currentLineP2 = new vec2(0.0, 0.2);

            abHeading = 0.0;
            isLineSet = false;
            isLineBeingSet = false;
            howManyPathsAway = 0.0;
            passNumber = 0;
        }
    }
}