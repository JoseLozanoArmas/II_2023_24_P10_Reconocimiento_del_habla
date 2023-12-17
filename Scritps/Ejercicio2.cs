using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using HuggingFace.API;

public class Ejercicio2 : MonoBehaviour
{
    AudioClip clip;
    float increase = 5;
    float dicrease = 2;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) {
           StartRecording();
        }
        if (Input.GetKeyDown(KeyCode.S)) {
           StopRecording();
        }
    }

    void StartRecording() {
        clip = Microphone.Start(null, false, 10, 44100);
    }

    void StopRecording() {
        Microphone.End(null);
        byte[] wavData = EncodeAsWAV(clip);
        SendToHuggingFaceAPI(wavData);
    }

    private byte[] EncodeAsWAV(AudioClip audioClip) {
        float[] samples = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(samples, 0);

        MemoryStream stream = new MemoryStream();
        using (BinaryWriter writer = new BinaryWriter(stream)) {
            writer.Write("RIFF".ToCharArray());
            writer.Write(36 + samples.Length * 2);
            writer.Write("WAVE".ToCharArray());
            writer.Write("fmt ".ToCharArray());
            writer.Write(16);
            writer.Write((ushort)1);
            writer.Write((ushort)audioClip.channels);
            writer.Write(audioClip.frequency);
            writer.Write(audioClip.frequency * audioClip.channels * 2);
            writer.Write((ushort)(audioClip.channels * 2));
            writer.Write((ushort)16);
            writer.Write("data".ToCharArray());
            writer.Write(samples.Length * 2);
            foreach (float sample in samples) {
                short value = (short)(sample * 32767.0f);
                writer.Write(value);
            }
        }

        return stream.ToArray();
    }

    private void SendToHuggingFaceAPI(byte[] wavData) {
        HuggingFaceAPI.AutomaticSpeechRecognition(wavData,
            response =>
            {
                ProcessTextResponse(response);
            },
            error =>
            {
                Debug.LogError("Error en la API de Hugging Face: " + error);
            });
    }

    private void ProcessTextResponse(string responseText) {
        Debug.Log("Se detectó:" + responseText);
        if (responseText == " Make smaller.") {
            MakeSmaller();
        }
        if (responseText == " Make bigger.") {
            MakeBigger();
        }
    }

    private void MakeSmaller() {
        transform.localScale /= dicrease;
    }

    private void MakeBigger() {
        transform.localScale *= increase;
    }
}
