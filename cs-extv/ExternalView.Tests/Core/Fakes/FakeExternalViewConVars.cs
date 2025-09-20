using LupercaliaMGCore.modules.ExternalView.API;

namespace ExternalView.Tests.Core.Fakes
{
    internal class FakeExternalViewConVars : IExternalViewConVars
    {
        public float ThirdPersonMinDistance { get; set; } = 50;
        public float ThirdPersonMaxDistance { get; set; } = 200;
        public float ModelViewCameraSpeed { get; set; } = 160;
        public float ModelViewCameraAltSpeed { get; set; } = 40;
        public float ModelViewCameraRadius { get; set; } = 120;
        public float FreeCameraSpeed { get; set; } = 800;
        public float FreeCameraAltSpeed { get; set; } = 2400;
        public bool IsObserverViewEnabled { get; set; } = true;
        public bool IsModelViewEnabled { get; set; } = true;
        public bool IsAdminPrivilegesEnabled { get; set; } = true;
        public bool IsThirdPersonTraceBlockEnabled { get; set; } = false;
    }
}
