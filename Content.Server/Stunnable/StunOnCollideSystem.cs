using Content.Server.Stunnable.Components;
using Content.Shared.Standing;
using Content.Shared.StatusEffect;
using JetBrains.Annotations;
using Robust.Shared.Physics.Dynamics;
using Content.Shared.Throwing;

namespace Content.Server.Stunnable
{
    [UsedImplicitly]
    internal sealed class StunOnCollideSystem : EntitySystem
    {
        [Dependency] private readonly StunSystem _stunSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<StunOnCollideComponent, StartCollideEvent>(HandleCollide);
            SubscribeLocalEvent<StunOnCollideComponent, ThrowDoHitEvent>(HandleThrow);
        }

        private void TryDoCollideStun(EntityUid uid, StunOnCollideComponent component, EntityUid target)
        {

            if (EntityManager.TryGetComponent<StatusEffectsComponent>(target, out var status))
            {
                StandingStateComponent? standingState = null;
                AppearanceComponent? appearance = null;

                // Let the actual methods log errors for these.
                Resolve(target, ref standingState, ref appearance, false);

                _stunSystem.TryStun(target, TimeSpan.FromSeconds(component.StunAmount), true, status);

                _stunSystem.TryKnockdown(target, TimeSpan.FromSeconds(component.KnockdownAmount), true,
                    status);

                _stunSystem.TrySlowdown(target, TimeSpan.FromSeconds(component.SlowdownAmount), true,
                    component.WalkSpeedMultiplier, component.RunSpeedMultiplier, status);
            }
        }
        private void HandleCollide(EntityUid uid, StunOnCollideComponent component, StartCollideEvent args)
        {
            TryDoCollideStun(uid, component, args.OtherFixture.Body.Owner);
        }

        private void HandleThrow(EntityUid uid, StunOnCollideComponent component, ThrowDoHitEvent args)
        {
            TryDoCollideStun(uid, component, args.Target);
        }
    }
}
