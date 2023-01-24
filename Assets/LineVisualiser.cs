using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


public class TrackedPoint
{
    public float _recordedTime;
    public Vector3 _trackedPosition;
    public float _velocity;
}

// adapted from https://stackoverflow.com/a/55148163

[RequireComponent(typeof(LineRenderer))]
public class LineVisualiser : MonoBehaviour
{
    public string csvFileLocation;
    public float maxVelocity;
    private LineRenderer _lineRenderer;
    private List<Vector3> _recordedPositions;
    private List<TrackedPoint> _trackedPoints;
    private float _recordingDuration;
    
    // Start is called before the first frame update
    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _trackedPoints = new List<TrackedPoint>();
        _recordedPositions = new List<Vector3>();
        
        var fileContent = "";
        var reader = new StreamReader(csvFileLocation);
        fileContent = reader.ReadToEnd();
        
        var lines = fileContent.Split("\n");

        foreach(var line in lines)
        {
            string[] parts = SplitCsvLine(line);

            if (parts.Length >= 4)
            {
                float t = float.TryParse(parts[0],System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out t) ? t : 0;
                float x = float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out x) ? x : 0;
                float y = float.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out y) ? y : 0;
                float z = float.TryParse(parts[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out z) ? z : 0;

                TrackedPoint trackedPoint = new TrackedPoint();
                trackedPoint._recordedTime = t;
                trackedPoint._trackedPosition = new Vector3(x, y, z);
                if (t > _recordingDuration)
                    _recordingDuration = t;
                _trackedPoints.Add(trackedPoint);
            }
        }

        for (int i = 0; i < _trackedPoints.Count - 1; ++i)
        {
            float posDif = (_trackedPoints[i + 1]._trackedPosition - _trackedPoints[i]._trackedPosition).magnitude;
            float timeDif = Mathf.Abs(_trackedPoints[i + 1]._recordedTime - _trackedPoints[i]._recordedTime);
            _trackedPoints[i]._velocity = posDif / timeDif;
        }
        
        foreach (var trackedPoint in _trackedPoints)
        {
            _recordedPositions.Add(trackedPoint._trackedPosition);
        }

        _lineRenderer.positionCount = _recordedPositions.Count;
        _lineRenderer.SetPositions(_recordedPositions.ToArray());
        Gradient colorGradient = new Gradient();
        GradientColorKey[] colorKey = new GradientColorKey[_trackedPoints.Count];

        for(int i = 0; i < _trackedPoints.Count; ++i)
        {
            float velocity = _trackedPoints[i]._velocity / maxVelocity;
            Color color = new Color(velocity, 0.0f, 0.0f);

            colorKey[i].color = color;
            colorKey[i].time = _trackedPoints[i]._recordedTime / _recordingDuration;
        }

        colorGradient.colorKeys = colorKey;
        _lineRenderer.colorGradient = colorGradient;

        _lineRenderer.numCapVertices = 10;
        _lineRenderer.numCornerVertices = 10;
    }
    
    private static string[] SplitCsvLine(string line)
    {
        string[] split = line.Split(";");
        for (int i = 0; i < split.Length; ++i)
        {
            split[i] = split[i].Replace("\r", "");
        }
        return split;
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
