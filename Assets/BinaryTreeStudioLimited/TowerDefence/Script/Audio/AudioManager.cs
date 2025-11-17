using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefence
{
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("BGM")]
        [SerializeField] private List<AudioSource> BGMList = new List<AudioSource>();

        [Header("Sound Effect")]
        [SerializeField] private AudioSource gameStart;
        [SerializeField] private AudioSource gameOver;
        [SerializeField] private AudioSource gameWin;
        [SerializeField] private AudioSource normalHit;
        [SerializeField] private AudioSource criticalHit;
        [SerializeField] private AudioSource playerHurt;
        [SerializeField] private AudioSource fireBall;

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
        public void PlayGameStartAudio() => gameStart.Play();
        public void PlayGameOverAudio() => gameOver.Play();
        public void PlayGameWinAudio() => gameWin.Play();
        public void PlayNormalHitAudio() => normalHit.Play();
        public void PlayCriticalHitAudio() => criticalHit.Play();
        public void PlayPlayerHurtAudio() => playerHurt.Play();
        public void PlayFireBallAudio() => fireBall.Play();

        #endregion

    }
}
