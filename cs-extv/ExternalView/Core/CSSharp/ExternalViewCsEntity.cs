using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using LupercaliaMGCore.modules.ExternalView.API;
using LupercaliaMGCore.modules.ExternalView.Utils;
using System.Drawing;
using System.Numerics;

namespace LupercaliaMGCore.modules.ExternalView.CSSharp
{
    internal class ExternalViewCsEntity : IExternalViewCsEntity
    {
        // Return nullable so callers can provide "possibly invalid" entities safely.
        public delegate CBaseEntity? GetEntityDelegate();

        private readonly GetEntityDelegate _GetEntity;

        private RenderMode_t? _Hide_LastRenderMode;
        private Color? _Hide_LastRenderColor;

        private bool TryGetEntity(out CBaseEntity entity)
        {
            entity = _GetEntity();
            if (entity == null || !entity.IsValid)
            {
                entity = null!;
                return false;
            }
            return true;
        }

        public ExternalViewCsEntity(CBaseEntity entity) : this(() => entity)
        {
        }

        public ExternalViewCsEntity(GetEntityDelegate getEntity)
        {
            _GetEntity = getEntity;
        }

        public bool IsValid
        {
            get
            {
                return TryGetEntity(out var ent);
            }
        }

        public uint HandleRaw
        {
            get
            {
                if (!TryGetEntity(out var ent))
                    return uint.MaxValue;
                return ent.EntityHandle.Raw;
            }
        }

        public Vector3 Origin
        {
            get
            {
                if (!TryGetEntity(out var ent))
                    return Vector3.Zero;
                return MathUtils.ToVector3(ent.AbsOrigin);
            }
        }

        public Vector3 Rotation
        {
            get
            {
                if (!TryGetEntity(out var ent))
                    return Vector3.Zero;
                return MathUtils.ToVector3(ent.AbsRotation);
            }
        }

        public Vector3 Velocity
        {
            get
            {
                if (!TryGetEntity(out var ent))
                    return Vector3.Zero;
                return MathUtils.ToVector3(ent.AbsVelocity);
            }
        }

        public void Teleport(Vector3? origin, Vector3? angle, Vector3? velocity)
        {
            if (!TryGetEntity(out var ent))
                return;

            ent.Teleport(
                MathUtils.ToVectorOrNull(origin),
                MathUtils.ToQAngleOrNull(angle),
                MathUtils.ToVectorOrNull(velocity)
            );
        }

        public void Remove()
        {
            if (!TryGetEntity(out var ent))
                return;

            ent.Remove();
        }

        public bool IsVisible
        {
            get => !_Hide_LastRenderMode.HasValue;
            set
            {
                if (value != _Hide_LastRenderMode.HasValue)
                    return;

                if (!TryGetEntity(out var ent))
                    return;

                var modelEntity = ent as CBaseModelEntity;
                if (modelEntity == null || !modelEntity.IsValid)
                    return;

                if (value)
                {
                    if (_Hide_LastRenderMode.HasValue && _Hide_LastRenderColor.HasValue)
                    {
                        modelEntity.RenderMode = _Hide_LastRenderMode.Value;
                        modelEntity.Render = _Hide_LastRenderColor.Value;
                    }
                    _Hide_LastRenderMode = null;
                    _Hide_LastRenderColor = null;
                }
                else
                {
                    _Hide_LastRenderMode = modelEntity.RenderMode;
                    _Hide_LastRenderColor = modelEntity.Render;

                    modelEntity.RenderMode = RenderMode_t.kRenderTransAlpha;
                    modelEntity.Render = Color.FromArgb(0, 255, 255, 255);
                }

                Utilities.SetStateChanged(modelEntity, "CBaseModelEntity", "m_clrRender");
            }
        }

        public IExternalViewCsEntity? Parent
        {
            set
            {
                if (!TryGetEntity(out var ent))
                    return;

                if (value == null)
                {
                    ent.AcceptInput("SetParent", null, null, "");
                    return;
                }

                var target = value as ExternalViewCsEntity;
                if (target == null || !target.TryGetEntity(out var targetEnt))
                    return;

                ent.AcceptInput("SetParent", targetEnt, null, "!activator");
            }
        }

        public string Model
        {
            get
            {
                if (!TryGetEntity(out var ent))
                    return "";
                return ent.CBodyComponent?.SceneNode?.GetSkeletonInstance().ModelState.ModelName ?? "";
            }
            set
            {
                if (!TryGetEntity(out var ent))
                    return;

                var modelEntity = ent as CBaseModelEntity;
                if (modelEntity == null || !modelEntity.IsValid)
                    return;

                modelEntity.SetModel(value);
            }
        }

        public bool Equals(IExternalViewCsEntity? other)
        {
            var otherEntity = other as ExternalViewCsEntity;

            if (otherEntity == null)
                return false;

            if (!TryGetEntity(out var ent) || !otherEntity.TryGetEntity(out var otherEnt))
                return false;

            return ent.Handle == otherEnt.Handle;
        }
    }
}
