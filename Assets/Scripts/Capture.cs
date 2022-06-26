using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using UnityEditor.Media;

public class Capture
{
  // width and height for one eye
  int width;
  int height;
  int frameNum;
  int sampleFramesPerVideoFrame;
  MediaEncoder encoder;
  float[] songSamples;

  // Start is called before the first frame update
  public Capture(string outputPath, string audioResourcePath)
  {
    frameNum = 0;
    AudioClip song = Resources.Load<AudioClip>(audioResourcePath);
    Debug.Log(song);
    Debug.Log(audioResourcePath);
    songSamples = new float[song.samples * song.channels];
    song.GetData(songSamples, 0);

    width = 1024;
    height = 1024;
    var videoAttr = new VideoTrackAttributes
    {
      frameRate = new MediaRational(60),
      width = (uint)width,
      height = (uint)height * 2,
      includeAlpha = false
    };

    var audioAttr = new AudioTrackAttributes
    {
      sampleRate = new MediaRational(song.frequency),
      channelCount = 2,
      language = "fr"
    };

    sampleFramesPerVideoFrame = audioAttr.channelCount *
        audioAttr.sampleRate.numerator / videoAttr.frameRate.numerator;

    File.Delete(outputPath);

    encoder = new MediaEncoder(outputPath, videoAttr, audioAttr);
  }

  public void update(Camera camera)
  {
    var cubemapLeftEye = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
    cubemapLeftEye.dimension = TextureDimension.Cube;
    var cubemapRightEye = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
    cubemapRightEye.dimension = TextureDimension.Cube;
    var equirect = new RenderTexture(width, height * 2, 24, RenderTextureFormat.ARGB32);

    camera.stereoSeparation = 0.064f; // Eye separation (IPD) of 64mm.
    camera.RenderToCubemap(cubemapLeftEye, 63, Camera.MonoOrStereoscopicEye.Left);
    camera.RenderToCubemap(cubemapRightEye, 63, Camera.MonoOrStereoscopicEye.Right);
    cubemapLeftEye.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Left);
    cubemapRightEye.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Right);

    encoder.AddFrame(toTexture2D(equirect));
    var audioBuffer = new NativeArray<float>(sampleFramesPerVideoFrame, Allocator.Temp);
    for (int i = 0; i < sampleFramesPerVideoFrame; i++)
    {
      var songIndex = frameNum * sampleFramesPerVideoFrame + i;
      if (songIndex < songSamples.Length)
      {
        audioBuffer[i] = songSamples[songIndex];
      }
    }
    encoder.AddSamples(audioBuffer);

    frameNum += 1;
  }

  Texture2D toTexture2D(RenderTexture rTex)
  {
    Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);
    // ReadPixels looks at the active RenderTexture.
    RenderTexture.active = rTex;
    tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
    tex.Apply();
    return tex;
  }

  public void OnApplicationQuit()
  {
    encoder.Dispose();
  }
}
