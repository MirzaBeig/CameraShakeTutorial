using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeTransform : MonoBehaviour
{
    [System.Serializable]
    public class ShakeEvent
    {
        float duration;
        float timeRemaining;

        ShakeTransformEventData data;

        public ShakeTransformEventData.Target target
        {
            get
            {
                return data.target;
            }
        }

        Vector3 noiseOffset;
        public Vector3 noise;

        public ShakeEvent(ShakeTransformEventData data)
        {
            this.data = data;

            duration = data.duration;
            timeRemaining = duration;

            float rand = 32.0f;

            noiseOffset.x = Random.Range(0.0f, rand);
            noiseOffset.y = Random.Range(0.0f, rand);
            noiseOffset.z = Random.Range(0.0f, rand);
        }

        public void Update()
        {
            float deltaTime = Time.deltaTime;

            timeRemaining -= deltaTime;

            float noiseOffsetDelta = deltaTime * data.frequency;

            noiseOffset.x += noiseOffsetDelta;
            noiseOffset.y += noiseOffsetDelta;
            noiseOffset.z += noiseOffsetDelta;

            noise.x = Mathf.PerlinNoise(noiseOffset.x, 0.0f);
            noise.y = Mathf.PerlinNoise(noiseOffset.y, 1.0f);
            noise.z = Mathf.PerlinNoise(noiseOffset.z, 2.0f);

            noise -= Vector3.one * 0.5f;

            noise *= data.amplitude;

            float agePercent = 1.0f - (timeRemaining / duration);
            noise *= data.blendOverLifetime.Evaluate(agePercent);
        }
        public bool IsAlive()
        {
            return timeRemaining > 0.0f;
        }
    }

    // ...

    List<ShakeEvent> shakeEvents = new List<ShakeEvent>();

    // ...

    public void AddShakeEvent(ShakeTransformEventData data)
    {
        shakeEvents.Add(new ShakeEvent(data));
    }
    public void AddShakeEvent(float amplitude, float frequency, float duration, AnimationCurve blendOverLifetime, ShakeTransformEventData.Target target)
    {
        ShakeTransformEventData data = ShakeTransformEventData.CreateInstance<ShakeTransformEventData>();
        data.Init(amplitude, frequency, duration, blendOverLifetime, target);

        AddShakeEvent(data);
    }

    void LateUpdate()
    {
        Vector3 positionOffset = Vector3.zero;
        Vector3 rotationOffset = Vector3.zero;

        for (int i = shakeEvents.Count - 1; i != -1; i--)
        {
            ShakeEvent se = shakeEvents[i]; se.Update();
            
            if (se.target == ShakeTransformEventData.Target.Position)
            {
                positionOffset += se.noise;
            }
            else
            {
                rotationOffset += se.noise;
            }

            if (!se.IsAlive())
            {
                shakeEvents.RemoveAt(i);
            }
        }

        transform.localPosition = positionOffset;
        transform.localEulerAngles = rotationOffset;
    }
}
