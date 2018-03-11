using Microsoft.Kinect;
using System.Windows;

namespace KinectCoordinateMapping
{
    /// <summary>
    /// Represents a single gesture segment which uses relative positioning of body parts to detect a gesture.
    /// </summary>
    public interface IGestureSegment
    {
        /// <summary>
        /// Updates the current gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>A GesturePartResult based on whether the gesture part has been completed.</returns>
        GesturePartResult Update(Skeleton skeleton);
    }

    public class StartHandsUp : IGestureSegment
    {
        /// <summary>
        /// Updates the current gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>A GesturePartResult based on whether the gesture part has been completed.</returns>
        public GesturePartResult Update(Skeleton skeleton)
        {
            if ((skeleton.Joints[JointType.ElbowRight].Position.Y > skeleton.Joints[JointType.ShoulderCenter].Position.Y) &&
                (skeleton.Joints[JointType.ElbowLeft].Position.Y > skeleton.Joints[JointType.ShoulderCenter].Position.Y))
            {
                if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.Head].Position.Y)
                {
                    if (skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.Head].Position.X)
                    {
                        Gesture.ShoulderPointIni = new Point(skeleton.Joints[JointType.ShoulderCenter].Position.X, skeleton.Joints[JointType.ShoulderCenter].Position.Y);
                        Gesture.ShoulderPointGoTo = new Point(skeleton.Joints[JointType.ShoulderCenter].Position.X, skeleton.Joints[JointType.ShoulderCenter].Position.Y);
                        return GesturePartResult.StartHandsUp;
                    }
                }
            }
            return GesturePartResult.None;
        }
    }


    public class GoToLeft : IGestureSegment
    {
        /// <summary>
        /// Updates the current gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>A GesturePartResult based on whether the gesture part has been completed.</returns>
        public GesturePartResult Update(Skeleton skeleton)
        {
            if (Gesture.ShoulderPointGoTo.X == 0)
            {
                return GesturePartResult.None;
            }

            var centroHombroX = skeleton.Joints[JointType.ShoulderCenter].Position.X;
            var diferencia = centroHombroX - Gesture.ShoulderPointGoTo.X;

            if (centroHombroX < Gesture.ShoulderPointGoTo.X && diferencia < -Constants.DIF_SHOULDER_X)
            {
                Gesture.ShoulderPointGoTo = new Point(skeleton.Joints[JointType.ShoulderCenter].Position.X, skeleton.Joints[JointType.ShoulderCenter].Position.Y);
                return GesturePartResult.GoToLeft;
            }

            return GesturePartResult.None;
        }
    }


    public class GoToRight : IGestureSegment
    {
        /// <summary>
        /// Updates the current gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>A GesturePartResult based on whether the gesture part has been completed.</returns>
        public GesturePartResult Update(Skeleton skeleton)
        {
            if (Gesture.ShoulderPointGoTo.X == 0)
            {
                return GesturePartResult.None;
            }

            var centroHombroX = skeleton.Joints[JointType.ShoulderCenter].Position.X;
            var diferencia = centroHombroX - Gesture.ShoulderPointGoTo.X;

            if (centroHombroX > Gesture.ShoulderPointGoTo.X && diferencia > Constants.DIF_SHOULDER_X)
            {
                Gesture.ShoulderPointGoTo = new Point(skeleton.Joints[JointType.ShoulderCenter].Position.X, skeleton.Joints[JointType.ShoulderCenter].Position.Y);
                return GesturePartResult.GoToRight;
            }

            return GesturePartResult.None;

        }
    }

    public class MoveToLeft : IGestureSegment
    {
        /// <summary>
        /// Updates the current gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>A GesturePartResult based on whether the gesture part has been completed.</returns>
        public GesturePartResult Update(Skeleton skeleton)
        {
            if (Gesture.ShoulderPointIni.X == 0)
            {
                return GesturePartResult.None;
            }

            var centroHombroX = skeleton.Joints[JointType.ShoulderCenter].Position.X;
            var diferencia = centroHombroX - Gesture.ShoulderPointIni.X;

            if (centroHombroX < Gesture.ShoulderPointIni.X && diferencia < -Constants.DIF_SHOULDER_X)
            {
                return GesturePartResult.MoveToLeft;
            }

            // Hand dropped
            return GesturePartResult.None;
        }
    }


    public class MoveToRight : IGestureSegment
    {
        /// <summary>
        /// Updates the current gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>A GesturePartResult based on whether the gesture part has been completed.</returns>
        public GesturePartResult Update(Skeleton skeleton)
        {
            if (Gesture.ShoulderPointIni.X == 0)
            {
                return GesturePartResult.None;
            }

            var centroHombroX = skeleton.Joints[JointType.ShoulderCenter].Position.X;
            var diferencia = centroHombroX - Gesture.ShoulderPointIni.X;

            if (centroHombroX > Gesture.ShoulderPointIni.X && diferencia > Constants.DIF_SHOULDER_X)
            {
                return GesturePartResult.MoveToRight;
            }

            // Hand dropped
            return GesturePartResult.None;
        }
    }

    public class MoveRightHand : IGestureSegment
    {
        /// <summary>
        /// Updates the current gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>A GesturePartResult based on whether the gesture part has been completed.</returns>
        public GesturePartResult Update(Skeleton skeleton)
        {
            if (Gesture.ShoulderPointIni.X == 0)
            {
                return GesturePartResult.None;
            }
            // Hand above elbow
            if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.ElbowRight].Position.Y)
            {
                // Hand right of elbow
                if (skeleton.Joints[JointType.HandRight].Position.X > skeleton.Joints[JointType.ElbowRight].Position.X)
                {
                    return GesturePartResult.MoveRightHand;
                }
            }

            return GesturePartResult.None;
        }
    }

    public class MoveLeftHand : IGestureSegment
    {
        /// <summary>
        /// Updates the current gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>A GesturePartResult based on whether the gesture part has been completed.</returns>
        public GesturePartResult Update(Skeleton skeleton)
        {
            if (Gesture.ShoulderPointIni.X == 0)
            {
                return GesturePartResult.None;
            }
            // Hand above elbow
            if (skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.ElbowLeft].Position.Y)
            {
                // Hand left of elbow
                if (skeleton.Joints[JointType.ElbowLeft].Position.X > skeleton.Joints[JointType.HandLeft].Position.X)
                {
                    return GesturePartResult.MoveLeftHand;
                }
            }
            return GesturePartResult.None;
        }
    }

}
