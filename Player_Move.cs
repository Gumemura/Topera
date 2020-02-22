using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;
 
// require SpriteRenderer for changing the player's sprite
[RequireComponent(typeof(SpriteRenderer))]
public class Player_Move : MonoBehaviour
{
	// walkspeed in tiles per second
	public float walkSpeed = 3f;
 
	// the tilemap which has the tiles we want to collide with
	public Tilemap tilemap;
 
	// the amount of time we can press an input without moving in seconds
	public float moveDelay = 0.2f;
 
	//enable and disable tilemaps
	public Grid grid;

	// our player's direction
	Direction currentDir = Direction.South;
 
	// a vector storing the input of our input-axis
	Vector2 input;
 
	// states if the player is moving or waiting for movement input
	bool isMoving = false;

	//chances the rigidbody2D sleep mode
	bool contact_trigger = false;
 
	// position before a move is executed
	Vector3 startPos;
 
	// target-position after the move is executed
	Vector3 endPos;

	//rigidbody2d used to get contact with triger colider
	Rigidbody2D rb2d;


	// stores the progress of the current move in a range from 0f to 1f
	float progress;
 
	// stores the time remaining before the player can move again
	float remainingMoveDelay = 0;
 
	// since we currently do not use any animation components we just use four
	// different sprites for our four directions
	public Sprite northSprite;
	public Sprite eastSprite;
	public Sprite southSprite;

	void Start(){
		rb2d = GetComponent<Rigidbody2D>();
	}

	void Update()
	{
		//Changes rigidoby2d sleep mode
		if(contact_trigger == false){
			rb2d.sleepMode = RigidbodySleepMode2D.StartAwake;
		}else{
			rb2d.sleepMode = RigidbodySleepMode2D.NeverSleep;	
		}

		// check if the player is moving
		if (!isMoving)
		{
			// The player is currently not moving so check if there is keyinput
			input = new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"));
 
			// if there is input in x direction disable input in y direction to
			// disable diagonal movement
			if (input.x != 0)
				input.y = 0;
 
			// check if there is infact movement or if the input axis are in idle
			// position
			if (input != Vector2.zero)
			{
				// save the old direction for later use
				Direction oldDirection = currentDir;
 
				// update the players direction according to the input
				#region update Direction
				if (input.x < 0){
					input.x = -1; 
					currentDir = Direction.West;
				}else if(input.x > 0){
					input.x = 1;	
					currentDir = Direction.East;
				}

				if (input.y > 0){
					input.y = 1;	
					currentDir = Direction.North;
				}else if(input.y < 0){
					input.y = -1;
					currentDir = Direction.South;
				}
				#endregion

				//flips the object
				gameObject.GetComponent<SpriteRenderer>().flipX = (input.x > 0);
 
				// seting the sprite according to the direction
				switch (currentDir)
				{
					case Direction.North:
						gameObject.GetComponent<SpriteRenderer>().sprite = northSprite;
						break;
					case Direction.East:
						gameObject.GetComponent<SpriteRenderer>().sprite = eastSprite;
						break;
					case Direction.West:
						gameObject.GetComponent<SpriteRenderer>().sprite = eastSprite;
						break;
					case Direction.South:
						gameObject.GetComponent<SpriteRenderer>().sprite = southSprite;
						break;
				}
 				

				// if the currentDirection is different from the old direction
				// we want to add a delay so the player can just change direction
				// without having to move
				if (currentDir != oldDirection)
				{
					remainingMoveDelay = moveDelay;
				}
 
				// if the direction of the input does not change then the move-
				// delay ticks down
				if (remainingMoveDelay > 0)
				{
					remainingMoveDelay -= Time.deltaTime;
					return;
				}
 
				// for the collision detection and movement we need the current
				// position as well as the target position where our player
				// is going to move to
				startPos = transform.position;
				endPos = new Vector3(startPos.x + input.x, startPos.y + input.y, startPos.z);
 
				// we subtract 0.5 both in x and y direction to get the coordinates
				// of the upper left corner of our player sprite and convert
				// the floating point vector into an int vector for tile search
				Vector3Int tilePosition = new Vector3Int((int)(endPos.x - .5f), (int)(endPos.y - .5f), 0);
 
				// with our freshly calculated tile position of the tile where our
				// player want to move to we can now check if there is in fact
				// a tile at that position which we would collide with
				// if there is no tile so the GetTile-function return null then
				// we can go ahead and move towards our target


				if (tilemap.GetTile(tilePosition) == null)
				{
					// we set our moving variable to true and our progress
					// towards the target position to 0
					isMoving = true;
					progress = 0;
				}
			}
		// check if the player is currently in the moving state
		}else{
			// check if the progress is still below 1f so the movement is still
			// going on
			contact_trigger = false;	
			if (progress < 1)
			{
				// increase our movement progress by our deltaTime times our
				// above specified walkspeed
				progress += Time.deltaTime * walkSpeed;
 
				// linearly interpolate between our start- and end-positions
				// with the value of our progress which is in range of [0, 1]
				transform.position = Vector3.Lerp(startPos, endPos, progress);
			}
			else
			{
				// if we are moving and our progress is above one that means we
				// either landed exactly on our desired position or we overshot
				// by some tiny amount so in ordered to not accumulate errors
				// we clamp our final position to our desired end-position
				isMoving = false;
				transform.position = endPos;
			}
		}
	}

	void OnTriggerStay2D(){
		contact_trigger = true;
		if(Input.GetKeyDown(KeyCode.Space)){
			foreach(Transform child in grid.transform){
				if(child.gameObject.name != "Subsolo"){
					child.gameObject.SetActive(!child.gameObject.activeSelf);
				}
			}
		}
	}
}
 
// small Enumeration to help us keep track of the player's direction more easyly
enum Direction
{
	North, East, South, West
}
