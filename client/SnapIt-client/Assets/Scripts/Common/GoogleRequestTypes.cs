using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class VisionRequests {
    public List<VisionRequest> requests;
}
[System.Serializable]
public class VisionRequest {
    public Image image;
    public List<Feature> features;
}
[System.Serializable]
public class Image {
    public string content;  // base64 인코딩된 이미지 데이터
}

[System.Serializable]
public class Feature {
    public string type;
    public int maxResults;
}


[System.Serializable]
public class VisionResponse {
    public Response[] responses;
}
[System.Serializable]
public class Response {
    public LocalizedObjectAnnotation[] localizedObjectAnnotations;
}

[System.Serializable]
public class LocalizedObjectAnnotation {
    public string name;
    public float score;
    public BoundingPoly boundingPoly;
}
[System.Serializable]
public class BoundingPoly {
    public NormalizedVertex[] normalizedVertices;
}
[System.Serializable]
public class NormalizedVertex {
    public float x;
    public float y;
}