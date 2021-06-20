using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip TitleBGM = null;
    [SerializeField] private AudioClip ShopBGM = null;
    [SerializeField] private AudioClip[] StageBGM = null;
    [SerializeField] private AudioClip[] SystemSE = null;
    [SerializeField] private AudioClip[] StageSE = null;
    [SerializeField] private AudioClip[] LoopSE = null;
    private AudioSource SEPlayer;
    private AudioSource BGMPlayer;
    private AudioSource LoopSEPlayer;
    private void Awake()
    {
        AudioSource[] auds = GetComponents<AudioSource>();
        BGMPlayer = auds[0];
        LoopSEPlayer = auds[1];
        SEPlayer = auds[2];
    }

   // private int playSECount = 0;

    /// <summary>
    /// タイトルBGMの再生
    /// </summary>
    public void PlayTitleBGM()
    {
        if (BGMPlayer.isPlaying) BGMPlayer.Stop();
        BGMPlayer.clip = TitleBGM;
        BGMPlayer.Play();
    }

    /// <summary>
    /// ショップBGMの再生
    /// </summary>
    public void PlayShopBGM()
    {
        if (BGMPlayer.isPlaying) BGMPlayer.Stop();
        BGMPlayer.clip = ShopBGM;
        BGMPlayer.Play();
    }

    /// <summary>
    /// ステージのBGMを再生
    /// </summary>
    /// <param name="num"></param>
    public void PlayStageBGM(int num)
    {
        if (BGMPlayer.isPlaying) BGMPlayer.Stop();
        BGMPlayer.clip = StageBGM[num];
        BGMPlayer.Play();
    }

    /// <summary>
    /// BGMの停止
    /// </summary>
    public void BGMStop()
    {
        if (BGMPlayer.isPlaying) BGMPlayer.Stop();
    }

    public void BGMReStart()
    {
        BGMPlayer.Play();
    }
    
    /// <summary>
    /// システムSEの再生
    /// </summary>
    /// <param name="num"></param>
    public void PlaySystemSE(int num)
    {
        SEPlayer.PlayOneShot(SystemSE[num]);
    }

    /// <summary>
    /// ステージSEの再生
    /// </summary>
    /// <param name="num"></param>
    public void PlayStageSE(int num)
    {
        SEPlayer.PlayOneShot(StageSE[num]);
    }

    /// <summary>
    /// ループさえる効果音の再生
    /// </summary>
    /// <param name="num"></param>
    public void PlayLoopSE(int num)
    {
        if (LoopSEPlayer.isPlaying) LoopSEPlayer.Stop();
        LoopSEPlayer.clip = LoopSE[num];
        LoopSEPlayer.Play();
    }

    /// <summary>
    /// ループさせてるSEの停止
    /// </summary>
    public void StopLoopSE() 
    {
        if (LoopSEPlayer.isPlaying) LoopSEPlayer.Stop();
    }

    /// <summary>
    /// BGMの音量を変更
    /// </summary>
    public void BGMValumeChange()
    {
        BGMPlayer.volume = GameManager.eventCount[5] / 100.0f;
    }

    /// <summary>
    /// SEの音量を変更
    /// </summary>
    public void SEValumeChange()
    {
        LoopSEPlayer.volume = GameManager.eventCount[6] / 100.0f;
        SEPlayer.volume = GameManager.eventCount[6] / 100.0f;
    }

    public AudioSource GetBGMAudioSource()
    {
        return BGMPlayer;
    }

}
