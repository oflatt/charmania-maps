using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Cubes : MonoBehaviour
{
  GameObject cube;
  Capture capture;
  int frame;
  // Start is called before the first frame update
  void Start()
  {
    cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    frame = 0;

    capture = new Capture(Path.Combine(Application.dataPath, "cube_movie.mp4"),
                          "Audio/Komiku-Last-Boss");
  }

  // Update is called once per frame
  void Update()
  {
    capture.update(GetComponent<Camera>());
    if ((frame / 60) % 2 == 0)
    {
      cube.transform.position += new Vector3(0.01f, 0, 0);
    }
    else
    {
      cube.transform.position += new Vector3(-0.01f, 0, 0);
    }
    frame += 1;
  }

  void OnApplicationQuit()
  {
    capture.OnApplicationQuit();
  }
}
