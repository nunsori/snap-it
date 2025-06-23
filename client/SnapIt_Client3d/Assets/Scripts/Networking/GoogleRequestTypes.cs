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
public class NormalizedVertex
{
    public float x;
    public float y;
}



[System.Serializable]
public class CandidateResponse
{
    public Candidate[] candidates;
}

[System.Serializable]
public class Candidate
{
    public Content content;
}

[System.Serializable]
public class Content
{
    public Part[] parts;
}

[System.Serializable]
public class Part
{
    public string text;
}

[System.Serializable]
public class ObjectInfo
{
    public string ObjectNameEng;
    public string ObjectNameKor;
}

[System.Serializable]
public class ObjectWrapper
{
    public ObjectInfo[] objects;
}