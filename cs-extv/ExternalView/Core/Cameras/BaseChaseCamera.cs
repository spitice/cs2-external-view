using LupercaliaMGCore.modules.ExternalView.API;
using LupercaliaMGCore.modules.ExternalView.Utils;
using System.Numerics;
using System.Runtime.CompilerServices;
using CS2TraceRay.Class;
using CS2TraceRay.Struct;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CS2TraceRay.Enum;

namespace LupercaliaMGCore.modules.ExternalView.Cameras
{
    /// <summary>
    /// The base class for chase (third person) cameras.
    /// </summary>
    public abstract class BaseChaseCamera : BaseCamera
    {
        private const float CollisionBackoff = 10.0f;
        private const float EpsilonSquared = 1e-6f;

        public BaseChaseCamera(ICameraContext ctx) : base(ctx)
        {
        }

        abstract protected IExternalViewCsPlayer? Target { get; }
        abstract protected float Distance { get; }
        abstract protected float YawOffset { get; }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override bool Update()
        {
            if (Target == null)
                return false;

            var targetPosition = Target.Origin + Consts.EyeOffset;
            var targetVelocity = Target.Velocity;
            var viewAngle = Ctx.Player.ViewAngle;

            //
            // Add yaw offset to calculate third-person offset
            // - positive angle = offsets the camera to the right side
            // - negative angle = offsets the camera to the left side
            //
            var angle = viewAngle + new Vector3(0, YawOffset, 0);

            var offset = MathUtils.CalculateThirdPersonOffset(angle, Distance);
            var desiredPosition = targetPosition + offset;

            Vector3 cameraPosition;
            if (!Ctx.Api.ConVars.IsThirdPersonTraceBlockEnabled)
            {
                cameraPosition = desiredPosition;
            }
            else
            {
                var start = MathUtils.ToVector(targetPosition);
                var end = MathUtils.ToVector(desiredPosition);

                // TODO: consider ignoring the target entity, enough Player | PlayerClip ?
                var tr = TraceRay.TraceShape(start, end, TraceMask.MaskAll, Contents.Player | Contents.PlayerClip | Contents.Hitbox | Contents.Debris, 0);

                if (tr.DidHit())
                {
                    var dir = desiredPosition - targetPosition;
                    var dirLengthSquared = CalculateLengthSquared(dir);
                    
                    if (dirLengthSquared > EpsilonSquared)
                    {
                        dir = MathUtils.NormalizeFast(dir);
                        cameraPosition = tr.Position - dir * CollisionBackoff;
                    }
                    else
                    {
                        cameraPosition = targetPosition;
                    }
                }
                else
                {
                    cameraPosition = desiredPosition;
                }
            }

            Ctx.CameraEntity?.Teleport(cameraPosition, viewAngle, targetVelocity);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CalculateLengthSquared(Vector3 vector)
        {
            return vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void BatchUpdateCameras(ReadOnlySpan<BaseChaseCamera> cameras)
        {
            if (cameras.Length == 0) return;

            Span<Vector3> viewAngles = stackalloc Vector3[cameras.Length];
            Span<float> distances = stackalloc float[cameras.Length];
            Span<float> yawOffsets = stackalloc float[cameras.Length];
            Span<Vector3> targetPositions = stackalloc Vector3[cameras.Length];

            int validCameras = 0;
            for (int i = 0; i < cameras.Length; i++)
            {
                var camera = cameras[i];
                if (camera.Target == null) continue;

                viewAngles[validCameras] = camera.Ctx.Player.ViewAngle;
                distances[validCameras] = camera.Distance;
                yawOffsets[validCameras] = camera.YawOffset;
                targetPositions[validCameras] = camera.Target.Origin + Consts.EyeOffset;
                validCameras++;
            }

            if (validCameras == 0) return;

            Span<Vector3> adjustedAngles = stackalloc Vector3[validCameras];
            for (int i = 0; i < validCameras; i++)
            {
                adjustedAngles[i] = viewAngles[i] + new Vector3(0, yawOffsets[i], 0);
            }

            Span<Vector3> offsets = stackalloc Vector3[validCameras];
            MathUtils.CalculateThirdPersonOffsetBatch(adjustedAngles, distances, offsets);

            int cameraIndex = 0;
            for (int i = 0; i < cameras.Length; i++)
            {
                var camera = cameras[i];
                if (camera.Target == null) continue;

                var desiredPosition = targetPositions[cameraIndex] + offsets[cameraIndex];

                Vector3 cameraPosition;
                if (!camera.Ctx.Api.ConVars.IsThirdPersonTraceBlockEnabled)
                {
                    cameraPosition = desiredPosition;
                }
                else
                {
                    var start = MathUtils.ToVector(targetPositions[cameraIndex]);
                    var end = MathUtils.ToVector(desiredPosition);
                    var tr = TraceRay.TraceShape(
                        start,
                        end,
                        TraceMask.MaskAll,
                        Contents.Player | Contents.PlayerClip | Contents.Hitbox | Contents.Debris,
                        0
                    );

                    if (tr.DidHit())
                    {
                        var dir = desiredPosition - targetPositions[cameraIndex];
                        var dirLengthSquared = CalculateLengthSquared(dir);
                        
                        if (dirLengthSquared > EpsilonSquared)
                        {
                            dir = MathUtils.NormalizeFast(dir);
                            cameraPosition = tr.Position - dir * CollisionBackoff;
                        }
                        else
                        {
                            cameraPosition = targetPositions[cameraIndex];
                        }
                    }
                    else
                    {
                        cameraPosition = desiredPosition;
                    }
                }

                camera.Ctx.CameraEntity?.Teleport(cameraPosition, viewAngles[cameraIndex], camera.Target.Velocity);
                cameraIndex++;
            }
        }
    }
}
