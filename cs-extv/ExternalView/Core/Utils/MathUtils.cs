using CounterStrikeSharp.API.Modules.Utils;
using System.Numerics;
using System.Runtime.CompilerServices;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace LupercaliaMGCore.modules.ExternalView.Utils
{
    public class MathUtils
    {
        private const float PI_OVER_180 = MathF.PI / 180.0f;
        private const float EPSILON = 1e-12f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToRad(float value)
        {
            return value * PI_OVER_180;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVector3(Vector? v)
        {
            if (v == null)
            {
                return Vector3.Zero;
            }

            return new Vector3(v.X, v.Y, v.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVector3(QAngle? v)
        {
            if (v == null)
            {
                return Vector3.Zero;
            }

            return new Vector3(v.X, v.Y, v.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector? ToVectorOrNull(Vector3? v)
        {
            if (v == null)
            {
                return null;
            }

            return ToVector(v.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector ToVector(Vector3 v)
        {
            return new Vector(v.X, v.Y, v.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QAngle? ToQAngleOrNull(Vector3? v)
        {
            if (v == null)
            {
                return null;
            }

            return ToQAngle(v.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QAngle ToQAngle(Vector3 v)
        {
            return new QAngle(v.X, v.Y, v.Z);
        }

        /// <summary>
        /// Fast vector normalization with early zero check
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 NormalizeFast(in Vector3 v)
        {
            float lengthSquared = v.X * v.X + v.Y * v.Y + v.Z * v.Z;
            if (lengthSquared <= EPSILON) 
                return Vector3.Zero;

            // Use MathF for better float precision
            float invLength = 1.0f / MathF.Sqrt(lengthSquared);
            return v * invLength;
        }

        /// <summary>
        /// Calculate third person camera offset using optimized scalar operations
        /// Based on Luna's recommendations for single-element calculations
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 CalculateThirdPersonOffset(Vector3 viewAngleDeg, float distance)
        {
            // Convert degrees to radians using MathF for better float precision
            float yaw = PI_OVER_180 * viewAngleDeg.Y;
            float pitch = PI_OVER_180 * viewAngleDeg.X;

            // Use MathF.SinCos for simultaneous sin/cos calculation (available in .NET 7+)
#if NET7_0_OR_GREATER
            (float sy, float cy) = MathF.SinCos(yaw);
            (float sp, float cp) = MathF.SinCos(pitch);
#else
            float sy = MathF.Sin(yaw);
            float cy = MathF.Cos(yaw);
            float sp = MathF.Sin(pitch);
            float cp = MathF.Cos(pitch);
#endif

            // Calculate direction vector directly without quaternion overhead
            // forward = (cy, sy, 0) in horizontal plane
            // Apply pitch: dir = -forward * cp + (0,0,1) * sp
            var dir = new Vector3(-cy * cp, -sy * cp, sp);

            // Direction is already normalized (unit vector), multiply by distance
            return dir * distance;
        }

        /// <summary>
        /// Batch calculate multiple third person offsets
        /// Only use for large batches (32+ elements) where overhead is justified
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void CalculateThirdPersonOffsetBatch(ReadOnlySpan<Vector3> viewAngles, ReadOnlySpan<float> distances, Span<Vector3> results)
        {
            if (viewAngles.Length != distances.Length || viewAngles.Length != results.Length)
                throw new ArgumentException("All spans must have the same length");

            // For small batches, scalar is faster due to overhead
            if (viewAngles.Length < 32)
            {
                CalculateThirdPersonOffsetBatch_Scalar(viewAngles, distances, results);
                return;
            }

            // For larger batches, use optimized scalar with pre-calculated trig
            CalculateThirdPersonOffsetBatch_OptimizedScalar(viewAngles, distances, results);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static void CalculateThirdPersonOffsetBatch_OptimizedScalar(ReadOnlySpan<Vector3> viewAngles, ReadOnlySpan<float> distances, Span<Vector3> results)
        {
            // Pre-calculate all trigonometric values in one pass
            int length = viewAngles.Length;
            
            // Use stack allocation for reasonable sizes, heap for very large batches
            bool useStack = length <= 256;
            
            Span<float> cosYaw = useStack ? stackalloc float[length] : new float[length];
            Span<float> sinYaw = useStack ? stackalloc float[length] : new float[length];
            Span<float> cosPitch = useStack ? stackalloc float[length] : new float[length];
            Span<float> sinPitch = useStack ? stackalloc float[length] : new float[length];

            // First pass: calculate all trigonometric values
            for (int i = 0; i < length; i++)
            {
                float yaw = PI_OVER_180 * viewAngles[i].Y;
                float pitch = PI_OVER_180 * viewAngles[i].X;

#if NET7_0_OR_GREATER
                (sinYaw[i], cosYaw[i]) = MathF.SinCos(yaw);
                (sinPitch[i], cosPitch[i]) = MathF.SinCos(pitch);
#else
                sinYaw[i] = MathF.Sin(yaw);
                cosYaw[i] = MathF.Cos(yaw);
                sinPitch[i] = MathF.Sin(pitch);
                cosPitch[i] = MathF.Cos(pitch);
#endif
            }

            // Second pass: calculate direction vectors and apply distance
            for (int i = 0; i < length; i++)
            {
                float cy = cosYaw[i];
                float sy = sinYaw[i];
                float cp = cosPitch[i];
                float sp = sinPitch[i];
                float dist = distances[i];

                results[i] = new Vector3(-cy * cp * dist, -sy * cp * dist, sp * dist);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static void CalculateThirdPersonOffsetBatch_Scalar(ReadOnlySpan<Vector3> viewAngles, ReadOnlySpan<float> distances, Span<Vector3> results)
        {
            for (int i = 0; i < viewAngles.Length; i++)
            {
                results[i] = CalculateThirdPersonOffset(viewAngles[i], distances[i]);
            }
        }

        /// <summary>
        /// Calculate movement direction from view angle and input
        /// Optimized for single calculations
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 CalculateWishDirection(Vector3 viewAngleDeg, float moveX, float moveY)
        {
            if (moveX == 0f && moveY == 0f) 
                return Vector3.Zero;

            float yaw = PI_OVER_180 * viewAngleDeg.Y;
            float pitch = PI_OVER_180 * viewAngleDeg.X;

#if NET7_0_OR_GREATER
            (float sy, float cy) = MathF.SinCos(yaw);
            (float sp, float cp) = MathF.SinCos(pitch);
#else
            float sy = MathF.Sin(yaw);
            float cy = MathF.Cos(yaw);
            float sp = MathF.Sin(pitch);
            float cp = MathF.Cos(pitch);
#endif

            // Calculate right and forward vectors
            var right = new Vector3(-sy, cy, 0f);
            var forward = new Vector3(cy * cp, sy * cp, sp);

            var wishDir = right * moveX + forward * moveY;
            return NormalizeFast(wishDir);
        }

        /// <summary>
        /// Transform multiple local offsets to world positions
        /// Optimized for coordinate transformations
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void TransformLocalOffsetsToWorld(
            Vector3 origin, Vector3 forward, Vector3 right, Vector3 up,
            ReadOnlySpan<Vector3> localOffsets, Span<Vector3> worldPositions)
        {
            if (localOffsets.Length != worldPositions.Length)
                throw new ArgumentException("Spans must have the same length");

            // Cache basis vectors for repeated use
            var fx = forward.X; var fy = forward.Y; var fz = forward.Z;
            var rx = right.X; var ry = right.Y; var rz = right.Z;
            var ux = up.X; var uy = up.Y; var uz = up.Z;
            var ox = origin.X; var oy = origin.Y; var oz = origin.Z;

            for (int i = 0; i < localOffsets.Length; i++)
            {
                var offset = localOffsets[i];
                
                // Transform: world = origin + right * local.X + up * local.Y + forward * local.Z
                worldPositions[i] = new Vector3(
                    ox + rx * offset.X + ux * offset.Y + fx * offset.Z,
                    oy + ry * offset.X + uy * offset.Y + fy * offset.Z,
                    oz + rz * offset.X + uz * offset.Y + fz * offset.Z
                );
            }
        }

        /// <summary>
        /// Create basis vectors from view angles
        /// Cached calculation for frame-based operations
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CalculateBasisVectors(Vector3 viewAngleDeg, out Vector3 forward, out Vector3 right, out Vector3 up)
        {
            float yaw = PI_OVER_180 * viewAngleDeg.Y;
            float pitch = PI_OVER_180 * viewAngleDeg.X;

#if NET7_0_OR_GREATER
            (float sy, float cy) = MathF.SinCos(yaw);
            (float sp, float cp) = MathF.SinCos(pitch);
#else
            float sy = MathF.Sin(yaw);
            float cy = MathF.Cos(yaw);
            float sp = MathF.Sin(pitch);
            float cp = MathF.Cos(pitch);
#endif

            forward = new Vector3(cy * cp, sy * cp, sp);
            right = new Vector3(-sy, cy, 0f);
            up = Vector3.Cross(right, forward);
        }
    }
}
