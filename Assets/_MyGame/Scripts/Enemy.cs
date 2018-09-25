using UnityEngine;
using System.Collections;

namespace MyGame
{
    //Enemy inherits from MovingObject, our base class for objects that can move, Player also inherits from this.
    public class Enemy : MovingObject
    {
        const int skipMoveTurn = 1;
        public int playerDamage;                            //The amount of food points to subtract from the player when attacking.


        private Animator animator;                          //Variable of type Animator to store a reference to the enemy's Animator component.
        private Transform target;                           //Transform to attempt to move toward each turn.
        // private bool skipMove;                              //Boolean to determine whether or not enemy should skip a turn or move this turn.
        private int enemyTurn = 0;

        public AudioClip attackSound1;                //1 of 2 Audio clips to play when player moves.
        public AudioClip attackSound2;                //2 of 2 Audio clips to play when player moves.


        //Start overrides the virtual Start function of the base class.
        protected override void Start()
        {
            //Register this enemy with our instance of GameManager by adding it to a list of Enemy objects. 
            //This allows the GameManager to issue movement commands.
            GameManager.instance.AddEnemyToList(this);

            //Get and store a reference to the attached Animator component.
            animator = GetComponent<Animator>();

            //Find the Player GameObject using it's tag and store a reference to its transform component.
            target = GameObject.FindGameObjectWithTag("Player").transform;

            //Call the start function of our base class MovingObject.
            base.Start();
        }

        private bool AttemptMove(int xDir, int yDir)
        {
            // try hit Player
            HitCallback<Player> hitTargetCB = (Player p) => {
                p.LoseFood(playerDamage);
                SoundManager.instance.RandomizeSfx(attackSound1, attackSound2);
                //Set the attack trigger of animator to trigger Enemy attack animation.
                animator.SetTrigger("enemyAttack");
            };

            if (AttemptMoveToTargetT<Player>(xDir, yDir, hitTargetCB))
                return true;

            // just move
            RaycastHit2D hit;
            return Move(xDir, yDir, out hit);
        }

        //MoveEnemy is called by the GameManger each turn to tell each Enemy to try to move towards the player.
        public void MoveEnemy()
        {
            //Check if skipMove is true, if so set it to false and skip this turn.
            if (enemyTurn++ < skipMoveTurn)
            {
                return;
            }

            enemyTurn = 0;

            //Declare variables for X and Y axis move directions, these range from -1 to 1.
            //These values allow us to choose between the cardinal directions: up, down, left and right.
            int xDir = 0;
            int yDir = 0;
            bool moved = false;

            //If the difference in positions is approximately zero (Epsilon) do the following:
            if (Mathf.Abs(target.position.x - transform.position.x) > float.Epsilon)
            {
                // enemy move left/right
                xDir = target.position.x > transform.position.x ? 1 : -1;
                yDir = 0;
                moved = AttemptMove(xDir, yDir);
            }

            if (!moved)
            {
                // enemy move up/down
                xDir = 0;
                yDir = target.position.y > transform.position.y ? 1 : -1;
                moved = AttemptMove(xDir, yDir);
            }
        }
    }
}
