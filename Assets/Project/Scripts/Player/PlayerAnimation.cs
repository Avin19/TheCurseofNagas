using UnityEngine;

using CurseOfNaga.Gameplay.Managers;

namespace CurseOfNaga.Gameplay.Player
{
    public class PlayerAnimation : MonoBehaviour
    {
        [SerializeField] private GameInput gameInput;
        private Vector2 inputVector;

        private Animator animator;
        private const string isWalking = "IsWalking";
        void Start()
        {
            animator = GetComponent<Animator>();
        }
        void Update()
        {
            inputVector = gameInput.GetMovementVector();

            AnimationController(inputVector);


        }

        private void AnimationController(Vector2 input)
        {
            if (input != Vector2.zero)
            {
                animator.SetBool(isWalking, true);
                if (input.x < 0)
                {

                    transform.rotation = Quaternion.Euler(new Vector3(0f, -180f, 0f));
                }
                else
                {

                    transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));

                }

            }
            else
            {
                animator.SetBool(isWalking, false);
            }
        }



    }
}