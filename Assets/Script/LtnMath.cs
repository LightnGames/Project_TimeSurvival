using UnityEngine;

public class LtnMath
{

    /// [0, 360] �͈̔͂ɐ��K��
    public static float NormalizeAngle360(float value) {
        return Mathf.Repeat(value, 360f);
    }

    /// [-180 - 180] �͈̔͂ɐ��K��
    public static float NormalizeAngle180(float value) {
        value = NormalizeAngle360(value);
        if (value > 180f) {
            value -= 360f;
        }
        return value;
    }
}
