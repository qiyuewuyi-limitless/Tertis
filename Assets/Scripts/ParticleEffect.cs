using UnityEngine;

public class ParticleEffect : MonoBehaviour
{
    public ParticleSystem particle;
    private void Awake()
    {
        particle.Stop();
    }
    public void ApplyEffect(int pos)
    {
        Vector3 curPos = particle.transform.localPosition;
        particle.transform.localPosition = new Vector3(curPos.x, pos, 0);
        particle.Play();
    }
}
