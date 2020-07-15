﻿using OpenTK.Graphics.OpenGL;
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
        static double endpointMultiplier= 4000.0;
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

        private readonly FormGPS mainForm;

        public vec2 refPointA = new vec2(0.2, 0.2);
        public vec2 refPointB = new vec2(0.3, 0.3);

        public vec2 refLineAEndpoint = new vec2(0.0, 0.0);
        public vec2 refLineBEndpoint = new vec2(0.0, 1.0);

        public vec2 currentLine_PointA = new vec2(0.0, 0.0);
        public vec2 currentLine_PointB = new vec2(0.0, 1.0);

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
            refPointA = new vec2(0.0, 0.0);
            refPointB = new vec2(0.0, 1.0);

            refLineAEndpoint = new vec2(0.0, 0.0);
            refLineBEndpoint = new vec2(0.0, 1.0);

            currentLine_PointA = new vec2(0.0, 0.0);
            currentLine_PointB = new vec2(0.0, 1.0);

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
            refPointB.easting = mainForm.pn.fix.easting;
            refPointB.northing = mainForm.pn.fix.northing;

            // calculate the AB Heading
            abHeading = Math.Atan2(refPointB.easting - refPointA.easting, 
                                   refPointB.northing - refPointA.northing);

            // sets the heading to a positive radian
            if (abHeading < 0) abHeading += glm.twoPI;

            // sin x cos z for endpoints, opposite for additional lines
            refLineAEndpoint.easting = refPointA.easting - (Math.Sin(abHeading) * endpointMultiplier);
            refLineAEndpoint.northing = refPointA.northing - (Math.Cos(abHeading) * endpointMultiplier);

            refLineBEndpoint.easting = refPointA.easting + (Math.Sin(abHeading) * endpointMultiplier);
            refLineBEndpoint.northing = refPointA.northing + (Math.Cos(abHeading) * endpointMultiplier);

            isLineSet = true;
        }

        /// <summary>
        /// Set the reference lines if the heading is already found
        /// </summary>
        public void SetABLineByHeading()
        {
            // heading is set in the AB Form
            refLineAEndpoint.easting = refPointA.easting - (Math.Sin(abHeading) * endpointMultiplier);
            refLineAEndpoint.northing = refPointA.northing - (Math.Cos(abHeading) * endpointMultiplier);

            refLineBEndpoint.easting = refPointA.easting + (Math.Sin(abHeading) * endpointMultiplier);
            refLineBEndpoint.northing = refPointA.northing + (Math.Cos(abHeading) * endpointMultiplier);

            refPointB.easting = refLineBEndpoint.easting;
            refPointB.northing = refLineBEndpoint.northing;

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
            refPointA.easting = (Math.Sin(headingCalc) * Math.Abs(distanceFromCurrentLine) * 0.001) + refPointA.easting;
            refPointA.northing = (Math.Cos(headingCalc) * Math.Abs(distanceFromCurrentLine) * 0.001) + refPointA.northing;

            refLineAEndpoint.easting = refPointA.easting - (Math.Sin(abHeading) * endpointMultiplier);
            refLineAEndpoint.northing = refPointA.northing - (Math.Cos(abHeading) * endpointMultiplier);

            refLineBEndpoint.easting = refPointA.easting + (Math.Sin(abHeading) * endpointMultiplier);
            refLineBEndpoint.northing = refPointA.northing + (Math.Cos(abHeading) * endpointMultiplier);

            refPointB.easting = refLineBEndpoint.easting;
            refPointB.northing = refLineBEndpoint.northing;
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
            refPointA.easting = (Math.Sin(headingCalc) * dist) + refPointA.easting;
            refPointA.northing = (Math.Cos(headingCalc) * dist) + refPointA.northing;

            refLineAEndpoint.easting = refPointA.easting - (Math.Sin(abHeading) * endpointMultiplier);
            refLineAEndpoint.northing = refPointA.northing - (Math.Cos(abHeading) * endpointMultiplier);

            refLineBEndpoint.easting = refPointA.easting + (Math.Sin(abHeading) * endpointMultiplier);
            refLineBEndpoint.northing = refPointA.northing + (Math.Cos(abHeading) * endpointMultiplier);

            refPointB.easting = refLineBEndpoint.easting;
            refPointB.northing = refLineBEndpoint.northing;
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
                double dx = refLineBEndpoint.easting - refLineAEndpoint.easting;
                // z2-z1
                double dy = refLineBEndpoint.northing - refLineAEndpoint.northing;

                // how far are we away from the reference line at 90 degrees
                distanceFromRefLine = ((dy * pivot.easting) - (dx * pivot.northing) + (refLineBEndpoint.easting
                                        * refLineAEndpoint.northing) - (refLineBEndpoint.northing * refLineAEndpoint.easting))
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
                    point1 = new vec2((Math.Cos(-abHeading) * ((widthMinusOverlap * howManyPathsAway * refLineSide) - toolOffset)) + refPointA.easting,
                    (Math.Sin(-abHeading) * ((widthMinusOverlap * howManyPathsAway * refLineSide) - toolOffset)) + refPointA.northing);
                }
                else
                {
                    point1 = new vec2((Math.Cos(-abHeading) * ((widthMinusOverlap * howManyPathsAway * refLineSide) + toolOffset)) + refPointA.easting,
                        (Math.Sin(-abHeading) * ((widthMinusOverlap * howManyPathsAway * refLineSide) + toolOffset)) + refPointA.northing);
                }

                //create the new line extent points for current ABLine based on original heading of AB line
                currentLine_PointA.easting = point1.easting - (Math.Sin(abHeading) * 40000.0);
                currentLine_PointA.northing = point1.northing - (Math.Cos(abHeading) * 40000.0);

                currentLine_PointB.easting = point1.easting + (Math.Sin(abHeading) * 40000.0);
                currentLine_PointB.northing = point1.northing + (Math.Cos(abHeading) * 40000.0);

                //get the distance from currently active AB line
                //x2-x1
                dx = currentLine_PointB.easting - currentLine_PointA.easting;
                //z2-z1
                dy = currentLine_PointB.northing - currentLine_PointA.northing;

                //how far from current AB Line is fix
                distanceFromCurrentLine = ((dy * steer.easting) - (dx * steer.northing) + (currentLine_PointB.easting
                            * currentLine_PointA.northing) - (currentLine_PointB.northing * currentLine_PointA.easting))
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
                double U = (((steer.easting - currentLine_PointA.easting) * dx)
                            + ((steer.northing - currentLine_PointA.northing) * dy))
                            / ((dx * dx) + (dy * dy));

                //point on AB line closest to pivot axle point
                rEast = currentLine_PointA.easting + (U * dx);
                rNorth = currentLine_PointA.northing + (U * dy);

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
                double dx = refLineBEndpoint.easting - refLineAEndpoint.easting;
                //z2-z1
                double dy = refLineBEndpoint.northing - refLineAEndpoint.northing;

                //how far are we away from the reference line at 90 degrees
                distanceFromRefLine = ((dy * pivot.easting) - (dx * pivot.northing) + (refLineBEndpoint.easting
                                        * refLineAEndpoint.northing) - (refLineBEndpoint.northing * refLineAEndpoint.easting))
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
                    point1 = new vec2((Math.Cos(-abHeading) * ((widthMinusOverlap * howManyPathsAway * refLineSide) - toolOffset)) + refPointA.easting,
                    (Math.Sin(-abHeading) * ((widthMinusOverlap * howManyPathsAway * refLineSide) - toolOffset)) + refPointA.northing);
                }
                else
                {
                    point1 = new vec2((Math.Cos(-abHeading) * ((widthMinusOverlap * howManyPathsAway * refLineSide) + toolOffset)) + refPointA.easting,
                        (Math.Sin(-abHeading) * ((widthMinusOverlap * howManyPathsAway * refLineSide) + toolOffset)) + refPointA.northing);
                }

                //create the new line extent points for current ABLine based on original heading of AB line
                currentLine_PointA.easting = point1.easting - (Math.Sin(abHeading) * 40000.0);
                currentLine_PointA.northing = point1.northing - (Math.Cos(abHeading) * 40000.0);

                currentLine_PointB.easting = point1.easting + (Math.Sin(abHeading) * 40000.0);
                currentLine_PointB.northing = point1.northing + (Math.Cos(abHeading) * 40000.0);

                //get the distance from currently active AB line
                //x2-x1
                dx = currentLine_PointB.easting - currentLine_PointA.easting;
                //z2-z1
                dy = currentLine_PointB.northing - currentLine_PointA.northing;

                //how far from current AB Line is fix
                distanceFromCurrentLine = ((dy * pivot.easting) - (dx * pivot.northing) + (currentLine_PointB.easting
                            * currentLine_PointA.northing) - (currentLine_PointB.northing * currentLine_PointA.easting))
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
                double U = (((pivot.easting - currentLine_PointA.easting) * dx)
                            + ((pivot.northing - currentLine_PointA.northing) * dy))
                            / ((dx * dx) + (dy * dy));

                //point on AB line closest to pivot axle point
                rEast = currentLine_PointA.easting + (U * dx);
                rNorth = currentLine_PointA.northing + (U * dy);

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
            GL.Vertex3(refPointA.easting, refPointA.northing, 0.0);
            // *** COLOR FOR POINT B ****
            GL.Color3(1.0f, 0.0f, 0.0f);
            // **************************
            GL.Vertex3(refPointB.easting, refPointB.northing, 0.0);
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
                GL.Vertex3(refLineAEndpoint.easting, refLineAEndpoint.northing, 0);
                GL.Vertex3(refLineBEndpoint.easting, refLineBEndpoint.northing, 0);

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

                GL.Vertex3(currentLine_PointA.easting, currentLine_PointA.northing, 0.0);
                GL.Vertex3(currentLine_PointB.easting, currentLine_PointB.northing, 0.0);
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
                        GL.Vertex3((cosHeading * (toolWidth + toolOffset)) + currentLine_PointA.easting, (sinHeading * (toolWidth + toolOffset)) + currentLine_PointA.northing, 0);
                        GL.Vertex3((cosHeading * (toolWidth + toolOffset)) + currentLine_PointB.easting, (sinHeading * (toolWidth + toolOffset)) + currentLine_PointB.northing, 0);
                        GL.Vertex3((cosHeading * (-toolWidth + toolOffset)) + currentLine_PointA.easting, (sinHeading * (-toolWidth + toolOffset)) + currentLine_PointA.northing, 0);
                        GL.Vertex3((cosHeading * (-toolWidth + toolOffset)) + currentLine_PointB.easting, (sinHeading * (-toolWidth + toolOffset)) + currentLine_PointB.northing, 0);

                        toolWidth *= 2;
                        GL.Vertex3((cosHeading * toolWidth) + currentLine_PointA.easting, (sinHeading * toolWidth) + currentLine_PointA.northing, 0);
                        GL.Vertex3((cosHeading * toolWidth) + currentLine_PointB.easting, (sinHeading * toolWidth) + currentLine_PointB.northing, 0);
                        GL.Vertex3((cosHeading * (-toolWidth)) + currentLine_PointA.easting, (sinHeading * (-toolWidth)) + currentLine_PointA.northing, 0);
                        GL.Vertex3((cosHeading * (-toolWidth)) + currentLine_PointB.easting, (sinHeading * (-toolWidth)) + currentLine_PointB.northing, 0);
                    }
                    else
                    {
                        GL.Vertex3((cosHeading * (toolWidth - toolOffset)) + currentLine_PointA.easting, (sinHeading * (toolWidth - toolOffset)) + currentLine_PointA.northing, 0);
                        GL.Vertex3((cosHeading * (toolWidth - toolOffset)) + currentLine_PointB.easting, (sinHeading * (toolWidth - toolOffset)) + currentLine_PointB.northing, 0);
                        GL.Vertex3((cosHeading * (-toolWidth - toolOffset)) + currentLine_PointA.easting, (sinHeading * (-toolWidth - toolOffset)) + currentLine_PointA.northing, 0);
                        GL.Vertex3((cosHeading * (-toolWidth - toolOffset)) + currentLine_PointB.easting, (sinHeading * (-toolWidth - toolOffset)) + currentLine_PointB.northing, 0);

                        toolWidth *= 2;
                        GL.Vertex3((cosHeading * toolWidth) + currentLine_PointA.easting, (sinHeading * toolWidth) + currentLine_PointA.northing, 0);
                        GL.Vertex3((cosHeading * toolWidth) + currentLine_PointB.easting, (sinHeading * toolWidth) + currentLine_PointB.northing, 0);
                        GL.Vertex3((cosHeading * (-toolWidth)) + currentLine_PointA.easting, (sinHeading * (-toolWidth)) + currentLine_PointA.northing, 0);
                        GL.Vertex3((cosHeading * (-toolWidth)) + currentLine_PointB.easting, (sinHeading * (-toolWidth)) + currentLine_PointB.northing, 0);
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
            refPointA = new vec2(0.2, 0.2);
            refPointB = new vec2(0.3, 0.3);

            refLineAEndpoint = new vec2(0.0, 0.0);
            refLineBEndpoint = new vec2(0.0, 1.0);

            currentLine_PointA = new vec2(0.0, 0.0);
            currentLine_PointB = new vec2(0.0, 0.2);

            abHeading = 0.0;
            isLineSet = false;
            isLineBeingSet = false;
            howManyPathsAway = 0.0;
            passNumber = 0;
        }
    }
}