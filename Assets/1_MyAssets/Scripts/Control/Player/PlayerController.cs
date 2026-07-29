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
        public PlayerCountTimeFall pCountTimeFall;


        public Rigidbody rb;
        public CapsuleCollider capsule;

        public Camera mainCam;

        public CharacterData data;
        public CharacterData GetCharactorData => data;
        bool inited = false;
        public bool Inited => inited;

        private void Start()
        {
            movement.Init(rb, capsule);
            //Init();
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

        }

        public void ContactCheckPoint()
        {

        }

        public void Dead()
        {
            Debug.Log(($"[PlayerController] Dead"));
            GamePlayController.instance.ShowPopup(PopupCanvas.PopupType.Dead, data);
            movement.SetCanMove(false);
            SetState(EPlayerState.Dead);
        }

        public void ResetPlayer()
        {
            SetState(EPlayerState.Normal);
            movement.SetCanMove(true);
            pCountTimeFall.ResetTimeFall();
        }

        void SetState(EPlayerState state)
        {
            playerState = state;

        }
    }

    public enum EPlayerState
    {
        None,
        Normal,
        Dead,
    }


}
