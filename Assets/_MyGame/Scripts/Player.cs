using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace MyGame
{
    //Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
    public class Player : MovingObject
    {
        public int pointsPerFood = 10;              //Number of points to add to player food points when picking up a food object.
        public int pointsPerSoda = 20;              //Number of points to add to player food points when picking up a soda object.
        public int wallDamage = 1;                  //How much damage a player does to a wall when chopping it.


        private Animator animator;                  //Used to store a reference to the Player's animator component.

        public AudioClip moveSound1;                //1 of 2 Audio clips to play when player moves.
        public AudioClip moveSound2;                //2 of 2 Audio clips to play when player moves.
        public AudioClip eatSound1;                 //1 of 2 Audio clips to play when player collects a food object.
        public AudioClip eatSound2;                 //2 of 2 Audio clips to play when player collects a food object.
        public AudioClip drinkSound1;               //1 of 2 Audio clips to play when player collects a soda object.
        public AudioClip drinkSound2;               //2 of 2 Audio clips to play when player collects a soda object.

        public GameObject popupTextPrefab;
        public GameObject gainTextPrefab;

        //Start overrides the Start function of MovingObject
        protected override void Start()
        {
            //Get a component reference to the Player's animator component
            animator = GetComponent<Animator>();

            //Call the Start function of the MovingObject base class.
            base.Start();
        }


        //This function is called when the behaviour becomes disabled or inactive.
        private void OnDisable()
        {
            // called when reload scene
        }


        private void Update()
        {
            // GameManager will trigger Player's move
        }

        public bool MovePlayer(int xDir, int yDir)
        {
            if (xDir * this.transform.localScale.x < 0)
            {
                // move backward, change flip renderer
                Vector3 localScale = this.transform.localScale;
                localScale.x = -localScale.x;
                this.transform.localScale = localScale;
            }

            //Every time player moves, subtract from food points total.
            UpdateFood(-1);

            HitCallback<Wall> hitTargetCB = (Wall wall) =>
            {
                wall.DamageWall(wallDamage);
                //Set the attack trigger of the player's animation controller in order to play the player's attack animation.
                animator.SetTrigger("Chop");
            };

            if (AttemptMoveToTargetT<Wall>(xDir, yDir, hitTargetCB))
                return true;

            // just move
            RaycastHit2D hit;
            bool canMove = Move(xDir, yDir, out hit);
            if (canMove)
                SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);

            return canMove;
        }

        private void UpdateFood(int delta, GameObject objectToAction = null)
        {
            GameManager.instance.UpdateFood(delta);

            if (delta < -1)
            {
                // pop up health loss
                // todo: save canvas at very first
                PopupText popupText = Instantiate(popupTextPrefab).GetComponent<PopupText>();
                GameObject canvas = GameObject.Find("Canvas");

                Vector2 screenPosition = Camera.main.WorldToScreenPoint(transform.position);

                // todo: add radom to position
                popupText.transform.SetParent(canvas.transform, false);
                popupText.transform.position = screenPosition;
                popupText.content.text = (-delta).ToString();
            }
            else if (delta > 0)
            {
                GainText text = Instantiate(gainTextPrefab).GetComponent<GainText>();
                GameObject canvas = GameObject.Find("Canvas");

                Vector2 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
                if (objectToAction != null) {
                    screenPosition = Camera.main.WorldToScreenPoint(objectToAction.transform.position);
                }

                // todo: add radom to position
                text.transform.SetParent(canvas.transform, false);
                text.transform.position = screenPosition;
                text.content.text = "+" + delta;
            }
        }

        //OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
        private void OnTriggerEnter2D(Collider2D other)
        {
            //Check if the tag of the trigger collided with is Exit.
            if (other.tag == "Exit")
            {
                GameManager.instance.onPlayerEnterExit();
            }

            //Check if the tag of the trigger collided with is Food.
            else if (other.tag == "Food")
            {
                //Add pointsPerFood to the players current food total.
                UpdateFood(pointsPerFood, other.gameObject);

                //Disable the food object the player collided with.
                other.gameObject.SetActive(false);
                SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
            }

            //Check if the tag of the trigger collided with is Soda.
            else if (other.tag == "Soda")
            {
                //Add pointsPerSoda to players food points total
                UpdateFood(pointsPerFood, other.gameObject);

                //Disable the soda object the player collided with.
                other.gameObject.SetActive(false);
                SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
            }
        }

        //LoseFood is called when an enemy attacks the player.
        //It takes a parameter loss which specifies how many points to lose.
        public void LoseFood(int loss)
        {
            //Set the trigger for the player animator to transition to the playerHit animation.
            animator.SetTrigger("Hit");
            UpdateFood(-loss);
        }

    }
}
