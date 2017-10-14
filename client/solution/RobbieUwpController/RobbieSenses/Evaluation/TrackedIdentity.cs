﻿using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media.FaceAnalysis;
using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Face.Contract;

namespace RobbieSenses.Evaluation
{
    /// <summary>
    /// Object use to keep track of tracked faces (on device), combining them with all known info,
    /// such as the persons name, the person ID (of Cognitive services) and the latest emotion scores.
    /// </summary>
    public class TrackedIdentity
    {
        /// <summary>
        /// The bounding box indicating the position and size of the face, as generated by the Windows FaceAnalysis namespace.
        /// </summary>
        private BitmapBounds boundingBox;

        /// <summary>
        /// The total surface area of the bounding box, used to determine the size of the face and consequently, select the largest face to focus on.
        /// </summary>
        private int surfaceArea;

        /// <summary>
        /// The center of the bounding box (FaceBox), used for calculating the distance between two bounding boxes.
        /// </summary>
        /// <remarks>
        /// By using the center point in your calculations, the calculations are not influenced by changes in size of the bounding box,
        /// making it easy and more robust for determining the nearest neighbour in three dimensions (moving back and forth changes the box size).
        /// </remarks>
        private Point centerPoint;

        /// <summary>
        /// The name of the person as identified by the Cognitive Services Face API; null if this face is not yet identified!
        /// </summary>
        public string Name;

        /// <summary>
        /// Identifier of the tracked face (does not correspond with any other API, local ID only),
        /// which is kept active as long as the face is being tracked continously.
        /// Losing a tracked face for a single frame (or more) drops the ID and creates a new one upon re-appearance.
        /// </summary>
        public Guid Identifier;

        /// <summary>
        /// The GUID identifying the person as stored in the Cognitive Services; null if this face is not yet identified!
        /// </summary>
        public Guid PersonId;

        /// <summary>
        /// The face attributes describing the appearance of the person as observed by the Cognitive Services Face API; null if this face is not yet identified!
        /// </summary>
        public FaceAttributes Appearance;

        /// <summary>
        /// The latest or most current emotion scores as detected by the Cognitive Services Emotion API; null if not yet detected or determined.
        /// </summary>
        public EmotionScores EmotionScores;

        /// <summary>
        /// Constructs a new Tracked Identity based on the detected face box, creating a new Guid to keep track of the corresponding face between frames.
        /// </summary>
        /// <param name="faceBox">The bounding box indicating the position and size of the face.</param>
        public TrackedIdentity(BitmapBounds faceBox)
        {
            Identifier = Guid.NewGuid();
            UpdatePosition(faceBox);
        }

        /// <summary>
        /// Returns the bounding box indicating the position and size of the face, as generated by the Windows FaceAnalysis namespace.
        /// </summary>
        public BitmapBounds FaceBox
        {
            get { return boundingBox; }
        }


        /// <summary>
        /// Returns the total surface area of the bounding box, used to determine the size of the face and consequently, select the largest face to focus on.
        /// </summary>
        public int SurfaceArea
        {
            get { return surfaceArea; }
        }

        /// <summary>
        /// Returns the center of the bounding box (FaceBox), used for calculating the distance between two bounding boxes.
        /// </summary>
        public Point CenterPoint
        {
            get { return centerPoint; }
        }

        /// <summary>
        /// Updates the position of the face (probably in a new frame).
        /// Updates both the bounding box as well as the center point of the (new) bounding box.
        /// </summary>
        /// <param name="faceBox">The bounding box indicating the position and size of the face.</param>
        public void UpdatePosition(BitmapBounds faceBox)
        {
            boundingBox = faceBox;
            surfaceArea = (int)faceBox.Width * (int)faceBox.Height;
            centerPoint = IdentityInterpolation.GetCenterPoint(faceBox);
        }

        /// <summary>
        /// Finds the detected face within the list of faces supplied, that is closest to the object identity's position.
        /// </summary>
        /// <param name="faces">A list of detected faces by the Windows FaceAnalysis namespace.</param>
        /// <returns>The face object that is the closest to the object identity's position.</returns>
        public DetectedFace FindNearestFace(IList<DetectedFace> faces)
        {
            if (faces.Count == 0)
            {
                return null;
            }

            DetectedFace nearestFace = null;
            var nearestFaceDistance = double.MaxValue;
            foreach (var face in faces)
            {
                var faceCenter = IdentityInterpolation.GetCenterPoint(face.FaceBox);
                var distance = IdentityInterpolation.GetRelativeDistance(centerPoint, faceCenter);
                if (distance < nearestFaceDistance)
                {
                    nearestFace = face;
                    nearestFaceDistance = distance;
                }
            }

            return nearestFace;
        }
    }
}