using UnityEngine;
using SmallHedge.SoundManager;
public class AnimationSoundEvents : MonoBehaviour
{
    // Called from an Animation Event — the string parameter lets you specify
    // which sound to play without needing a separate method per sound.
    public void PlaySoundEvent(string soundName)
    {
        if (System.Enum.TryParse(soundName, out SoundType sound))
        {
            SoundManager.PlaySound(sound);
        }
        else
        {
            Debug.LogWarning($"AnimationSoundEvents: no SoundType matches '{soundName}'");
        }
    }

    // Alternative: a strongly-typed version if you'd rather drag-select the
    // enum value in the Animation Event dropdown instead of typing a string
    public void PlaySound(SoundType sound)
    {
        SoundManager.PlaySound(sound);
    }
}
