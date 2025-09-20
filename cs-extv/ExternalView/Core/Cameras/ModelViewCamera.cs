using LupercaliaMGCore.modules.ExternalView.Utils;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace LupercaliaMGCore.modules.ExternalView.Cameras
{
    public class ModelViewCamera : BaseFlyCamera
    {
        private Vector3? _LastRelPos = null;

        public ModelViewCamera(ICameraContext ctx) : base(ctx)
        {
        }

        protected override float CameraSpeed => Ctx.Api.ConVars.ModelViewCameraSpeed;

        protected override float AltCameraSpeed => Ctx.Api.ConVars.ModelViewCameraAltSpeed;

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override bool Update()
        {
            var cameraEntity = Ctx.CameraEntity;
            if (cameraEntity == null)
                return false;

            if (Ctx.Player.Buttons.IsPressed(CounterStrikeSharp.API.PlayerButtons.Jump))
            {
                // Exit model view camera
                Ctx.Player.PrintToChat("ExternalView.ModelView.EndedByJump");
                return false;
            }

            if (_LastRelPos == null)
            {
                // Initialize the camera position using optimized scalar calculation
                _LastRelPos = MathUtils.CalculateThirdPersonOffset(Ctx.Player.ViewAngle, Consts.ModelViewInitialCameraDistance);
            }

            var velocity = CalculateVelocity();
            var relPos = velocity * Ctx.Api.DeltaTime + _LastRelPos!.Value;

            // Restrict the movement by the specified radius using optimized scalar calculation
            var distance = relPos.Length();
            var maxRadius = Ctx.Api.ConVars.ModelViewCameraRadius;
            
            if (distance > maxRadius)
            {
                relPos = relPos * (maxRadius / distance);
            }

            _LastRelPos = relPos;

            // Update the camera position
            var eyePos = Ctx.Player.Origin + Consts.EyeOffset;
            var absPos = eyePos + relPos;
            cameraEntity.Teleport(absPos, Ctx.Player.ViewAngle, null);

            return true;
        }
    }
}
