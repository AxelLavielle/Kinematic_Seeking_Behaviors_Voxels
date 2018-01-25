using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tagHandler : MonoBehaviour {

	int	_nbPlayerMax;
	int	_nbPlayer;
	List<NPCTag> _child = new List<NPCTag>();
	int hunter;
	[SerializeField]
	Material basic;
	[SerializeField]
	Material frozen;
	[SerializeField]
	Material hunt;

	void Start () {
		_nbPlayerMax = 0;
		foreach (Transform child in transform) { // Get all the children
			_nbPlayerMax++;
			_child.Add (child.transform.gameObject.GetComponent<NPCTag>());
			_child[_nbPlayerMax - 1]._id = _nbPlayerMax - 1;
		}
		hunter = Random.Range (0, _nbPlayerMax); // Set a hunter
		_child [hunter].GetComponent<MeshRenderer> ().material = hunt;
		_nbPlayer = _nbPlayerMax;
		int i = -1;
		while (++i != _nbPlayerMax)
			_child [i].setTarget (_child [hunter], true); // Make every other child flee the hunter
		_child[hunter].setTarget(_child[(hunter + 1) % _nbPlayerMax], false);//make the hunter hunt a random target, will be reset in the next update anyway
		_child [hunter]._hunter = true;
	}
	
	void Update () {
		int i = -1;
		int idNear = hunter;
		float maxDist = 10000;
		float dist;
		Vector3 dist_stock;
		//This part choose the target to hunt, choosing the nearest unfrozen target
		while (++i != _nbPlayerMax)
			if (i != hunter && !_child [i]._frozen) {
				//This part handle the walls knowledge of the AI
				Vector3 _wallTarget = _child[i].transform.position;
				if (_wallTarget.x - transform.position.x > 4.5f) // left
					_wallTarget.x -= 10f;
				else if (_wallTarget.x - transform.position.x < -4.5f) // Right
					_wallTarget.x += 10f;
				if (_wallTarget.z - transform.position.z > 4.5f) // Down
					_wallTarget.z -= 10f;
				else if (_wallTarget.z - transform.position.z < -4.5f) // Up
					_wallTarget.z += 10f;

				//Basic min distance searching
				dist_stock = new Vector3(_child[hunter].transform.position.x - _wallTarget.x, 0, _child[hunter].transform.position.z - _wallTarget.z);
				dist = Mathf.Sqrt (dist_stock.x * dist_stock.x + dist_stock.z * dist_stock.z);
				if (dist < maxDist) {
					maxDist = dist;
					idNear = i;
				}
			}
		//Set the new target to the hunter and make this target flee the hunter
		_child [hunter].setTarget (_child [idNear], false);
		_child [idNear].setTarget (_child [hunter], true);

		//This part select the frozen target
		i = -1;
		int frozen = -1;
		while (++i != _nbPlayerMax)
			if (_child[i]._frozen)
				frozen = i;
		//If there is a frozen target, we assign every person available (not hunted) to the frozen target, else we make them flee the hunter
		i = -1;
		while (++i != _nbPlayerMax)
			if (i != idNear && i != hunter && frozen != -1)
				_child [i].setTarget (_child [frozen], false);
			else if (i != idNear && i != hunter)
				_child [i].setTarget (_child [hunter], true);
	}

	public void incrementNbPlayer(int id) { // Unfreeze a player
		_nbPlayer++;
		_child [id].GetComponent<MeshRenderer> ().material = basic;
		_child [id]._frozen = false;
	}

	public void reduceNbPlayer(int id) { // Freeze a player and reset the game if game is finished
		_nbPlayer--;
		if (_nbPlayer == 1) { // Reset the game
			_child [hunter]._hunter = false;
			hunter = id;
			_child [hunter]._hunter = true;
			int j = 0;
			while (j != _nbPlayerMax) {
				_child [j]._frozen = false;
				_child [j++].GetComponent<MeshRenderer> ().material = basic;
			}
			_child [hunter].GetComponent<MeshRenderer> ().material = hunt;
			_nbPlayer = _nbPlayerMax;
		} else { // Freeze a player
			_child [id]._frozen = true;
			_child [id].GetComponent<MeshRenderer> ().material = frozen;
		}
	}
}
