using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class clip_set
{
    public string clip_name;
    public AudioClip clip;

}

public class SoundController : MonoBehaviour
{
    public static SoundController Instance;

    [Header("BGM")]
    public clip_set[] BGM_Clips;

    [Header("Effect")]
    public clip_set[] Effect_Clips;

    [Header("audio_emit")]
    public AudioSource BGM;
    public AudioSource[] Effects;

    [Header("Global Setting")]
    public float BGM_Volume = 1f;
    public float Effect_Volume = 1f;

    //private bool temp = false;

    private void Awake()
    {
        //singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void Play_BGM(string name, float volume, bool is_loop)
    {
        for(int i =0; i<BGM_Clips.Length; i++)
        {
            if (BGM_Clips[i].clip_name == name)
            {
                BGM.clip = BGM_Clips[i].clip;
                //temp = true;
                BGM.volume = volume;
                BGM.loop = is_loop;
                BGM.Play();
                return;
            }
        }

        Debug.Log("not narrtion : " + name);

        return;
    }

    public void Play_Effect(string name, float volume, bool is_loop)
    {
        for(int i =0; i<Effect_Clips.Length; i++)
        {
            if (Effect_Clips[i].clip_name == name)
            {
                for(int j =0; j<Effects.Length; j++)
                {
                    if (Effects[j].isPlaying)
                    {
                        continue;
                    }
                    else
                    {
                        Effects[j].clip = Effect_Clips[i].clip;
                        Effects[j].volume = volume;
                        Effects[j].loop = is_loop;
                        Effects[j].Play();

                        return;
                    }
                }
            }
        }


        Debug.Log("not effect : " + name + "\n or all effects are playing");
    }


    public void change_sound()
    {
        // BGM.volume = save_load_Data.Instance.play_data.BGM_Volume;
        // for(int i =0; i<Effects.Length; i++)
        // {
        //     Effects[i].volume = save_load_Data.Instance.play_data.Narr_Volume;
        // }
    }
}
