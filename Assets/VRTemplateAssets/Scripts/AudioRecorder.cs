using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioRecorder : MonoBehaviour
{
    AudioSource audioSource;
    bool isRecording = false;
    AudioClip recordedClip;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void toggleAudio()
    {
        if (isRecording)
        {
            StopRecordingAndPlayBack();
        } else
        {
            StartRecording();
        }
    }

        public void StartRecording()
    {
        Debug.Log("start recording");
        if (Microphone.IsRecording(null))
        {
            return; // Already recording
        }

        // Start recording (using the first microphone)
        recordedClip = Microphone.Start(null, false, 10, 44100);
        isRecording = true;
    }

    public void StopRecordingAndPlayBack()
    {
        Debug.Log("stop recording");
        if (!isRecording)
        {
            return; // Not recording
        }

        // Stop the microphone
        Microphone.End(null);
        isRecording = false;

        // Play back the recorded audio
        audioSource.clip = recordedClip;
        audioSource.Play();
    }
}
