using UnityEngine;

public class AdaptiveDoubleExponentialQuaternion
{
    private AdaptiveDoubleExponentialFilterFloat x;
    private AdaptiveDoubleExponentialFilterFloat y;
    private AdaptiveDoubleExponentialFilterFloat z;
    private AdaptiveDoubleExponentialFilterFloat w;

    public Quaternion Value
    {
        get { return new Quaternion(x.Value, y.Value, z.Value, w.Value); }
        set { Update(value); }
    }

    public AdaptiveDoubleExponentialQuaternion()
    {
        x = new AdaptiveDoubleExponentialFilterFloat();
        y = new AdaptiveDoubleExponentialFilterFloat();
        z = new AdaptiveDoubleExponentialFilterFloat();
        w = new AdaptiveDoubleExponentialFilterFloat();
    }

    private void Update(Quaternion v)
    {
        x.Value = v.x;
        y.Value = v.y;
        z.Value = v.z;
        w.Value = v.w;
    }
}