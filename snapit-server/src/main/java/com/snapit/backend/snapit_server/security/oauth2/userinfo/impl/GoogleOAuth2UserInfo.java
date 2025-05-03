package com.snapit.backend.snapit_server.security.oauth2.userinfo.impl;

import com.snapit.backend.snapit_server.security.oauth2.userinfo.OAuth2UserInfo;

import java.util.Map;

// Google
public class GoogleOAuth2UserInfo implements OAuth2UserInfo {
    private final Map<String,Object> attrs;
    public GoogleOAuth2UserInfo(Map<String,Object> attrs) {
        this.attrs = attrs;
    }
    public String getId()    { return (String) attrs.get("sub"); }
    public String getEmail() { return (String) attrs.get("email"); }
    public String getName()  { return (String) attrs.get("name"); }
    public Map<String,Object> getAttributes() { return attrs; }
}