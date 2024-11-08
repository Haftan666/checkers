using System.Collections;
using UnityEngine;

public static class CameraController
{
    public static IEnumerator RotateCamera(float duration, bool isWhiteTurn)
    {
        Quaternion startRotation = Camera.main.transform.rotation;
        Quaternion endRotation = isWhiteTurn ? startRotation * Quaternion.Euler(0, 0, 180) : startRotation * Quaternion.Euler(0, 0, -180);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Camera.main.transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.rotation = endRotation;
    }
}
