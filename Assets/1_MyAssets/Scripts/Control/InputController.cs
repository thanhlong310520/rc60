using UnityEngine;

namespace Raccoon.InputCtr
{

    public class InputController : MonoBehaviour
    {
        public static InputController instance;


        CameraInput cameraInput = new CameraInput();
        PlayerInput playerInput = new PlayerInput();    

        [SerializeField] bool canRotateCamera = true;
        [SerializeField] bool canUseInput = true;
        public CameraInput CameraInput
        {
            get { return cameraInput; }
        }

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

        private void Start()
        {
            cameraInput = new CameraInput();
            playerInput = new PlayerInput();
            SetCanRotateCamera(true);
        }

        private void Update()
        {
            if (!canUseInput) return;
            EventBus.Publish(playerInput);

            if (!canRotateCamera) return;
            EventBus.Publish(cameraInput);
        }


        public void SetCanUseInput(bool canUse)
        {
            canUseInput = canUse;
            if (!canUseInput)
            {
                cameraInput.Reset();
                playerInput.Reset();
            }
            EventBus.Publish(cameraInput);
            EventBus.Publish(playerInput);

        }
        public void UpdateInputCamera(float mouseX , float mouseY)
        {
            cameraInput.mouseX = mouseX;
            cameraInput.mouseY = mouseY;
        }
        public void SetCanRotateCamera(bool canRotate)
        {
            canRotateCamera = canRotate;
            if (!canRotateCamera)
            {
                cameraInput.Reset();
            }
        }
        public void UpdateMovePlayer(Vector2 value)
        {
            playerInput.horizontalAxis = value.x;
            playerInput.verticalAxis = value.y;
        }
        public void SetPlayerAttack(bool isAttack)
        {
            playerInput.isAttack = isAttack;
        }
        public void SetPlayerJump(bool isJump)
        {
            playerInput.isJump = isJump;
        }
        public void SetIsRunning(bool isRunning)
        {
            playerInput.isRunning = isRunning;
        }
        public void SetIsDropBox()
        {
            playerInput.isDropBox = true;
        }
        public bool GetIsRunning()
        {
            return playerInput.isRunning;
        }
    }

    [System.Serializable]
    public class CameraInput
    {
        public float mouseX;
        public float mouseY;
        public float mouseScrollWheel;

        public CameraInput()
        {
            mouseX = 0;
            mouseY = 0;
            mouseScrollWheel = 0;
        }  
        
        public void Reset()
        {
            mouseX = 0;
            mouseY = 0;
            mouseScrollWheel = 0;
        }
    }

    [System.Serializable]   
    public class PlayerInput
    {
        public float horizontalAxis;
        public float verticalAxis;
        public bool isRunning;
        public bool isJump;
        public bool isAttack;
        public bool isDropBox;
        public PlayerInput()
        {
            horizontalAxis = 0;
            verticalAxis = 0;
            isRunning = false;
            isJump = false;
            isAttack = false;
            isDropBox = false;
        }
        public void Reset()
        {
            horizontalAxis = 0;
            verticalAxis = 0;
            isAttack = false;
            isJump = false;
            isDropBox = false;
        }
    }
}