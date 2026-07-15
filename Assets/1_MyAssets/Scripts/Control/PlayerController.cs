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
    public class PlayerController : MonoBehaviour
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
        public PlayerMovement movement;


        public Rigidbody rb;
        public CapsuleCollider capsule;

        public Camera mainCam;

        bool inited = false;

        private void Start()
        {
            EventBus.Subscribe<PlayerInput>(UpdateValueInput);
            movement.Init(rb, capsule);
        }

        #region ICharactor

        public void Jump()
        {
            //ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.Jump);

        }
        
        public void SetStartPoint(Vector3 startPoint)
        {
            transform.position = startPoint;
        }

        #endregion
        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerInput>(UpdateValueInput);
        }

        private void Update()
        {
            //if (!inited) return;
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
    }

    public enum EPlayerState
    {
        None,
        Normal,
        Dead,
    }


}
