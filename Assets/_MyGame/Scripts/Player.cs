using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;      //Allows us to use SceneManager
using UnityEngine.UI;

namespace MyGame
{
    //Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
    public class Player : MovingObject
    {
        public float restartLevelDelay = 1f;        //Delay time in seconds to restart level.
        public int pointsPerFood = 10;              //Number of points to add to player food points when picking up a food object.
        public int pointsPerSoda = 20;              //Number of points to add to player food points when picking up a soda object.
        public int wallDamage = 1;                  //How much damage a player does to a wall when chopping it.


        public Text foodText;                       //UI Text to display current player food total.
        private Animator animator;                  //Used to store a reference to the Player's animator component.
        private int food;                           //Used to store player food points total during level.

        public AudioClip moveSound1;                //1 of 2 Audio clips to play when player moves.
        public AudioClip moveSound2;                //2 of 2 Audio clips to play when player moves.
        public AudioClip eatSound1;                 //1 of 2 Audio clips to play when player collects a food object.
        public AudioClip eatSound2;                 //2 of 2 Audio clips to play when player collects a food object.
        public AudioClip drinkSound1;               //1 of 2 Audio clips to play when player collects a soda object.
        public AudioClip drinkSound2;               //2 of 2 Audio clips to play when player collects a soda object.
        public AudioClip gameOverSound;             //Audio clip to play when player dies.

        //Start overrides the Start function of MovingObject
        protected override void Start()
        {
            //Get a component reference to the Player's animator component
            animator = GetComponent<Animator>();
            foodText = GameObject.Find("FoodText").GetComponent<Text>();

            //Get the current food point total stored in GameManager.instance between levels.
            food = GameManager.instance.playerFoodPoints;
            UpdateFoodText();

            //Call the Start function of the MovingObject base class.
            base.Start();
        }


        //This function is called when the behaviour becomes disabled or inactive.
        private void OnDisable()
        {
            //When Player object is disabled, store the current local food total in the GameManager so it can be re-loaded in next level.
            GameManager.instance.playerFoodPoints = food;
        }


        private void Update()
        {
            // GameManager will trigger Player's move
        }

        private void UpdateFoodText(int delta = 0)
        {
            if (delta != 0)
            {
                foodText.text = delta.ToString() + " Food: " + food;
            }
            else
            {
                foodText.text = "Food: " + food;
            }
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

            //Every time player moves, subtract from food points total.
            food--;
            UpdateFoodText();
            //Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
            bool canMove = base.AttemptMoveWithCallback<T>(xDir, yDir, callback);
            if (canMove) {
                SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
            }

            //Since the player has moved and lost food points, check if the game has ended.
            // todo: check it in GameManager in GameManager
            CheckIfGameOver();
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
                //Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
                Invoke("Restart", restartLevelDelay);

                //Disable the player object since level is over.
                enabled = false;
            }

            //Check if the tag of the trigger collided with is Food.
            else if (other.tag == "Food")
            {
                //Add pointsPerFood to the players current food total.
                food += pointsPerFood;
                UpdateFoodText(pointsPerFood);
                //Disable the food object the player collided with.
                other.gameObject.SetActive(false);
                SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
            }

            //Check if the tag of the trigger collided with is Soda.
            else if (other.tag == "Soda")
            {
                //Add pointsPerSoda to players food points total
                food += pointsPerSoda;
                UpdateFoodText(pointsPerFood);

                //Disable the soda object the player collided with.
                other.gameObject.SetActive(false);
                SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
            }
        }


        //Restart reloads the scene when called.
        private void Restart()
        {
            //Load the last scene loaded, in this case Main, the only scene in the game.
            SceneManager.LoadScene(0);
        }


        //LoseFood is called when an enemy attacks the player.
        //It takes a parameter loss which specifies how many points to lose.
        public void LoseFood(int loss)
        {
            //Set the trigger for the player animator to transition to the playerHit animation.
            animator.SetTrigger("Hit");

            //Subtract lost food points from the players total.
            food -= loss;
            UpdateFoodText(-loss);
            //Check to see if game has ended.
            CheckIfGameOver();
        }


        //CheckIfGameOver checks if the player is out of food points and if so, ends the game.
        private void CheckIfGameOver()
        {
            //Check if food point total is less than or equal to zero.
            if (food <= 0)
            {
                SoundManager.instance.PlaySingle(gameOverSound);
                SoundManager.instance.musicSource.Stop();
                //Call the GameOver function of GameManager.
                GameManager.instance.GameOver();
            }
        }
    }
}
