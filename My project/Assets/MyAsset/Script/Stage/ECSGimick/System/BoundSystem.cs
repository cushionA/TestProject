using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct BoundSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var elapsed = (float)SystemAPI.Time.ElapsedTime;

        foreach (var (Bound, xform) in
                 SystemAPI.Query<RefRO<ECSBound>,
                                 RefRW<LocalTransform>>())
        {
            var t = Bound.ValueRO.boundSpeed * elapsed;
            var y = math.abs(math.sin(t)) * 0.1f;
            var bank = math.cos(t) * 0.5f;

            var fwd = xform.ValueRO.Forward();
            var rot = quaternion.AxisAngle(fwd, bank);
            var up = math.mul(rot, math.float3(0, 1, 0));

            xform.ValueRW.Position.y = y;
            xform.ValueRW.Rotation = quaternion.LookRotation(fwd, up);



        }
    }


    


}