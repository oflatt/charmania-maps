using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Playables;

public class Cubes : MonoBehaviour
{
  Capture capture;
  int frame;
  // Start is called before the first frame update
  void Start()
  {
    frame = 0;
    capture = new Capture(Path.Combine(Application.dataPath, "cube_movie.mp4"),
                          "Audio/Komiku-Last-Boss");
  }

  // Update is called once per frame
  void Update()
  {
    setAnimationTime(((float)frame) / 60.0f);
    capture.update(GetComponent<Camera>());
    frame += 1;
  }

  void OnApplicationQuit()
  {
    capture.OnApplicationQuit();
  }

  public void setAnimationTime(float time)
  {
    PlayableDirector playableDirector = GetComponent<PlayableDirector>();
    playableDirector.time = time;
    playableDirector.RebuildGraph();
    playableDirector.Play();
    playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(0);
  }
}
