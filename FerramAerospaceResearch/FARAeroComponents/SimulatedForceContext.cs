using System;
using ferram4;
using UnityEngine;

namespace FerramAerospaceResearch.FARAeroComponents
{
    internal class SimulatedForceContext : FARAeroSection.IForceContext
    {
        /// <summary>
        /// The world-space velocity of the part whose force is being simulated
        /// </summary>
        private Vector3 worldVel;

        /// <summary>
        /// The center with which force should be accumulated
        /// </summary>
        private FARCenterQuery center;

        /// <summary>
        /// The atmospheric density that the force is being simulated at
        /// </summary>
        private float atmDensity;

        public SimulatedForceContext(Vector3 worldVel, FARCenterQuery center, float atmDensity)
        {
            this.worldVel = worldVel;
            this.center = center;
            this.atmDensity = atmDensity;
        }

        // ReSharper disable ParameterHidesMember -> updating member values
        public void UpdateSimulationContext(Vector4 worldVel, FARCenterQuery center, float atmDensity)
        {
            this.worldVel = worldVel;
            this.center = center;
            this.atmDensity = atmDensity;
        }
        // ReSharper restore ParameterHidesMember

        public Vector3 LocalVelocity(FARAeroSection.PartData pd)
        {
            if (pd.aeroModule.part == null || pd.aeroModule.part.partTransform == null)
                return Vector3.zero;
            return pd.aeroModule.part.partTransform.InverseTransformVector(worldVel);
        }

        public void ApplyForce(FARAeroSection.PartData pd, Vector3 localVel, Vector3 forceVector, Vector3 torqueVector)
        {
            double tmp = 0.0005 * Vector3.SqrMagnitude(localVel);
            double dynamicPressurekPa = tmp * atmDensity;
            double dragFactor = dynamicPressurekPa * Mathf.Max(PhysicsGlobals.DragCurvePseudoReynolds.Evaluate(atmDensity * Vector3.Magnitude(localVel)), 1.0f);
            double liftFactor = dynamicPressurekPa;

            Vector3 localVelNorm = Vector3.Normalize(localVel);
            Vector3 localForceTemp = Vector3.Dot(localVelNorm, forceVector) * localVelNorm;
            Vector3 partLocalForce = localForceTemp * (float)dragFactor + (forceVector - localForceTemp) * (float)liftFactor;
            forceVector = pd.aeroModule.part.transform.TransformDirection(partLocalForce);
            torqueVector = pd.aeroModule.part.transform.TransformDirection(torqueVector * (float)dynamicPressurekPa);
            if (Single.IsNaN(forceVector.x) || Single.IsNaN(torqueVector.x))
                return;
            Vector3 centroid = pd.aeroModule.part.transform.TransformPoint(pd.centroidPartSpace - pd.aeroModule.part.CoMOffset);
            center.AddForce(centroid, forceVector);
            center.AddTorque(torqueVector);
        }
    }
}