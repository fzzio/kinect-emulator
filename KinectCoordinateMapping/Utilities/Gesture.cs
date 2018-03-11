using Microsoft.Kinect;
using System;
using System.Windows;

namespace KinectCoordinateMapping
{
    public class Gesture
    {
        IGestureSegment[] _segments;

        int _frameCount = 0;


        public event EventHandler GestureRecognized;

        public static Point ShoulderPointIni = new Point(0,0);

        public static Point ShoulderPointGoTo = new Point(0,0);

        public Gesture()
        {
            ShoulderPointIni = new Point(0, 0);
            ShoulderPointGoTo = new Point(0, 0);

            StartHandsUp start = new StartHandsUp();
            MoveToLeft moveToLeft = new MoveToLeft();
            MoveToRight moveToRight = new MoveToRight();

            _segments = new IGestureSegment[]
            {
                start,
                moveToRight,
                moveToLeft
            };

            Reset();
        }

        /// <summary>
        /// Updates the current gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton data.</param>
        public GesturePartResult Update(Skeleton skeleton)
        {
            GesturePartResult result = GesturePartResult.None;

            for (int i=0; i< _segments.Length; i++)
            {
                result = _segments[i].Update(skeleton);

                if (result != GesturePartResult.None)
                {
                    _frameCount = 0;
                    return result;
                }
                else if (result == GesturePartResult.None || _frameCount == Constants.WINDOW_SIZE)
                {
                    Reset();
                }
                else
                {
                    _frameCount++;
                }
            }

            if (GestureRecognized != null)
            {
                GestureRecognized(this, new EventArgs());
                Reset();
            }

            return GesturePartResult.None;
        }

        /// <summary>
        /// Resets the current gesture.
        /// </summary>
        public void Reset()
        {
            _frameCount = 0;
        }
    }
}
