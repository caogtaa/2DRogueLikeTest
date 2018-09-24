using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;      //Allows us to use SceneManager

using System.Collections.Generic;       //Allows us to use Lists. 
using UnityEngine.UI;                   //Allows us to use UI.

namespace MyGame
{
    public class GameManager : GTBaseMonoBehaviour
    {
        public float levelStartDelay = 2f;                      //Time to wait before starting level, in seconds.
        public float restartLevelDelay = 1f;        //Delay time in seconds to restart level.

        public float turnDelay = 0.1f;                          //Delay between each Player turn.
        public int playerFoodPoints = 100;                      //Starting value for Player food points.
        public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.

        private bool playersTurn = true;                        //Boolean to check if it's players turn
        private bool playerMoving = false;

        private List<Enemy> enemies;                          //List of all Enemy units, used to issue them move commands.
        private bool enemiesMoving = false;                             //Boolean to check if enemies are moving.

        private Text levelText;                                 //Text to display current level number.
        private GameObject levelImage;                          //Image to block out level as levels are being set up, background for levelText.

        public Text foodText;                       //UI Text to display current player food total.

        private BoardManager boardScript;                       //Store a reference to our BoardManager which will set up the level.
        private int level = 1;                                  //Current level number, expressed in game as "Day 1".
        private Player player;
        private bool doingSetup = true;                         //Boolean to check if we're setting up board, prevent Player from moving during setup.

        public AudioClip gameOverSound;             //Audio clip to play when player dies.

        class InputMovingDir
        {
            public int horizontal;
            public int vertical;

            public bool isEmpty() {
                return horizontal == 0 && vertical == 0;
            }
        };



        //Awake is always called before any Start functions
        void Awake()
        {
            //Check if instance already exists
            if (instance == null)

                //if not, set instance to this
                instance = this;

            //If instance already exists and it's not this:
            else if (instance != this)

                //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
                Destroy(gameObject);

            //Sets this to not be destroyed when reloading scene
            DontDestroyOnLoad(gameObject);

            //Assign enemies to a new List of Enemy objects.
            enemies = new List<Enemy>();

            //Get a component reference to the attached BoardManager script
            boardScript = GetComponent<BoardManager>();

            //Call the InitGame function to initialize the first level 
            InitGame();
        }

        //This is called each time a scene is loaded.
        void OnLevelWasLoaded(int index)
        {
            //Add one to our level number.
            level++;
            //Call InitGame to initialize our level.
            InitGame();
        }

        //Initializes the game for each level.
        void InitGame()
        {
            //While doingSetup is true the player can't move, prevent player from moving while title card is up.
            doingSetup = true;

            //Get a reference to our image LevelImage by finding it by name.
            levelImage = GameObject.Find("LevelImage");

            //Get a reference to our text LevelText's text component by finding it by name and calling GetComponent.
            levelText = GameObject.Find("LevelText").GetComponent<Text>();

            //Set the text of levelText to the string "Day" and append the current level number.
            levelText.text = "Day " + level;

            //Set levelImage to active blocking player's view of the game board during setup.
            levelImage.SetActive(true);

            //Call the HideLevelImage function with a delay in seconds of levelStartDelay.
            Invoke("HideLevelImage", levelStartDelay);

            //Clear any Enemy objects in our List to prepare for next level.
            enemies.Clear();

            //Call the SetupScene function of the BoardManager script, pass it current level number.
            boardScript.SetupScene(level);

            LocateUnits();

            foodText = GameObject.Find("FoodText").GetComponent<Text>();
            UpdateFoodText();
        }

        void LocateUnits()
        {
            player = GameObject.FindWithTag("Player").GetComponent<Player>();
            // todo: locate enemies
        }


        //Hides black image used between levels
        void HideLevelImage()
        {
            //Disable the levelImage gameObject.
            levelImage.SetActive(false);

            //Set doingSetup to false allowing player to move again.
            doingSetup = false;
        }

        public void UpdateFood(int delta)
        {
            playerFoodPoints += delta;
            if (delta < 0)
                CheckIfGameOver();

            int notifyDelta = delta;
            if (delta == -1)
                notifyDelta = 0;

            UpdateFoodText(notifyDelta);
        }

        //CheckIfGameOver checks if the player is out of food points and if so, ends the game.
        private void CheckIfGameOver()
        {
            //Check if food point total is less than or equal to zero.
            if (playerFoodPoints <= 0)
            {
                SoundManager.instance.PlaySingle(gameOverSound);
                SoundManager.instance.musicSource.Stop();

                //Call the GameOver function of GameManager.
                GameOver();
            }
        }

        private void UpdateFoodText(int delta = 0)
        {
            if (delta != 0)
            {
                foodText.text = delta.ToString() + " Food: " + playerFoodPoints;
            }
            else
            {
                foodText.text = "Food: " + playerFoodPoints;
            }
        }

        private InputMovingDir DetectPlayerMovingDir()
        {
            InputMovingDir result = new InputMovingDir();
            // int horizontal = 0;     //Used to store the horizontal move direction.
            // int vertical = 0;       //Used to store the vertical move direction.

            result.horizontal = (int)(Input.GetAxisRaw("Horizontal"));

            //Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
            result.vertical = (int)(Input.GetAxisRaw("Vertical"));

            //Check if moving horizontally, if so set vertical to zero.
            if (result.horizontal != 0)
                result.vertical = 0;

            return result;
        }

        //Update is called every frame.
        void Update()
        {
            if (doingSetup) {
                return;
            }

            if (playersTurn) {
                if (playerMoving) {
                    return;
                }

                InputMovingDir movingDir = DetectPlayerMovingDir();
                if (!movingDir.isEmpty()) {
                    playerMoving = true;
                    Debug.Log("Player Moving Called");
                    player.AttemptMoveWithCallback<Wall>(
                        movingDir.horizontal,
                        movingDir.vertical,() => {
                            playerMoving = false;
                        });

                    // enable enemies action immediately
                    playersTurn = false;
                }
            } else {
                // ememy's turn
                if (enemiesMoving) {
                    return;
                }

                enemiesMoving = true;
                StartCoroutine(CombineCoroutineWithCallback(
                    MoveEnemies(), () => {
                        enemiesMoving = false;
                        playersTurn = true;
                    }));
            }
        }

        //Call this to add the passed in Enemy to the List of Enemy objects.
        public void AddEnemyToList(Enemy script)
        {
            //Add Enemy to List enemies.
            enemies.Add(script);
        }


        //GameOver is called when the player reaches 0 food points
        public void GameOver()
        {
            //Set levelText to display number of levels passed and game over message
            levelText.text = "After " + level + " days, you starved.";

            //Enable black background image gameObject.
            levelImage.SetActive(true);

            //Disable this GameManager.
            enabled = false;
        }

        //Coroutine to move enemies in sequence.
        IEnumerator MoveEnemies()
        {
            //While enemiesMoving is true player is unable to move.
            enemiesMoving = true;

            //Wait for turnDelay seconds, defaults to .1 (100 ms).
            yield return new WaitForSeconds(turnDelay);

            //If there are no enemies spawned (IE in first level):
            if (enemies.Count == 0)
            {
                //Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
                yield return new WaitForSeconds(turnDelay);
            }

            //Loop through List of Enemy objects.
            for (int i = 0; i < enemies.Count; i++)
            {
                //Call the MoveEnemy function of Enemy at index i in the enemies List.
                enemies[i].MoveEnemy();

                //Wait for Enemy's moveTime before moving next Enemy, 
                yield return new WaitForSeconds(enemies[i].moveTime);
            }
            //Once Enemies are done moving, set playersTurn to true so player can move.
            playersTurn = true;

            //Enemies are done moving, set enemiesMoving to false.
            enemiesMoving = false;
        }

        public void onPlayerEnterExit()
        {
            // to disable input
            doingSetup = true;

            //Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
            Invoke("Restart", restartLevelDelay);
        }

        //Restart reloads the scene when called.
        private void Restart()
        {
            //Load the last scene loaded, in this case Main, the only scene in the game.
            SceneManager.LoadScene(0);
        }
    }
}
