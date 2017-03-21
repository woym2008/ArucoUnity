﻿using ArucoUnity.Plugin;
using ArucoUnity.Plugin.cv;
using ArucoUnity.Utility;

namespace ArucoUnity
{
  /// \addtogroup aruco_unity_package
  /// \{

  public class ArucoGridBoardTracker : ArucoObjectTracker
  {
    // ArucoObjectTracker methods

    /// <summary>
    /// <see cref="ArucoObjectTracker.Detect(int, Dictionary, HashSet{ArucoObject})"/>
    /// </summary>
    public override void Detect(int cameraId, Dictionary dictionary)
    {
      if (!IsActivated)
      {
        return;
      }

      if (arucoTracker.RefineDetectedMarkers)
      {
        foreach (var arucoBoard in arucoTracker.GetArucoObjects<ArucoGridBoard>(dictionary))
        {
          Functions.RefineDetectedMarkers(arucoTracker.ArucoCamera.Images[cameraId], arucoBoard.Board, arucoTracker.MarkerTracker.MarkerCorners[cameraId][dictionary], 
            arucoTracker.MarkerTracker.MarkerIds[cameraId][dictionary], arucoTracker.MarkerTracker.RejectedCandidateCorners[cameraId][dictionary]);
          arucoTracker.MarkerTracker.DetectedMarkers[cameraId][dictionary] = (int)arucoTracker.MarkerTracker.MarkerIds[cameraId][dictionary].Size();
        }
      }
    }

    /// <summary>
    /// <see cref="ArucoObjectTracker.EstimateTranforms(int, Dictionary, HashSet{ArucoObject})"/>
    /// </summary>
    public override void EstimateTranforms(int cameraId, Dictionary dictionary)
    {
      if (!IsActivated || arucoTracker.MarkerTracker.DetectedMarkers[cameraId][dictionary] <= 0)
      {
        return;
      }

      CameraParameters cameraParameters = arucoTracker.ArucoCamera.CameraParameters;

      foreach (var arucoGridBoard in arucoTracker.GetArucoObjects<ArucoGridBoard>(dictionary))
      {
        Vec3d rvec = null, tvec = null;
        arucoGridBoard.MarkersUsedForEstimation = Functions.EstimatePoseBoard(arucoTracker.MarkerTracker.MarkerCorners[cameraId][dictionary],
          arucoTracker.MarkerTracker.MarkerIds[cameraId][dictionary], arucoGridBoard.Board, cameraParameters.CamerasMatrix[cameraId], 
          cameraParameters.DistCoeffs[cameraId], out rvec, out tvec);

        arucoGridBoard.Rvec = rvec;
        arucoGridBoard.Tvec = tvec;
      }
    }

    /// <summary>
    /// <see cref="ArucoObjectTracker.Draw(int, Dictionary, HashSet{ArucoObject})"/>
    /// </summary>
    public override void Draw(int cameraId, Dictionary dictionary)
    {
      if (!IsActivated || arucoTracker.MarkerTracker.DetectedMarkers[cameraId][dictionary] <= 0)
      {
        return;
      }

      bool updatedCameraImage = false;
      Mat[] cameraImages = arucoTracker.ArucoCamera.Images;
      CameraParameters cameraParameters = arucoTracker.ArucoCamera.CameraParameters;

      foreach (var arucoGridBoard in arucoTracker.GetArucoObjects<ArucoGridBoard>(dictionary))
      {
        if (arucoTracker.DrawAxes && cameraParameters != null && arucoGridBoard.MarkersUsedForEstimation > 0 && arucoGridBoard.Rvec != null)
        {
          Functions.DrawAxis(cameraImages[cameraId], cameraParameters.CamerasMatrix[cameraId], cameraParameters.DistCoeffs[cameraId], 
            arucoGridBoard.Rvec, arucoGridBoard.Tvec, arucoGridBoard.AxisLength);
          updatedCameraImage = true;
        }
      }

      if (updatedCameraImage)
      {
        arucoTracker.ArucoCamera.Images = cameraImages;
      }
    }

    /// <summary>
    /// <see cref="ArucoObjectTracker.Place(int, Dictionary, HashSet{ArucoObject})"/>
    /// </summary>
    public override void Place(int cameraId, Dictionary dictionary)
    {
      if (!IsActivated || arucoTracker.MarkerTracker.DetectedMarkers[cameraId][dictionary] <= 0)
      {
        return;
      }

      foreach (var arucoGridBoard in arucoTracker.GetArucoObjects<ArucoGridBoard>(dictionary))
      {
        if (arucoGridBoard.MarkersUsedForEstimation > 0 && arucoGridBoard.Rvec != null)
        {
          PlaceArucoObject(arucoGridBoard, arucoGridBoard.Rvec, arucoGridBoard.Tvec, cameraId);
        }
      }
    }
  }

  /// \} aruco_unity_package
}