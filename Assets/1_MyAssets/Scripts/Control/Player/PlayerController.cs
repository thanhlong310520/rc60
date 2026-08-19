using Raccoon.InputCtr;
using Raccoon.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Raccoon.Controller
{
    public class PlayerController : MonoBehaviour, ICharactor
    {
        #region singleton
        public static PlayerController instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        EPlayerState playerState;
        public EPlayerState PlayerState { get { return playerState; } }
        
        public PlayerMovement movement;
        public PlayerAnimation playerAnimation;
        public PlayerCountTimeFall pCountTimeFall;


        public Rigidbody rb;
        public CapsuleCollider capsule;

        public Camera mainCam;
        public Transform modelHolder;
        public CharacterData data;
        public CharacterData GetCharactorData => data;
        bool inited = false;
        public bool Inited => inited;

        private void Start()
        {
            movement.Init(rb, capsule);
            //Init();

            var currentSkinSOs = GameData.Get.currentSkinSOs;
            SetModel(currentSkinSOs);
        }

        public void SetModel(List<SoSkin> soSkins)
        {

            foreach (var so in soSkins)
            {
                var go = Instantiate(so.prefab, modelHolder);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                playerAnimation.SetSkin(go);
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerInput>(UpdateValueInput);
        }
        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerInput>(UpdateValueInput);
        }

        public void Init()
        {
            inited = true;
            SetState(EPlayerState.Normal);
            movement.SetCanMove(true);
        }


        public void Jump()
        {
            ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.Jump);
        }

        private void Update()
        {
            if (!inited) return;
            SetDirForMovement();
        }



        void SetDirForMovement()
        {
            if (mainCam == null) mainCam = Camera.main;

            movement.SetDirFoward(mainCam.transform.forward);
            movement.SetDirRight(mainCam.transform.right);
        }


        public void UpdateValueInput(PlayerInput value)
        {
            if (value == null) return;
            //if (playerState == EPlayerState.Win)
            //{
            //    value.Reset();
            //    playerAnimation.OnDance(true);
            //}
            movement.SetInput(value);

        }
        public void SetStartPoint(Transform startPoint)
        {
            Debug.Log($"[PlayerController] SetStartPoint: {startPoint}");
            if (startPoint != null)
            {
                Debug.Log($"[PlayerController] SetStartPoint1: {startPoint}");
                rb.position = startPoint.position;
                rb.rotation = startPoint.rotation;
            }
            VfxCtrl.instance.SpawnRandomVfx(Raccoon.EnumHolder.TypeVFX.Spawn, startPoint.position + Vector3.up *(-1f));

        }

        public void ContactCheckPoint()
        {

        }

        public void Dead()
        {
            Debug.Log(($"[PlayerController] Dead"));
            modelHolder.gameObject.SetActive(false);
            GamePlayController.instance.ShowPopup(PopupCanvas.PopupType.Dead, data);
            movement.SetCanMove(false);
            SetState(EPlayerState.Dead);
            VfxCtrl.instance.SpawnRandomVfx(Raccoon.EnumHolder.TypeVFX.Dead, transform.position + Vector3.up);

        }

        public void ResetPlayer()
        {
            SetState(EPlayerState.Normal);
            modelHolder.gameObject.SetActive(true);
            movement.SetCanMove(true);
            pCountTimeFall.ResetTimeFall();
        }

        void SetState(EPlayerState state)
        {
            playerState = state;

        }

        public void SetWin(Vector3 dirCheckpoint)
        {
            //transform.rotation = Quaternion.LookRotation(dirCheckpoint);
            //SetState(EPlayerState.Win);
        }


        public void OnContactCoin()
        {
            ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.ContactCoin);
            PlayerData.Get.GetCharacterData().AddCoin(1);
        }
        
    }

    public enum EPlayerState
    {
        None,
        Normal,
        Dead,
        Win,
    }


}
