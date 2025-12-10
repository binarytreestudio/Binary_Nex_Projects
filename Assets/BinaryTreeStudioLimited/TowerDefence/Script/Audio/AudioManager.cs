using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
public class AudioManager : Singleton<AudioManager>
{
    [Header("BGM")]
    [SerializeField] private List<AudioSource> BGMList = new List<AudioSource>();

    [Header("Sound Effect")]
    [Header("System")]
    [SerializeField] private AudioSource gameStart;
    [SerializeField] private AudioSource gameOver;
    [SerializeField] private AudioSource gameWin;
    [SerializeField] private AudioSource normalHit;
    [SerializeField] private AudioSource criticalHit;
    [SerializeField] private AudioSource playerHurt;
    [SerializeField] private AudioSource collectPowerup;

    [Header("Attack")]
    [SerializeField] private AudioSource fireBall;
    [SerializeField] private AudioSource iceBall;
    [SerializeField] private AudioSource iceArea;
    [SerializeField] private AudioSource poisonBall;
    [SerializeField] private AudioSource poisonArea;
    [SerializeField] private AudioSource rockBall;

    #region BGM

    public void PlayBGM(int num)
    {
        foreach (AudioSource source in BGMList)
        {
            source.Stop();
        }
        BGMList[num].Play();
    }

    #endregion

    #region SFX

    #region System
    public void PlayGameStartAudio() => gameStart.Play();
    public void PlayGameOverAudio() => gameOver.Play();
    public void PlayGameWinAudio() => gameWin.Play();
    public void PlayNormalHitAudio() => normalHit.Play();
    public void PlayCriticalHitAudio() => criticalHit.Play();
    public void PlayPlayerHurtAudio() => playerHurt.Play();
    public void PlayCollectPowerup() => collectPowerup.Play();

    #endregion

    #region Attack
    public void PlayFireBallAudio() => fireBall.Play();
    public void PlayIceBallAudio() => iceBall.Play();
    public void PlayIceAreaAudio() => iceArea.Play();
    public void PlayPoisonBallAudio() => poisonBall.Play();
    public void PlayPoisonAreaAudio() => poisonArea.Play();
    public void PlayRockBallAudio() => rockBall.Play();

    #endregion

    #endregion

}

