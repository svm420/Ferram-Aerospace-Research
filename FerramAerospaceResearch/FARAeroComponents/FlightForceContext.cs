using UnityEngine;

namespace FerramAerospaceResearch.FARAeroComponents
{
    internal class FlightForceContext : FARAeroSection.IForceContext
    {
        public Vector3 LocalVelocity(FARAeroSection.PartData pd)
        {
            return pd.aeroModule.partLocalVel;
        }

        public void ApplyForce(FARAeroSection.PartData pd, Vector3 localVel, Vector3 forceVector, Vector3 torqueVector)
        {
            pd.aeroModule.AddLocalForceAndTorque(forceVector, torqueVector, pd.centroidPartSpace);
        }
    }
}
