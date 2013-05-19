using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Particles
{
    public enum ParticleDeathMode
    {
        Seconds,
        Distance
    }

    public enum ParticleRandType
    {
        Gaussian,
        Uniform,
        Constant
    }

    public enum PrebuiltEmitter
    {
        RocketBoostFire,
        BurstBoostFire,
        IdleBoostFire,
        EnemyDeathExplosion,
        BulletHitExplosion,
        SmallBulletSparks,
        MediumBulletSparks,
        LargeBulletSparks,
        DrillSparks,
        CollectedSparks,
        PrisonerSparks,
        ChargingSparks,
        BlinkOriginSparks,
        BlinkEndSparks,
        FlameFire,
    }

    public enum PrebuiltLineEmitter
    {
        FlamethrowerFire,
        RiotGuardWallDrillSparks,
        FireChainsFire,
    }
}
