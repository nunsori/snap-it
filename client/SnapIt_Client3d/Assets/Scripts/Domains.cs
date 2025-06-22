using UnityEngine;

public static class Domains
{
    private static string loginDomaink = "http://chabin37.iptime.org:32766/oauth2/authorization/";

    public static string GetLogInDomain(int type)
    {
        switch (type)
        {
            case 0: return loginDomaink + "google"; break;
            case 1: return loginDomaink + "kakao"; break;
            default : return ""; break;
        }
    }
}
