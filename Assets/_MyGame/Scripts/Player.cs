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

        //AttemptMove overrides the AttemptMove function in the base class MovingObject
        //AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
        public override bool AttemptMoveWithCallback<T>(int xDir, int yDir, CallbackDelegate callback)
        {
            if (xDir * this.transform.localScale.x < 0)
            {
                // move backward, change flip renderer
                Vector3 localScale = this.transform.localScale;
                localScale.x = -localScale.x;
                this.transform.localScale = localScale;
            }

            //Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
            bool canMove = base.AttemptMoveWithCallback<T>(xDir, yDir, callback);
            if (canMove) {
                SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
            }

            //Every time player moves, subtract from food points total.
            GameManager.instance.UpdateFood(-1);
            return canMove;
        }

        //OnCantMove overrides the abstract function OnCantMove in MovingObject.
        //It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
        protected override void OnCantMove<T>(T component)
        {
            //Set hitWall to equal the component passed in as a parameter.
            Wall hitWall = component as Wall;

            //Call the DamageWall function of the Wall we are hitting.
            hitWall.DamageWall(wallDamage);

            //Set the attack trigger of the player's animation controller in order to play the player's attack animation.
            animator.SetTrigger("Chop");
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
                GameManager.instance.UpdateFood(pointsPerFood);

                //Disable the food object the player collided with.
                other.gameObject.SetActive(false);
                SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
            }

            //Check if the tag of the trigger collided with is Soda.
            else if (other.tag == "Soda")
            {
                //Add pointsPerSoda to players food points total
                GameManager.instance.UpdateFood(pointsPerSoda);

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
            GameManager.instance.UpdateFood(-loss);
        }

    }
}
