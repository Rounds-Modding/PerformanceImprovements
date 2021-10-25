using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

namespace PerformanceImprovements.Extensions
{
    // ADD FIELDS TO PROJECTILEHIT
    [Serializable]
    public class ProjectileHitAdditionalData
    {

        public float lastCheckedTime;

        public ProjectileHitAdditionalData()
        {
            lastCheckedTime = -1f;
        }
    }
    public static class ProjectileHitExtension
    {
        public static readonly ConditionalWeakTable<ProjectileHit, ProjectileHitAdditionalData> data =
            new ConditionalWeakTable<ProjectileHit, ProjectileHitAdditionalData>();

        public static ProjectileHitAdditionalData GetAdditionalData(this ProjectileHit projHit)
        {
            return data.GetOrCreateValue(projHit);
        }

        public static void AddData(this ProjectileHit projHit, ProjectileHitAdditionalData value)
        {
            try
            {
                data.Add(projHit, value);
            }
            catch (Exception) { }
        }
    }
}
