using Unity.Entities;
using UnityEngine;

public struct ECSBound : IComponentData
{
    public float boundSpeed;
}

public class BoundAuthoring : MonoBehaviour
{
    public float _boundSpeed = 1;

    class Baker : Baker<BoundAuthoring>
    {
        public override void Bake(BoundAuthoring bdc)
        {
            var data = new ECSBound() { boundSpeed = bdc._boundSpeed };
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), data);
        }
    }
}
